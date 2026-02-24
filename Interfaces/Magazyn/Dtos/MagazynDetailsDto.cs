using Data.Data.Magazyn;
using MagazynEntity = Data.Data.Magazyn.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class MagazynDetailsDto
    {
        public MagazynEntity Magazyn { get; set; } = null!;

        public int LiczbaLokacji { get; set; }
        public int LiczbaDokumentowPz { get; set; }
        public int LiczbaDokumentowWz { get; set; }
        public int LiczbaDokumentowMm { get; set; }
        public int LiczbaInwentaryzacji { get; set; }
        public int LiczbaRezerwacji { get; set; }
        public int LiczbaRegulAlertow { get; set; }
        public int LiczbaAlertow { get; set; }

        public IList<Lokacja> LokacjeMagazynu { get; set; } = new List<Lokacja>();
        public IList<RegulaAlertu> RegulyAlertowMagazynu { get; set; } = new List<RegulaAlertu>();
        public IList<Alert> OstatnieAlertyMagazynu { get; set; } = new List<Alert>();
        public IList<DokumentPZ> OstatnieDokumentyPz { get; set; } = new List<DokumentPZ>();
        public IList<DokumentWZ> OstatnieDokumentyWz { get; set; } = new List<DokumentWZ>();
        public IList<DokumentMM> OstatnieDokumentyMm { get; set; } = new List<DokumentMM>();
        public IList<Rezerwacja> OstatnieRezerwacjeMagazynu { get; set; } = new List<Rezerwacja>();
        public IList<Inwentaryzacja> OstatnieInwentaryzacjeMagazynu { get; set; } = new List<Inwentaryzacja>();
    }
}
