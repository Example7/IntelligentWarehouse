using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class PozycjaMMDeleteDto
    {
        public PozycjaMM Pozycja { get; set; } = null!;
        public bool CzyDokumentDraft { get; set; }
        public bool CzyMoznaUsunac => CzyDokumentDraft;
        public string? PowodBlokady { get; set; }
    }
}
