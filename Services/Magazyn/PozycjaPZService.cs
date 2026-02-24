using Data.Data;
using Interfaces.Magazyn;
using Interfaces.Magazyn.Dtos;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.Magazyn
{
    public class PozycjaPZService : BaseService, IPozycjaPZService
    {
        public PozycjaPZService(DataContext context) : base(context)
        {
        }

        public async Task<PozycjaPZIndexDto> GetIndexDataAsync(string? searchTerm)
        {
            var query = _context.PozycjaPZ
                .AsNoTracking()
                .Include(p => p.Dokument)
                .Include(p => p.Lokacja)
                    .ThenInclude(l => l.Magazyn)
                .Include(p => p.Produkt)
                    .ThenInclude(p => p.DomyslnaJednostka)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(p =>
                    (p.Dokument != null && EF.Functions.Like(p.Dokument.Numer, $"%{term}%")) ||
                    (p.Dokument != null && EF.Functions.Like(p.Dokument.Status, $"%{term}%")) ||
                    (p.Produkt != null && EF.Functions.Like(p.Produkt.Kod, $"%{term}%")) ||
                    (p.Produkt != null && p.Produkt.Nazwa != null && EF.Functions.Like(p.Produkt.Nazwa, $"%{term}%")) ||
                    (p.Lokacja != null && EF.Functions.Like(p.Lokacja.Kod, $"%{term}%")) ||
                    (p.Lokacja != null && p.Lokacja.Opis != null && EF.Functions.Like(p.Lokacja.Opis, $"%{term}%")) ||
                    (p.Lokacja != null && p.Lokacja.Magazyn != null && EF.Functions.Like(p.Lokacja.Magazyn.Nazwa, $"%{term}%")));
            }

            return new PozycjaPZIndexDto
            {
                SearchTerm = searchTerm,
                Items = await query.ToListAsync()
            };
        }

        public async Task<PozycjaPZDetailsDto?> GetDetailsDataAsync(int idPozycji)
        {
            var pozycja = await _context.PozycjaPZ
                .AsNoTracking()
                .Include(p => p.Dokument)
                    .ThenInclude(d => d.Magazyn)
                .Include(p => p.Lokacja)
                    .ThenInclude(l => l.Magazyn)
                .Include(p => p.Produkt)
                    .ThenInclude(p => p.DomyslnaJednostka)
                .FirstOrDefaultAsync(p => p.Id == idPozycji);
            if (pozycja == null)
            {
                return null;
            }

            return new PozycjaPZDetailsDto
            {
                Pozycja = pozycja,
                Wartosc = pozycja.Ilosc * (pozycja.CenaJednostkowa ?? 0m),
                CzyDokumentDraft = string.Equals(pozycja.Dokument?.Status, "Draft", StringComparison.OrdinalIgnoreCase)
            };
        }

        public async Task<PozycjaPZDeleteDto?> GetDeleteDataAsync(int idPozycji)
        {
            var pozycja = await _context.PozycjaPZ
                .AsNoTracking()
                .Include(p => p.Dokument)
                    .ThenInclude(d => d.Magazyn)
                .Include(p => p.Lokacja)
                    .ThenInclude(l => l.Magazyn)
                .Include(p => p.Produkt)
                    .ThenInclude(p => p.DomyslnaJednostka)
                .FirstOrDefaultAsync(p => p.Id == idPozycji);
            if (pozycja == null)
            {
                return null;
            }

            var isDraft = string.Equals(pozycja.Dokument?.Status, "Draft", StringComparison.OrdinalIgnoreCase);
            return new PozycjaPZDeleteDto
            {
                Pozycja = pozycja,
                Wartosc = pozycja.Ilosc * (pozycja.CenaJednostkowa ?? 0m),
                CzyDokumentDraft = isDraft,
                PowodBlokady = isDraft ? null : "Pozycje można usuwać tylko dla dokumentu PZ w statusie Draft."
            };
        }
    }
}
