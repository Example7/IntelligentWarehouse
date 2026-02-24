using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class PozycjaRezerwacjiDeleteDto
    {
        public PozycjaRezerwacji Pozycja { get; set; } = null!;
        public bool CzyRezerwacjaEdytowalna { get; set; }
        public bool CzyMoznaUsunac => CzyRezerwacjaEdytowalna;
        public string? PowodBlokady { get; set; }
    }
}
