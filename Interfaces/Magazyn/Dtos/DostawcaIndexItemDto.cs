using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class DostawcaIndexItemDto
    {
        public Dostawca Dostawca { get; set; } = null!;
        public int LiczbaDokumentowPz { get; set; }
        public int LiczbaPartii { get; set; }
    }
}
