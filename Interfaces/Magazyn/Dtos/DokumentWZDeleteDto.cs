using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class DokumentWZDeleteDto
    {
        public DokumentWZ Dokument { get; set; } = null!;
        public int LiczbaPozycji { get; set; }
        public bool CzyMoznaUsunac => LiczbaPozycji == 0;
    }
}
