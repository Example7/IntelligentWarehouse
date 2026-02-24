using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class ProduktJednostkaIndexDto
    {
        public string? SearchTerm { get; set; }
        public List<ProduktJednostka> Items { get; set; } = new();
    }
}
