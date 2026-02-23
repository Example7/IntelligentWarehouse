using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.Magazyn
{
    [Table("ReservationItems")]
    public class PozycjaRezerwacji
    {
        [Key]
        [Column("ReservationItemId")]
        public int Id { get; set; }

        [Required]
        [Column("ReservationId")]
        public int IdRezerwacji { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdRezerwacji))]
        public Rezerwacja Rezerwacja { get; set; } = null!;

        [Required]
        [Column("LineNo")]
        public int Lp { get; set; }

        [Required]
        [Column("ProductId")]
        public int IdProduktu { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdProduktu))]
        public Produkt Produkt { get; set; } = null!;

        [Column("LocationId")]
        public int? IdLokacji { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdLokacji))]
        public Lokacja? Lokacja { get; set; }

        [Required]
        [Column("Quantity", TypeName = "decimal(18,3)")]
        public decimal Ilosc { get; set; }
    }
}