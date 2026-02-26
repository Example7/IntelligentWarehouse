namespace MobileApi.Models.Client;

public sealed class ClientProductLookupDto
{
    public int ProductId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? DefaultUom { get; set; }
    public decimal? AvailableQuantity { get; set; }
}
