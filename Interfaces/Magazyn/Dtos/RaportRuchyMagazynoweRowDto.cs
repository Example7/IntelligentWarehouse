using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class RaportRuchyMagazynoweRowDto
    {
        public int IdRuchu { get; set; }
        public TypRuchuMagazynowego Typ { get; set; }
        public int IdProduktu { get; set; }
        public string ProduktKod { get; set; } = "-";
        public string ProduktNazwa { get; set; } = "-";
        public string Jednostka { get; set; } = "j.m.";
        public decimal Ilosc { get; set; }
        public int? IdLokacjiZ { get; set; }
        public string LokacjaZKod { get; set; } = "-";
        public string MagazynZNazwa { get; set; } = "-";
        public int? IdLokacjiDo { get; set; }
        public string LokacjaDoKod { get; set; } = "-";
        public string MagazynDoNazwa { get; set; } = "-";
        public string? Referencja { get; set; }
        public string? Notatka { get; set; }
        public DateTime UtworzonoUtc { get; set; }
        public string UzytkownikLabel { get; set; } = "-";
    }
}
