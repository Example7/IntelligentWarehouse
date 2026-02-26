using Data.Data;
using Data.Data.Magazyn;
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

        public async Task<AlertGenerateFromRulesResultDto> GenerujAlertyZRegulAsync()
        {
            var result = new AlertGenerateFromRulesResultDto();

            var reguly = await _context.RegulaAlertu
                .AsNoTracking()
                .Where(r => r.CzyWlaczona)
                .OrderBy(r => r.IdMagazynu)
                .ThenBy(r => r.IdProduktu)
                .ThenBy(r => r.Typ)
                .ToListAsync();

            result.LiczbaPrzetworzonychRegul = reguly.Count;
            if (reguly.Count == 0)
            {
                return result;
            }

            var warehouseIds = reguly.Select(r => r.IdMagazynu).Distinct().ToList();
            var productIdsFromRules = reguly.Where(r => r.IdProduktu.HasValue).Select(r => r.IdProduktu!.Value).Distinct().ToList();

            var productsByWarehouse = await _context.StanMagazynowy
                .AsNoTracking()
                .Where(s => warehouseIds.Contains(s.Lokacja.IdMagazynu))
                .GroupBy(s => new { s.Lokacja.IdMagazynu, s.IdProduktu })
                .Select(g => new
                {
                    WarehouseId = g.Key.IdMagazynu,
                    ProductId = g.Key.IdProduktu,
                    PhysicalQty = g.Sum(x => x.Ilosc)
                })
                .ToListAsync();

            var activeReserved = await _context.PozycjaRezerwacji
                .AsNoTracking()
                .Where(p =>
                    p.Rezerwacja.Status == "Active" &&
                    warehouseIds.Contains(p.Rezerwacja.IdMagazynu))
                .GroupBy(p => new { p.Rezerwacja.IdMagazynu, p.IdProduktu })
                .Select(g => new
                {
                    WarehouseId = g.Key.IdMagazynu,
                    ProductId = g.Key.IdProduktu,
                    Qty = g.Sum(x => x.Ilosc)
                })
                .ToListAsync();

            var draftWzReserved = await _context.PozycjaWZ
                .AsNoTracking()
                .Where(p =>
                    p.Dokument.Status == "Draft" &&
                    warehouseIds.Contains(p.Dokument.IdMagazynu))
                .GroupBy(p => new { p.Dokument.IdMagazynu, p.IdProduktu })
                .Select(g => new
                {
                    WarehouseId = g.Key.IdMagazynu,
                    ProductId = g.Key.IdProduktu,
                    Qty = g.Sum(x => x.Ilosc)
                })
                .ToListAsync();

            var productMetadata = await _context.Produkt
                .AsNoTracking()
                .Include(p => p.DomyslnaJednostka)
                .Where(p => p.CzyAktywny)
                .Select(p => new
                {
                    p.IdProduktu,
                    p.Kod,
                    p.Nazwa,
                    Unit = p.DomyslnaJednostka != null ? p.DomyslnaJednostka.Kod : "j.m."
                })
                .ToListAsync();

            var metaByProductId = productMetadata.ToDictionary(
                x => x.IdProduktu,
                x => (x.Kod, x.Nazwa, x.Unit));

            var warehouseNames = await _context.Magazyn
                .AsNoTracking()
                .Where(m => warehouseIds.Contains(m.IdMagazynu))
                .ToDictionaryAsync(m => m.IdMagazynu, m => m.Nazwa);

            var keyData = new Dictionary<string, (decimal Physical, decimal ActiveRes, decimal DraftWz)>(StringComparer.Ordinal);
            foreach (var row in productsByWarehouse)
            {
                var key = BuildStockKey(row.WarehouseId, row.ProductId);
                keyData[key] = (row.PhysicalQty, 0m, 0m);
            }

            foreach (var row in activeReserved)
            {
                var key = BuildStockKey(row.WarehouseId, row.ProductId);
                if (keyData.TryGetValue(key, out var current))
                {
                    keyData[key] = (current.Physical, row.Qty, current.DraftWz);
                }
                else
                {
                    keyData[key] = (0m, row.Qty, 0m);
                }
            }

            foreach (var row in draftWzReserved)
            {
                var key = BuildStockKey(row.WarehouseId, row.ProductId);
                if (keyData.TryGetValue(key, out var current))
                {
                    keyData[key] = (current.Physical, current.ActiveRes, row.Qty);
                }
                else
                {
                    keyData[key] = (0m, 0m, row.Qty);
                }
            }

            var productsByWarehouseLookup = keyData.Keys
                .Select(k =>
                {
                    var split = k.Split(':');
                    return new { WarehouseId = int.Parse(split[0]), ProductId = int.Parse(split[1]) };
                })
                .GroupBy(x => x.WarehouseId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.ProductId).Distinct().ToList());

            var supportedRuleIds = reguly
                .Where(r => IsSupportedRuleType((r.Typ ?? string.Empty).Trim()))
                .Select(r => r.Id)
                .ToHashSet();

            var existingUnackedAlerts = await _context.Alert
                .Where(a => !a.CzyPotwierdzony && supportedRuleIds.Contains(a.IdReguly))
                .ToListAsync();

            var existingUnackedMap = existingUnackedAlerts
                .GroupBy(a => BuildAlertIdentityKey(a.IdReguly, a.IdMagazynu, a.IdProduktu))
                .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);

            var now = DateTime.UtcNow;
            var newAlerts = new List<Alert>();

            foreach (var regula in reguly)
            {
                var typ = (regula.Typ ?? string.Empty).Trim();
                if (!IsSupportedRuleType(typ))
                {
                    result.LiczbaPominietychNieobslugiwanychRegul++;
                    continue;
                }

                IEnumerable<int> candidateProductIds;
                if (regula.IdProduktu.HasValue)
                {
                    candidateProductIds = new[] { regula.IdProduktu.Value };
                }
                else
                {
                    candidateProductIds = productsByWarehouseLookup.TryGetValue(regula.IdMagazynu, out var ids)
                        ? ids
                        : Enumerable.Empty<int>();
                }

                foreach (var productId in candidateProductIds.Distinct())
                {
                    result.LiczbaSprawdzonychPozycji++;

                    var stockKey = BuildStockKey(regula.IdMagazynu, productId);
                    keyData.TryGetValue(stockKey, out var stock);
                    var available = stock.Physical - stock.ActiveRes - stock.DraftWz;

                    var triggered = RuleTriggered(typ, regula.Prog, available);
                    var identityKey = BuildAlertIdentityKey(regula.Id, regula.IdMagazynu, productId);

                    if (!triggered)
                    {
                        if (existingUnackedMap.TryGetValue(identityKey, out var staleAlerts))
                        {
                            foreach (var alert in staleAlerts.Where(a => !a.CzyPotwierdzony))
                            {
                                alert.CzyPotwierdzony = true;
                                alert.PotwierdzonoUtc = now;
                                alert.PotwierdzilUserId = null;
                                result.LiczbaAutoPotwierdzonychAlertow++;
                            }
                        }
                        continue;
                    }

                    if (existingUnackedMap.TryGetValue(identityKey, out var existingForIdentity) &&
                        existingForIdentity.Any(a => !a.CzyPotwierdzony))
                    {
                        result.LiczbaPominietychDuplikatow++;
                        continue;
                    }

                    var meta = metaByProductId.TryGetValue(productId, out var m)
                        ? m
                        : (Kod: $"ID={productId}", Nazwa: "Produkt", Unit: "j.m.");
                    var warehouseName = warehouseNames.TryGetValue(regula.IdMagazynu, out var wn)
                        ? wn
                        : $"Magazyn {regula.IdMagazynu}";

                    newAlerts.Add(new Alert
                    {
                        IdReguly = regula.Id,
                        IdMagazynu = regula.IdMagazynu,
                        IdProduktu = productId,
                        Waga = DetermineSeverity(typ, regula.Prog, available),
                        Tresc = BuildAlertMessage(typ, regula.Prog, available, stock.Physical, stock.ActiveRes, stock.DraftWz, meta.Kod, warehouseName, meta.Unit),
                        UtworzonoUtc = now,
                        CzyPotwierdzony = false,
                        PotwierdzilUserId = null,
                        PotwierdzonoUtc = null
                    });

                    if (!existingUnackedMap.TryGetValue(identityKey, out var list))
                    {
                        list = new List<Alert>();
                        existingUnackedMap[identityKey] = list;
                    }
                    list.Add(newAlerts[^1]);
                }
            }

            if (newAlerts.Count > 0)
            {
                _context.Alert.AddRange(newAlerts);
            }

            if (newAlerts.Count > 0 || result.LiczbaAutoPotwierdzonychAlertow > 0)
            {
                await _context.SaveChangesAsync();
            }

            result.LiczbaNowychAlertow = newAlerts.Count;
            return result;
        }

        private static bool IsSupportedRuleType(string typ) =>
            typ.Equals("LowStock", StringComparison.OrdinalIgnoreCase) ||
            typ.Equals("ReorderPoint", StringComparison.OrdinalIgnoreCase) ||
            typ.Equals("NoStock", StringComparison.OrdinalIgnoreCase);

        private static bool RuleTriggered(string typ, decimal prog, decimal available)
        {
            if (typ.Equals("NoStock", StringComparison.OrdinalIgnoreCase))
            {
                return available <= 0m;
            }

            if (typ.Equals("LowStock", StringComparison.OrdinalIgnoreCase))
            {
                return available < prog;
            }

            if (typ.Equals("ReorderPoint", StringComparison.OrdinalIgnoreCase))
            {
                return available <= prog;
            }

            return false;
        }

        private static string DetermineSeverity(string typ, decimal prog, decimal available)
        {
            if (typ.Equals("NoStock", StringComparison.OrdinalIgnoreCase))
            {
                return "CRIT";
            }

            if (available <= 0m)
            {
                return "CRIT";
            }

            if (typ.Equals("ReorderPoint", StringComparison.OrdinalIgnoreCase) && available <= prog)
            {
                return "WARN";
            }

            return "WARN";
        }

        private static string BuildAlertMessage(
            string typ,
            decimal prog,
            decimal available,
            decimal physical,
            decimal reservedActive,
            decimal reservedDraftWz,
            string productCode,
            string warehouseName,
            string unit)
        {
            var baseMsg = typ.Equals("NoStock", StringComparison.OrdinalIgnoreCase)
                ? $"Brak dostępnego stanu produktu {productCode} w magazynie {warehouseName}."
                : typ.Equals("ReorderPoint", StringComparison.OrdinalIgnoreCase)
                    ? $"Produkt {productCode} osiągnął/przekroczył punkt ponownego zamówienia w magazynie {warehouseName}."
                    : $"Niski stan produktu {productCode} w magazynie {warehouseName}.";

            return $"{baseMsg} Dostępne: {available:0.###} {unit}, próg: {prog:0.###}. Stan fiz.: {physical:0.###}, rez. aktywne: {reservedActive:0.###}, WZ Draft: {reservedDraftWz:0.###}.";
        }

        private static string BuildStockKey(int warehouseId, int productId) => $"{warehouseId}:{productId}";
        private static string BuildAlertIdentityKey(int ruleId, int warehouseId, int productId) => $"{ruleId}:{warehouseId}:{productId}";
    }
}