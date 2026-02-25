using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class UstawienieAplikacjiIndexItemDto
    {
        public UstawienieAplikacji Ustawienie { get; set; } = null!;
        public string? ZmienilEmail { get; set; }
    }
}
