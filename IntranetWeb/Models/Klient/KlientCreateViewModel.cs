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
}
