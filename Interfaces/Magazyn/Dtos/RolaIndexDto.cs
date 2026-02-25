using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class RolaIndexDto
    {
        public string? SearchTerm { get; set; }
        public IList<RolaIndexItemDto> Items { get; set; } = new List<RolaIndexItemDto>();
    }
}
