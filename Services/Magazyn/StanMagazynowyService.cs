using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;
using Interfaces.Magazyn.Dtos;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.Magazyn
{
    public class StanMagazynowyService : BaseService, IStanMagazynowyService
    {
        public StanMagazynowyService(DataContext context) : base(context)
        {
        }

        public async Task<StanMagazynowyIndexDto> GetIndexDataAsync(string? searchTerm)
        {
            var query = _context.StanMagazynowy
                .AsNoTracking()
                .Include(s => s.Lokacja)
                    .ThenInclude(l => l.Magazyn)
                .Include(s => s.Produkt)
                    .ThenInclude(p => p.DomyslnaJednostka)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(s =>
                    EF.Functions.Like(s.Produkt.Kod, $"%{term}%") ||
                    EF.Functions.Like(s.Produkt.Nazwa, $"%{term}%") ||
                    EF.Functions.Like(s.Lokacja.Kod, $"%{term}%") ||
                    (s.Lokacja.Opis != null && EF.Functions.Like(s.Lokacja.Opis, $"%{term}%")) ||
                    EF.Functions.Like(s.Lokacja.Magazyn.Nazwa, $"%{term}%"));
            }

            var items = await query.ToListAsync();
            var keys = items.Select(x => new { x.IdProduktu, x.IdLokacji }).Distinct().ToList();
            var aktywneRezerwacjePoKluczu = new Dictionary<string, decimal>();

            if (keys.Count > 0)
            {
                var productIds = keys.Select(k => k.IdProduktu).Distinct().ToList();
                var locationIds = keys.Select(k => k.IdLokacji).Distinct().ToList();

                var activeReservations = await _context.PozycjaRezerwacji
                    .AsNoTracking()
                    .Where(p =>
                        p.IdLokacji.HasValue &&
                        productIds.Contains(p.IdProduktu) &&
                        locationIds.Contains(p.IdLokacji.Value) &&
                        p.Rezerwacja.Status == "Active")
                    .GroupBy(p => new { p.IdProduktu, IdLokacji = p.IdLokacji!.Value })
                    .Select(g => new
                    {
                        g.Key.IdProduktu,
                        g.Key.IdLokacji,
                        Qty = g.Sum(x => x.Ilosc)
                    })
                    .ToListAsync();

                var draftWzAllocations = await _context.PozycjaWZ
                    .AsNoTracking()
                    .Where(p =>
                        p.IdLokacji.HasValue &&
                        productIds.Contains(p.IdProduktu) &&
                        locationIds.Contains(p.IdLokacji.Value) &&
                        p.Dokument.Status == "Draft")
                    .GroupBy(p => new { p.IdProduktu, IdLokacji = p.IdLokacji!.Value })
                    .Select(g => new
                    {
                        g.Key.IdProduktu,
                        g.Key.IdLokacji,
                        Qty = g.Sum(x => x.Ilosc)
                    })
                    .ToListAsync();

                aktywneRezerwacjePoKluczu = activeReservations
                    .Concat(draftWzAllocations)
                    .GroupBy(x => new { x.IdProduktu, x.IdLokacji })
                    .ToDictionary(
                        g => StanMagazynowyIndexDto.BuildKey(g.Key.IdProduktu, g.Key.IdLokacji),
                        g => g.Sum(x => x.Qty));
            }

            var sumaWgJednostki = items
                .GroupBy(x => x.Produkt?.DomyslnaJednostka?.Kod ?? "j.m.")
                .OrderBy(g => g.Key)
                .Select(g => new StanMagazynowyUomSummaryDto
                {
                    Jednostka = g.Key,
                    Ilosc = g.Sum(x => x.Ilosc)
                })
                .ToList();

            return new StanMagazynowyIndexDto
            {
                SearchTerm = searchTerm,
                Items = items,
                AktywneRezerwacjePoKluczu = aktywneRezerwacjePoKluczu,
                SumaWgJednostki = sumaWgJednostki,
                SumaWgJednostkiLabel = sumaWgJednostki.Count == 0
                    ? "-"
                    : string.Join(", ", sumaWgJednostki.Select(x => x.Etykieta))
            };
        }

        public async Task<StanMagazynowyFormDto> GetCreateFormAsync()
        {
            return await PrepareFormAsync(new StanMagazynowy(), isEdit: false);
        }

        public async Task<StanMagazynowyFormDto?> GetEditFormAsync(int idStanu)
        {
            var stan = await _context.StanMagazynowy.AsNoTracking().FirstOrDefaultAsync(x => x.IdStanu == idStanu);
            if (stan == null)
            {
                return null;
            }

            return await PrepareFormAsync(stan, isEdit: true);
        }

        public async Task<StanMagazynowyFormDto> PrepareFormAsync(StanMagazynowy stanMagazynowy, bool isEdit)
        {
            var produkty = await _context.Produkt
                .AsNoTracking()
                .Include(p => p.DomyslnaJednostka)
                .OrderBy(p => p.Kod)
                .ThenBy(p => p.Nazwa)
                .Select(p => new
                {
                    p.IdProduktu,
                    Text = p.Kod + " - " + p.Nazwa + " (" + (p.DomyslnaJednostka != null ? p.DomyslnaJednostka.Kod : "j.m.") + ")",
                    Jm = p.DomyslnaJednostka != null ? p.DomyslnaJednostka.Kod : "j.m."
                })
                .ToListAsync();

            var lokacje = await _context.Lokacja
                .AsNoTracking()
                .Include(l => l.Magazyn)
                .OrderBy(l => l.Magazyn.Nazwa)
                .ThenBy(l => l.Kod)
                .Select(l => new
                {
                    l.IdLokacji,
                    Text = (l.Magazyn != null ? l.Magazyn.Nazwa : "-") + " / " + l.Kod
                })
                .ToListAsync();

            var currentProductUom = produkty.FirstOrDefault(x => x.IdProduktu == stanMagazynowy.IdProduktu)?.Jm ?? "j.m.";

            return new StanMagazynowyFormDto
            {
                StanMagazynowy = stanMagazynowy,
                IsEdit = isEdit,
                Produkty = produkty.Select(x => new StanMagazynowySelectOptionDto { Value = x.IdProduktu, Text = x.Text }).ToList(),
                Lokacje = lokacje.Select(x => new StanMagazynowySelectOptionDto { Value = x.IdLokacji, Text = x.Text }).ToList(),
                ProductUomMap = produkty.ToDictionary(x => x.IdProduktu, x => x.Jm),
                CurrentProductUom = currentProductUom
            };
        }
    }
}
