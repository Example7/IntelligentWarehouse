using Data.Data;
using Interfaces.Magazyn;
using Interfaces.Magazyn.Dtos;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.Magazyn
{
    public class UstawienieAplikacjiService : BaseService, IUstawienieAplikacjiService
    {
        public UstawienieAplikacjiService(DataContext context) : base(context)
        {
        }

        public async Task<UstawienieAplikacjiIndexDto> GetIndexDataAsync(string? searchTerm)
        {
            var query = _context.UstawienieAplikacji.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(x =>
                    EF.Functions.Like(x.Klucz, $"%{term}%") ||
                    EF.Functions.Like(x.Wartosc, $"%{term}%") ||
                    (x.Opis != null && EF.Functions.Like(x.Opis, $"%{term}%")));
            }

            var items = await query.OrderBy(x => x.Klucz).ToListAsync();

            var userIds = items
                .Where(x => x.ZmienilUserId.HasValue)
                .Select(x => x.ZmienilUserId!.Value)
                .Distinct()
                .ToList();

            var userEmails = userIds.Count == 0
                ? new Dictionary<int, string>()
                : await _context.Uzytkownik
                    .AsNoTracking()
                    .Where(u => userIds.Contains(u.IdUzytkownika))
                    .ToDictionaryAsync(u => u.IdUzytkownika, u => u.Email);

            return new UstawienieAplikacjiIndexDto
            {
                SearchTerm = searchTerm,
                Items = items.Select(x => new UstawienieAplikacjiIndexItemDto
                {
                    Ustawienie = x,
                    ZmienilEmail = x.ZmienilUserId.HasValue && userEmails.TryGetValue(x.ZmienilUserId.Value, out var email)
                        ? email
                        : null
                }).ToList()
            };
        }

        public async Task<UstawienieAplikacjiDetailsDto?> GetDetailsDataAsync(string klucz)
        {
            var ustawienie = await _context.UstawienieAplikacji
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Klucz == klucz);
            if (ustawienie == null)
            {
                return null;
            }

            string? email = null;
            if (ustawienie.ZmienilUserId.HasValue)
            {
                email = await _context.Uzytkownik
                    .AsNoTracking()
                    .Where(u => u.IdUzytkownika == ustawienie.ZmienilUserId.Value)
                    .Select(u => u.Email)
                    .FirstOrDefaultAsync();
            }

            return new UstawienieAplikacjiDetailsDto
            {
                Ustawienie = ustawienie,
                ZmienilEmail = email
            };
        }

        public async Task<UstawienieAplikacjiDeleteDto?> GetDeleteDataAsync(string klucz)
        {
            var details = await GetDetailsDataAsync(klucz);
            if (details == null)
            {
                return null;
            }

            return new UstawienieAplikacjiDeleteDto
            {
                Ustawienie = details.Ustawienie,
                ZmienilEmail = details.ZmienilEmail
            };
        }
    }
}
