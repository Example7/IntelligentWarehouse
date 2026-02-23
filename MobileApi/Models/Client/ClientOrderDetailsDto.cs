namespace MobileApi.Models.Client;

public sealed class ClientOrderDetailsDto
{
    public int OrderId { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime IssuedAtUtc { get; set; }
    public DateTime? PostedAtUtc { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string? Note { get; set; }
    public decimal TotalQuantity { get; set; }
    public List<ClientOrderItemDto> Items { get; set; } = new();
}
