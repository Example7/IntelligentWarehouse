using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.Magazyn
{
    [Table("ProductCodes")]
    public class KodProduktu
    {
        [Key]
        [Column("ProductCodeId")]
        public int IdKodu { get; set; }

        [Required]
        [Column("ProductId")]
        public int IdProduktu { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdProduktu))]
        public Produkt Produkt { get; set; } = null!;

        [Required, MaxLength(120)]
        [Column("CodeValue")]
        public string Wartosc { get; set; } = null!; // EAN/QR/itp.

        [Required, MaxLength(20)]
        [Column("CodeType")]
        public string Typ { get; set; } = "EAN";

        [Column("IsPrimary")]
        public bool CzyGlowny { get; set; }
    }
}