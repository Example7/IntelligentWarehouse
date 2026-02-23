using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.Magazyn
{
    public enum TypRuchuMagazynowego
    {
        Przyjecie = 1,
        Wydanie = 2,
        Przesuniecie = 3,
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
        public int IdProduktu { get; set; }

        [ForeignKey(nameof(IdProduktu))]
        public Produkt Produkt { get; set; } = null!;

        [Column("FromLocationId")]
        public int? IdLokacjiZ { get; set; }

        [ForeignKey(nameof(IdLokacjiZ))]
        public Lokacja? LokacjaZ { get; set; }

        [Column("ToLocationId")]
        public int? IdLokacjiDo { get; set; }

        [ForeignKey(nameof(IdLokacjiDo))]
        public Lokacja? LokacjaDo { get; set; }

        [Required]
        [Column("Quantity", TypeName = "decimal(18,3)")]
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
        public int? IdUzytkownika { get; set; }

        [ForeignKey(nameof(IdUzytkownika))]
        public Uzytkownik? Uzytkownik { get; set; }
    }
}
