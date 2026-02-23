using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.Magazyn
{
    [Table("Batches")]
    public class Partia
    {
        [Key]
        [Column("BatchId")]
        public int IdPartii { get; set; }

        [Required]
        [Column("ProductId")]
        public int IdProduktu { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdProduktu))]
        public Produkt Produkt { get; set; } = null!;

        [Required, MaxLength(80)]
        [Column("BatchNumber")]
        public string NumerPartii { get; set; } = null!;

        [Column("ProductionDate", TypeName = "date")]
        public DateTime? DataProdukcji { get; set; }

        [Column("ExpiryDate", TypeName = "date")]
        public DateTime? DataWaznosci { get; set; }

        [Column("SupplierId")]
        public int? IdDostawcy { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdDostawcy))]
        public Dostawca? Dostawca { get; set; }
    }
}