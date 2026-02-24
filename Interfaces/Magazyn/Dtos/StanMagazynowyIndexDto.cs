namespace Interfaces.Magazyn.Dtos
{
    public class StanMagazynowyIndexDto
    {
        public string? SearchTerm { get; set; }
        public IList<Data.Data.Magazyn.StanMagazynowy> Items { get; set; } = new List<Data.Data.Magazyn.StanMagazynowy>();
        public IList<StanMagazynowyUomSummaryDto> SumaWgJednostki { get; set; } = new List<StanMagazynowyUomSummaryDto>();
        public string SumaWgJednostkiLabel { get; set; } = "-";
        public IDictionary<string, decimal> AktywneRezerwacjePoKluczu { get; set; } = new Dictionary<string, decimal>();

        public static string BuildKey(int idProduktu, int idLokacji) => $"{idProduktu}:{idLokacji}";
    }
}
