namespace Interfaces.Magazyn.Dtos
{
    public class KlientIndexDto
    {
        public string? SearchTerm { get; set; }
        public IList<KlientIndexItemDto> Items { get; set; } = new List<KlientIndexItemDto>();
    }
}
