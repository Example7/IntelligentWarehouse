using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

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
        [ValidateNever]
        public Uzytkownik Uzytkownik { get; set; } = null!;

        [Required]
        [Column("RoleId")]
        public int IdRoli { get; set; }

        [ForeignKey(nameof(IdRoli))]
        [ValidateNever]
        public Rola Rola { get; set; } = null!;
    }
}
