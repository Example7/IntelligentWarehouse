namespace MobileApi.Models.Client;

public sealed class ClientNotificationDto
{
    public long NotificationId { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Status { get; set; }
    public string? DocumentType { get; set; }
    public string? DocumentNumber { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public bool IsAcknowledged { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
}
