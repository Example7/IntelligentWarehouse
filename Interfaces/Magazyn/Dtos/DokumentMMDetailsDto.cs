using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class DokumentMMDetailsDto
    {
        public DokumentMM Dokument { get; set; } = null!;
        public int LiczbaPozycji { get; set; }
        public decimal SumaIlosci { get; set; }
        public IList<PozycjaMM> Pozycje { get; set; } = new List<PozycjaMM>();
    }
}
