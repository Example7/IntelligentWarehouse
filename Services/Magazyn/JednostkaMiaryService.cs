using Data.Data;
using Interfaces.Magazyn;
using Interfaces.Magazyn.Dtos;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.Magazyn
{
    public class JednostkaMiaryService : BaseService, IJednostkaMiaryService
    {
        public JednostkaMiaryService(DataContext context) : base(context)
        {
        }

        public async Task<JednostkaMiaryIndexDto> GetIndexDataAsync(string? searchTerm)
        {
            var query = _context.JednostkaMiary.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(j =>
                    EF.Functions.Like(j.Kod, $"%{term}%") ||
                    EF.Functions.Like(j.Nazwa, $"%{term}%"));
            }

            var jednostki = await query.ToListAsync();
            var idsJednostek = jednostki.Select(j => j.IdJednostki).Distinct().ToList();

            var liczbyProduktow = new Dictionary<int, int>();
            if (idsJednostek.Count > 0)
            {
                liczbyProduktow = await _context.Produkt
                    .AsNoTracking()
                    .Where(p => idsJednostek.Contains(p.IdDomyslnejJednostki))
                    .GroupBy(p => p.IdDomyslnejJednostki)
                    .Select(g => new { IdJednostki = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.IdJednostki, x => x.Count);
            }

            return new JednostkaMiaryIndexDto
            {
                SearchTerm = searchTerm,
                Items = jednostki.Select(j => new JednostkaMiaryIndexItemDto
                {
                    JednostkaMiary = j,
                    LiczbaProduktow = liczbyProduktow.TryGetValue(j.IdJednostki, out var count) ? count : 0
                }).ToList()
            };
        }

        public async Task<JednostkaMiaryDetailsDto?> GetDetailsDataAsync(int idJednostki)
        {
            var jednostkaMiary = await _context.JednostkaMiary
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdJednostki == idJednostki);

            if (jednostkaMiary == null)
            {
                return null;
            }

            var produktyJednostki = await _context.Produkt
                .AsNoTracking()
                .Include(p => p.Kategoria)
                .Where(p => p.IdDomyslnejJednostki == idJednostki)
                .OrderBy(p => p.Kod)
                .ThenBy(p => p.Nazwa)
                .ToListAsync();

            return new JednostkaMiaryDetailsDto
            {
                JednostkaMiary = jednostkaMiary,
                LiczbaProduktow = produktyJednostki.Count,
                ProduktyJednostki = produktyJednostki
            };
        }

        public async Task<JednostkaMiaryDeleteDto?> GetDeleteDataAsync(int idJednostki)
        {
            var jednostkaMiary = await _context.JednostkaMiary
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdJednostki == idJednostki);

            if (jednostkaMiary == null)
            {
                return null;
            }

            var liczbaProduktow = await GetAssignedProductsCountAsync(idJednostki);

            return new JednostkaMiaryDeleteDto
            {
                JednostkaMiary = jednostkaMiary,
                LiczbaProduktow = liczbaProduktow,
                CzyMoznaUsunac = liczbaProduktow == 0
            };
        }

        public async Task<int> GetAssignedProductsCountAsync(int idJednostki)
        {
            return await _context.Produkt.CountAsync(p => p.IdDomyslnejJednostki == idJednostki);
        }
    }
}
