namespace Interfaces.Magazyn.Dtos
{
    public class RaportPropozycjeZamowienDto
    {
        public string? SearchTerm { get; set; }
        public int? IdMagazynu { get; set; }
        public DateTime WygenerowanoUtc { get; set; }
        public IList<RaportMagazynSelectOptionDto> Magazyny { get; set; } = new List<RaportMagazynSelectOptionDto>();
        public IList<RaportPropozycjeZamowienRowDto> Rows { get; set; } = new List<RaportPropozycjeZamowienRowDto>();
    }

    public class RaportPropozycjeZamowienRowDto
    {
        public int IdProduktu { get; set; }
        public string ProduktKod { get; set; } = string.Empty;
        public string ProduktNazwa { get; set; } = string.Empty;
        public string KategoriaNazwa { get; set; } = string.Empty;
        public string Jednostka { get; set; } = string.Empty;
        public int IdMagazynu { get; set; }
        public string MagazynNazwa { get; set; } = string.Empty;
        public decimal StanFizyczny { get; set; }
        public decimal ZarezerwowaneAktywnie { get; set; }
        public decimal ZarezerwowaneWzDraft { get; set; }
        public decimal DostepneDoRezerwacji { get; set; }
        public decimal StanMinimalny { get; set; }
        public decimal PunktPonownegoZamowienia { get; set; }
        public decimal IloscPonownegoZamowienia { get; set; }
        public decimal ProponowanaIloscZamowienia { get; set; }
        public decimal BrakDoRop { get; set; }
    }
}
