namespace Interfaces.Magazyn.Dtos
{
    public class UstawienieAplikacjiIndexDto
    {
        public string? SearchTerm { get; set; }
        public IList<UstawienieAplikacjiIndexItemDto> Items { get; set; } = new List<UstawienieAplikacjiIndexItemDto>();
    }
}
