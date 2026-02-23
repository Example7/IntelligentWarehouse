namespace MobileApi.Models.Auth;

public sealed class CurrentUserDto
{
    public int UserId { get; set; }
    public string Login { get; set; } = string.Empty;
    public string? Email { get; set; }
    public List<string> Roles { get; set; } = new();
}
