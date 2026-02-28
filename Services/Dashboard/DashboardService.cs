using Data.Data;
using Interfaces.Dashboard;
using Interfaces.Magazyn;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.Dashboard
{
    public class DashboardService : BaseService, IDashboardService
    {
        private readonly IRaportMagazynowyService _raportMagazynowyService;

        public DashboardService(DataContext context, IRaportMagazynowyService raportMagazynowyService) : base(context)
        {
            _raportMagazynowyService = raportMagazynowyService;
        }

        public async Task<DashboardVm> GetDashboardAsync()
        {
            var nowUtc = DateTime.UtcNow;
            var todayUtc = nowUtc.Date;
            var tomorrowUtc = todayUtc.AddDays(1);
            var last7DaysUtc = nowUtc.AddDays(-7);

            var activeAlerts = await _context.Alert.AsNoTracking()
                .Where(x => !x.CzyPotwierdzony)
                .OrderByDescending(x => x.UtworzonoUtc)
                .Select(x => new DashboardAlertVm
                {
                    Id = x.Id,
                    Severity = x.Waga,
                    Message = x.Tresc,
                    CreatedAtUtc = x.UtworzonoUtc,
                    ProductId = x.IdProduktu,
                    WarehouseId = x.IdMagazynu,
                    ProductCode = x.Produkt.Kod,
                    ProductName = x.Produkt.Nazwa,
                    WarehouseName = x.Magazyn.Nazwa,
                    UomCode = x.Produkt.DomyslnaJednostka != null ? x.Produkt.DomyslnaJednostka.Kod : "j.m."
                })
                .Take(6)
                .ToListAsync();

            var alertWarehouseIds = activeAlerts
                .Select(x => x.WarehouseId)
                .Distinct()
                .ToList();

            var suggestedQtyByKey = new Dictionary<string, (decimal Qty, string Uom)>(StringComparer.Ordinal);
            foreach (var warehouseId in alertWarehouseIds)
            {
                var report = await _raportMagazynowyService.GetRaportPropozycjiZamowienAsync(null, warehouseId);
                foreach (var row in report.Rows)
                {
                    suggestedQtyByKey[BuildAlertKey(row.IdMagazynu, row.IdProduktu)] = (row.ProponowanaIloscZamowienia, row.Jednostka);
                }
            }

            foreach (var alert in activeAlerts)
            {
                if (suggestedQtyByKey.TryGetValue(BuildAlertKey(alert.WarehouseId, alert.ProductId), out var suggested))
                {
                    alert.SuggestedOrderQty = suggested.Qty;
                    alert.UomCode = string.IsNullOrWhiteSpace(suggested.Uom) ? alert.UomCode : suggested.Uom;
                }
            }

            var vm = new DashboardVm
            {
                Kpi = new DashboardKpiVm
                {
                    ActiveProducts = await _context.Produkt.AsNoTracking().CountAsync(x => x.CzyAktywny),
                    StockEntries = await _context.StanMagazynowy.AsNoTracking().CountAsync(),
                    MovementsToday = await _context.RuchMagazynowy.AsNoTracking()
                        .CountAsync(x => x.UtworzonoUtc >= todayUtc && x.UtworzonoUtc < tomorrowUtc),
                    PzToday = await _context.DokumentPZ.AsNoTracking()
                        .CountAsync(x => x.DataPrzyjeciaUtc >= todayUtc && x.DataPrzyjeciaUtc < tomorrowUtc),
                    WzToday = await _context.DokumentWZ.AsNoTracking()
                        .CountAsync(x => x.DataWydaniaUtc >= todayUtc && x.DataWydaniaUtc < tomorrowUtc),
                    ActiveAlerts = await _context.Alert.AsNoTracking().CountAsync(x => !x.CzyPotwierdzony),
                    ReservationsToday = await _context.Rezerwacja.AsNoTracking()
                        .CountAsync(x => x.UtworzonoUtc >= todayUtc && x.UtworzonoUtc < tomorrowUtc)
                },
                BusinessMetrics = new DashboardBusinessMetricsVm
                {
                    UnacknowledgedCritAlerts = await _context.Alert.AsNoTracking()
                        .CountAsync(x => !x.CzyPotwierdzony && x.Waga == "CRIT"),
                    DraftPzProductsWithoutUnitPrice = await _context.PozycjaPZ.AsNoTracking()
                        .Where(x => x.Dokument.Status == "Draft" && !x.CenaJednostkowa.HasValue)
                        .Select(x => x.IdProduktu)
                        .Distinct()
                        .CountAsync(),
                    DraftWzDocuments = await _context.DokumentWZ.AsNoTracking()
                        .CountAsync(x => x.Status == "Draft")
                },
                RecentMovements = await _context.RuchMagazynowy.AsNoTracking()
                    .OrderByDescending(x => x.UtworzonoUtc)
                    .Select(x => new DashboardRecentMovementVm
                    {
                        Id = x.IdRuchu,
                        Type = x.Typ,
                        ProductCode = x.Produkt.Kod,
                        ProductName = x.Produkt.Nazwa,
                        UomCode = x.Produkt.DomyslnaJednostka != null ? x.Produkt.DomyslnaJednostka.Kod : "j.m.",
                        FromLocationCode = x.LokacjaZ != null ? x.LokacjaZ.Kod : null,
                        FromWarehouseName = x.LokacjaZ != null && x.LokacjaZ.Magazyn != null ? x.LokacjaZ.Magazyn.Nazwa : null,
                        ToLocationCode = x.LokacjaDo != null ? x.LokacjaDo.Kod : null,
                        ToWarehouseName = x.LokacjaDo != null && x.LokacjaDo.Magazyn != null ? x.LokacjaDo.Magazyn.Nazwa : null,
                        Quantity = x.Ilosc,
                        Reference = x.Referencja,
                        CreatedAtUtc = x.UtworzonoUtc,
                        UserEmail = x.Uzytkownik != null ? x.Uzytkownik.Email : null
                    })
                    .Take(8)
                    .ToListAsync(),
                RecentPzDocuments = await _context.DokumentPZ.AsNoTracking()
                    .OrderByDescending(x => x.DataPrzyjeciaUtc)
                    .Select(x => new DashboardDocumentVm
                    {
                        Id = x.Id,
                        Number = x.Numer,
                        Status = x.Status,
                        DateUtc = x.DataPrzyjeciaUtc,
                        WarehouseName = x.Magazyn.Nazwa,
                        ContractorName = x.Dostawca.Nazwa
                    })
                    .Take(5)
                    .ToListAsync(),
                RecentWzDocuments = await _context.DokumentWZ.AsNoTracking()
                    .OrderByDescending(x => x.DataWydaniaUtc)
                    .Select(x => new DashboardDocumentVm
                    {
                        Id = x.Id,
                        Number = x.Numer,
                        Status = x.Status,
                        DateUtc = x.DataWydaniaUtc,
                        WarehouseName = x.Magazyn.Nazwa,
                        ContractorName = x.Klient != null ? x.Klient.Nazwa : "-"
                    })
                    .Take(5)
                    .ToListAsync(),
                ActiveAlerts = activeAlerts,
                ReorderSuppliers = await _context.Dostawca.AsNoTracking()
                    .Where(d => d.CzyAktywny)
                    .OrderBy(d => d.Nazwa)
                    .Select(d => new DashboardSelectOptionVm
                    {
                        Value = d.IdDostawcy,
                        Text = d.Nazwa
                    })
                    .ToListAsync(),
                ReorderReceiptLocations = await _context.Lokacja.AsNoTracking()
                    .Where(l => l.CzyAktywna)
                    .OrderBy(l => l.Magazyn.Nazwa)
                    .ThenBy(l => l.Kod)
                    .Select(l => new DashboardSelectOptionVm
                    {
                        Value = l.IdLokacji,
                        Text = (l.Magazyn != null ? l.Magazyn.Nazwa + " / " : string.Empty) + l.Kod,
                        WarehouseId = l.IdMagazynu
                    })
                    .ToListAsync(),
                TopProductsLast7Days = await _context.RuchMagazynowy.AsNoTracking()
                    .Where(x => x.UtworzonoUtc >= last7DaysUtc)
                    .GroupBy(x => new { x.IdProduktu, x.Produkt.Kod, x.Produkt.Nazwa })
                    .Select(g => new DashboardTopProductVm
                    {
                        ProductId = g.Key.IdProduktu,
                        ProductCode = g.Key.Kod,
                        ProductName = g.Key.Nazwa,
                        MovementCount = g.Count(),
                        TotalQuantity = g.Sum(x => x.Ilosc),
                        LastMovementUtc = g.Max(x => x.UtworzonoUtc)
                    })
                    .OrderByDescending(x => x.MovementCount)
                    .ThenByDescending(x => x.LastMovementUtc)
                    .Take(5)
                    .ToListAsync()
            };

            return vm;
        }

        private static string BuildAlertKey(int warehouseId, int productId) => $"{warehouseId}:{productId}";
    }
}
