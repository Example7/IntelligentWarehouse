using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class DostawcaDeleteDto
    {
        public Dostawca Dostawca { get; set; } = null!;
        public int LiczbaDokumentowPz { get; set; }
        public int LiczbaPartii { get; set; }
        public bool CzyMoznaUsunac => LiczbaDokumentowPz == 0 && LiczbaPartii == 0;
    }
}
