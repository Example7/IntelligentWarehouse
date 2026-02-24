using Data.Data.Magazyn;
using KategoriaEntity = Data.Data.Magazyn.Kategoria;

namespace Interfaces.Magazyn.Dtos
{
    public class KategoriaDetailsDto
    {
        public KategoriaEntity Kategoria { get; set; } = null!;
        public int LiczbaProduktow { get; set; }
        public int LiczbaPodkategorii { get; set; }
        public IList<Produkt> ProduktyKategorii { get; set; } = new List<Produkt>();
    }
}
