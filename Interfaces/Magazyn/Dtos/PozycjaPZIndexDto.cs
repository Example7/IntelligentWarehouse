using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class PozycjaPZIndexDto
    {
        public string? SearchTerm { get; set; }
        public IList<PozycjaPZ> Items { get; set; } = new List<PozycjaPZ>();
    }
}
