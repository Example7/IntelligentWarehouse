using MagazynEntity = Data.Data.Magazyn.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class MagazynIndexItemDto
    {
        public MagazynEntity Magazyn { get; set; } = null!;
        public int LiczbaLokacji { get; set; }
        public int LiczbaDokumentowPz { get; set; }
        public int LiczbaDokumentowWz { get; set; }
        public int LiczbaDokumentowMm { get; set; }
        public int LiczbaRezerwacji { get; set; }
        public int LiczbaAlertow { get; set; }
    }
}
