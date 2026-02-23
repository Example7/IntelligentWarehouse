using Data.Data;
using Interfaces.Dashboard;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.Dashboard
{
    public class DashboardService : BaseService, IDashboardService
    {
        public DashboardService(DataContext context) : base(context)
        {
        }

        public async Task<DashboardVm> GetDashboardAsync()
        {
            var nowUtc = DateTime.UtcNow;
            var todayUtc = nowUtc.Date;
            var tomorrowUtc = todayUtc.AddDays(1);
            var olderThan24hUtc = nowUtc.AddHours(-24);
            var last7DaysUtc = nowUtc.AddDays(-7);

            var draftPzOlderThan24h = await _context.DokumentPZ.AsNoTracking()
                .CountAsync(x => x.Status == "Draft" && x.DataPrzyjeciaUtc <= olderThan24hUtc);

            var draftWzOlderThan24h = await _context.DokumentWZ.AsNoTracking()
                .CountAsync(x => x.Status == "Draft" && x.DataWydaniaUtc <= olderThan24hUtc);

            var lowStockProducts = await _context.Produkt.AsNoTracking()
                .Where(p => p.CzyAktywny)
                .Select(p => new
                {
                    p.IdProduktu,
                    p.StanMinimalny,
                    CurrentQty = _context.StanMagazynowy
                        .Where(s => s.IdProduktu == p.IdProduktu)
                        .Select(s => (decimal?)s.Ilosc)
                        .Sum() ?? 0m
                })
                .CountAsync(x => x.CurrentQty < x.StanMinimalny);

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
                    ActiveUsers = await _context.Uzytkownik.AsNoTracking().CountAsync(x => x.CzyAktywny)
                },
                BusinessMetrics = new DashboardBusinessMetricsVm
                {
                    LowStockProducts = lowStockProducts,
                    DraftPzOlderThan24h = draftPzOlderThan24h,
                    DraftWzOlderThan24h = draftWzOlderThan24h,
                    DraftDocumentsOlderThan24h = draftPzOlderThan24h + draftWzOlderThan24h
                },
                RecentMovements = await _context.RuchMagazynowy.AsNoTracking()
                    .OrderByDescending(x => x.UtworzonoUtc)
                    .Select(x => new DashboardRecentMovementVm
                    {
                        Id = x.IdRuchu,
                        Type = x.Typ,
                        ProductCode = x.Produkt.Kod,
                        FromLocationCode = x.LokacjaZ != null ? x.LokacjaZ.Kod : null,
                        ToLocationCode = x.LokacjaDo != null ? x.LokacjaDo.Kod : null,
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
                ActiveAlerts = await _context.Alert.AsNoTracking()
                    .Where(x => !x.CzyPotwierdzony)
                    .OrderByDescending(x => x.UtworzonoUtc)
                    .Select(x => new DashboardAlertVm
                    {
                        Id = x.Id,
                        Severity = x.Waga,
                        Message = x.Tresc,
                        CreatedAtUtc = x.UtworzonoUtc,
                        ProductCode = x.Produkt.Kod,
                        WarehouseName = x.Magazyn.Nazwa
                    })
                    .Take(6)
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
    }
}
