using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class UzytkownikIndexItemDto
    {
        public Uzytkownik Uzytkownik { get; set; } = null!;
        public int LiczbaRol { get; set; }
        public int LiczbaDokumentow { get; set; }
    }
}
