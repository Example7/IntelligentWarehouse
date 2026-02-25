namespace Interfaces.Magazyn.Dtos
{
    public class RegulaAlertuIndexDto
    {
        public string? SearchTerm { get; set; }
        public IList<RegulaAlertuIndexItemDto> Items { get; set; } = new List<RegulaAlertuIndexItemDto>();
    }
}
