using Data.Data.Magazyn;   // <-- dodaj
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.CMS
{
    [Table("MediaFiles")]
    public class PlikMedia
    {
        [Key]
        [Column("MediaFileId")]
        public long Id { get; set; }

        [Required, MaxLength(260)]
        [Column("FileName")]
        [Display(Name = "Nazwa pliku")]
        public string NazwaPliku { get; set; } = null!;

        [Required, MaxLength(100)]
        [Column("ContentType")]
        [Display(Name = "Typ zawartości")]
        public string ContentType { get; set; } = null!;

        [Required, MaxLength(400)]
        [Column("FilePath")]
        [Display(Name = "Ścieżka")]
        public string Sciezka { get; set; } = null!;

        [Column("SizeBytes")]
        [Display(Name = "Rozmiar (bajty)")]
        public long RozmiarBajty { get; set; }

        [MaxLength(300)]
        [Column("Description")]
        public string? Opis { get; set; }

        [Column("UploadedAt")]
        public DateTime WgranoUtc { get; set; } = DateTime.UtcNow;

        [Column("UploadedByUserId")]
        [Display(Name = "Wgrany przez")]
        public int? WgralUserId { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(WgralUserId))]
        public Uzytkownik? Wgral { get; set; }
    }
}