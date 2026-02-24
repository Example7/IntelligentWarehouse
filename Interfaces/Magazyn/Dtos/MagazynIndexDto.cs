namespace Interfaces.Magazyn.Dtos
{
    public class MagazynIndexDto
    {
        public string? SearchTerm { get; set; }
        public IList<MagazynIndexItemDto> Items { get; set; } = new List<MagazynIndexItemDto>();
    }
}
