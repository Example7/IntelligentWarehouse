using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class UzytkownikDeleteDto
    {
        public Uzytkownik Uzytkownik { get; set; } = null!;
        public int LiczbaRol { get; set; }
        public int LiczbaKlientow { get; set; }
        public int LiczbaDokumentowPz { get; set; }
        public int LiczbaDokumentowWz { get; set; }
        public int LiczbaDokumentowMm { get; set; }
        public int LiczbaInwentaryzacji { get; set; }
        public int LiczbaRezerwacji { get; set; }
        public int LiczbaRuchow { get; set; }
        public int LiczbaLogowAudytu { get; set; }

        public int LiczbaPowiazan =>
            LiczbaRol + LiczbaKlientow + LiczbaDokumentowPz + LiczbaDokumentowWz + LiczbaDokumentowMm +
            LiczbaInwentaryzacji + LiczbaRezerwacji + LiczbaRuchow + LiczbaLogowAudytu;

        public bool CzyMoznaUsunac => LiczbaPowiazan == 0;
    }
}
