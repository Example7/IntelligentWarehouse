using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class StanMagazynowyFormDto
    {
        public StanMagazynowy StanMagazynowy { get; set; } = new();
        public IList<StanMagazynowySelectOptionDto> Produkty { get; set; } = new List<StanMagazynowySelectOptionDto>();
        public IList<StanMagazynowySelectOptionDto> Lokacje { get; set; } = new List<StanMagazynowySelectOptionDto>();
        public IDictionary<int, string> ProductUomMap { get; set; } = new Dictionary<int, string>();
        public string CurrentProductUom { get; set; } = "j.m.";
        public bool IsEdit { get; set; }
        public decimal AktywneRezerwacjeQty { get; set; }
        public decimal DraftWzQty { get; set; }
        public decimal ZajeteRazemQty => AktywneRezerwacjeQty + DraftWzQty;
        public decimal DostepnePoZapisieQty => StanMagazynowy.Ilosc - ZajeteRazemQty;
        public bool HasCapacityData => StanMagazynowy.IdProduktu > 0 && StanMagazynowy.IdLokacji > 0;
    }
}
