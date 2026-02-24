using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class RezerwacjaDetailsDto
    {
        public Rezerwacja Dokument { get; set; } = null!;
        public int LiczbaPozycji { get; set; }
        public decimal SumaIlosci { get; set; }
        public IList<PozycjaRezerwacji> Pozycje { get; set; } = new List<PozycjaRezerwacji>();
    }
}
