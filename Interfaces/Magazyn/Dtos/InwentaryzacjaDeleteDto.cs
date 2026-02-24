using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class InwentaryzacjaDeleteDto
    {
        public Inwentaryzacja Dokument { get; set; } = null!;
        public int LiczbaPozycji { get; set; }
        public bool CzyMoznaUsunac => LiczbaPozycji == 0;
    }
}
