using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class RolaIndexItemDto
    {
        public Rola Rola { get; set; } = null!;
        public int LiczbaUzytkownikow { get; set; }
    }
}
