using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class KlientDetailsDto
    {
        public Klient Klient { get; set; } = null!;
        public int LiczbaDokumentowWz { get; set; }
        public int LiczbaDokumentowWzPosted { get; set; }
        public int LiczbaDokumentowWzDraft { get; set; }
        public IList<DokumentWZ> OstatnieDokumentyWz { get; set; } = new List<DokumentWZ>();
    }
}
