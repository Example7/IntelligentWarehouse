using Data.Data.Magazyn;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.CMS
{
    [Table("DocumentAttachments")]
    public class ZalacznikDokumentu
    {
        [Key]
        [Column("AttachmentId")]
        public long Id { get; set; }

        [Required, MaxLength(10)]
        [Column("DocumentType")]
        public string TypDokumentu { get; set; } = null!; // PZ/WZ/MM/...

        [Required]
        [Column("DocumentId")]
        public int IdDokumentu { get; set; }

        [Required, MaxLength(260)]
        [Column("FileName")]
        public string NazwaPliku { get; set; } = null!;

        [Required, MaxLength(100)]
        [Column("ContentType")]
        public string ContentType { get; set; } = null!;

        [Required, MaxLength(400)]
        [Column("FilePath")]
        public string Sciezka { get; set; } = null!;

        [Column("UploadedAt")]
        public DateTime WgranoUtc { get; set; } = DateTime.UtcNow;

        [Column("UploadedByUserId")]
        public int? WgralUserId { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(WgralUserId))]
        public Uzytkownik? Wgral { get; set; }
    }
}