using Data.Data.Magazyn;

namespace Interfaces.Dashboard
{
    public interface IDashboardService
    {
        Task<DashboardVm> GetDashboardAsync();
    }

    public class DashboardVm
    {
        public DashboardKpiVm Kpi { get; set; } = new();
        public DashboardBusinessMetricsVm BusinessMetrics { get; set; } = new();
        public IList<DashboardRecentMovementVm> RecentMovements { get; set; } = new List<DashboardRecentMovementVm>();
        public IList<DashboardDocumentVm> RecentPzDocuments { get; set; } = new List<DashboardDocumentVm>();
        public IList<DashboardDocumentVm> RecentWzDocuments { get; set; } = new List<DashboardDocumentVm>();
        public IList<DashboardAlertVm> ActiveAlerts { get; set; } = new List<DashboardAlertVm>();
        public IList<DashboardTopProductVm> TopProductsLast7Days { get; set; } = new List<DashboardTopProductVm>();
    }

    public class DashboardKpiVm
    {
        public int ActiveProducts { get; set; }
        public int StockEntries { get; set; }
        public int MovementsToday { get; set; }
        public int PzToday { get; set; }
        public int WzToday { get; set; }
        public int ActiveAlerts { get; set; }
        public int ActiveUsers { get; set; }
    }

    public class DashboardRecentMovementVm
    {
        public int Id { get; set; }
        public TypRuchuMagazynowego Type { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string UomCode { get; set; } = "j.m.";
        public string? FromLocationCode { get; set; }
        public string? FromWarehouseName { get; set; }
        public string? ToLocationCode { get; set; }
        public string? ToWarehouseName { get; set; }
        public decimal Quantity { get; set; }
        public string? Reference { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string? UserEmail { get; set; }
    }

    public class DashboardBusinessMetricsVm
    {
        public int LowStockProducts { get; set; }
        public int DraftDocumentsOlderThan24h { get; set; }
        public int DraftPzOlderThan24h { get; set; }
        public int DraftWzOlderThan24h { get; set; }
    }

    public class DashboardDocumentVm
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DateUtc { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string ContractorName { get; set; } = string.Empty;
    }

    public class DashboardAlertVm
    {
        public long Id { get; set; }
        public string Severity { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
    }

    public class DashboardTopProductVm
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int MovementCount { get; set; }
        public decimal TotalQuantity { get; set; }
        public DateTime LastMovementUtc { get; set; }
    }
}
