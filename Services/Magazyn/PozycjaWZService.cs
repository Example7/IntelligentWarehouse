using Data.Data;
using Interfaces.Magazyn;
using Interfaces.Magazyn.Dtos;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.Magazyn
{
    public class PozycjaWZService : BaseService, IPozycjaWZService
    {
        public PozycjaWZService(DataContext context) : base(context)
        {
        }

        public async Task<PozycjaWZIndexDto> GetIndexDataAsync(string? searchTerm)
        {
            var query = _context.PozycjaWZ
                .AsNoTracking()
                .Include(p => p.Dokument)
                .Include(p => p.Lokacja)
                    .ThenInclude(l => l.Magazyn)
                .Include(p => p.Produkt)
                    .ThenInclude(p => p.DomyslnaJednostka)
                .Include(p => p.Partia)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(p =>
                    (p.Dokument != null && EF.Functions.Like(p.Dokument.Numer, $"%{term}%")) ||
                    (p.Dokument != null && EF.Functions.Like(p.Dokument.Status, $"%{term}%")) ||
                    (p.Produkt != null && EF.Functions.Like(p.Produkt.Kod, $"%{term}%")) ||
                    (p.Produkt != null && EF.Functions.Like(p.Produkt.Nazwa, $"%{term}%")) ||
                    (p.Partia != null && EF.Functions.Like(p.Partia.NumerPartii, $"%{term}%")) ||
                    (p.Lokacja != null && EF.Functions.Like(p.Lokacja.Kod, $"%{term}%")) ||
                    (p.Lokacja != null && p.Lokacja.Opis != null && EF.Functions.Like(p.Lokacja.Opis, $"%{term}%")) ||
                    (p.Lokacja != null && p.Lokacja.Magazyn != null && EF.Functions.Like(p.Lokacja.Magazyn.Nazwa, $"%{term}%")));
            }

            return new PozycjaWZIndexDto { SearchTerm = searchTerm, Items = await query.ToListAsync() };
        }

        public async Task<PozycjaWZDetailsDto?> GetDetailsDataAsync(int idPozycji)
        {
            var pozycja = await _context.PozycjaWZ
                .AsNoTracking()
                .Include(p => p.Dokument).ThenInclude(d => d.Magazyn)
                .Include(p => p.Lokacja).ThenInclude(l => l.Magazyn)
                .Include(p => p.Produkt).ThenInclude(p => p.DomyslnaJednostka)
                .Include(p => p.Partia)
                .FirstOrDefaultAsync(p => p.Id == idPozycji);
            if (pozycja == null)
            {
                return null;
            }

            return new PozycjaWZDetailsDto
            {
                Pozycja = pozycja,
                CzyDokumentDraft = string.Equals(pozycja.Dokument?.Status, "Draft", StringComparison.OrdinalIgnoreCase)
            };
        }

        public async Task<PozycjaWZDeleteDto?> GetDeleteDataAsync(int idPozycji)
        {
            var pozycja = await _context.PozycjaWZ
                .AsNoTracking()
                .Include(p => p.Dokument).ThenInclude(d => d.Magazyn)
                .Include(p => p.Lokacja).ThenInclude(l => l.Magazyn)
                .Include(p => p.Produkt).ThenInclude(p => p.DomyslnaJednostka)
                .Include(p => p.Partia)
                .FirstOrDefaultAsync(p => p.Id == idPozycji);
            if (pozycja == null)
            {
                return null;
            }

            var isDraft = string.Equals(pozycja.Dokument?.Status, "Draft", StringComparison.OrdinalIgnoreCase);
            return new PozycjaWZDeleteDto
            {
                Pozycja = pozycja,
                CzyDokumentDraft = isDraft,
                PowodBlokady = isDraft ? null : "Pozycje można usuwać tylko dla dokumentu WZ w statusie Draft."
            };
        }
    }
}
