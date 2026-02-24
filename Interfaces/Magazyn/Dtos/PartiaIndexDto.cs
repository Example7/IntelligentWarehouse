using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class PartiaIndexDto
    {
        public string? SearchTerm { get; set; }
        public List<Partia> Items { get; set; } = new();
    }
}
