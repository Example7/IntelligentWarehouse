using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.Magazyn
{
    [Table("StockCountItems")]
    public class PozycjaInwentaryzacji
    {
        [Key]
        [Column("StockCountItemId")]
        public int Id { get; set; }

        [Required]
        [Column("StockCountId")]
        public int IdDokumentu { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdDokumentu))]
        public Inwentaryzacja Dokument { get; set; } = null!;

        [Required]
        [Column("LineNo")]
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

        [Required]
        [Column("ExpectedQty", TypeName = "decimal(18,3)")]
        public decimal IloscOczekiwana { get; set; }

        [Required]
        [Column("CountedQty", TypeName = "decimal(18,3)")]
        public decimal IloscPoliczona { get; set; }
    }
}