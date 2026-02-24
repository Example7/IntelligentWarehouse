using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class PozycjaPZDeleteDto
    {
        public PozycjaPZ Pozycja { get; set; } = null!;
        public decimal Wartosc { get; set; }
        public bool CzyDokumentDraft { get; set; }
        public bool CzyMoznaUsunac => CzyDokumentDraft;
        public string? PowodBlokady { get; set; }
    }
}
