using ProduktEntity = Data.Data.Magazyn.Produkt;

namespace Interfaces.Magazyn.Dtos
{
    public class ProduktDeleteDto
    {
        public ProduktEntity Produkt { get; set; } = null!;

        public int LiczbaStanowMagazynowych { get; set; }
        public int LiczbaPozycjiPz { get; set; }
        public int LiczbaPozycjiWz { get; set; }
        public int LiczbaPozycjiMm { get; set; }
        public int LiczbaPozycjiInwentaryzacji { get; set; }
        public int LiczbaPozycjiRezerwacji { get; set; }
        public int LiczbaRuchowMagazynowych { get; set; }
        public int LiczbaAlertow { get; set; }
        public int LiczbaRegulAlertow { get; set; }
        public int LiczbaKodowProduktu { get; set; }
        public int LiczbaJednostekProduktu { get; set; }
        public int LiczbaPartii { get; set; }

        public bool CzyMoznaUsunac =>
            LiczbaStanowMagazynowych == 0 &&
            LiczbaPozycjiPz == 0 &&
            LiczbaPozycjiWz == 0 &&
            LiczbaPozycjiMm == 0 &&
            LiczbaPozycjiInwentaryzacji == 0 &&
            LiczbaPozycjiRezerwacji == 0 &&
            LiczbaRuchowMagazynowych == 0 &&
            LiczbaAlertow == 0 &&
            LiczbaRegulAlertow == 0 &&
            LiczbaKodowProduktu == 0 &&
            LiczbaJednostekProduktu == 0 &&
            LiczbaPartii == 0;
    }
}
