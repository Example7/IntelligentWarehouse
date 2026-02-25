using System.Collections.Generic;

namespace Interfaces.Magazyn.Dtos
{
    public class UzytkownikIndexDto
    {
        public string? SearchTerm { get; set; }
        public IList<UzytkownikIndexItemDto> Items { get; set; } = new List<UzytkownikIndexItemDto>();
    }
}
