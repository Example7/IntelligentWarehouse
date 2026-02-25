using System.Globalization;

namespace Interfaces.Magazyn.Dtos
{
    public class RaportStanyMagazynoweSummaryDto
    {
        public string Jednostka { get; set; } = "j.m.";
        public decimal Ilosc { get; set; }
        public decimal ZarezerwowaneLokacyjnie { get; set; }
        public decimal Dostepne { get; set; }

        public string Etykieta
            => $"{Ilosc.ToString("0.###", CultureInfo.InvariantCulture)} {Jednostka}";
    }
}
