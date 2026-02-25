using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class UzytkownikDetailsDto
    {
        public Uzytkownik Uzytkownik { get; set; } = null!;
        public IList<string> RoleNazwy { get; set; } = new List<string>();
        public int LiczbaRol { get; set; }
        public int LiczbaKlientow { get; set; }
        public int LiczbaDokumentowPz { get; set; }
        public int LiczbaDokumentowWz { get; set; }
        public int LiczbaDokumentowMm { get; set; }
        public int LiczbaInwentaryzacji { get; set; }
        public int LiczbaRezerwacji { get; set; }
        public int LiczbaRuchow { get; set; }
        public int LiczbaLogowAudytu { get; set; }

        public IList<DokumentPZ> OstatnieDokumentyPz { get; set; } = new List<DokumentPZ>();
        public IList<DokumentWZ> OstatnieDokumentyWz { get; set; } = new List<DokumentWZ>();
        public IList<DokumentMM> OstatnieDokumentyMm { get; set; } = new List<DokumentMM>();
        public IList<RuchMagazynowy> OstatnieRuchy { get; set; } = new List<RuchMagazynowy>();
        public IList<LogAudytu> OstatnieLogiAudytu { get; set; } = new List<LogAudytu>();
    }
}
