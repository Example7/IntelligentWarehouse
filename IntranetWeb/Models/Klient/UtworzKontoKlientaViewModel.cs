using System.ComponentModel.DataAnnotations;

namespace IntranetWeb.Models.Klient;

public class UtworzKontoKlientaViewModel
{
    [Required]
    public int KlientId { get; set; }

    [Required(ErrorMessage = "Login jest wymagany.")]
    [StringLength(80)]
    [Display(Name = "Login")]
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email jest wymagany.")]
    [EmailAddress(ErrorMessage = "Podaj poprawny adres e-mail.")]
    [StringLength(120)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Hasło jest wymagane.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Hasło musi mieć od 8 do 100 znaków.")]
    [DataType(DataType.Password)]
    [Display(Name = "Hasło tymczasowe")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Potwierdzenie hasła jest wymagane.")]
    [Compare(nameof(Password), ErrorMessage = "Hasła muszą być takie same.")]
    [DataType(DataType.Password)]
    [Display(Name = "Potwierdź hasło")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Display(Name = "Aktywne konto")]
    public bool IsActive { get; set; } = true;
}