using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.Magazyn
{
    [Table("UnitsOfMeasure")]
    public class JednostkaMiary
    {
        [Key]
        [Column("UomId")]
        public int IdJednostki { get; set; }

        [Required, MaxLength(20)]
        [Column("Code")]
        public string Kod { get; set; } = null!; // np. szt, kg, m

        [Required, MaxLength(100)]
        [Column("Name")]
        public string Nazwa { get; set; } = null!;

        [Column("IsActive")]
        public bool CzyAktywna { get; set; } = true;
    }
}
