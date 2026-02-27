using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class LogAudytuDetailsDto
    {
        public LogAudytu Log { get; set; } = null!;
        public IList<LogAudytuChangeDto> Changes { get; set; } = new List<LogAudytuChangeDto>();
    }
}
