using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public int IdProduktu { get; set; }

        [ForeignKey(nameof(IdProduktu))]
        public Produkt Produkt { get; set; } = null!;

        [Required]
        [Column("LocationId")]
        public int IdLokacji { get; set; }

        [ForeignKey(nameof(IdLokacji))]
        public Lokacja Lokacja { get; set; } = null!;

        [Column("Quantity", TypeName = "decimal(18,3)")]
        public decimal Ilosc { get; set; }

        [Timestamp]
        [Column("RowVersion")]
        public byte[] RowVersion { get; set; } = null!;
    }
}
