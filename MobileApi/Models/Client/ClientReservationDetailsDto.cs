namespace MobileApi.Models.Client;

public sealed class ClientReservationDetailsDto
{
    public int ReservationId { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string? Note { get; set; }
    public decimal TotalQuantity { get; set; }
    public List<ClientReservationItemDto> Items { get; set; } = new();
}
