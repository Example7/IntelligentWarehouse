using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.Magazyn
{
    [Table("GoodsIssues")]
    public class DokumentWZ
    {
        [Key]
        [Column("IssueId")]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        [Column("DocumentNo")]
        public string Numer { get; set; } = null!;

        [Required]
        [Column("WarehouseId")]
        public int IdMagazynu { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdMagazynu))]
        public Magazyn Magazyn { get; set; } = null!;

        [Column("CustomerId")]
        public int? IdKlienta { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdKlienta))]
        public Klient? Klient { get; set; }

        [Required, MaxLength(20)]
        [Column("Status")]
        public string Status { get; set; } = "Draft";

        [Column("IssuedAt")]
        public DateTime DataWydaniaUtc { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("CreatedByUserId")]
        public int IdUtworzyl { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdUtworzyl))]
        public Uzytkownik Utworzyl { get; set; } = null!;

        [Column("PostedAt")]
        [Display(Name = "Zaksięgowano")]
        public DateTime? ZaksiegowanoUtc { get; set; }

        [MaxLength(500)]
        [Column("Note")]
        public string? Notatka { get; set; }

        [Timestamp]
        [ValidateNever]
        [Column("RowVersion")]
        public byte[] RowVersion { get; set; } = null!;

        [ValidateNever]
        public ICollection<PozycjaWZ> Pozycje { get; set; } = new List<PozycjaWZ>();
    }
}