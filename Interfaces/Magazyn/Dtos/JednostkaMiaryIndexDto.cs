namespace Interfaces.Magazyn.Dtos
{
    public class JednostkaMiaryIndexDto
    {
        public string? SearchTerm { get; set; }
        public IList<JednostkaMiaryIndexItemDto> Items { get; set; } = new List<JednostkaMiaryIndexItemDto>();
    }
}
