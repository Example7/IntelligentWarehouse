namespace Interfaces.Magazyn.Dtos
{
    public class StanMagazynowyUomSummaryDto
    {
        public string Jednostka { get; set; } = "j.m.";
        public decimal Ilosc { get; set; }
        public string Etykieta => $"{Ilosc.ToString("0.###")} {Jednostka}";
    }
}
