using Data.Data;
using Interfaces.Magazyn;
using Interfaces.Magazyn.Dtos;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.Magazyn
{
    public class PartiaService : BaseService, IPartiaService
    {
        public PartiaService(DataContext context) : base(context)
        {
        }

        public async Task<PartiaIndexDto> GetIndexDataAsync(string? searchTerm)
        {
            var query = _context.Partia
                .AsNoTracking()
                .Include(p => p.Produkt)
                    .ThenInclude(x => x.DomyslnaJednostka)
                .Include(p => p.Dostawca)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(p =>
                    EF.Functions.Like(p.NumerPartii, $"%{term}%") ||
                    EF.Functions.Like(p.Produkt.Kod, $"%{term}%") ||
                    EF.Functions.Like(p.Produkt.Nazwa, $"%{term}%") ||
                    (p.Dostawca != null && EF.Functions.Like(p.Dostawca.Nazwa, $"%{term}%")));
            }

            var items = await query
                .OrderBy(p => p.DataWaznosci ?? DateTime.MaxValue)
                .ThenBy(p => p.NumerPartii)
                .ToListAsync();

            return new PartiaIndexDto
            {
                SearchTerm = searchTerm,
                Items = items
            };
        }

        public async Task<PartiaDetailsDto?> GetDetailsDataAsync(int idPartii)
        {
            var partia = await _context.Partia
                .AsNoTracking()
                .Include(p => p.Produkt)
                    .ThenInclude(x => x.DomyslnaJednostka)
                .Include(p => p.Dostawca)
                .FirstOrDefaultAsync(p => p.IdPartii == idPartii);

            if (partia == null)
            {
                return null;
            }

            var count = await _context.Partia
                .AsNoTracking()
                .CountAsync(p => p.IdProduktu == partia.IdProduktu);

            return new PartiaDetailsDto
            {
                Partia = partia,
                LiczbaPartiiProduktu = count
            };
        }

        public async Task<PartiaDeleteDto?> GetDeleteDataAsync(int idPartii)
        {
            var partia = await _context.Partia
                .AsNoTracking()
                .Include(p => p.Produkt)
                    .ThenInclude(x => x.DomyslnaJednostka)
                .Include(p => p.Dostawca)
                .FirstOrDefaultAsync(p => p.IdPartii == idPartii);

            if (partia == null)
            {
                return null;
            }

            var count = await _context.Partia
                .AsNoTracking()
                .CountAsync(p => p.IdProduktu == partia.IdProduktu);

            return new PartiaDeleteDto
            {
                Partia = partia,
                LiczbaPartiiProduktu = count
            };
        }
    }
}
