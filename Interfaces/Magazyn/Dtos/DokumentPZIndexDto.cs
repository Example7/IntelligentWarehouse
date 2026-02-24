using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class DokumentPZIndexDto
    {
        public string? SearchTerm { get; set; }
        public IList<DokumentPZ> Items { get; set; } = new List<DokumentPZ>();
    }
}
