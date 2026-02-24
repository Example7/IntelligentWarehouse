using Data.Data.Magazyn;
using LokacjaEntity = Data.Data.Magazyn.Lokacja;

namespace Interfaces.Magazyn.Dtos
{
    public class LokacjaDetailsDto
    {
        public LokacjaEntity Lokacja { get; set; } = null!;

        public decimal AktualnyStan { get; set; }
        public int LiczbaProduktow { get; set; }
        public int LiczbaRezerwacji { get; set; }
        public IDictionary<int, decimal> AktywneRezerwacjePoProdukcie { get; set; } = new Dictionary<int, decimal>();

        public IList<StanMagazynowy> StanyLokacji { get; set; } = new List<StanMagazynowy>();
        public IList<PozycjaRezerwacji> OstatniePozycjeRezerwacji { get; set; } = new List<PozycjaRezerwacji>();
        public IList<PozycjaPZ> OstatniePozycjePz { get; set; } = new List<PozycjaPZ>();
        public IList<PozycjaWZ> OstatniePozycjeWz { get; set; } = new List<PozycjaWZ>();
        public IList<PozycjaMM> OstatniePozycjeMmZ { get; set; } = new List<PozycjaMM>();
        public IList<PozycjaMM> OstatniePozycjeMmDo { get; set; } = new List<PozycjaMM>();
    }
}
