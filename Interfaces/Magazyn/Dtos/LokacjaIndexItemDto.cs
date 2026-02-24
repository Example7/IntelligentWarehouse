using Data.Data.Magazyn;
using LokacjaEntity = Data.Data.Magazyn.Lokacja;

namespace Interfaces.Magazyn.Dtos
{
    public class LokacjaIndexItemDto
    {
        public LokacjaEntity Lokacja { get; set; } = null!;
        public int LiczbaProduktow { get; set; }
        public int LiczbaRezerwacji { get; set; }
    }
}
