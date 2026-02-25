namespace Interfaces.CMS.Dtos
{
    public class StronaIndexDto
    {
        public string SearchTerm { get; set; } = string.Empty;

        public IList<StronaIndexItemDto> Items { get; set; } = new List<StronaIndexItemDto>();
    }
}
