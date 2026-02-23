using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.Magazyn
{
    [Table("Alerts")]
    public class Alert
    {
        [Key]
        [Column("AlertId")]
        public long Id { get; set; }

        [Required]
        [Column("AlertRuleId")]
        public int IdReguly { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdReguly))]
        public RegulaAlertu Regula { get; set; } = null!;

        [Required]
        [Column("WarehouseId")]
        public int IdMagazynu { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdMagazynu))]
        public Magazyn Magazyn { get; set; } = null!;

        [Required]
        [Column("ProductId")]
        public int IdProduktu { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdProduktu))]
        public Produkt Produkt { get; set; } = null!;

        [Required, MaxLength(10)]
        [Column("Severity")]
        public string Waga { get; set; } = "WARN";

        [Required, MaxLength(500)]
        [Column("Message")]
        public string Tresc { get; set; } = null!;

        [Column("CreatedAt")]
        public DateTime UtworzonoUtc { get; set; } = DateTime.UtcNow;

        [Column("IsAcknowledged")]
        public bool CzyPotwierdzony { get; set; }

        [Column("AcknowledgedByUserId")]
        public int? PotwierdzilUserId { get; set; }

        [Column("AcknowledgedAt")]
        public DateTime? PotwierdzonoUtc { get; set; }
    }
}