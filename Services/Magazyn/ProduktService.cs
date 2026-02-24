using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;
using Interfaces.Magazyn.Dtos;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.Magazyn
{
    public class ProduktService : BaseService, IProduktService
    {
        public ProduktService(DataContext context) : base(context)
        {
        }

        public async Task<Produkt?> GetProdukt(int? idProduktu)
        {
            return await _context.Produkt
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdProduktu == idProduktu);
        }

        public async Task<IList<Produkt>> GetProdukty(int idKategorii)
        {
            return await _context.Produkt
                .AsNoTracking()
                .Where(p => p.IdKategorii == idKategorii)
                .ToListAsync();
        }

        public async Task<ProduktDetailsDto?> GetDetailsDataAsync(int idProduktu)
        {
            var produkt = await _context.Produkt
                .AsNoTracking()
                .Include(p => p.DomyslnaJednostka)
                .Include(p => p.Kategoria)
                .FirstOrDefaultAsync(p => p.IdProduktu == idProduktu);

            if (produkt == null)
            {
                return null;
            }

            var stanyMagazynowe = await _context.StanMagazynowy
                .AsNoTracking()
                .Include(s => s.Lokacja)
                    .ThenInclude(l => l.Magazyn)
                .Where(s => s.IdProduktu == idProduktu)
                .OrderBy(s => s.Lokacja.Magazyn.Nazwa)
                .ThenBy(s => s.Lokacja.Kod)
                .ToListAsync();

            var ostatniePozycjeRezerwacji = await _context.PozycjaRezerwacji
                .AsNoTracking()
                .Include(p => p.Rezerwacja)
                    .ThenInclude(r => r.Magazyn)
                .Include(p => p.Lokacja)
                .Where(p => p.IdProduktu == idProduktu)
                .OrderByDescending(p => p.Rezerwacja.UtworzonoUtc)
                .ThenByDescending(p => p.Id)
                .Take(10)
                .ToListAsync();

            var ostatniePozycjePz = await _context.PozycjaPZ
                .AsNoTracking()
                .Include(p => p.Dokument)
                    .ThenInclude(d => d.Magazyn)
                .Include(p => p.Lokacja)
                .Where(p => p.IdProduktu == idProduktu)
                .OrderByDescending(p => p.Dokument.DataPrzyjeciaUtc)
                .ThenByDescending(p => p.Id)
                .Take(5)
                .ToListAsync();

            var ostatniePozycjeWz = await _context.PozycjaWZ
                .AsNoTracking()
                .Include(p => p.Dokument)
                    .ThenInclude(d => d.Magazyn)
                .Include(p => p.Lokacja)
                .Where(p => p.IdProduktu == idProduktu)
                .OrderByDescending(p => p.Dokument.DataWydaniaUtc)
                .ThenByDescending(p => p.Id)
                .Take(5)
                .ToListAsync();

            var ostatniePozycjeMm = await _context.PozycjaMM
                .AsNoTracking()
                .Include(p => p.Dokument)
                    .ThenInclude(d => d.Magazyn)
                .Include(p => p.LokacjaZ)
                .Include(p => p.LokacjaDo)
                .Where(p => p.IdProduktu == idProduktu)
                .OrderByDescending(p => p.Dokument.DataUtc)
                .ThenByDescending(p => p.Id)
                .Take(5)
                .ToListAsync();

            var ostatnieAlerty = await _context.Alert
                .AsNoTracking()
                .Include(a => a.Magazyn)
                .Include(a => a.Regula)
                .Where(a => a.IdProduktu == idProduktu)
                .OrderByDescending(a => a.UtworzonoUtc)
                .ThenByDescending(a => a.Id)
                .Take(10)
                .ToListAsync();

            var liczbaAlertow = await _context.Alert.CountAsync(a => a.IdProduktu == idProduktu);
            var liczbaNowychAlertow = await _context.Alert.CountAsync(a => a.IdProduktu == idProduktu && !a.CzyPotwierdzony);

            var iloscZarezerwowana = await _context.PozycjaRezerwacji
                .Where(p => p.IdProduktu == idProduktu)
                .Select(p => (decimal?)p.Ilosc)
                .SumAsync() ?? 0m;

            var liczbaPozycjiRezerwacji = await _context.PozycjaRezerwacji.CountAsync(p => p.IdProduktu == idProduktu);

            return new ProduktDetailsDto
            {
                Produkt = produkt,
                StanyMagazynowe = stanyMagazynowe,
                AktualnyStan = stanyMagazynowe.Sum(s => s.Ilosc),
                LiczbaLokacjiZeStanem = stanyMagazynowe.Count,
                LiczbaAlertow = liczbaAlertow,
                LiczbaNowychAlertow = liczbaNowychAlertow,
                LiczbaPozycjiRezerwacji = liczbaPozycjiRezerwacji,
                IloscZarezerwowana = iloscZarezerwowana,
                OstatniePozycjeRezerwacji = ostatniePozycjeRezerwacji,
                OstatniePozycjePz = ostatniePozycjePz,
                OstatniePozycjeWz = ostatniePozycjeWz,
                OstatniePozycjeMm = ostatniePozycjeMm,
                OstatnieAlerty = ostatnieAlerty
            };
        }
    }
}
