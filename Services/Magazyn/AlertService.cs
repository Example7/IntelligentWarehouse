using Data.Data;
using Interfaces.Magazyn;
using Interfaces.Magazyn.Dtos;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.Magazyn
{
    public class AlertService : BaseService, IAlertService
    {
        public AlertService(DataContext context) : base(context)
        {
        }

        public async Task<AlertIndexDto> GetIndexDataAsync(string? searchTerm)
        {
            var query = _context.Alert
                .AsNoTracking()
                .Include(a => a.Magazyn)
                .Include(a => a.Produkt)
                    .ThenInclude(p => p.DomyslnaJednostka)
                .Include(a => a.Regula)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(a =>
                    EF.Functions.Like(a.Waga, $"%{term}%") ||
                    EF.Functions.Like(a.Tresc, $"%{term}%") ||
                    (a.Magazyn != null && EF.Functions.Like(a.Magazyn.Nazwa, $"%{term}%")) ||
                    (a.Produkt != null && (EF.Functions.Like(a.Produkt.Kod, $"%{term}%") || EF.Functions.Like(a.Produkt.Nazwa, $"%{term}%"))) ||
                    (a.Regula != null && EF.Functions.Like(a.Regula.Typ, $"%{term}%")));
            }

            var alerts = await query
                .OrderByDescending(a => a.UtworzonoUtc)
                .ThenByDescending(a => a.Id)
                .ToListAsync();

            return new AlertIndexDto
            {
                SearchTerm = searchTerm,
                Items = alerts.Select(a => new AlertIndexItemDto { Alert = a }).ToList()
            };
        }

        public async Task<AlertDetailsDto?> GetDetailsDataAsync(long idAlertu)
        {
            var alert = await _context.Alert
                .AsNoTracking()
                .Include(a => a.Magazyn)
                .Include(a => a.Produkt)
                    .ThenInclude(p => p.DomyslnaJednostka)
                .Include(a => a.Regula)
                .FirstOrDefaultAsync(a => a.Id == idAlertu);

            if (alert == null)
            {
                return null;
            }

            string? potwierdzilEmail = null;
            if (alert.PotwierdzilUserId.HasValue)
            {
                potwierdzilEmail = await _context.Uzytkownik
                    .AsNoTracking()
                    .Where(u => u.IdUzytkownika == alert.PotwierdzilUserId.Value)
                    .Select(u => u.Email)
                    .FirstOrDefaultAsync();
            }

            return new AlertDetailsDto
            {
                Alert = alert,
                PotwierdzilEmail = potwierdzilEmail
            };
        }

        public async Task<AlertDeleteDto?> GetDeleteDataAsync(long idAlertu)
        {
            var details = await GetDetailsDataAsync(idAlertu);
            if (details == null)
            {
                return null;
            }

            return new AlertDeleteDto
            {
                Alert = details.Alert,
                PotwierdzilEmail = details.PotwierdzilEmail
            };
        }
    }
}
