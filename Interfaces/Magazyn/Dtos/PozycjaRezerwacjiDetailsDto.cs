using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class PozycjaRezerwacjiDetailsDto
    {
        public PozycjaRezerwacji Pozycja { get; set; } = null!;
        public bool CzyRezerwacjaEdytowalna { get; set; }
    }
}
