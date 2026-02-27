using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.Magazyn
{
    [Table("Users")]
    public class Uzytkownik
    {
        [Key]
        [Column("UserId")]
        public int IdUzytkownika { get; set; }

        [Required, MaxLength(80)]
        [Column("Username")]
        public string Login { get; set; } = null!;

        [Required, MaxLength(200)]
        [Column("PasswordHash")]
        [Display(Name = "Hasło")]
        public string HashHasla { get; set; } = null!;

        [Required, MaxLength(120)]
        [Column("Email")]
        public string Email { get; set; } = null!;

        [Column("IsActive")]
        public bool CzyAktywny { get; set; } = true;

        public ICollection<UzytkownikRola> RoleUzytkownika { get; set; } = new List<UzytkownikRola>();
        public ICollection<Klient> Klienci { get; set; } = new List<Klient>();
    }
}
