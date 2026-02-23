using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.CMS
{
    public class Aktualnosc
    {
        [Key]
        public int IdAktualnosci { get; set; }

        [Required(ErrorMessage = "Tytuł odnośnika jest wymagany")]
        [MaxLength(20, ErrorMessage = "Link może zawierać max 20 znaków")]
        [Display(Name = "Tytuł odnośnika")]
        public required string TytulLinku { get; set; }

        [Required(ErrorMessage = "Nazwa aktualności jest wymagana")]
        [MaxLength(50, ErrorMessage = "Nazwa aktualności może zawierać max 50 znaków")]
        public required string Nazwa { get; set; }

        [Display(Name = "Treść aktualności")]
        [Column(TypeName = "nvarchar(max)")]
        public required string Tresc { get; set; }

        [Required(ErrorMessage = "Pozycja wyświetlania jest wymagana")]
        [Display(Name = "Pozycja wyświetlania")]
        public int Pozycja { get; set; }
    }
}
