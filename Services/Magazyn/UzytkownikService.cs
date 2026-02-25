using Data.Data;
using Interfaces.Magazyn;
using Interfaces.Magazyn.Dtos;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.Magazyn
{
    public class UzytkownikService : BaseService, IUzytkownikService
    {
        public UzytkownikService(DataContext context) : base(context)
        {
        }

        public async Task<UzytkownikIndexDto> GetIndexDataAsync(string? searchTerm)
        {
            var query = _context.Uzytkownik.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(x =>
                    EF.Functions.Like(x.Login, $"%{term}%") ||
                    EF.Functions.Like(x.Email, $"%{term}%"));
            }

            var users = await query.OrderBy(x => x.Login).ToListAsync();
            var ids = users.Select(x => x.IdUzytkownika).ToList();

            var roleCounts = ids.Count == 0
                ? new Dictionary<int, int>()
                : await _context.UzytkownikRola
                    .AsNoTracking()
                    .Where(x => ids.Contains(x.IdUzytkownika))
                    .GroupBy(x => x.IdUzytkownika)
                    .Select(g => new { Id = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Id, x => x.Count);

            var pzCounts = ids.Count == 0
                ? new Dictionary<int, int>()
                : await _context.DokumentPZ
                    .AsNoTracking()
                    .Where(x => ids.Contains(x.IdUtworzyl))
                    .GroupBy(x => x.IdUtworzyl)
                    .Select(g => new { Id = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Id, x => x.Count);

            var wzCounts = ids.Count == 0
                ? new Dictionary<int, int>()
                : await _context.DokumentWZ
                    .AsNoTracking()
                    .Where(x => ids.Contains(x.IdUtworzyl))
                    .GroupBy(x => x.IdUtworzyl)
                    .Select(g => new { Id = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Id, x => x.Count);

            var mmCounts = ids.Count == 0
                ? new Dictionary<int, int>()
                : await _context.DokumentMM
                    .AsNoTracking()
                    .Where(x => ids.Contains(x.IdUtworzyl))
                    .GroupBy(x => x.IdUtworzyl)
                    .Select(g => new { Id = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Id, x => x.Count);

            return new UzytkownikIndexDto
            {
                SearchTerm = searchTerm,
                Items = users.Select(x => new UzytkownikIndexItemDto
                {
                    Uzytkownik = x,
                    LiczbaRol = roleCounts.GetValueOrDefault(x.IdUzytkownika),
                    LiczbaDokumentow =
                        pzCounts.GetValueOrDefault(x.IdUzytkownika) +
                        wzCounts.GetValueOrDefault(x.IdUzytkownika) +
                        mmCounts.GetValueOrDefault(x.IdUzytkownika)
                }).ToList()
            };
        }

        public async Task<UzytkownikDetailsDto?> GetDetailsDataAsync(int idUzytkownika)
        {
            var user = await _context.Uzytkownik
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdUzytkownika == idUzytkownika);

            if (user == null)
            {
                return null;
            }

            var roleNames = await _context.UzytkownikRola
                .AsNoTracking()
                .Where(x => x.IdUzytkownika == idUzytkownika)
                .Include(x => x.Rola)
                .OrderBy(x => x.Rola.Nazwa)
                .Select(x => x.Rola.Nazwa)
                .ToListAsync();

            var ostatnieDokumentyPz = await _context.DokumentPZ
                .AsNoTracking()
                .Include(x => x.Magazyn)
                .Where(x => x.IdUtworzyl == idUzytkownika)
                .OrderByDescending(x => x.DataPrzyjeciaUtc)
                .ThenByDescending(x => x.Id)
                .Take(5)
                .ToListAsync();

            var ostatnieDokumentyWz = await _context.DokumentWZ
                .AsNoTracking()
                .Include(x => x.Magazyn)
                .Include(x => x.Klient)
                .Where(x => x.IdUtworzyl == idUzytkownika)
                .OrderByDescending(x => x.DataWydaniaUtc)
                .ThenByDescending(x => x.Id)
                .Take(5)
                .ToListAsync();

            var ostatnieDokumentyMm = await _context.DokumentMM
                .AsNoTracking()
                .Include(x => x.Magazyn)
                .Where(x => x.IdUtworzyl == idUzytkownika)
                .OrderByDescending(x => x.DataUtc)
                .ThenByDescending(x => x.Id)
                .Take(5)
                .ToListAsync();

            var ostatnieRuchy = await _context.RuchMagazynowy
                .AsNoTracking()
                .Include(x => x.Produkt)
                .Include(x => x.LokacjaZ)
                .Include(x => x.LokacjaDo)
                .Where(x => x.IdUzytkownika == idUzytkownika)
                .OrderByDescending(x => x.UtworzonoUtc)
                .ThenByDescending(x => x.IdRuchu)
                .Take(8)
                .ToListAsync();

            var ostatnieLogiAudytu = await _context.LogAudytu
                .AsNoTracking()
                .Where(x => x.UserId == idUzytkownika)
                .OrderByDescending(x => x.KiedyUtc)
                .ThenByDescending(x => x.Id)
                .Take(8)
                .ToListAsync();

            return new UzytkownikDetailsDto
            {
                Uzytkownik = user,
                RoleNazwy = roleNames,
                LiczbaRol = await _context.UzytkownikRola.AsNoTracking().CountAsync(x => x.IdUzytkownika == idUzytkownika),
                LiczbaKlientow = await _context.Klient.AsNoTracking().CountAsync(x => x.IdUzytkownika == idUzytkownika),
                LiczbaDokumentowPz = await _context.DokumentPZ.AsNoTracking().CountAsync(x => x.IdUtworzyl == idUzytkownika),
                LiczbaDokumentowWz = await _context.DokumentWZ.AsNoTracking().CountAsync(x => x.IdUtworzyl == idUzytkownika),
                LiczbaDokumentowMm = await _context.DokumentMM.AsNoTracking().CountAsync(x => x.IdUtworzyl == idUzytkownika),
                LiczbaInwentaryzacji = await _context.Inwentaryzacja.AsNoTracking().CountAsync(x => x.IdUtworzyl == idUzytkownika),
                LiczbaRezerwacji = await _context.Rezerwacja.AsNoTracking().CountAsync(x => x.IdUtworzyl == idUzytkownika),
                LiczbaRuchow = await _context.RuchMagazynowy.AsNoTracking().CountAsync(x => x.IdUzytkownika == idUzytkownika),
                LiczbaLogowAudytu = await _context.LogAudytu.AsNoTracking().CountAsync(x => x.UserId == idUzytkownika),
                OstatnieDokumentyPz = ostatnieDokumentyPz,
                OstatnieDokumentyWz = ostatnieDokumentyWz,
                OstatnieDokumentyMm = ostatnieDokumentyMm,
                OstatnieRuchy = ostatnieRuchy,
                OstatnieLogiAudytu = ostatnieLogiAudytu
            };
        }

        public async Task<UzytkownikDeleteDto?> GetDeleteDataAsync(int idUzytkownika)
        {
            var details = await GetDetailsDataAsync(idUzytkownika);
            if (details == null)
            {
                return null;
            }

            return new UzytkownikDeleteDto
            {
                Uzytkownik = details.Uzytkownik,
                LiczbaRol = details.LiczbaRol,
                LiczbaKlientow = details.LiczbaKlientow,
                LiczbaDokumentowPz = details.LiczbaDokumentowPz,
                LiczbaDokumentowWz = details.LiczbaDokumentowWz,
                LiczbaDokumentowMm = details.LiczbaDokumentowMm,
                LiczbaInwentaryzacji = details.LiczbaInwentaryzacji,
                LiczbaRezerwacji = details.LiczbaRezerwacji,
                LiczbaRuchow = details.LiczbaRuchow,
                LiczbaLogowAudytu = details.LiczbaLogowAudytu
            };
        }
    }
}
