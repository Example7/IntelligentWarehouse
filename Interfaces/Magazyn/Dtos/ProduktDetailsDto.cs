using Data.Data.Magazyn;
using ProduktEntity = Data.Data.Magazyn.Produkt;

namespace Interfaces.Magazyn.Dtos
{
    public class ProduktDetailsDto
    {
        public ProduktEntity Produkt { get; set; } = null!;

        public decimal AktualnyStan { get; set; }
        public int LiczbaLokacjiZeStanem { get; set; }
        public int LiczbaAlertow { get; set; }
        public int LiczbaNowychAlertow { get; set; }
        public int LiczbaPozycjiRezerwacji { get; set; }
        public decimal IloscZarezerwowana { get; set; }

        public IList<StanMagazynowy> StanyMagazynowe { get; set; } = new List<StanMagazynowy>();
        public IList<PozycjaRezerwacji> OstatniePozycjeRezerwacji { get; set; } = new List<PozycjaRezerwacji>();
        public IList<PozycjaPZ> OstatniePozycjePz { get; set; } = new List<PozycjaPZ>();
        public IList<PozycjaWZ> OstatniePozycjeWz { get; set; } = new List<PozycjaWZ>();
        public IList<PozycjaMM> OstatniePozycjeMm { get; set; } = new List<PozycjaMM>();
        public IList<Alert> OstatnieAlerty { get; set; } = new List<Alert>();
    }
}
