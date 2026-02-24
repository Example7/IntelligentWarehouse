using Data.Data.Magazyn;
using JednostkaMiaryEntity = Data.Data.Magazyn.JednostkaMiary;

namespace Interfaces.Magazyn.Dtos
{
    public class JednostkaMiaryDetailsDto
    {
        public JednostkaMiaryEntity JednostkaMiary { get; set; } = null!;
        public int LiczbaProduktow { get; set; }
        public IList<Produkt> ProduktyJednostki { get; set; } = new List<Produkt>();
    }
}
