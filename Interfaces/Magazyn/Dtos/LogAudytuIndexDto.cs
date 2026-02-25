namespace Interfaces.Magazyn.Dtos
{
    public class LogAudytuIndexDto
    {
        public string? SearchTerm { get; set; }
        public IList<LogAudytuIndexItemDto> Items { get; set; } = new List<LogAudytuIndexItemDto>();
    }
}
