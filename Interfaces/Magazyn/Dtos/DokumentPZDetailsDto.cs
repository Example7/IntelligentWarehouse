using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class DokumentPZDetailsDto
    {
        public DokumentPZ Dokument { get; set; } = null!;
        public int LiczbaPozycji { get; set; }
        public decimal SumaIlosci { get; set; }
        public decimal SumaWartosci { get; set; }
        public IList<PozycjaPZ> Pozycje { get; set; } = new List<PozycjaPZ>();
    }
}
