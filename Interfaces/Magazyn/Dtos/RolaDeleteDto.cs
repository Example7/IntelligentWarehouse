using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class RolaDeleteDto
    {
        public Rola Rola { get; set; } = null!;
        public int LiczbaPrzypisan { get; set; }
        public bool CzyMoznaUsunac => LiczbaPrzypisan == 0;
    }
}
