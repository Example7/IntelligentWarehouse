using Data.Data;
using Interfaces.Magazyn;
using Interfaces.Magazyn.Dtos;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;
using System.Globalization;

namespace Services.Magazyn
{
    public class RezerwacjaService : BaseService, IRezerwacjaService
    {
        public RezerwacjaService(DataContext context) : base(context) { }

        public async Task<RezerwacjaIndexDto> GetIndexDataAsync(string? searchTerm)
        {
            var query = _context.Rezerwacja
                .AsNoTracking()
                .Include(r => r.Magazyn)
                .Include(r => r.Utworzyl)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(r =>
                    EF.Functions.Like(r.Numer, $"%{term}%") ||
                    EF.Functions.Like(r.Status, $"%{term}%") ||
                    (r.Notatka != null && EF.Functions.Like(r.Notatka, $"%{term}%")) ||
                    (r.Magazyn != null && EF.Functions.Like(r.Magazyn.Nazwa, $"%{term}%")) ||
                    (r.Utworzyl != null && EF.Functions.Like(r.Utworzyl.Email, $"%{term}%")));
            }

            return new RezerwacjaIndexDto
            {
                SearchTerm = searchTerm,
                Items = await query.ToListAsync()
            };
        }

        public async Task<RezerwacjaDetailsDto?> GetDetailsDataAsync(int id)
        {
            var dokument = await _context.Rezerwacja
                .AsNoTracking()
                .Include(r => r.Magazyn)
                .Include(r => r.Utworzyl)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (dokument == null)
            {
                return null;
            }

            var pozycje = await _context.PozycjaRezerwacji
                .AsNoTracking()
                .Include(p => p.Rezerwacja)
                .Include(p => p.Produkt).ThenInclude(p => p.DomyslnaJednostka)
                .Include(p => p.Lokacja).ThenInclude(l => l.Magazyn)
                .Where(p => p.IdRezerwacji == id)
                .OrderBy(p => p.Lp)
                .ThenBy(p => p.Id)
                .ToListAsync();

            return new RezerwacjaDetailsDto
            {
                Dokument = dokument,
                LiczbaPozycji = pozycje.Count,
                SumaIlosci = pozycje.Sum(p => p.Ilosc),
                Pozycje = pozycje
            };
        }

        public async Task<RezerwacjaDeleteDto?> GetDeleteDataAsync(int id)
        {
            var dokument = await _context.Rezerwacja
                .AsNoTracking()
                .Include(r => r.Magazyn)
                .Include(r => r.Utworzyl)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (dokument == null)
            {
                return null;
            }

            var liczbaPozycji = await _context.PozycjaRezerwacji.AsNoTracking().CountAsync(p => p.IdRezerwacji == id);
            return new RezerwacjaDeleteDto { Dokument = dokument, LiczbaPozycji = liczbaPozycji };
        }

        public async Task<RezerwacjaStatusChangeResultDto> ActivateAsync(int id)
        {
            var rezerwacja = await _context.Rezerwacja
                .FirstOrDefaultAsync(r => r.Id == id);
            if (rezerwacja == null)
            {
                return RezerwacjaStatusChangeResultDto.Fail("Nie znaleziono rezerwacji.");
            }

            if (!string.Equals(rezerwacja.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            {
                return RezerwacjaStatusChangeResultDto.Fail("Aktywować można tylko rezerwacje w statusie Draft.");
            }

            var pozycje = await _context.PozycjaRezerwacji
                .AsNoTracking()
                .Include(p => p.Produkt)
                .Include(p => p.Lokacja)
                .Where(p => p.IdRezerwacji == id)
                .ToListAsync();

            if (pozycje.Count == 0)
            {
                return RezerwacjaStatusChangeResultDto.Fail("Nie można aktywować rezerwacji bez pozycji.");
            }

            var errors = new List<(string Field, string Error)>();

            // Warehouse-level reservations (without location) compete with all active reservations in the same warehouse.
            var warehouseLevelGroups = pozycje
                .Where(p => p.IdLokacji == null)
                .GroupBy(p => p.IdProduktu)
                .Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Ilosc), Sample = g.First() })
                .ToList();

            foreach (var group in warehouseLevelGroups)
            {
                var stockInWarehouse = await _context.StanMagazynowy
                    .AsNoTracking()
                    .Where(s => s.IdProduktu == group.ProductId && s.Lokacja.IdMagazynu == rezerwacja.IdMagazynu)
                    .SumAsync(s => (decimal?)s.Ilosc) ?? 0m;

                var activeReserved = await _context.PozycjaRezerwacji
                    .AsNoTracking()
                    .Where(p =>
                        p.IdRezerwacji != id &&
                        p.IdProduktu == group.ProductId &&
                        p.Rezerwacja.IdMagazynu == rezerwacja.IdMagazynu &&
                        p.Rezerwacja.Status == "Active")
                    .SumAsync(p => (decimal?)p.Ilosc) ?? 0m;

                var available = stockInWarehouse - activeReserved;
                if (group.Qty > available)
                {
                    var code = group.Sample.Produkt?.Kod ?? $"ID:{group.ProductId}";
                    errors.Add((nameof(group.Sample.IdProduktu),
                        $"Brak dostępnego stanu dla produktu {code} w magazynie. Dostępne: {Fmt(available)}, rezerwujesz: {Fmt(group.Qty)}."));
                }
            }

            // Location-level reservations compete on a specific location.
            var locationLevelGroups = pozycje
                .Where(p => p.IdLokacji.HasValue)
                .GroupBy(p => new { p.IdProduktu, p.IdLokacji })
                .Select(g => new { g.Key.IdProduktu, g.Key.IdLokacji, Qty = g.Sum(x => x.Ilosc), Sample = g.First() })
                .ToList();

            foreach (var group in locationLevelGroups)
            {
                var locationId = group.IdLokacji!.Value;
                var stockAtLocation = await _context.StanMagazynowy
                    .AsNoTracking()
                    .Where(s => s.IdProduktu == group.IdProduktu && s.IdLokacji == locationId)
                    .SumAsync(s => (decimal?)s.Ilosc) ?? 0m;

                var activeReservedAtLocation = await _context.PozycjaRezerwacji
                    .AsNoTracking()
                    .Where(p =>
                        p.IdRezerwacji != id &&
                        p.IdProduktu == group.IdProduktu &&
                        p.IdLokacji == locationId &&
                        p.Rezerwacja.Status == "Active")
                    .SumAsync(p => (decimal?)p.Ilosc) ?? 0m;

                var available = stockAtLocation - activeReservedAtLocation;
                if (group.Qty > available)
                {
                    var code = group.Sample.Produkt?.Kod ?? $"ID:{group.IdProduktu}";
                    var loc = group.Sample.Lokacja?.Kod ?? $"ID:{locationId}";
                    errors.Add((nameof(group.Sample.IdLokacji),
                        $"Brak dostępnego stanu dla produktu {code} w lokacji {loc}. Dostępne: {Fmt(available)}, rezerwujesz: {Fmt(group.Qty)}."));
                }
            }

            if (errors.Count > 0)
            {
                return new RezerwacjaStatusChangeResultDto
                {
                    Success = false,
                    Message = "Nie można aktywować rezerwacji. Popraw pozycje lub zmniejsz ilości.",
                    Errors = errors
                };
            }

            using var tx = await _context.Database.BeginTransactionAsync();
            rezerwacja.Status = "Active";
            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return RezerwacjaStatusChangeResultDto.Ok("Rezerwacja została aktywowana.");
        }

        public async Task<RezerwacjaStatusChangeResultDto> ReleaseAsync(int id)
        {
            var rezerwacja = await _context.Rezerwacja.FirstOrDefaultAsync(r => r.Id == id);
            if (rezerwacja == null)
            {
                return RezerwacjaStatusChangeResultDto.Fail("Nie znaleziono rezerwacji.");
            }

            if (!string.Equals(rezerwacja.Status, "Active", StringComparison.OrdinalIgnoreCase))
            {
                return RezerwacjaStatusChangeResultDto.Fail("Zwolnić można tylko aktywną rezerwację.");
            }

            rezerwacja.Status = "Released";
            await _context.SaveChangesAsync();
            return RezerwacjaStatusChangeResultDto.Ok("Rezerwacja została zwolniona.");
        }

        public async Task<int> ReleaseExpiredAsync(DateTime utcNow, CancellationToken cancellationToken = default)
        {
            var expired = await _context.Rezerwacja
                .Where(r => r.Status == "Active" && r.WygasaUtc.HasValue && r.WygasaUtc.Value <= utcNow)
                .ToListAsync(cancellationToken);

            if (expired.Count == 0)
            {
                return 0;
            }

            foreach (var rezerwacja in expired)
            {
                rezerwacja.Status = "Released";
            }

            await _context.SaveChangesAsync(cancellationToken);
            return expired.Count;
        }
        private static string Fmt(decimal value)
            => value.ToString("0.###", CultureInfo.InvariantCulture);
    }
}

