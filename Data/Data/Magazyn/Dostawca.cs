using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.Magazyn
{
    [Table("Suppliers")]
    public class Dostawca
    {
        [Key]
        [Column("SupplierId")]
        public int IdDostawcy { get; set; }

        [Required, MaxLength(250)]
        [Column("Name")]
        public string Nazwa { get; set; } = null!;

        [MaxLength(20)]
        [Column("TaxId")]
        public string? NIP { get; set; }

        [MaxLength(200)]
        [Column("Email")]
        public string? Email { get; set; }

        [MaxLength(60)]
        [Column("Phone")]
        public string? Telefon { get; set; }

        [MaxLength(400)]
        [Column("Address")]
        public string? Adres { get; set; }

        [Column("IsActive")]
        public bool CzyAktywny { get; set; } = true;

        [Column("CreatedAt")]
        public DateTime UtworzonoUtc { get; set; } = DateTime.UtcNow;

        [Timestamp]
        [ValidateNever]
        [Column("RowVersion")]
        public byte[] RowVersion { get; set; } = null!;

        [ValidateNever]
        public ICollection<DokumentPZ> Przyjecia { get; set; } = new List<DokumentPZ>();
    }
}