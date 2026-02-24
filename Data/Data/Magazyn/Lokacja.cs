using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.Magazyn
{
    [Table("Locations")]
    public class Lokacja
    {
        [Key]
        [Column("LocationId")]
        public int IdLokacji { get; set; }

        [Required]
        [Column("WarehouseId")]
        [Display(Name = "Magazyn")]
        public int IdMagazynu { get; set; }

        [ForeignKey(nameof(IdMagazynu))]
        [ValidateNever]
        public Magazyn Magazyn { get; set; } = null!;

        [Required, MaxLength(60)]
        [Column("Code")]
        [Display(Name = "Kod lokacji")]
        public string Kod { get; set; } = null!; // np. A1-02-03

        [MaxLength(150)]
        [Column("Description")]
        public string? Opis { get; set; }

        [Column("IsActive")]
        public bool CzyAktywna { get; set; } = true;

        [ValidateNever]
        public ICollection<StanMagazynowy> Stany { get; set; } = new List<StanMagazynowy>();
    }
}
