namespace Interfaces.Magazyn.Dtos
{
    public class RaportStanyMagazynoweDto
    {
        public string? SearchTerm { get; set; }
        public int? IdMagazynu { get; set; }
        public IList<RaportMagazynSelectOptionDto> Magazyny { get; set; } = new List<RaportMagazynSelectOptionDto>();
        public IList<RaportStanyMagazynoweRowDto> Rows { get; set; } = new List<RaportStanyMagazynoweRowDto>();
        public IList<RaportStanyMagazynoweSummaryDto> SumaWgJednostki { get; set; } = new List<RaportStanyMagazynoweSummaryDto>();
        public DateTime WygenerowanoUtc { get; set; } = DateTime.UtcNow;
        public string SumaWgJednostkiLabel { get; set; } = "-";
    }
}
