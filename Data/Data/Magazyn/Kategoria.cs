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
        [Display(Name = "Kategoria nadrzędna")]
        public int? IdNadrzednejKategorii { get; set; }  // self-FK

        [Required]
        [MaxLength(200)]
        [Column("Name")]
        public required string Nazwa { get; set; }

        [MaxLength(500)]
        [Column("Path")]
        [Display(Name = "Ścieżka")]
        public string? Sciezka { get; set; }

        [ForeignKey(nameof(IdNadrzednejKategorii))]
        public Kategoria? NadrzednaKategoria { get; set; }
        public ICollection<Kategoria> Podkategorie { get; set; } = new List<Kategoria>();

        public ICollection<Produkt> Produkty { get; set; } = new List<Produkt>();
    }
}
