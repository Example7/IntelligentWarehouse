namespace MobileApi.Models.Client;

public sealed class ClientOrderListItemDto
{
    public int OrderId { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime IssuedAtUtc { get; set; }
    public DateTime? PostedAtUtc { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int ItemsCount { get; set; }
    public decimal TotalQuantity { get; set; }
}
