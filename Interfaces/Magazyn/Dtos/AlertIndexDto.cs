namespace Interfaces.Magazyn.Dtos
{
    public class AlertIndexDto
    {
        public string? SearchTerm { get; set; }
        public IList<AlertIndexItemDto> Items { get; set; } = new List<AlertIndexItemDto>();
    }
}
