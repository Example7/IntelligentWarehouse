using Data.Data;
using Interfaces.Magazyn;
using Interfaces.Magazyn.Dtos;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.Magazyn
{
    public class PozycjaInwentaryzacjiService : BaseService, IPozycjaInwentaryzacjiService
    {
        public PozycjaInwentaryzacjiService(DataContext context) : base(context)
        {
        }

        public async Task<PozycjaInwentaryzacjiIndexDto> GetIndexDataAsync(string? searchTerm)
        {
            var query = _context.PozycjaInwentaryzacji
                .AsNoTracking()
                .Include(p => p.Dokument)
                .Include(p => p.Lokacja).ThenInclude(l => l.Magazyn)
                .Include(p => p.Produkt).ThenInclude(p => p.DomyslnaJednostka)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(p =>
                    (p.Dokument != null && EF.Functions.Like(p.Dokument.Numer, $"%{term}%")) ||
                    (p.Dokument != null && EF.Functions.Like(p.Dokument.Status, $"%{term}%")) ||
                    (p.Produkt != null && EF.Functions.Like(p.Produkt.Kod, $"%{term}%")) ||
                    (p.Produkt != null && EF.Functions.Like(p.Produkt.Nazwa, $"%{term}%")) ||
                    (p.Lokacja != null && EF.Functions.Like(p.Lokacja.Kod, $"%{term}%")) ||
                    (p.Lokacja != null && p.Lokacja.Magazyn != null && EF.Functions.Like(p.Lokacja.Magazyn.Nazwa, $"%{term}%")));
            }

            return new PozycjaInwentaryzacjiIndexDto { SearchTerm = searchTerm, Items = await query.ToListAsync() };
        }

        public async Task<PozycjaInwentaryzacjiDetailsDto?> GetDetailsDataAsync(int idPozycji)
        {
            var pozycja = await _context.PozycjaInwentaryzacji
                .AsNoTracking()
                .Include(p => p.Dokument).ThenInclude(d => d.Magazyn)
                .Include(p => p.Lokacja).ThenInclude(l => l.Magazyn)
                .Include(p => p.Produkt).ThenInclude(p => p.DomyslnaJednostka)
                .FirstOrDefaultAsync(p => p.Id == idPozycji);
            if (pozycja == null)
            {
                return null;
            }

            return new PozycjaInwentaryzacjiDetailsDto
            {
                Pozycja = pozycja,
                CzyDokumentDraft = string.Equals(pozycja.Dokument?.Status, "Draft", StringComparison.OrdinalIgnoreCase)
            };
        }

        public async Task<PozycjaInwentaryzacjiDeleteDto?> GetDeleteDataAsync(int idPozycji)
        {
            var pozycja = await _context.PozycjaInwentaryzacji
                .AsNoTracking()
                .Include(p => p.Dokument).ThenInclude(d => d.Magazyn)
                .Include(p => p.Lokacja).ThenInclude(l => l.Magazyn)
                .Include(p => p.Produkt).ThenInclude(p => p.DomyslnaJednostka)
                .FirstOrDefaultAsync(p => p.Id == idPozycji);
            if (pozycja == null)
            {
                return null;
            }

            var isDraft = string.Equals(pozycja.Dokument?.Status, "Draft", StringComparison.OrdinalIgnoreCase);
            return new PozycjaInwentaryzacjiDeleteDto
            {
                Pozycja = pozycja,
                CzyDokumentDraft = isDraft,
                PowodBlokady = isDraft ? null : "Pozycje mozna usuwac tylko dla inwentaryzacji w statusie Draft."
            };
        }
    }
}
