using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class PozycjaRezerwacjiIndexDto
    {
        public string? SearchTerm { get; set; }
        public IList<PozycjaRezerwacji> Items { get; set; } = new List<PozycjaRezerwacji>();
    }
}
