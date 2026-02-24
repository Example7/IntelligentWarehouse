using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Data.Data.Magazyn
{
    [Table("Stock")]
    public class StanMagazynowy
    {
        [Key]
        [Column("StockId")]
        public int IdStanu { get; set; }

        [Required]
        [Column("ProductId")]
        [Display(Name = "Produkt")]
        public int IdProduktu { get; set; }

        [ForeignKey(nameof(IdProduktu))]
        [ValidateNever]
        public Produkt Produkt { get; set; } = null!;

        [Required]
        [Column("LocationId")]
        [Display(Name = "Lokacja")]
        public int IdLokacji { get; set; }

        [ForeignKey(nameof(IdLokacji))]
        [ValidateNever]
        public Lokacja Lokacja { get; set; } = null!;

        [Column("Quantity", TypeName = "decimal(18,3)")]
        [Display(Name = "Ilość")]
        public decimal Ilosc { get; set; }

        [Timestamp]
        [ValidateNever]
        [Column("RowVersion")]
        public byte[] RowVersion { get; set; } = null!;
    }
}


