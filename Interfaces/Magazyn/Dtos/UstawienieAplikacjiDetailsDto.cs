using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class UstawienieAplikacjiDetailsDto
    {
        public UstawienieAplikacji Ustawienie { get; set; } = null!;
        public string? ZmienilEmail { get; set; }
    }
}
