using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class AlertDeleteDto
    {
        public Alert Alert { get; set; } = null!;
        public string? PotwierdzilEmail { get; set; }
    }
}
