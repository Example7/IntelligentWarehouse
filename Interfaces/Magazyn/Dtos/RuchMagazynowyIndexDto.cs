using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class RuchMagazynowyIndexDto
    {
        public string? SearchTerm { get; set; }
        public IList<RuchMagazynowy> Items { get; set; } = new List<RuchMagazynowy>();
    }
}
