namespace MobileApi.Models.Auth;

public sealed class LoginRequestDto
{
    public string LoginOrEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
