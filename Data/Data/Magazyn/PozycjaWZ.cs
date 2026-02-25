using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.Magazyn
{
    [Table("GoodsIssueItems")]
    public class PozycjaWZ
    {
        [Key]
        [Column("IssueItemId")]
        public int Id { get; set; }

        [Required]
        [Column("IssueId")]
        public int IdDokumentu { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdDokumentu))]
        public DokumentWZ Dokument { get; set; } = null!;

        [Required]
        [Column("LineNo")]
        public int Lp { get; set; }

        [Required]
        [Column("ProductId")]
        public int IdProduktu { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdProduktu))]
        public Produkt Produkt { get; set; } = null!;

        // W praktyce lokacja może być wybrana dopiero przy kompletacji → nullable
        [Column("LocationId")]
        public int? IdLokacji { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdLokacji))]
        public Lokacja? Lokacja { get; set; }

        [Column("BatchId")]
        [Display(Name = "Partia")]
        public int? IdPartii { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdPartii))]
        public Partia? Partia { get; set; }

        [Required]
        [Column("Quantity", TypeName = "decimal(18,3)")]
        public decimal Ilosc { get; set; }
    }
}
