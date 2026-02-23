using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.Magazyn
{
    [Table("Categories")]
    public class Kategoria
    {
        [Key]
        [Column("CategoryId")]
        public int IdKategorii { get; set; }

        [Column("ParentCategoryId")]
        public int? IdNadrzednejKategorii { get; set; }  // self-FK

        [Required]
        [MaxLength(200)]
        [Column("Name")]
        public required string Nazwa { get; set; }

        [MaxLength(500)]
        [Column("Path")]
        public string? Sciezka { get; set; }

        public Kategoria? NadrzednaKategoria { get; set; }
        public ICollection<Kategoria> Podkategorie { get; set; } = new List<Kategoria>();

        public ICollection<Produkt> Produkty { get; set; } = new List<Produkt>();
    }
}
