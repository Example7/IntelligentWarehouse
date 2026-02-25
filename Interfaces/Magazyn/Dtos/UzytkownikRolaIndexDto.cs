using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class UzytkownikRolaIndexDto
    {
        public string? SearchTerm { get; set; }
        public IList<UzytkownikRola> Items { get; set; } = new List<UzytkownikRola>();
    }
}
