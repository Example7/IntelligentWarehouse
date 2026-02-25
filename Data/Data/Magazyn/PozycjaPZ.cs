using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.Magazyn
{
    [Table("GoodsReceiptItems")]
    public class PozycjaPZ
    {
        [Key]
        [Column("ReceiptItemId")]
        public int Id { get; set; }

        [Required]
        [Column("ReceiptId")]
        public int IdDokumentu { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdDokumentu))]
        public DokumentPZ Dokument { get; set; } = null!;

        [Required]
        [Column("LineNo")]
        [Display(Name = "Lp.")]
        public int Lp { get; set; }

        [Required]
        [Column("ProductId")]
        public int IdProduktu { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdProduktu))]
        public Produkt Produkt { get; set; } = null!;

        [Required]
        [Column("LocationId")]
        public int IdLokacji { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdLokacji))]
        public Lokacja Lokacja { get; set; } = null!;

        [Column("BatchId")]
        [Display(Name = "Partia")]
        public int? IdPartii { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdPartii))]
        public Partia? Partia { get; set; }

        [Required]
        [Column("Quantity", TypeName = "decimal(18,3)")]
        public decimal Ilosc { get; set; }

        [Column("UnitPrice", TypeName = "decimal(18,2)")]
        public decimal? CenaJednostkowa { get; set; }
    }
}
