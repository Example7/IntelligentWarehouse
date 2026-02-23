using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.Magazyn
{
    [Table("Roles")]
    public class Rola
    {
        [Key]
        [Column("RoleId")]
        public int IdRoli { get; set; }

        [Required, MaxLength(50)]
        [Column("Name")]
        public string Nazwa { get; set; } = null!; // Admin, Worker

        public ICollection<UzytkownikRola> Uzytkownicy { get; set; } = new List<UzytkownikRola>();
    }
}
