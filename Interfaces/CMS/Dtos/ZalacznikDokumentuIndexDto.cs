namespace Interfaces.CMS.Dtos
{
    public class ZalacznikDokumentuIndexDto
    {
        public string SearchTerm { get; set; } = string.Empty;

        public IList<ZalacznikDokumentuIndexItemDto> Items { get; set; } = new List<ZalacznikDokumentuIndexItemDto>();
    }
}
