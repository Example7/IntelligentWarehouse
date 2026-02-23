using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.Magazyn
{
    [Table("Customers")]
    public class Klient
    {
        [Key]
        [Column("CustomerId")]
        public int IdKlienta { get; set; }

        [Required, MaxLength(250)]
        [Column("Name")]
        public string Nazwa { get; set; } = null!;

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
        public ICollection<DokumentWZ> Wydania { get; set; } = new List<DokumentWZ>();
    }
}