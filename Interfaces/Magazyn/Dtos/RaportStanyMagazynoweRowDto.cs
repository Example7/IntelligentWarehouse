namespace Interfaces.Magazyn.Dtos
{
    public class RaportStanyMagazynoweRowDto
    {
        public int IdStanu { get; set; }
        public int IdProduktu { get; set; }
        public int IdLokacji { get; set; }
        public string ProduktKod { get; set; } = "-";
        public string ProduktNazwa { get; set; } = "-";
        public string MagazynNazwa { get; set; } = "-";
        public string LokacjaKod { get; set; } = "-";
        public decimal Ilosc { get; set; }
        public decimal ZarezerwowaneLokacyjnie { get; set; }
        public decimal Dostepne { get; set; }
        public string Jednostka { get; set; } = "j.m.";
    }
}
