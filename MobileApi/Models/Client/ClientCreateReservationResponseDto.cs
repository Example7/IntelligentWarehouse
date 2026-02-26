namespace MobileApi.Models.Client;

public sealed class ClientCreateReservationResponseDto
{
    public int ReservationId { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public bool AutoActivationAttempted { get; set; }
    public bool AutoActivationSucceeded { get; set; }
    public string? AutoActivationMessage { get; set; }
}
