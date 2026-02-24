using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class PozycjaInwentaryzacjiIndexDto
    {
        public string? SearchTerm { get; set; }
        public IList<PozycjaInwentaryzacji> Items { get; set; } = new List<PozycjaInwentaryzacji>();
    }
}
