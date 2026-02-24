using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class KodProduktuDeleteDto
    {
        public KodProduktu KodProduktu { get; set; } = null!;
        public int LiczbaKodowProduktu { get; set; }
    }
}
