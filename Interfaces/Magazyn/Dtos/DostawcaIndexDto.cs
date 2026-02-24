namespace Interfaces.Magazyn.Dtos
{
    public class DostawcaIndexDto
    {
        public string? SearchTerm { get; set; }
        public IList<DostawcaIndexItemDto> Items { get; set; } = new List<DostawcaIndexItemDto>();
    }
}
