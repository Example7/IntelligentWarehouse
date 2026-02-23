using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.Magazyn
{
    [Table("AlertRules")]
    public class RegulaAlertu
    {
        [Key]
        [Column("AlertRuleId")]
        public int Id { get; set; }

        [Required]
        [Column("WarehouseId")]
        public int IdMagazynu { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdMagazynu))]
        public Magazyn Magazyn { get; set; } = null!;

        [Column("ProductId")]
        public int? IdProduktu { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdProduktu))]
        public Produkt? Produkt { get; set; }

        [Required, MaxLength(20)]
        [Column("RuleType")]
        public string Typ { get; set; } = "LowStock";

        [Required]
        [Column("ThresholdValue", TypeName = "decimal(18,3)")]
        public decimal Prog { get; set; }

        [Column("IsEnabled")]
        public bool CzyWlaczona { get; set; } = true;

        [Column("CreatedAt")]
        public DateTime UtworzonoUtc { get; set; } = DateTime.UtcNow;
    }
}