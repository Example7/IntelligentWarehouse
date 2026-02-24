using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class InwentaryzacjaIndexDto
    {
        public string? SearchTerm { get; set; }
        public IList<Inwentaryzacja> Items { get; set; } = new List<Inwentaryzacja>();
    }
}
