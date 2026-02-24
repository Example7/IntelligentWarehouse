using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;
using Interfaces.Magazyn.Dtos;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.Magazyn
{
    public class LokacjaService : BaseService, ILokacjaService
    {
        public LokacjaService(DataContext context) : base(context) { }

        public async Task<IList<Lokacja>> GetLokacje()
        {
            return await _context.Lokacja
                .Include(l => l.Magazyn)
                .Include(l => l.Stany)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<LokacjaIndexDto> GetIndexDataAsync(string? searchTerm)
        {
            var query = _context.Lokacja
                .AsNoTracking()
                .Include(l => l.Magazyn)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(l =>
                    EF.Functions.Like(l.Kod, $"%{term}%") ||
                    (l.Opis != null && EF.Functions.Like(l.Opis, $"%{term}%")));
            }

            var lokacje = await query.ToListAsync();
            var idsLokacji = lokacje.Select(x => x.IdLokacji).Distinct().ToList();

            var liczbyProduktowLokacji = new Dictionary<int, int>();
            var liczbyRezerwacjiLokacji = new Dictionary<int, int>();

            if (idsLokacji.Count > 0)
            {
                liczbyProduktowLokacji = await _context.StanMagazynowy
                    .AsNoTracking()
                    .Where(s => idsLokacji.Contains(s.IdLokacji))
                    .GroupBy(s => s.IdLokacji)
                    .Select(g => new { IdLokacji = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.IdLokacji, x => x.Count);

                liczbyRezerwacjiLokacji = await _context.PozycjaRezerwacji
                    .AsNoTracking()
                    .Where(r => r.IdLokacji.HasValue && idsLokacji.Contains(r.IdLokacji.Value))
                    .GroupBy(r => r.IdLokacji!.Value)
                    .Select(g => new { IdLokacji = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.IdLokacji, x => x.Count);
            }

            return new LokacjaIndexDto
            {
                SearchTerm = searchTerm,
                Items = lokacje
                    .Select(l => new LokacjaIndexItemDto
                    {
                        Lokacja = l,
                        LiczbaProduktow = liczbyProduktowLokacji.TryGetValue(l.IdLokacji, out var lp) ? lp : 0,
                        LiczbaRezerwacji = liczbyRezerwacjiLokacji.TryGetValue(l.IdLokacji, out var lr) ? lr : 0
                    })
                    .ToList()
            };
        }

        public async Task<LokacjaDetailsDto?> GetDetailsDataAsync(int idLokacji)
        {
            var lokacja = await _context.Lokacja
                .AsNoTracking()
                .Include(l => l.Magazyn)
                .FirstOrDefaultAsync(l => l.IdLokacji == idLokacji);

            if (lokacja == null)
            {
                return null;
            }

            var stanyLokacji = await _context.StanMagazynowy
                .AsNoTracking()
                .Include(s => s.Produkt)
                    .ThenInclude(p => p.DomyslnaJednostka)
                .Include(s => s.Produkt)
                    .ThenInclude(p => p.Kategoria)
                .Where(s => s.IdLokacji == idLokacji)
                .OrderBy(s => s.Produkt.Kod)
                .ThenBy(s => s.Produkt.Nazwa)
                .ToListAsync();

            var ostatniePozycjeRezerwacji = await _context.PozycjaRezerwacji
                .AsNoTracking()
                .Include(p => p.Rezerwacja)
                    .ThenInclude(r => r.Magazyn)
                .Include(p => p.Produkt)
                    .ThenInclude(pr => pr.DomyslnaJednostka)
                .Where(p => p.IdLokacji == idLokacji)
                .OrderByDescending(p => p.Rezerwacja.UtworzonoUtc)
                .ThenByDescending(p => p.Id)
                .Take(10)
                .ToListAsync();

            var ostatniePozycjePz = await _context.PozycjaPZ
                .AsNoTracking()
                .Include(p => p.Dokument)
                .Include(p => p.Produkt)
                    .ThenInclude(pr => pr.DomyslnaJednostka)
                .Where(p => p.IdLokacji == idLokacji)
                .OrderByDescending(p => p.Dokument.DataPrzyjeciaUtc)
                .ThenByDescending(p => p.Id)
                .Take(5)
                .ToListAsync();

            var ostatniePozycjeWz = await _context.PozycjaWZ
                .AsNoTracking()
                .Include(p => p.Dokument)
                .Include(p => p.Produkt)
                    .ThenInclude(pr => pr.DomyslnaJednostka)
                .Where(p => p.IdLokacji == idLokacji)
                .OrderByDescending(p => p.Dokument.DataWydaniaUtc)
                .ThenByDescending(p => p.Id)
                .Take(5)
                .ToListAsync();

            var ostatniePozycjeMmZ = await _context.PozycjaMM
                .AsNoTracking()
                .Include(p => p.Dokument)
                .Include(p => p.Produkt)
                    .ThenInclude(pr => pr.DomyslnaJednostka)
                .Include(p => p.LokacjaDo)
                .Where(p => p.IdLokacjiZ == idLokacji)
                .OrderByDescending(p => p.Dokument.DataUtc)
                .ThenByDescending(p => p.Id)
                .Take(5)
                .ToListAsync();

            var ostatniePozycjeMmDo = await _context.PozycjaMM
                .AsNoTracking()
                .Include(p => p.Dokument)
                .Include(p => p.Produkt)
                    .ThenInclude(pr => pr.DomyslnaJednostka)
                .Include(p => p.LokacjaZ)
                .Where(p => p.IdLokacjiDo == idLokacji)
                .OrderByDescending(p => p.Dokument.DataUtc)
                .ThenByDescending(p => p.Id)
                .Take(5)
                .ToListAsync();

            return new LokacjaDetailsDto
            {
                Lokacja = lokacja,
                StanyLokacji = stanyLokacji,
                AktualnyStan = stanyLokacji.Sum(s => s.Ilosc),
                LiczbaProduktow = stanyLokacji.Count,
                LiczbaRezerwacji = ostatniePozycjeRezerwacji.Count,
                OstatniePozycjeRezerwacji = ostatniePozycjeRezerwacji,
                OstatniePozycjePz = ostatniePozycjePz,
                OstatniePozycjeWz = ostatniePozycjeWz,
                OstatniePozycjeMmZ = ostatniePozycjeMmZ,
                OstatniePozycjeMmDo = ostatniePozycjeMmDo
            };
        }
    }
}
