using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.Magazyn
{
    [Table("Warehouses")]
    public class Magazyn
    {
        [Key]
        [Column("WarehouseId")]
        public int IdMagazynu { get; set; }

        [Required, MaxLength(150)]
        [Column("Name")]
        public string Nazwa { get; set; } = null!;

        [MaxLength(300)]
        [Column("Address")]
        public string? Adres { get; set; }

        [Column("IsActive")]
        public bool CzyAktywny { get; set; } = true;

        [ValidateNever]
        public ICollection<Lokacja> Lokacje { get; set; } = new List<Lokacja>();
    }
}
