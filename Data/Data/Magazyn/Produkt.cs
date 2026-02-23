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
        public int IdKategorii { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdKategorii))]
        public Kategoria Kategoria { get; set; } = null!;

        [Column("DefaultUomId")]
        public int IdDomyslnejJednostki { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdDomyslnejJednostki))]
        public JednostkaMiary DomyslnaJednostka { get; set; } = null!;

        [Column("MinStock", TypeName = "decimal(18,3)")]
        public decimal StanMinimalny { get; set; } = 0m;

        [Column("ReorderPoint", TypeName = "decimal(18,3)")]
        public decimal? PunktPonownegoZamowienia { get; set; }

        [Column("ReorderQty", TypeName = "decimal(18,3)")]
        public decimal? IloscPonownegoZamowienia { get; set; }

        [Column("IsActive")]
        public bool CzyAktywny { get; set; } = true;

        [Column("CreatedAt")]
        public DateTime UtworzonoUtc { get; set; } = DateTime.UtcNow;

        [Timestamp]
        [ValidateNever]
        [Column("RowVersion")]
        public byte[] RowVersion { get; set; } = null!;
    }
}