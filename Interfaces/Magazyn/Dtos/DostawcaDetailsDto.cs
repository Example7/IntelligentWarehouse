using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class DostawcaDetailsDto
    {
        public Dostawca Dostawca { get; set; } = null!;
        public int LiczbaDokumentowPz { get; set; }
        public int LiczbaDokumentowPzPosted { get; set; }
        public int LiczbaDokumentowPzDraft { get; set; }
        public int LiczbaPartii { get; set; }
        public IList<DokumentPZ> OstatnieDokumentyPz { get; set; } = new List<DokumentPZ>();
        public IList<Partia> OstatniePartie { get; set; } = new List<Partia>();
    }
}
