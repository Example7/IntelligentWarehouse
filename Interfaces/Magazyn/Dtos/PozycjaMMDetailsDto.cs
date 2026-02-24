using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class PozycjaMMDetailsDto
    {
        public PozycjaMM Pozycja { get; set; } = null!;
        public bool CzyDokumentDraft { get; set; }
    }
}
