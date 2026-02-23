namespace MobileApi.Models.Client;

public sealed class ClientReservationItemDto
{
    public int ItemId { get; set; }
    public int LineNo { get; set; }
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public int? LocationId { get; set; }
    public string? LocationCode { get; set; }
}
