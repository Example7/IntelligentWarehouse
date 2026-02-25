using System.ComponentModel.DataAnnotations;

namespace IntranetWeb.Models.Auth;

public class LoginViewModel
{
    [Required(ErrorMessage = "Podaj login lub email.")]
    [Display(Name = "Login lub email")]
    public string LoginOrEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Podaj hasło.")]
    [DataType(DataType.Password)]
    [Display(Name = "Hasło")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Zapamiętaj mnie")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}
