using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class ProduktJednostkaDeleteDto
    {
        public ProduktJednostka ProduktJednostka { get; set; } = null!;
        public int LiczbaJednostekProduktu { get; set; }
    }
}
