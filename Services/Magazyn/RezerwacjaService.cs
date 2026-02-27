using Data.Data;
using Interfaces.Magazyn;
using Interfaces.Magazyn.Dtos;
using Data.Data.Magazyn;
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
                .Include(p => p.Lokacja!).ThenInclude(l => l.Magazyn)
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
                .Include(p => p.Produkt)
                .Include(p => p.Lokacja)
                .Where(p => p.IdRezerwacji == id)
                .OrderBy(p => p.Lp)
                .ThenBy(p => p.Id)
                .ToListAsync();

            if (pozycje.Count == 0)
            {
                return RezerwacjaStatusChangeResultDto.Fail("Nie można aktywować rezerwacji bez pozycji.");
            }

            var errors = new List<(string Field, string Error)>();
            var productIds = pozycje.Select(p => p.IdProduktu).Distinct().ToList();

            var stockRows = await _context.StanMagazynowy
                .AsNoTracking()
                .Where(s =>
                    productIds.Contains(s.IdProduktu) &&
                    s.Lokacja.IdMagazynu == rezerwacja.IdMagazynu &&
                    s.Lokacja.CzyAktywna)
                .Select(s => new
                {
                    s.IdProduktu,
                    s.IdLokacji,
                    s.Ilosc,
                    LocationCode = s.Lokacja.Kod
                })
                .ToListAsync();

            var activeReservedRows = await _context.PozycjaRezerwacji
                .AsNoTracking()
                .Where(p =>
                    p.IdRezerwacji != id &&
                    productIds.Contains(p.IdProduktu) &&
                    p.Rezerwacja.IdMagazynu == rezerwacja.IdMagazynu &&
                    p.Rezerwacja.Status == "Active")
                .GroupBy(p => new { p.IdProduktu, p.IdLokacji })
                .Select(g => new
                {
                    g.Key.IdProduktu,
                    g.Key.IdLokacji,
                    Qty = g.Sum(x => x.Ilosc)
                })
                .ToListAsync();

            var draftWzRows = await _context.PozycjaWZ
                .AsNoTracking()
                .Where(p =>
                    p.IdLokacji.HasValue &&
                    productIds.Contains(p.IdProduktu) &&
                    p.Dokument.IdMagazynu == rezerwacja.IdMagazynu &&
                    p.Dokument.Status == "Draft")
                .GroupBy(p => new { p.IdProduktu, p.IdLokacji })
                .Select(g => new
                {
                    g.Key.IdProduktu,
                    g.Key.IdLokacji,
                    Qty = g.Sum(x => x.Ilosc)
                })
                .ToListAsync();

            var warehouseStockByProduct = stockRows
                .GroupBy(x => x.IdProduktu)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Ilosc));

            var activeReservedWarehouseByProduct = activeReservedRows
                .GroupBy(x => x.IdProduktu)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Qty));

            foreach (var draft in draftWzRows)
            {
                activeReservedWarehouseByProduct[draft.IdProduktu] =
                    activeReservedWarehouseByProduct.GetValueOrDefault(draft.IdProduktu, 0m) + draft.Qty;
            }

            var activeReservedAtLocation = activeReservedRows
                .Where(x => x.IdLokacji.HasValue)
                .ToDictionary(x => (x.IdProduktu, x.IdLokacji!.Value), x => x.Qty);

            foreach (var draft in draftWzRows.Where(x => x.IdLokacji.HasValue))
            {
                var key = (draft.IdProduktu, draft.IdLokacji!.Value);
                activeReservedAtLocation[key] = activeReservedAtLocation.GetValueOrDefault(key, 0m) + draft.Qty;
            }

            // Warehouse-level reservations (without location) compete with all active reservations in the same warehouse.
            var warehouseLevelGroups = pozycje
                .Where(p => p.IdLokacji == null)
                .GroupBy(p => p.IdProduktu)
                .Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Ilosc), Sample = g.First() })
                .ToList();

            foreach (var group in warehouseLevelGroups)
            {
                var stockInWarehouse = warehouseStockByProduct.GetValueOrDefault(group.ProductId, 0m);
                var activeReserved = activeReservedWarehouseByProduct.GetValueOrDefault(group.ProductId, 0m);
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
                var stockAtLocation = stockRows
                    .Where(s => s.IdProduktu == group.IdProduktu && s.IdLokacji == locationId)
                    .Sum(s => s.Ilosc);

                var activeReservedQty = activeReservedAtLocation.GetValueOrDefault((group.IdProduktu, locationId), 0m);
                var available = stockAtLocation - activeReservedQty;
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

            // During activation reserve concrete locations for warehouse-level positions.
            // This makes active reservations visible in location-aware flows and later WZ generation deterministic.
            var locationAvailabilityByProduct = stockRows
                .GroupBy(s => s.IdProduktu)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(s => new ReservationLocationAllocation
                    {
                        LocationId = s.IdLokacji,
                        LocationCode = s.LocationCode,
                        Available = s.Ilosc - activeReservedAtLocation.GetValueOrDefault((s.IdProduktu, s.IdLokacji), 0m)
                    })
                    .Where(x => x.Available > 0m)
                    .OrderByDescending(x => x.Available)
                    .ThenBy(x => x.LocationCode, StringComparer.OrdinalIgnoreCase)
                    .ToList());

            // First, consume capacity for positions that already have a fixed location.
            foreach (var fixedGroup in pozycje
                         .Where(p => p.IdLokacji.HasValue)
                         .GroupBy(p => new { p.IdProduktu, LocationId = p.IdLokacji!.Value }))
            {
                if (!locationAvailabilityByProduct.TryGetValue(fixedGroup.Key.IdProduktu, out var buckets))
                {
                    continue;
                }

                var bucket = buckets.FirstOrDefault(b => b.LocationId == fixedGroup.Key.LocationId);
                if (bucket == null)
                {
                    continue;
                }

                bucket.Available -= fixedGroup.Sum(x => x.Ilosc);
            }

            var finalLines = new List<PozycjaRezerwacji>(pozycje.Count);
            var splitRowsToAdd = new List<PozycjaRezerwacji>();

            foreach (var pozycja in pozycje.OrderBy(p => p.Lp).ThenBy(p => p.Id))
            {
                if (pozycja.IdLokacji.HasValue)
                {
                    finalLines.Add(pozycja);
                    continue;
                }

                if (!locationAvailabilityByProduct.TryGetValue(pozycja.IdProduktu, out var buckets) || buckets.Count == 0)
                {
                    var code = pozycja.Produkt?.Kod ?? $"ID:{pozycja.IdProduktu}";
                    return RezerwacjaStatusChangeResultDto.Fail($"Nie można przypisać lokacji dla produktu {code} podczas aktywacji rezerwacji.");
                }

                var required = pozycja.Ilosc;
                var allocations = new List<(int LocationId, decimal Qty)>();

                foreach (var bucket in buckets.Where(b => b.Available > 0m))
                {
                    if (required <= 0m)
                    {
                        break;
                    }

                    var take = Math.Min(required, bucket.Available);
                    if (take <= 0m)
                    {
                        continue;
                    }

                    allocations.Add((bucket.LocationId, take));
                    bucket.Available -= take;
                    required -= take;
                }

                if (required > 0m)
                {
                    var code = pozycja.Produkt?.Kod ?? $"ID:{pozycja.IdProduktu}";
                    return RezerwacjaStatusChangeResultDto.Fail(
                        $"Nie można przypisać lokacji dla produktu {code}. Brakuje {Fmt(required)} do pełnej alokacji podczas aktywacji.");
                }

                // Replace warehouse-level line with one or more location-specific lines.
                pozycja.IdLokacji = allocations[0].LocationId;
                pozycja.Ilosc = allocations[0].Qty;
                finalLines.Add(pozycja);

                for (var i = 1; i < allocations.Count; i++)
                {
                    var split = new PozycjaRezerwacji
                    {
                        IdRezerwacji = pozycja.IdRezerwacji,
                        IdProduktu = pozycja.IdProduktu,
                        IdLokacji = allocations[i].LocationId,
                        Ilosc = allocations[i].Qty
                    };
                    splitRowsToAdd.Add(split);
                    finalLines.Add(split);
                }
            }

            if (splitRowsToAdd.Count > 0)
            {
                _context.PozycjaRezerwacji.AddRange(splitRowsToAdd);
            }

            for (var i = 0; i < finalLines.Count; i++)
            {
                finalLines[i].Lp = i + 1;
            }

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

        public async Task<RezerwacjaCreateShortagePzResultDto> CreateShortagePzDraftAsync(int reservationId, int supplierId, int receiptLocationId, int currentUserId)
        {
            var reservation = await _context.Rezerwacja
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null)
            {
                return RezerwacjaCreateShortagePzResultDto.Fail("Nie znaleziono rezerwacji.");
            }

            if (!string.Equals(reservation.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            {
                return RezerwacjaCreateShortagePzResultDto.Fail("PZ z braków można utworzyć tylko dla rezerwacji w statusie Draft.");
            }

            var supplierExists = await _context.Dostawca
                .AsNoTracking()
                .AnyAsync(d => d.IdDostawcy == supplierId && d.CzyAktywny);
            if (!supplierExists)
            {
                return RezerwacjaCreateShortagePzResultDto.Fail("Wybrany dostawca jest nieprawidłowy lub nieaktywny.");
            }

            var location = await _context.Lokacja
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.IdLokacji == receiptLocationId && l.CzyAktywna);
            if (location == null || location.IdMagazynu != reservation.IdMagazynu)
            {
                return RezerwacjaCreateShortagePzResultDto.Fail("Wybrana lokacja przyjęcia nie należy do magazynu tej rezerwacji.");
            }

            var positions = await _context.PozycjaRezerwacji
                .AsNoTracking()
                .Where(p => p.IdRezerwacji == reservationId)
                .OrderBy(p => p.Lp)
                .ThenBy(p => p.Id)
                .ToListAsync();

            if (positions.Count == 0)
            {
                return RezerwacjaCreateShortagePzResultDto.Fail("Rezerwacja nie ma pozycji.");
            }

            var shortages = await CalculateShortagesByProductAsync(reservation, positions);
            if (shortages.Count == 0)
            {
                return RezerwacjaCreateShortagePzResultDto.Fail("Brakujące ilości nie zostały wykryte. Rezerwację można aktywować bez dodatkowego zamówienia.");
            }

            var productIds = shortages.Keys.ToList();
            var products = await _context.Produkt
                .AsNoTracking()
                .Include(p => p.DomyslnaJednostka)
                .Where(p => productIds.Contains(p.IdProduktu))
                .ToDictionaryAsync(p => p.IdProduktu);

            var rows = shortages
                .Where(x => x.Value > 0m)
                .Select(x =>
                {
                    products.TryGetValue(x.Key, out var product);
                    return new PzShortageRow
                    {
                        ProductId = x.Key,
                        Qty = x.Value,
                        ProductCode = product?.Kod ?? $"ID:{x.Key}",
                        Unit = product?.DomyslnaJednostka?.Kod ?? "j.m."
                    };
                })
                .OrderBy(x => x.ProductCode, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (rows.Count == 0)
            {
                return RezerwacjaCreateShortagePzResultDto.Fail("Brakujące ilości nie zostały wykryte.");
            }

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var now = DateTime.UtcNow;
                var pz = new DokumentPZ
                {
                    Numer = await GenerateNextPzNumberAsync(now.Year),
                    IdMagazynu = reservation.IdMagazynu,
                    IdDostawcy = supplierId,
                    Status = "Draft",
                    DataPrzyjeciaUtc = now,
                    IdUtworzyl = currentUserId,
                    ZaksiegowanoUtc = null,
                    Notatka = BuildPzShortageNote(reservation.Numer, rows)
                };

                var lp = 1;
                foreach (var row in rows)
                {
                    pz.Pozycje.Add(new PozycjaPZ
                    {
                        Lp = lp++,
                        IdProduktu = row.ProductId,
                        IdLokacji = receiptLocationId,
                        IdPartii = null,
                        Ilosc = row.Qty,
                        CenaJednostkowa = null
                    });
                }

                _context.DokumentPZ.Add(pz);
                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return RezerwacjaCreateShortagePzResultDto.Ok(
                    pz.Id,
                    pz.Numer,
                    $"Utworzono PZ Draft {pz.Numer} z brakujących ilości rezerwacji {reservation.Numer}.");
            }
            catch (DbUpdateException ex)
            {
                await tx.RollbackAsync();
                return RezerwacjaCreateShortagePzResultDto.Fail($"Nie udało się utworzyć PZ Draft: {ex.GetBaseException().Message}");
            }
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

        public async Task<RezerwacjaCreateClientResultDto> CreateClientDraftAsync(RezerwacjaCreateClientCommandDto command, CancellationToken cancellationToken = default)
        {
            if (command.Items.Count == 0)
            {
                return RezerwacjaCreateClientResultDto.Fail("Rezerwacja musi zawierać co najmniej jedną pozycję.");
            }

            var warehouse = await _context.Magazyn
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.IdMagazynu == command.WarehouseId && w.CzyAktywny, cancellationToken);
            if (warehouse == null)
            {
                return RezerwacjaCreateClientResultDto.Fail("Nieprawidłowy magazyn.");
            }

            var productIds = command.Items.Select(i => i.ProductId).Distinct().ToList();
            var productsCount = await _context.Produkt
                .AsNoTracking()
                .CountAsync(p => productIds.Contains(p.IdProduktu) && p.CzyAktywny, cancellationToken);
            if (productsCount != productIds.Count)
            {
                return RezerwacjaCreateClientResultDto.Fail("Jedna lub więcej pozycji wskazuje nieprawidłowy produkt.");
            }

            var locationIds = command.Items
                .Where(i => i.LocationId.HasValue)
                .Select(i => i.LocationId!.Value)
                .Distinct()
                .ToList();

            if (locationIds.Count > 0)
            {
                var validLocationsCount = await _context.Lokacja
                    .AsNoTracking()
                    .CountAsync(l => locationIds.Contains(l.IdLokacji) && l.CzyAktywna && l.IdMagazynu == command.WarehouseId, cancellationToken);

                if (validLocationsCount != locationIds.Count)
                {
                    return RezerwacjaCreateClientResultDto.Fail("Jedna lub więcej lokacji jest nieprawidłowa lub nie należy do wybranego magazynu.");
                }
            }

            if (command.Items.Any(i => i.Quantity <= 0m))
            {
                return RezerwacjaCreateClientResultDto.Fail("Ilość musi być większa od zera.");
            }

            var utcNowValue = DateTime.UtcNow;
            var reservation = new Rezerwacja
            {
                Numer = await GenerateNextReservationNumberAsync(utcNowValue.Year, cancellationToken),
                IdMagazynu = command.WarehouseId,
                Status = "Draft",
                UtworzonoUtc = utcNowValue,
                WygasaUtc = NormalizeInputUtc(command.ExpiresAtUtc),
                IdUtworzyl = command.UserId,
                Notatka = string.IsNullOrWhiteSpace(command.Note) ? null : command.Note.Trim()
            };

            for (var i = 0; i < command.Items.Count; i++)
            {
                var item = command.Items[i];
                reservation.Pozycje.Add(new PozycjaRezerwacji
                {
                    Lp = i + 1,
                    IdProduktu = item.ProductId,
                    IdLokacji = item.LocationId,
                    Ilosc = decimal.Round(item.Quantity, 3, MidpointRounding.AwayFromZero)
                });
            }

            _context.Rezerwacja.Add(reservation);
            await _context.SaveChangesAsync(cancellationToken);

            return RezerwacjaCreateClientResultDto.Ok(reservation.Id, reservation.Numer, reservation.Status, reservation.UtworzonoUtc);
        }

        private async Task<string> GenerateNextReservationNumberAsync(int year, CancellationToken cancellationToken)
        {
            var prefix = $"REZ/{year}/";
            var lastNumber = await _context.Rezerwacja
                .AsNoTracking()
                .Where(r => r.Numer.StartsWith(prefix))
                .OrderByDescending(r => r.Id)
                .Select(r => r.Numer)
                .FirstOrDefaultAsync(cancellationToken);

            var next = 1;
            if (!string.IsNullOrWhiteSpace(lastNumber))
            {
                var suffix = lastNumber[prefix.Length..];
                if (int.TryParse(suffix, out var parsed))
                {
                    next = parsed + 1;
                }
            }

            return $"{prefix}{next:0000}";
        }

        private async Task<string> GenerateNextPzNumberAsync(int year)
        {
            var prefix = $"PZ/{year}/";
            var lastNumber = await _context.DokumentPZ
                .AsNoTracking()
                .Where(d => d.Numer.StartsWith(prefix))
                .OrderByDescending(d => d.Id)
                .Select(d => d.Numer)
                .FirstOrDefaultAsync();

            var next = 1;
            if (!string.IsNullOrWhiteSpace(lastNumber))
            {
                var suffix = lastNumber[prefix.Length..];
                if (int.TryParse(suffix, out var parsed))
                {
                    next = parsed + 1;
                }
            }

            return $"{prefix}{next:0000}";
        }

        private static DateTime? NormalizeInputUtc(DateTime? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            var dt = value.Value;
            if (dt.Kind == DateTimeKind.Utc)
            {
                return dt;
            }

            if (dt.Kind == DateTimeKind.Unspecified)
            {
                dt = DateTime.SpecifyKind(dt, DateTimeKind.Local);
            }

            return dt.ToUniversalTime();
        }

        private static string Fmt(decimal value)
            => value.ToString("0.###", CultureInfo.InvariantCulture);

        private static string BuildPzShortageNote(string reservationNumber, IEnumerable<PzShortageRow> rows)
        {
            var items = rows
                .Select(r => $"{r.ProductCode}: {r.Qty.ToString("0.###", CultureInfo.InvariantCulture)} {r.Unit}")
                .ToList();

            var joined = string.Join("; ", items);
            var source = $"Utworzono z braków rezerwacji {reservationNumber}.";
            return $"{source} Pozycje: {joined}";
        }

        private async Task<Dictionary<int, decimal>> CalculateShortagesByProductAsync(Rezerwacja reservation, List<PozycjaRezerwacji> pozycje)
        {
            var productIds = pozycje.Select(p => p.IdProduktu).Distinct().ToList();
            if (productIds.Count == 0)
            {
                return new Dictionary<int, decimal>();
            }

            var stockRows = await _context.StanMagazynowy
                .AsNoTracking()
                .Where(s =>
                    productIds.Contains(s.IdProduktu) &&
                    s.Lokacja.IdMagazynu == reservation.IdMagazynu &&
                    s.Lokacja.CzyAktywna)
                .Select(s => new
                {
                    s.IdProduktu,
                    s.IdLokacji,
                    s.Ilosc
                })
                .ToListAsync();

            var activeReservedRows = await _context.PozycjaRezerwacji
                .AsNoTracking()
                .Where(p =>
                    p.IdRezerwacji != reservation.Id &&
                    productIds.Contains(p.IdProduktu) &&
                    p.Rezerwacja.IdMagazynu == reservation.IdMagazynu &&
                    p.Rezerwacja.Status == "Active")
                .GroupBy(p => new { p.IdProduktu, p.IdLokacji })
                .Select(g => new
                {
                    g.Key.IdProduktu,
                    g.Key.IdLokacji,
                    Qty = g.Sum(x => x.Ilosc)
                })
                .ToListAsync();

            var draftWzRows = await _context.PozycjaWZ
                .AsNoTracking()
                .Where(p =>
                    p.IdLokacji.HasValue &&
                    productIds.Contains(p.IdProduktu) &&
                    p.Dokument.IdMagazynu == reservation.IdMagazynu &&
                    p.Dokument.Status == "Draft")
                .GroupBy(p => new { p.IdProduktu, p.IdLokacji })
                .Select(g => new
                {
                    g.Key.IdProduktu,
                    g.Key.IdLokacji,
                    Qty = g.Sum(x => x.Ilosc)
                })
                .ToListAsync();

            var warehouseStockByProduct = stockRows
                .GroupBy(x => x.IdProduktu)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Ilosc));

            var activeReservedWarehouseByProduct = activeReservedRows
                .GroupBy(x => x.IdProduktu)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Qty));

            foreach (var draft in draftWzRows)
            {
                activeReservedWarehouseByProduct[draft.IdProduktu] =
                    activeReservedWarehouseByProduct.GetValueOrDefault(draft.IdProduktu, 0m) + draft.Qty;
            }

            var activeReservedAtLocation = activeReservedRows
                .Where(x => x.IdLokacji.HasValue)
                .ToDictionary(x => (x.IdProduktu, x.IdLokacji!.Value), x => x.Qty);

            foreach (var draft in draftWzRows.Where(x => x.IdLokacji.HasValue))
            {
                var key = (draft.IdProduktu, draft.IdLokacji!.Value);
                activeReservedAtLocation[key] = activeReservedAtLocation.GetValueOrDefault(key, 0m) + draft.Qty;
            }

            var shortagesByProduct = new Dictionary<int, decimal>();

            var warehouseLevelGroups = pozycje
                .Where(p => p.IdLokacji == null)
                .GroupBy(p => p.IdProduktu)
                .Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Ilosc) })
                .ToList();

            foreach (var group in warehouseLevelGroups)
            {
                var stockInWarehouse = warehouseStockByProduct.GetValueOrDefault(group.ProductId, 0m);
                var activeReserved = activeReservedWarehouseByProduct.GetValueOrDefault(group.ProductId, 0m);
                var available = stockInWarehouse - activeReserved;
                var missing = group.Qty - available;
                if (missing > 0m)
                {
                    shortagesByProduct[group.ProductId] = shortagesByProduct.GetValueOrDefault(group.ProductId, 0m) + missing;
                }
            }

            var locationLevelGroups = pozycje
                .Where(p => p.IdLokacji.HasValue)
                .GroupBy(p => new { p.IdProduktu, p.IdLokacji })
                .Select(g => new { g.Key.IdProduktu, g.Key.IdLokacji, Qty = g.Sum(x => x.Ilosc) })
                .ToList();

            foreach (var group in locationLevelGroups)
            {
                var locationId = group.IdLokacji!.Value;
                var stockAtLocation = stockRows
                    .Where(s => s.IdProduktu == group.IdProduktu && s.IdLokacji == locationId)
                    .Sum(s => s.Ilosc);

                var activeReservedQty = activeReservedAtLocation.GetValueOrDefault((group.IdProduktu, locationId), 0m);
                var available = stockAtLocation - activeReservedQty;
                var missing = group.Qty - available;
                if (missing > 0m)
                {
                    shortagesByProduct[group.IdProduktu] = shortagesByProduct.GetValueOrDefault(group.IdProduktu, 0m) + missing;
                }
            }

            return shortagesByProduct
                .Where(x => x.Value > 0m)
                .ToDictionary(x => x.Key, x => x.Value);
        }

        private sealed class ReservationLocationAllocation
        {
            public int LocationId { get; set; }
            public string LocationCode { get; set; } = string.Empty;
            public decimal Available { get; set; }
        }

        private sealed class PzShortageRow
        {
            public int ProductId { get; set; }
            public decimal Qty { get; set; }
            public string ProductCode { get; set; } = string.Empty;
            public string Unit { get; set; } = "j.m.";
        }
    }
}