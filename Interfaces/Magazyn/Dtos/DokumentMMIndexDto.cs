using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class DokumentMMIndexDto
    {
        public string? SearchTerm { get; set; }
        public IList<DokumentMM> Items { get; set; } = new List<DokumentMM>();
    }
}
