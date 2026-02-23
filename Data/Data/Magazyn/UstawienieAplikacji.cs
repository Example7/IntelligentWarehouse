using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.Magazyn
{
    [Table("AppSettings")]
    public class UstawienieAplikacji
    {
        [Key]
        [MaxLength(120)]
        [Column("Key")]
        public string Klucz { get; set; } = null!;

        [Required]
        [Column("Value")]
        public string Wartosc { get; set; } = null!;

        [MaxLength(300)]
        [Column("Description")]
        public string? Opis { get; set; }

        [Column("UpdatedAt")]
        public DateTime ZmienionoUtc { get; set; } = DateTime.UtcNow;

        [Column("UpdatedByUserId")]
        public int? ZmienilUserId { get; set; }
    }
}