namespace Interfaces.Magazyn.Dtos
{
    public class LokacjaIndexDto
    {
        public string? SearchTerm { get; set; }
        public IList<LokacjaIndexItemDto> Items { get; set; } = new List<LokacjaIndexItemDto>();
    }
}
