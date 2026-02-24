using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.Magazyn
{
    public enum TypRuchuMagazynowego
    {
        Przyjęcie = 1,
        Wydanie = 2,
        Przesunięcie = 3,
        Korekta = 4,
        Inwentaryzacja = 5
    }

    [Table("StockMovements")]
    public class RuchMagazynowy
    {
        [Key]
        [Column("StockMovementId")]
        public int IdRuchu { get; set; }

        [Required]
        [Column("MovementType")]
        public TypRuchuMagazynowego Typ { get; set; }

        [Required]
        [Column("ProductId")]
        [Display(Name = "Produkt")]
        public int IdProduktu { get; set; }

        [ForeignKey(nameof(IdProduktu))]
        [ValidateNever]
        public Produkt Produkt { get; set; } = null!;

        [Column("FromLocationId")]
        [Display(Name = "Z lokacji")]
        public int? IdLokacjiZ { get; set; }

        [ForeignKey(nameof(IdLokacjiZ))]
        [ValidateNever]
        public Lokacja? LokacjaZ { get; set; }

        [Column("ToLocationId")]
        [Display(Name = "Do lokacji")]
        public int? IdLokacjiDo { get; set; }

        [ForeignKey(nameof(IdLokacjiDo))]
        [ValidateNever]
        public Lokacja? LokacjaDo { get; set; }

        [Required]
        [Column("Quantity", TypeName = "decimal(18,3)")]
        [Display(Name = "Ilość")]
        public decimal Ilosc { get; set; }

        [MaxLength(200)]
        [Column("Reference")]
        public string? Referencja { get; set; } // np. numer PZ/WZ/TRANSFER

        [MaxLength(500)]
        [Column("Note")]
        public string? Notatka { get; set; }

        [Required]
        [Column("CreatedAt")]
        public DateTime UtworzonoUtc { get; set; } = DateTime.UtcNow;

        [Column("CreatedByUserId")]
        [Display(Name = "Użytkownik")]
        public int? IdUzytkownika { get; set; }

        [ForeignKey(nameof(IdUzytkownika))]
        [ValidateNever]
        public Uzytkownik? Uzytkownik { get; set; }
    }
}
