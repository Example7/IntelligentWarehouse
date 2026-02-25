using Data.Data;
using Interfaces.Magazyn;
using Interfaces.Magazyn.Dtos;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.Magazyn
{
    public class LogAudytuService : BaseService, ILogAudytuService
    {
        public LogAudytuService(DataContext context) : base(context)
        {
        }

        public async Task<LogAudytuIndexDto> GetIndexDataAsync(string? searchTerm)
        {
            var query = _context.LogAudytu
                .AsNoTracking()
                .Include(l => l.Uzytkownik)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(l =>
                    EF.Functions.Like(l.Akcja, $"%{term}%") ||
                    EF.Functions.Like(l.Encja, $"%{term}%") ||
                    (l.IdEncji != null && EF.Functions.Like(l.IdEncji, $"%{term}%")) ||
                    (l.Uzytkownik != null && EF.Functions.Like(l.Uzytkownik.Email, $"%{term}%")));
            }

            var logs = await query
                .OrderByDescending(l => l.KiedyUtc)
                .ThenByDescending(l => l.Id)
                .ToListAsync();

            return new LogAudytuIndexDto
            {
                SearchTerm = searchTerm,
                Items = logs.Select(l => new LogAudytuIndexItemDto { Log = l }).ToList()
            };
        }

        public async Task<LogAudytuDetailsDto?> GetDetailsDataAsync(long idLogu)
        {
            var log = await _context.LogAudytu
                .AsNoTracking()
                .Include(l => l.Uzytkownik)
                .FirstOrDefaultAsync(l => l.Id == idLogu);

            if (log == null)
            {
                return null;
            }

            return new LogAudytuDetailsDto { Log = log };
        }

        public async Task<LogAudytuDeleteDto?> GetDeleteDataAsync(long idLogu)
        {
            var details = await GetDetailsDataAsync(idLogu);
            if (details == null)
            {
                return null;
            }

            return new LogAudytuDeleteDto { Log = details.Log };
        }
    }
}
