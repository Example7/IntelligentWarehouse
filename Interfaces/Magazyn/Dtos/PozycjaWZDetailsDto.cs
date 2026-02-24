using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class PozycjaWZDetailsDto
    {
        public PozycjaWZ Pozycja { get; set; } = null!;
        public bool CzyDokumentDraft { get; set; }
    }
}
