using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class RolaDetailsDto
    {
        public Rola Rola { get; set; } = null!;
        public int LiczbaPrzypisan { get; set; }
        public IList<UzytkownikRola> OstatniePrzypisania { get; set; } = new List<UzytkownikRola>();
    }
}
