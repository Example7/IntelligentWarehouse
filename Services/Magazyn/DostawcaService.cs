using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;
using Interfaces.Magazyn.Dtos;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.Magazyn
{
    public class DostawcaService : BaseService, IDostawcaService
    {
        public DostawcaService(DataContext context) : base(context)
        {
        }

        public async Task<DostawcaIndexDto> GetIndexDataAsync(string? searchTerm)
        {
            var query = _context.Dostawca.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(x =>
                    EF.Functions.Like(x.Nazwa, $"%{term}%") ||
                    (x.NIP != null && EF.Functions.Like(x.NIP, $"%{term}%")) ||
                    (x.Email != null && EF.Functions.Like(x.Email, $"%{term}%")) ||
                    (x.Telefon != null && EF.Functions.Like(x.Telefon, $"%{term}%")) ||
                    (x.Adres != null && EF.Functions.Like(x.Adres, $"%{term}%")));
            }

            var dostawcy = await query.ToListAsync();
            var ids = dostawcy.Select(x => x.IdDostawcy).ToList();

            var pzCounts = await _context.DokumentPZ
                .AsNoTracking()
                .Where(d => ids.Contains(d.IdDostawcy))
                .GroupBy(d => d.IdDostawcy)
                .Select(g => new { IdDostawcy = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.IdDostawcy, x => x.Count);

            var batchCounts = await _context.Partia
                .AsNoTracking()
                .Where(p => p.IdDostawcy.HasValue && ids.Contains(p.IdDostawcy.Value))
                .GroupBy(p => p.IdDostawcy!.Value)
                .Select(g => new { IdDostawcy = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.IdDostawcy, x => x.Count);

            return new DostawcaIndexDto
            {
                SearchTerm = searchTerm,
                Items = dostawcy.Select(d => new DostawcaIndexItemDto
                {
                    Dostawca = d,
                    LiczbaDokumentowPz = pzCounts.GetValueOrDefault(d.IdDostawcy),
                    LiczbaPartii = batchCounts.GetValueOrDefault(d.IdDostawcy)
                }).ToList()
            };
        }

        public async Task<DostawcaDetailsDto?> GetDetailsDataAsync(int idDostawcy)
        {
            var dostawca = await _context.Dostawca
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdDostawcy == idDostawcy);
            if (dostawca == null)
            {
                return null;
            }

            var pzQuery = _context.DokumentPZ
                .AsNoTracking()
                .Include(d => d.Magazyn)
                .Include(d => d.Utworzyl)
                .Where(d => d.IdDostawcy == idDostawcy);

            var liczbaDokumentowPz = await pzQuery.CountAsync();
            var liczbaDokumentowPzPosted = await pzQuery.CountAsync(d => d.Status == "Posted");
            var liczbaDokumentowPzDraft = await pzQuery.CountAsync(d => d.Status == "Draft");

            var ostatnieDokumentyPz = await pzQuery
                .OrderByDescending(d => d.DataPrzyjeciaUtc)
                .ThenByDescending(d => d.Id)
                .Take(8)
                .ToListAsync();

            var partieQuery = _context.Partia
                .AsNoTracking()
                .Include(p => p.Produkt)
                .Where(p => p.IdDostawcy == idDostawcy);

            var liczbaPartii = await partieQuery.CountAsync();
            var ostatniePartie = await partieQuery
                .OrderByDescending(p => p.DataWaznosci ?? DateTime.MinValue)
                .ThenByDescending(p => p.IdPartii)
                .Take(8)
                .ToListAsync();

            return new DostawcaDetailsDto
            {
                Dostawca = dostawca,
                LiczbaDokumentowPz = liczbaDokumentowPz,
                LiczbaDokumentowPzPosted = liczbaDokumentowPzPosted,
                LiczbaDokumentowPzDraft = liczbaDokumentowPzDraft,
                LiczbaPartii = liczbaPartii,
                OstatnieDokumentyPz = ostatnieDokumentyPz,
                OstatniePartie = ostatniePartie
            };
        }

        public async Task<DostawcaDeleteDto?> GetDeleteDataAsync(int idDostawcy)
        {
            var dostawca = await _context.Dostawca
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdDostawcy == idDostawcy);
            if (dostawca == null)
            {
                return null;
            }

            var liczbaDokumentowPz = await _context.DokumentPZ
                .AsNoTracking()
                .CountAsync(d => d.IdDostawcy == idDostawcy);
            var liczbaPartii = await _context.Partia
                .AsNoTracking()
                .CountAsync(p => p.IdDostawcy == idDostawcy);

            return new DostawcaDeleteDto
            {
                Dostawca = dostawca,
                LiczbaDokumentowPz = liczbaDokumentowPz,
                LiczbaPartii = liczbaPartii
            };
        }
    }
}
