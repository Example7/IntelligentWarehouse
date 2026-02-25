using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class UzytkownikRolaDeleteDto
    {
        public UzytkownikRola UzytkownikRola { get; set; } = null!;
        public int LiczbaRolUzytkownika { get; set; }
        public int LiczbaUzytkownikowWRoli { get; set; }
    }
}
