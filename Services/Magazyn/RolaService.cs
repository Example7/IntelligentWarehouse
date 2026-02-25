using Data.Data;
using Interfaces.Magazyn;
using Interfaces.Magazyn.Dtos;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.Magazyn
{
    public class RolaService : BaseService, IRolaService
    {
        public RolaService(DataContext context) : base(context)
        {
        }

        public async Task<RolaIndexDto> GetIndexDataAsync(string? searchTerm)
        {
            var query = _context.Rola.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(x => EF.Functions.Like(x.Nazwa, $"%{term}%"));
            }

            var roles = await query.OrderBy(x => x.Nazwa).ToListAsync();
            var roleIds = roles.Select(x => x.IdRoli).ToList();

            var counts = roleIds.Count == 0
                ? new Dictionary<int, int>()
                : await _context.UzytkownikRola
                    .AsNoTracking()
                    .Where(x => roleIds.Contains(x.IdRoli))
                    .GroupBy(x => x.IdRoli)
                    .Select(g => new { Id = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Id, x => x.Count);

            return new RolaIndexDto
            {
                SearchTerm = searchTerm,
                Items = roles.Select(x => new RolaIndexItemDto
                {
                    Rola = x,
                    LiczbaUzytkownikow = counts.GetValueOrDefault(x.IdRoli)
                }).ToList()
            };
        }

        public async Task<RolaDetailsDto?> GetDetailsDataAsync(int idRoli)
        {
            var role = await _context.Rola
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdRoli == idRoli);

            if (role == null)
            {
                return null;
            }

            var przypisania = await _context.UzytkownikRola
                .AsNoTracking()
                .Include(x => x.Uzytkownik)
                .Include(x => x.Rola)
                .Where(x => x.IdRoli == idRoli)
                .OrderBy(x => x.Uzytkownik.Login)
                .ThenBy(x => x.Id)
                .Take(10)
                .ToListAsync();

            var count = await _context.UzytkownikRola
                .AsNoTracking()
                .CountAsync(x => x.IdRoli == idRoli);

            return new RolaDetailsDto
            {
                Rola = role,
                LiczbaPrzypisan = count,
                OstatniePrzypisania = przypisania
            };
        }

        public async Task<RolaDeleteDto?> GetDeleteDataAsync(int idRoli)
        {
            var role = await _context.Rola
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdRoli == idRoli);

            if (role == null)
            {
                return null;
            }

            var count = await _context.UzytkownikRola
                .AsNoTracking()
                .CountAsync(x => x.IdRoli == idRoli);

            return new RolaDeleteDto
            {
                Rola = role,
                LiczbaPrzypisan = count
            };
        }
    }
}
