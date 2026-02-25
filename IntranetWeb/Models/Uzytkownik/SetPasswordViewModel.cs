using System.ComponentModel.DataAnnotations;

namespace IntranetWeb.Models.Uzytkownik;

public class SetPasswordViewModel
{
    public int UserId { get; set; }

    public string UserLogin { get; set; } = string.Empty;

    public string? UserEmail { get; set; }

    [Required(ErrorMessage = "Podaj nowe hasło.")]
    [MinLength(6, ErrorMessage = "Hasło musi mieć co najmniej 6 znaków.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nowe hasło")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Potwierdź hasło.")]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Hasła nie są takie same.")]
    [Display(Name = "Potwierdź nowe hasło")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
