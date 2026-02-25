using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class RaportRuchyMagazynoweDto
    {
        public string? SearchTerm { get; set; }
        public int? IdMagazynu { get; set; }
        public TypRuchuMagazynowego? Typ { get; set; }
        public DateTime? DataOd { get; set; }
        public DateTime? DataDo { get; set; }
        public DateTime WygenerowanoUtc { get; set; } = DateTime.UtcNow;

        public IList<RaportMagazynSelectOptionDto> Magazyny { get; set; } = new List<RaportMagazynSelectOptionDto>();
        public IList<RaportRuchyMagazynoweRowDto> Rows { get; set; } = new List<RaportRuchyMagazynoweRowDto>();
        public IList<RaportRuchyMagazynoweTypeSummaryDto> PodsumowanieTypow { get; set; } = new List<RaportRuchyMagazynoweTypeSummaryDto>();
    }
}
