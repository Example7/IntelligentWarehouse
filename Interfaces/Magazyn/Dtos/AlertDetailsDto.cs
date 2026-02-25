using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class AlertDetailsDto
    {
        public Alert Alert { get; set; } = null!;
        public string? PotwierdzilEmail { get; set; }
    }
}
