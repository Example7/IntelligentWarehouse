using Data.Data;
using Interfaces.Magazyn;
using Interfaces.Magazyn.Dtos;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.Magazyn
{
    public class KlientService : BaseService, IKlientService
    {
        public KlientService(DataContext context) : base(context)
        {
        }

        public async Task<KlientIndexDto> GetIndexDataAsync(string? searchTerm)
        {
            var query = _context.Klient.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(x =>
                    EF.Functions.Like(x.Nazwa, $"%{term}%") ||
                    (x.Email != null && EF.Functions.Like(x.Email, $"%{term}%")) ||
                    (x.Telefon != null && EF.Functions.Like(x.Telefon, $"%{term}%")) ||
                    (x.Adres != null && EF.Functions.Like(x.Adres, $"%{term}%")));
            }

            var klienci = await query.ToListAsync();
            var ids = klienci.Select(x => x.IdKlienta).ToList();

            var wzCounts = await _context.DokumentWZ
                .AsNoTracking()
                .Where(d => d.IdKlienta.HasValue && ids.Contains(d.IdKlienta.Value))
                .GroupBy(d => d.IdKlienta!.Value)
                .Select(g => new { IdKlienta = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.IdKlienta, x => x.Count);

            return new KlientIndexDto
            {
                SearchTerm = searchTerm,
                Items = klienci.Select(k => new KlientIndexItemDto
                {
                    Klient = k,
                    LiczbaDokumentowWz = wzCounts.GetValueOrDefault(k.IdKlienta)
                }).ToList()
            };
        }

        public async Task<KlientDetailsDto?> GetDetailsDataAsync(int idKlienta)
        {
            var klient = await _context.Klient
                .AsNoTracking()
                .Include(k => k.Uzytkownik)
                .FirstOrDefaultAsync(x => x.IdKlienta == idKlienta);
            if (klient == null)
            {
                return null;
            }

            var wzQuery = _context.DokumentWZ
                .AsNoTracking()
                .Include(d => d.Magazyn)
                .Include(d => d.Utworzyl)
                .Where(d => d.IdKlienta == idKlienta);

            var liczbaDokumentowWz = await wzQuery.CountAsync();
            var liczbaDokumentowWzPosted = await wzQuery.CountAsync(d => d.Status == "Posted");
            var liczbaDokumentowWzDraft = await wzQuery.CountAsync(d => d.Status == "Draft");

            var ostatnieDokumentyWz = await wzQuery
                .OrderByDescending(d => d.DataWydaniaUtc)
                .ThenByDescending(d => d.Id)
                .Take(8)
                .ToListAsync();

            return new KlientDetailsDto
            {
                Klient = klient,
                LiczbaDokumentowWz = liczbaDokumentowWz,
                LiczbaDokumentowWzPosted = liczbaDokumentowWzPosted,
                LiczbaDokumentowWzDraft = liczbaDokumentowWzDraft,
                OstatnieDokumentyWz = ostatnieDokumentyWz
            };
        }

        public async Task<KlientDeleteDto?> GetDeleteDataAsync(int idKlienta)
        {
            var klient = await _context.Klient
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdKlienta == idKlienta);
            if (klient == null)
            {
                return null;
            }

            var liczbaDokumentowWz = await _context.DokumentWZ
                .AsNoTracking()
                .CountAsync(d => d.IdKlienta == idKlienta);

            return new KlientDeleteDto
            {
                Klient = klient,
                LiczbaDokumentowWz = liczbaDokumentowWz
            };
        }
    }
}
