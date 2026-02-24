using Data.Data;
using Interfaces.Magazyn;
using Interfaces.Magazyn.Dtos;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.Magazyn
{
    public class KodProduktuService : BaseService, IKodProduktuService
    {
        public KodProduktuService(DataContext context) : base(context)
        {
        }

        public async Task<KodProduktuIndexDto> GetIndexDataAsync(string? searchTerm)
        {
            var query = _context.KodProduktu
                .AsNoTracking()
                .Include(k => k.Produkt)
                    .ThenInclude(p => p.DomyslnaJednostka)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(k =>
                    EF.Functions.Like(k.Wartosc, $"%{term}%") ||
                    EF.Functions.Like(k.Typ, $"%{term}%") ||
                    EF.Functions.Like(k.Produkt.Kod, $"%{term}%") ||
                    EF.Functions.Like(k.Produkt.Nazwa, $"%{term}%"));
            }

            var items = await query
                .OrderBy(k => k.Typ)
                .ThenBy(k => k.Wartosc)
                .ToListAsync();

            return new KodProduktuIndexDto
            {
                SearchTerm = searchTerm,
                Items = items
            };
        }

        public async Task<KodProduktuDetailsDto?> GetDetailsDataAsync(int idKodu)
        {
            var kod = await _context.KodProduktu
                .AsNoTracking()
                .Include(k => k.Produkt)
                    .ThenInclude(p => p.DomyslnaJednostka)
                .FirstOrDefaultAsync(k => k.IdKodu == idKodu);

            if (kod == null)
            {
                return null;
            }

            var liczbaKodowProduktu = await _context.KodProduktu
                .AsNoTracking()
                .CountAsync(k => k.IdProduktu == kod.IdProduktu);

            return new KodProduktuDetailsDto
            {
                KodProduktu = kod,
                LiczbaKodowProduktu = liczbaKodowProduktu
            };
        }

        public async Task<KodProduktuDeleteDto?> GetDeleteDataAsync(int idKodu)
        {
            var kod = await _context.KodProduktu
                .AsNoTracking()
                .Include(k => k.Produkt)
                    .ThenInclude(p => p.DomyslnaJednostka)
                .FirstOrDefaultAsync(k => k.IdKodu == idKodu);

            if (kod == null)
            {
                return null;
            }

            var liczbaKodowProduktu = await _context.KodProduktu
                .AsNoTracking()
                .CountAsync(k => k.IdProduktu == kod.IdProduktu);

            return new KodProduktuDeleteDto
            {
                KodProduktu = kod,
                LiczbaKodowProduktu = liczbaKodowProduktu
            };
        }
    }
}
