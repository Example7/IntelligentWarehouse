using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class PozycjaInwentaryzacjiDetailsDto
    {
        public PozycjaInwentaryzacji Pozycja { get; set; } = null!;
        public bool CzyDokumentDraft { get; set; }
    }
}
