using Data.Data;
using Interfaces.Magazyn;
using Interfaces.Magazyn.Dtos;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.Magazyn
{
    public class PozycjaMMService : BaseService, IPozycjaMMService
    {
        public PozycjaMMService(DataContext context) : base(context)
        {
        }

        public async Task<PozycjaMMIndexDto> GetIndexDataAsync(string? searchTerm)
        {
            var query = _context.PozycjaMM
                .AsNoTracking()
                .Include(p => p.Dokument)
                .Include(p => p.LokacjaZ)
                    .ThenInclude(l => l.Magazyn)
                .Include(p => p.LokacjaDo)
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
                    (p.LokacjaZ != null && EF.Functions.Like(p.LokacjaZ.Kod, $"%{term}%")) ||
                    (p.LokacjaZ != null && p.LokacjaZ.Opis != null && EF.Functions.Like(p.LokacjaZ.Opis, $"%{term}%")) ||
                    (p.LokacjaDo != null && EF.Functions.Like(p.LokacjaDo.Kod, $"%{term}%")) ||
                    (p.LokacjaDo != null && p.LokacjaDo.Opis != null && EF.Functions.Like(p.LokacjaDo.Opis, $"%{term}%")) ||
                    (p.LokacjaZ != null && p.LokacjaZ.Magazyn != null && EF.Functions.Like(p.LokacjaZ.Magazyn.Nazwa, $"%{term}%")) ||
                    (p.LokacjaDo != null && p.LokacjaDo.Magazyn != null && EF.Functions.Like(p.LokacjaDo.Magazyn.Nazwa, $"%{term}%")));
            }

            return new PozycjaMMIndexDto { SearchTerm = searchTerm, Items = await query.ToListAsync() };
        }

        public async Task<PozycjaMMDetailsDto?> GetDetailsDataAsync(int idPozycji)
        {
            var pozycja = await _context.PozycjaMM
                .AsNoTracking()
                .Include(p => p.Dokument).ThenInclude(d => d.Magazyn)
                .Include(p => p.LokacjaZ).ThenInclude(l => l.Magazyn)
                .Include(p => p.LokacjaDo).ThenInclude(l => l.Magazyn)
                .Include(p => p.Produkt).ThenInclude(p => p.DomyslnaJednostka)
                .Include(p => p.Partia)
                .FirstOrDefaultAsync(p => p.Id == idPozycji);
            if (pozycja == null)
            {
                return null;
            }

            return new PozycjaMMDetailsDto
            {
                Pozycja = pozycja,
                CzyDokumentDraft = string.Equals(pozycja.Dokument?.Status, "Draft", StringComparison.OrdinalIgnoreCase)
            };
        }

        public async Task<PozycjaMMDeleteDto?> GetDeleteDataAsync(int idPozycji)
        {
            var pozycja = await _context.PozycjaMM
                .AsNoTracking()
                .Include(p => p.Dokument).ThenInclude(d => d.Magazyn)
                .Include(p => p.LokacjaZ).ThenInclude(l => l.Magazyn)
                .Include(p => p.LokacjaDo).ThenInclude(l => l.Magazyn)
                .Include(p => p.Produkt).ThenInclude(p => p.DomyslnaJednostka)
                .Include(p => p.Partia)
                .FirstOrDefaultAsync(p => p.Id == idPozycji);
            if (pozycja == null)
            {
                return null;
            }

            var isDraft = string.Equals(pozycja.Dokument?.Status, "Draft", StringComparison.OrdinalIgnoreCase);
            return new PozycjaMMDeleteDto
            {
                Pozycja = pozycja,
                CzyDokumentDraft = isDraft,
                PowodBlokady = isDraft ? null : "Pozycje mozna usuwac tylko dla dokumentu MM w statusie Draft."
            };
        }
    }
}
