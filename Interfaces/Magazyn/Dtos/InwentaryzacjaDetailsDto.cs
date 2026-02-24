using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class InwentaryzacjaDetailsDto
    {
        public Inwentaryzacja Dokument { get; set; } = null!;
        public int LiczbaPozycji { get; set; }
        public decimal SumaOczekiwana { get; set; }
        public decimal SumaPoliczona { get; set; }
        public decimal SumaRoznic { get; set; }
        public IList<PozycjaInwentaryzacji> Pozycje { get; set; } = new List<PozycjaInwentaryzacji>();
    }
}
