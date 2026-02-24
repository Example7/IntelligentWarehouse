using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class PozycjaWZIndexDto
    {
        public string? SearchTerm { get; set; }
        public IList<PozycjaWZ> Items { get; set; } = new List<PozycjaWZ>();
    }
}
