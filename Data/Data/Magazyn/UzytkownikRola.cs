using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.Magazyn
{
    [Table("UserRoles")]
    public class UzytkownikRola
    {
        [Key]
        [Column("UserRoleId")]
        public int Id { get; set; }

        [Required]
        [Column("UserId")]
        public int IdUzytkownika { get; set; }

        [ForeignKey(nameof(IdUzytkownika))]
        public Uzytkownik Uzytkownik { get; set; } = null!;

        [Required]
        [Column("RoleId")]
        public int IdRoli { get; set; }

        [ForeignKey(nameof(IdRoli))]
        public Rola Rola { get; set; } = null!;
    }
}
