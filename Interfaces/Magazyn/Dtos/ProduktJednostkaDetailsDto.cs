using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class ProduktJednostkaDetailsDto
    {
        public ProduktJednostka ProduktJednostka { get; set; } = null!;
        public int LiczbaJednostekProduktu { get; set; }
    }
}
