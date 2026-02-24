using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class KodProduktuIndexDto
    {
        public string? SearchTerm { get; set; }
        public List<KodProduktu> Items { get; set; } = new();
    }
}
