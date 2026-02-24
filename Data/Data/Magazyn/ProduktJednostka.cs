using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.Magazyn
{
    [Table("ProductUoms")]
    public class ProduktJednostka
    {
        [Key]
        [Column("ProductUomId")]
        public int Id { get; set; }

        [Required]
        [Column("ProductId")]
        [Display(Name = "Produkt")]
        public int IdProduktu { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdProduktu))]
        public Produkt Produkt { get; set; } = null!;

        [Required]
        [Column("UomId")]
        [Display(Name = "Jednostka miary")]
        public int IdJednostki { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdJednostki))]
        public JednostkaMiary Jednostka { get; set; } = null!;

        [Required]
        [Column("FactorToDefault", TypeName = "decimal(18,6)")]
        [Display(Name = "Przelicznik do domyślnej")]
        public decimal PrzelicznikDoDomyslnej { get; set; }
    }
}
