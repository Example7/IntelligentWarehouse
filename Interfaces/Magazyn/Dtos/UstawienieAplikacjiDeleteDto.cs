using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class UstawienieAplikacjiDeleteDto
    {
        public UstawienieAplikacji Ustawienie { get; set; } = null!;
        public string? ZmienilEmail { get; set; }
    }
}
