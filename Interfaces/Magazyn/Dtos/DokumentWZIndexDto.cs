using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class DokumentWZIndexDto
    {
        public string? SearchTerm { get; set; }
        public IList<DokumentWZ> Items { get; set; } = new List<DokumentWZ>();
    }
}
