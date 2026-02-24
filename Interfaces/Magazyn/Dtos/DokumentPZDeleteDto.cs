using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class DokumentPZDeleteDto
    {
        public DokumentPZ Dokument { get; set; } = null!;
        public int LiczbaPozycji { get; set; }
        public bool CzyMoznaUsunac => LiczbaPozycji == 0;
    }
}
