using Data.Data;
using Data.Data.CMS;
using Interfaces.CMS;
using Interfaces.CMS.Dtos;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.CMS
{
    public class AktualnoscService : BaseService, IAktualnoscService
    {
        public AktualnoscService(DataContext context) : base(context)
        {
        }

        public async Task<Aktualnosc?> GetAktualnoscById(int? id)
        {
            if (id == null)
            {
                return null;
            }

            return await _context.Aktualnosc
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.IdAktualnosci == id);
        }

        public async Task<IList<Aktualnosc>> GetAktualnosciByPosition(int number)
        {
            return await _context.Aktualnosc
                .AsNoTracking()
                .OrderByDescending(a => a.Pozycja)
                .Take(number)
                .ToListAsync();
        }

        public async Task<AktualnoscIndexDto> GetIndexDataAsync(string? searchTerm)
        {
            var query = _context.Aktualnosc.AsNoTracking();
            var term = (searchTerm ?? string.Empty).Trim();

            if (!string.IsNullOrWhiteSpace(term))
            {
                query = query.Where(x =>
                    x.Nazwa.Contains(term) ||
                    x.TytulLinku.Contains(term) ||
                    x.Tresc.Contains(term));
            }

            var items = await query
                .OrderBy(x => x.Pozycja)
                .ThenBy(x => x.Nazwa)
                .Select(x => new AktualnoscIndexItemDto
                {
                    IdAktualnosci = x.IdAktualnosci,
                    TytulLinku = x.TytulLinku,
                    Nazwa = x.Nazwa,
                    Tresc = x.Tresc,
                    Pozycja = x.Pozycja
                })
                .ToListAsync();

            return new AktualnoscIndexDto
            {
                SearchTerm = term,
                Items = items
            };
        }

        public async Task<AktualnoscDetailsDto?> GetDetailsDataAsync(int id)
        {
            return await _context.Aktualnosc
                .AsNoTracking()
                .Where(x => x.IdAktualnosci == id)
                .Select(x => new AktualnoscDetailsDto
                {
                    IdAktualnosci = x.IdAktualnosci,
                    TytulLinku = x.TytulLinku,
                    Nazwa = x.Nazwa,
                    Tresc = x.Tresc,
                    Pozycja = x.Pozycja
                })
                .FirstOrDefaultAsync();
        }

        public async Task<AktualnoscDeleteDto?> GetDeleteDataAsync(int id)
        {
            return await _context.Aktualnosc
                .AsNoTracking()
                .Where(x => x.IdAktualnosci == id)
                .Select(x => new AktualnoscDeleteDto
                {
                    IdAktualnosci = x.IdAktualnosci,
                    TytulLinku = x.TytulLinku,
                    Nazwa = x.Nazwa,
                    Tresc = x.Tresc,
                    Pozycja = x.Pozycja
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> TytulLinkuExistsAsync(string tytulLinku, int? excludeId = null)
        {
            var normalized = (tytulLinku ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return false;
            }

            var query = _context.Aktualnosc.AsNoTracking().Where(x => x.TytulLinku == normalized);
            if (excludeId.HasValue)
            {
                query = query.Where(x => x.IdAktualnosci != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
