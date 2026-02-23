using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.Magazyn
{
    [Table("AuditLog")]
    public class LogAudytu
    {
        [Key]
        [Column("AuditId")]
        public long Id { get; set; }

        [Column("UserId")]
        public int? UserId { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(UserId))]
        public Uzytkownik? Uzytkownik { get; set; }

        [Required, MaxLength(30)]
        [Column("Action")]
        public string Akcja { get; set; } = null!; // CREATE/UPDATE/DELETE/LOGIN...

        [Required, MaxLength(100)]
        [Column("EntityName")]
        public string Encja { get; set; } = null!;

        [MaxLength(80)]
        [Column("EntityId")]
        public string? IdEncji { get; set; }

        [Column("At")]
        public DateTime KiedyUtc { get; set; } = DateTime.UtcNow;

        [Column("OldValuesJson")]
        public string? StareJson { get; set; }

        [Column("NewValuesJson")]
        public string? NoweJson { get; set; }
    }
}