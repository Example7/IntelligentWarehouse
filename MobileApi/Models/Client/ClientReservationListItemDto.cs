namespace MobileApi.Models.Client;

public sealed class ClientReservationListItemDto
{
    public int ReservationId { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int ItemsCount { get; set; }
    public decimal TotalQuantity { get; set; }
}
