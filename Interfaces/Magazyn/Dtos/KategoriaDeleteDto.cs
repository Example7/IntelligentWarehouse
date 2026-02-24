using KategoriaEntity = Data.Data.Magazyn.Kategoria;

namespace Interfaces.Magazyn.Dtos
{
    public class KategoriaDeleteDto
    {
        public KategoriaEntity Kategoria { get; set; } = null!;
        public int LiczbaProduktow { get; set; }
        public int LiczbaPodkategorii { get; set; }
        public bool CzyMoznaUsunac { get; set; }
    }
}
