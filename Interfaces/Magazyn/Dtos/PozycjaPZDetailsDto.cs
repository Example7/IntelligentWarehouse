using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class PozycjaPZDetailsDto
    {
        public PozycjaPZ Pozycja { get; set; } = null!;
        public decimal Wartosc { get; set; }
        public bool CzyDokumentDraft { get; set; }
    }
}
