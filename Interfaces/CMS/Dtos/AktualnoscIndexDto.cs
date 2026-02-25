namespace Interfaces.CMS.Dtos
{
    public class AktualnoscIndexDto
    {
        public string SearchTerm { get; set; } = string.Empty;

        public IList<AktualnoscIndexItemDto> Items { get; set; } = new List<AktualnoscIndexItemDto>();
    }
}
