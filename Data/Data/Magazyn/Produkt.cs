using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.Magazyn
{
    [Table("Products")]
    public class Produkt
    {
        [Key]
        [Column("ProductId")]
        public int IdProduktu { get; set; }

        [Required, MaxLength(60)]
        [Column("SKU")]
        public required string Kod { get; set; }

        [Required, MaxLength(250)]
        [Column("Name")]
        public required string Nazwa { get; set; }

        [Column("Description")]
        public string? Opis { get; set; }

        [Column("CategoryId")]
        [Display(Name = "Kategoria")]
        public int IdKategorii { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdKategorii))]
        public Kategoria Kategoria { get; set; } = null!;

        [Column("DefaultUomId")]
        [Display(Name = "Domyślna jednostka miary")]
        public int IdDomyslnejJednostki { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdDomyslnejJednostki))]
        public JednostkaMiary DomyslnaJednostka { get; set; } = null!;

        [Column("MinStock", TypeName = "decimal(18,3)")]
        [Display(Name = "Stan minimalny")]
        public decimal StanMinimalny { get; set; } = 0m;

        [Column("ReorderPoint", TypeName = "decimal(18,3)")]
        [Display(Name = "Punkt ponownego zamówienia")]
        public decimal? PunktPonownegoZamowienia { get; set; }

        [Column("ReorderQty", TypeName = "decimal(18,3)")]
        [Display(Name = "Ilość ponownego zamówienia")]
        public decimal? IloscPonownegoZamowienia { get; set; }

        [Column("IsActive")]
        [Display(Name = "Aktywny")]
        public bool CzyAktywny { get; set; } = true;

        [Column("CreatedAt")]
        public DateTime UtworzonoUtc { get; set; } = DateTime.UtcNow;

        [Timestamp]
        [ValidateNever]
        [Column("RowVersion")]
        public byte[] RowVersion { get; set; } = null!;
    }
}