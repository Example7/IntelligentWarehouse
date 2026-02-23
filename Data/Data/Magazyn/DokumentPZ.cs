using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.Magazyn
{
    [Table("GoodsReceipts")]
    public class DokumentPZ
    {
        [Key]
        [Column("ReceiptId")]
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

        [Required]
        [Column("SupplierId")]
        public int IdDostawcy { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdDostawcy))]
        public Dostawca Dostawca { get; set; } = null!;

        [Required, MaxLength(20)]
        [Column("Status")]
        public string Status { get; set; } = "Draft"; // Draft/Posted/Cancelled

        [Column("ReceivedAt")]
        public DateTime DataPrzyjeciaUtc { get; set; } = DateTime.UtcNow;

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
        public ICollection<PozycjaPZ> Pozycje { get; set; } = new List<PozycjaPZ>();
    }
}