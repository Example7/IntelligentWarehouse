namespace MobileApi.Models.Auth;

public sealed class RegisterClientResponseDto
{
    public int UserId { get; set; }
    public int CustomerId { get; set; }
    public string Message { get; set; } = string.Empty;
}
