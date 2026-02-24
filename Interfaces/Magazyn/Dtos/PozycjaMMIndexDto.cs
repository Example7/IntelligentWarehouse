using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class PozycjaMMIndexDto
    {
        public string? SearchTerm { get; set; }
        public IList<PozycjaMM> Items { get; set; } = new List<PozycjaMM>();
    }
}
