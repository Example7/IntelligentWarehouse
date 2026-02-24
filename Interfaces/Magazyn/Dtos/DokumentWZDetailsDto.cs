using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class DokumentWZDetailsDto
    {
        public DokumentWZ Dokument { get; set; } = null!;
        public int LiczbaPozycji { get; set; }
        public decimal SumaIlosci { get; set; }
        public IList<PozycjaWZ> Pozycje { get; set; } = new List<PozycjaWZ>();
    }
}
