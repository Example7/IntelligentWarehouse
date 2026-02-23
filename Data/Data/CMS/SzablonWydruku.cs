using Data.Data.Magazyn;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.CMS
{
    [Table("PrintTemplates")]
    public class SzablonWydruku
    {
        [Key]
        [Column("TemplateId")]
        public int Id { get; set; }

        [Required, MaxLength(10)]
        [Column("DocType")]
        public string TypDokumentu { get; set; } = null!; // PZ/WZ/MM/...

        [Required, MaxLength(200)]
        [Column("Name")]
        public string Nazwa { get; set; } = null!;

        [Required, MaxLength(30)]
        [Column("Version")]
        public string Wersja { get; set; } = "1.0";

        [Required, MaxLength(260)]
        [Column("FileName")]
        public string NazwaPliku { get; set; } = null!;

        [Required, MaxLength(400)]
        [Column("FilePath")]
        public string Sciezka { get; set; } = null!;

        [Column("IsActive")]
        public bool CzyAktywny { get; set; } = true;

        [Column("UploadedAt")]
        public DateTime WgranoUtc { get; set; } = DateTime.UtcNow;

        [Column("UploadedByUserId")]
        public int? WgralUserId { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(WgralUserId))]
        public Uzytkownik? Wgral { get; set; }
    }
}