using System.ComponentModel.DataAnnotations;

namespace IntranetWeb.Models.Klient;

public class KlientCreateViewModel
{
    [Required(ErrorMessage = "Nazwa jest wymagana.")]
    [StringLength(250)]
    [Display(Name = "Nazwa")]
    public string Nazwa { get; set; } = string.Empty;

    [StringLength(200)]
    [EmailAddress(ErrorMessage = "Podaj poprawny adres e-mail.")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [StringLength(60)]
    [Display(Name = "Telefon")]
    public string? Telefon { get; set; }

    [StringLength(400)]
    [Display(Name = "Adres")]
    public string? Adres { get; set; }

    [Display(Name = "Aktywny klient")]
    public bool CzyAktywny { get; set; } = true;

    [Display(Name = "Utwórz od razu konto mobilne")]
    public bool UtworzKontoMobilne { get; set; }

    [StringLength(80)]
    [Display(Name = "Login")]
    public string? LoginMobilny { get; set; }

    [StringLength(120)]
    [EmailAddress(ErrorMessage = "Podaj poprawny adres e-mail konta mobilnego.")]
    [Display(Name = "Email konta mobilnego")]
    public string? EmailMobilny { get; set; }

    [StringLength(100, MinimumLength = 8, ErrorMessage = "Hasło musi mieć od 8 do 100 znaków.")]
    [DataType(DataType.Password)]
    [Display(Name = "Hasło tymczasowe")]
    public string? HasloMobilne { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Potwierdź hasło")]
    public string? PotwierdzHasloMobilne { get; set; }

    [Display(Name = "Aktywne konto mobilne")]
    public bool CzyAktywneKontoMobilne { get; set; } = true;
}