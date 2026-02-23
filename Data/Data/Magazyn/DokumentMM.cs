using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.Magazyn
{
    [Table("StockTransfers")]
    public class DokumentMM
    {
        [Key]
        [Column("TransferId")]
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

        [Required, MaxLength(20)]
        [Column("Status")]
        public string Status { get; set; } = "Draft";

        [Column("TransferredAt")]
        public DateTime DataUtc { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("CreatedByUserId")]
        public int IdUtworzyl { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdUtworzyl))]
        public Uzytkownik Utworzyl { get; set; } = null!;

        [Column("PostedAt")]
        public DateTime? ZaksiegowanoUtc { get; set; }

        [MaxLength(500)]
        [Column("Note")]
        public string? Notatka { get; set; }

        [Timestamp]
        [ValidateNever]
        [Column("RowVersion")]
        public byte[] RowVersion { get; set; } = null!;

        [ValidateNever]
        public ICollection<PozycjaMM> Pozycje { get; set; } = new List<PozycjaMM>();
    }
}