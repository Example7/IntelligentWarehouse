using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class PartiaDeleteDto
    {
        public Partia Partia { get; set; } = null!;
        public int LiczbaPartiiProduktu { get; set; }
    }
}
