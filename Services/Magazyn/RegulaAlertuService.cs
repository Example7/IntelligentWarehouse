using Data.Data;
using Interfaces.Magazyn;
using Interfaces.Magazyn.Dtos;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.Magazyn
{
    public class RegulaAlertuService : BaseService, IRegulaAlertuService
    {
        public RegulaAlertuService(DataContext context) : base(context)
        {
        }

        public async Task<RegulaAlertuIndexDto> GetIndexDataAsync(string? searchTerm)
        {
            var query = _context.RegulaAlertu
                .AsNoTracking()
                .Include(r => r.Magazyn)
                .Include(r => r.Produkt)
                    .ThenInclude(p => p!.DomyslnaJednostka)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(r =>
                    EF.Functions.Like(r.Typ, $"%{term}%") ||
                    (r.Magazyn != null && EF.Functions.Like(r.Magazyn.Nazwa, $"%{term}%")) ||
                    (r.Produkt != null && EF.Functions.Like(r.Produkt.Kod, $"%{term}%")) ||
                    (r.Produkt != null && EF.Functions.Like(r.Produkt.Nazwa, $"%{term}%")));
            }

            var reguly = await query.ToListAsync();
            var ids = reguly.Select(r => r.Id).ToList();

            var alertStats = await _context.Alert
                .AsNoTracking()
                .Where(a => ids.Contains(a.IdReguly))
                .GroupBy(a => a.IdReguly)
                .Select(g => new
                {
                    IdReguly = g.Key,
                    Count = g.Count(),
                    Unacked = g.Count(a => !a.CzyPotwierdzony)
                })
                .ToDictionaryAsync(x => x.IdReguly, x => new { x.Count, x.Unacked });

            return new RegulaAlertuIndexDto
            {
                SearchTerm = searchTerm,
                Items = reguly.Select(r => new RegulaAlertuIndexItemDto
                {
                    Regula = r,
                    LiczbaAlertow = alertStats.TryGetValue(r.Id, out var s) ? s.Count : 0,
                    LiczbaAlertowNiepotwierdzonych = alertStats.TryGetValue(r.Id, out var s2) ? s2.Unacked : 0
                }).ToList()
            };
        }

        public async Task<RegulaAlertuDetailsDto?> GetDetailsDataAsync(int idReguly)
        {
            var regula = await _context.RegulaAlertu
                .AsNoTracking()
                .Include(r => r.Magazyn)
                .Include(r => r.Produkt)
                    .ThenInclude(p => p!.DomyslnaJednostka)
                .FirstOrDefaultAsync(r => r.Id == idReguly);

            if (regula == null)
            {
                return null;
            }

            var alertsQuery = _context.Alert
                .AsNoTracking()
                .Include(a => a.Produkt)
                .Include(a => a.Magazyn)
                .Where(a => a.IdReguly == idReguly);

            return new RegulaAlertuDetailsDto
            {
                Regula = regula,
                LiczbaAlertow = await alertsQuery.CountAsync(),
                LiczbaAlertowNiepotwierdzonych = await alertsQuery.CountAsync(a => !a.CzyPotwierdzony),
                OstatnieAlerty = await alertsQuery
                    .OrderByDescending(a => a.UtworzonoUtc)
                    .ThenByDescending(a => a.Id)
                    .Take(10)
                    .ToListAsync()
            };
        }

        public async Task<RegulaAlertuDeleteDto?> GetDeleteDataAsync(int idReguly)
        {
            var regula = await _context.RegulaAlertu
                .AsNoTracking()
                .Include(r => r.Magazyn)
                .Include(r => r.Produkt)
                .FirstOrDefaultAsync(r => r.Id == idReguly);
            if (regula == null)
            {
                return null;
            }

            var liczbaAlertow = await _context.Alert
                .AsNoTracking()
                .CountAsync(a => a.IdReguly == idReguly);

            return new RegulaAlertuDeleteDto
            {
                Regula = regula,
                LiczbaAlertow = liczbaAlertow
            };
        }
    }
}
