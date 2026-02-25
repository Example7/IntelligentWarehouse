using Data.Data;
using Interfaces.Magazyn;
using Interfaces.Magazyn.Dtos;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.Magazyn
{
    public class UzytkownikRolaService : BaseService, IUzytkownikRolaService
    {
        public UzytkownikRolaService(DataContext context) : base(context)
        {
        }

        public async Task<UzytkownikRolaIndexDto> GetIndexDataAsync(string? searchTerm)
        {
            var query = _context.UzytkownikRola
                .AsNoTracking()
                .Include(x => x.Uzytkownik)
                .Include(x => x.Rola)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(x =>
                    EF.Functions.Like(x.Uzytkownik.Login, $"%{term}%") ||
                    EF.Functions.Like(x.Uzytkownik.Email, $"%{term}%") ||
                    EF.Functions.Like(x.Rola.Nazwa, $"%{term}%"));
            }

            return new UzytkownikRolaIndexDto
            {
                SearchTerm = searchTerm,
                Items = await query
                    .OrderBy(x => x.Uzytkownik.Email)
                    .ThenBy(x => x.Rola.Nazwa)
                    .ToListAsync()
            };
        }

        public async Task<UzytkownikRolaDetailsDto?> GetDetailsDataAsync(int id)
        {
            var item = await _context.UzytkownikRola
                .AsNoTracking()
                .Include(x => x.Uzytkownik)
                .Include(x => x.Rola)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (item == null)
            {
                return null;
            }

            var liczbaRolUzytkownika = await _context.UzytkownikRola
                .AsNoTracking()
                .CountAsync(x => x.IdUzytkownika == item.IdUzytkownika);

            var liczbaUzytkownikowWRoli = await _context.UzytkownikRola
                .AsNoTracking()
                .CountAsync(x => x.IdRoli == item.IdRoli);

            return new UzytkownikRolaDetailsDto
            {
                UzytkownikRola = item,
                LiczbaRolUzytkownika = liczbaRolUzytkownika,
                LiczbaUzytkownikowWRoli = liczbaUzytkownikowWRoli
            };
        }

        public async Task<UzytkownikRolaDeleteDto?> GetDeleteDataAsync(int id)
        {
            var item = await _context.UzytkownikRola
                .AsNoTracking()
                .Include(x => x.Uzytkownik)
                .Include(x => x.Rola)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (item == null)
            {
                return null;
            }

            return new UzytkownikRolaDeleteDto
            {
                UzytkownikRola = item,
                LiczbaRolUzytkownika = await _context.UzytkownikRola.AsNoTracking().CountAsync(x => x.IdUzytkownika == item.IdUzytkownika),
                LiczbaUzytkownikowWRoli = await _context.UzytkownikRola.AsNoTracking().CountAsync(x => x.IdRoli == item.IdRoli)
            };
        }
    }
}
