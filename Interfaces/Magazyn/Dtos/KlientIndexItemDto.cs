using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class KlientIndexItemDto
    {
        public Klient Klient { get; set; } = null!;
        public int LiczbaDokumentowWz { get; set; }
    }
}
