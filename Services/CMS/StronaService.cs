using Data.Data;
using Data.Data.CMS;
using Interfaces.CMS;
using Interfaces.CMS.Dtos;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.CMS
{
    public class StronaService : BaseService, IStronaService
    {
        public StronaService(DataContext context) : base(context)
        {
        }

        public async Task<Strona?> GetStronaById(int? id)
        {
            if (id == null)
            {
                return null;
            }

            return await _context.Strona.FindAsync(id);
        }

        public async Task<IList<Strona>> GetStronyByPosition()
        {
            return await _context.Strona
                .AsNoTracking()
                .OrderBy(s => s.Pozycja)
                .ThenBy(s => s.Nazwa)
                .ToListAsync();
        }

        public async Task<StronaIndexDto> GetIndexDataAsync(string? searchTerm)
        {
            var query = _context.Strona.AsNoTracking();
            var term = (searchTerm ?? string.Empty).Trim();

            if (!string.IsNullOrWhiteSpace(term))
            {
                query = query.Where(x =>
                    x.TytulLinku.Contains(term) ||
                    x.Nazwa.Contains(term) ||
                    x.Tresc.Contains(term));
            }

            var items = await query
                .OrderBy(x => x.Pozycja)
                .ThenBy(x => x.Nazwa)
                .Select(x => new StronaIndexItemDto
                {
                    IdStrony = x.IdStrony,
                    TytulLinku = x.TytulLinku,
                    Nazwa = x.Nazwa,
                    Tresc = x.Tresc,
                    Pozycja = x.Pozycja
                })
                .ToListAsync();

            return new StronaIndexDto
            {
                SearchTerm = term,
                Items = items
            };
        }

        public async Task<StronaDetailsDto?> GetDetailsDataAsync(int id)
        {
            return await _context.Strona
                .AsNoTracking()
                .Where(x => x.IdStrony == id)
                .Select(x => new StronaDetailsDto
                {
                    IdStrony = x.IdStrony,
                    TytulLinku = x.TytulLinku,
                    Nazwa = x.Nazwa,
                    Tresc = x.Tresc,
                    Pozycja = x.Pozycja
                })
                .FirstOrDefaultAsync();
        }

        public async Task<StronaDeleteDto?> GetDeleteDataAsync(int id)
        {
            return await _context.Strona
                .AsNoTracking()
                .Where(x => x.IdStrony == id)
                .Select(x => new StronaDeleteDto
                {
                    IdStrony = x.IdStrony,
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

            var query = _context.Strona.AsNoTracking().Where(x => x.TytulLinku == normalized);
            if (excludeId.HasValue)
            {
                query = query.Where(x => x.IdStrony != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
