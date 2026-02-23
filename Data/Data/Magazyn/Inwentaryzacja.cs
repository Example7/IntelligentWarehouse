using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.Magazyn
{
    [Table("StockCounts")]
    public class Inwentaryzacja
    {
        [Key]
        [Column("StockCountId")]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        [Column("DocumentNo")]
        public string Numer { get; set; } = null!;

        [Required]
        [Column("WarehouseId")]
        public int IdMagazynu { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdMagazynu))]
        public Magazyn Magazyn { get; set; } = null!;

        [Required, MaxLength(20)]
        [Column("Status")]
        public string Status { get; set; } = "Draft";

        [Column("StartedAt")]
        public DateTime StartUtc { get; set; } = DateTime.UtcNow;

        [Column("ClosedAt")]
        public DateTime? KoniecUtc { get; set; }

        [Required]
        [Column("CreatedByUserId")]
        public int IdUtworzyl { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdUtworzyl))]
        public Uzytkownik Utworzyl { get; set; } = null!;

        [MaxLength(500)]
        [Column("Note")]
        public string? Notatka { get; set; }

        [Timestamp]
        [ValidateNever]
        [Column("RowVersion")]
        public byte[] RowVersion { get; set; } = null!;

        [ValidateNever]
        public ICollection<PozycjaInwentaryzacji> Pozycje { get; set; } = new List<PozycjaInwentaryzacji>();
    }
}