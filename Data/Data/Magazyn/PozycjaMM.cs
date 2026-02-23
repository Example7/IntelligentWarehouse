using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.Magazyn
{
    [Table("StockTransferItems")]
    public class PozycjaMM
    {
        [Key]
        [Column("TransferItemId")]
        public int Id { get; set; }

        [Required]
        [Column("TransferId")]
        public int IdDokumentu { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdDokumentu))]
        public DokumentMM Dokument { get; set; } = null!;

        [Required]
        [Column("LineNo")]
        public int Lp { get; set; }

        [Required]
        [Column("ProductId")]
        public int IdProduktu { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdProduktu))]
        public Produkt Produkt { get; set; } = null!;

        [Required]
        [Column("FromLocationId")]
        public int IdLokacjiZ { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdLokacjiZ))]
        public Lokacja LokacjaZ { get; set; } = null!;

        [Required]
        [Column("ToLocationId")]
        public int IdLokacjiDo { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdLokacjiDo))]
        public Lokacja LokacjaDo { get; set; } = null!;

        [Required]
        [Column("Quantity", TypeName = "decimal(18,3)")]
        public decimal Ilosc { get; set; }
    }
}