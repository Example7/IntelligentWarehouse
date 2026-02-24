using Data.Data;
using Interfaces.Magazyn;
using Interfaces.Magazyn.Dtos;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.Magazyn
{
    public class ProduktJednostkaService : BaseService, IProduktJednostkaService
    {
        public ProduktJednostkaService(DataContext context) : base(context)
        {
        }

        public async Task<ProduktJednostkaIndexDto> GetIndexDataAsync(string? searchTerm)
        {
            var query = _context.ProduktJednostka
                .AsNoTracking()
                .Include(p => p.Produkt)
                    .ThenInclude(x => x.DomyslnaJednostka)
                .Include(p => p.Jednostka)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(x =>
                    EF.Functions.Like(x.Produkt.Kod, $"%{term}%") ||
                    EF.Functions.Like(x.Produkt.Nazwa, $"%{term}%") ||
                    EF.Functions.Like(x.Jednostka.Kod, $"%{term}%") ||
                    EF.Functions.Like(x.Jednostka.Nazwa, $"%{term}%"));
            }

            var items = await query
                .OrderBy(x => x.Produkt.Kod)
                .ThenBy(x => x.Jednostka.Kod)
                .ToListAsync();

            return new ProduktJednostkaIndexDto
            {
                SearchTerm = searchTerm,
                Items = items
            };
        }

        public async Task<ProduktJednostkaDetailsDto?> GetDetailsDataAsync(int id)
        {
            var item = await _context.ProduktJednostka
                .AsNoTracking()
                .Include(x => x.Produkt)
                    .ThenInclude(p => p.DomyslnaJednostka)
                .Include(x => x.Jednostka)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (item == null)
            {
                return null;
            }

            var count = await _context.ProduktJednostka
                .AsNoTracking()
                .CountAsync(x => x.IdProduktu == item.IdProduktu);

            return new ProduktJednostkaDetailsDto
            {
                ProduktJednostka = item,
                LiczbaJednostekProduktu = count
            };
        }

        public async Task<ProduktJednostkaDeleteDto?> GetDeleteDataAsync(int id)
        {
            var item = await _context.ProduktJednostka
                .AsNoTracking()
                .Include(x => x.Produkt)
                    .ThenInclude(p => p.DomyslnaJednostka)
                .Include(x => x.Jednostka)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (item == null)
            {
                return null;
            }

            var count = await _context.ProduktJednostka
                .AsNoTracking()
                .CountAsync(x => x.IdProduktu == item.IdProduktu);

            return new ProduktJednostkaDeleteDto
            {
                ProduktJednostka = item,
                LiczbaJednostekProduktu = count
            };
        }
    }
}
