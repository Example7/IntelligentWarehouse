using Data.Data;
using Interfaces.CMS;
using Interfaces.CMS.Dtos;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.CMS
{
    public class SzablonWydrukuService : BaseService, ISzablonWydrukuService
    {
        public SzablonWydrukuService(DataContext context) : base(context)
        {
        }

        public async Task<SzablonWydrukuIndexDto> GetIndexDataAsync(string? searchTerm)
        {
            var term = (searchTerm ?? string.Empty).Trim();
            var query = _context.SzablonWydruku
                .AsNoTracking()
                .Include(x => x.Wgral)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(term))
            {
                query = query.Where(x =>
                    x.TypDokumentu.Contains(term) ||
                    x.Nazwa.Contains(term) ||
                    x.Wersja.Contains(term) ||
                    x.NazwaPliku.Contains(term) ||
                    x.Sciezka.Contains(term));
            }

            var items = await query
                .OrderBy(x => x.TypDokumentu)
                .ThenBy(x => x.Nazwa)
                .ThenByDescending(x => x.Wersja)
                .Select(x => new SzablonWydrukuIndexItemDto
                {
                    Id = x.Id,
                    TypDokumentu = x.TypDokumentu,
                    Nazwa = x.Nazwa,
                    Wersja = x.Wersja,
                    NazwaPliku = x.NazwaPliku,
                    Sciezka = x.Sciezka,
                    CzyAktywny = x.CzyAktywny,
                    WgranoUtc = x.WgranoUtc,
                    WgralEmail = x.Wgral != null ? x.Wgral.Email : null
                })
                .ToListAsync();

            return new SzablonWydrukuIndexDto
            {
                SearchTerm = term,
                Items = items
            };
        }

        public async Task<SzablonWydrukuDetailsDto?> GetDetailsDataAsync(int id)
        {
            return await _context.SzablonWydruku
                .AsNoTracking()
                .Include(x => x.Wgral)
                .Where(x => x.Id == id)
                .Select(x => new SzablonWydrukuDetailsDto
                {
                    Id = x.Id,
                    TypDokumentu = x.TypDokumentu,
                    Nazwa = x.Nazwa,
                    Wersja = x.Wersja,
                    NazwaPliku = x.NazwaPliku,
                    Sciezka = x.Sciezka,
                    CzyAktywny = x.CzyAktywny,
                    WgranoUtc = x.WgranoUtc,
                    WgralUserId = x.WgralUserId,
                    WgralEmail = x.Wgral != null ? x.Wgral.Email : null
                })
                .FirstOrDefaultAsync();
        }

        public async Task<SzablonWydrukuDeleteDto?> GetDeleteDataAsync(int id)
        {
            return await _context.SzablonWydruku
                .AsNoTracking()
                .Include(x => x.Wgral)
                .Where(x => x.Id == id)
                .Select(x => new SzablonWydrukuDeleteDto
                {
                    Id = x.Id,
                    TypDokumentu = x.TypDokumentu,
                    Nazwa = x.Nazwa,
                    Wersja = x.Wersja,
                    NazwaPliku = x.NazwaPliku,
                    Sciezka = x.Sciezka,
                    CzyAktywny = x.CzyAktywny,
                    WgranoUtc = x.WgranoUtc,
                    WgralEmail = x.Wgral != null ? x.Wgral.Email : null
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> TypIWersjaExistsAsync(string typDokumentu, string wersja, int? excludeId = null)
        {
            var typ = (typDokumentu ?? string.Empty).Trim();
            var ver = (wersja ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(typ) || string.IsNullOrWhiteSpace(ver))
            {
                return false;
            }

            var query = _context.SzablonWydruku
                .AsNoTracking()
                .Where(x => x.TypDokumentu == typ && x.Wersja == ver);

            if (excludeId.HasValue)
            {
                query = query.Where(x => x.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
