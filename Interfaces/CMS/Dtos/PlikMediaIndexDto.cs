namespace Interfaces.CMS.Dtos
{
    public class PlikMediaIndexDto
    {
        public string SearchTerm { get; set; } = string.Empty;

        public long SumaRozmiaruBajty { get; set; }

        public IList<PlikMediaIndexItemDto> Items { get; set; } = new List<PlikMediaIndexItemDto>();
    }
}
