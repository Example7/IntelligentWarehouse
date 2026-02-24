using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class KlientDeleteDto
    {
        public Klient Klient { get; set; } = null!;
        public int LiczbaDokumentowWz { get; set; }
        public bool CzyMoznaUsunac => LiczbaDokumentowWz == 0;
    }
}
