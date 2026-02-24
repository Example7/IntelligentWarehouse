using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class RezerwacjaIndexDto
    {
        public string? SearchTerm { get; set; }
        public IList<Rezerwacja> Items { get; set; } = new List<Rezerwacja>();
    }
}
