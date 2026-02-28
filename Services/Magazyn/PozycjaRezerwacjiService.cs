using Data.Data;
using Interfaces.Magazyn;
using Interfaces.Magazyn.Dtos;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.Magazyn
{
    public class PozycjaRezerwacjiService : BaseService, IPozycjaRezerwacjiService
    {
        public PozycjaRezerwacjiService(DataContext context) : base(context) { }

        public async Task<PozycjaRezerwacjiIndexDto> GetIndexDataAsync(string? searchTerm)
        {
            var query = _context.PozycjaRezerwacji
                .AsNoTracking()
                .Include(p => p.Rezerwacja).ThenInclude(r => r.Magazyn)
                .Include(p => p.Produkt).ThenInclude(p => p.DomyslnaJednostka)
                .Include(p => p.Lokacja).ThenInclude(l => l!.Magazyn)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(p =>
                    (p.Rezerwacja != null && EF.Functions.Like(p.Rezerwacja.Numer, $"%{term}%")) ||
                    (p.Rezerwacja != null && EF.Functions.Like(p.Rezerwacja.Status, $"%{term}%")) ||
                    (p.Produkt != null && EF.Functions.Like(p.Produkt.Kod, $"%{term}%")) ||
                    (p.Produkt != null && EF.Functions.Like(p.Produkt.Nazwa, $"%{term}%")) ||
                    (p.Lokacja != null && EF.Functions.Like(p.Lokacja.Kod, $"%{term}%")) ||
                    (p.Lokacja != null && p.Lokacja.Magazyn != null && EF.Functions.Like(p.Lokacja.Magazyn.Nazwa, $"%{term}%")));
            }

            return new PozycjaRezerwacjiIndexDto { SearchTerm = searchTerm, Items = await query.ToListAsync() };
        }

        public async Task<PozycjaRezerwacjiDetailsDto?> GetDetailsDataAsync(int id)
        {
            var pozycja = await _context.PozycjaRezerwacji
                .AsNoTracking()
                .Include(p => p.Rezerwacja).ThenInclude(r => r.Magazyn)
                .Include(p => p.Produkt).ThenInclude(p => p.DomyslnaJednostka)
                .Include(p => p.Lokacja).ThenInclude(l => l!.Magazyn)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (pozycja == null)
            {
                return null;
            }

            return new PozycjaRezerwacjiDetailsDto
            {
                Pozycja = pozycja,
                CzyRezerwacjaEdytowalna = CzyStatusEdytowalny(pozycja.Rezerwacja?.Status)
            };
        }

        public async Task<PozycjaRezerwacjiDeleteDto?> GetDeleteDataAsync(int id)
        {
            var pozycja = await _context.PozycjaRezerwacji
                .AsNoTracking()
                .Include(p => p.Rezerwacja).ThenInclude(r => r.Magazyn)
                .Include(p => p.Produkt).ThenInclude(p => p.DomyslnaJednostka)
                .Include(p => p.Lokacja).ThenInclude(l => l!.Magazyn)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (pozycja == null)
            {
                return null;
            }

            var editable = CzyStatusEdytowalny(pozycja.Rezerwacja?.Status);
            return new PozycjaRezerwacjiDeleteDto
            {
                Pozycja = pozycja,
                CzyRezerwacjaEdytowalna = editable,
                PowodBlokady = editable ? null : "Pozycje mozna usuwac tylko dla rezerwacji w statusie edytowalnym (np. Draft/Active)."
            };
        }

        private static bool CzyStatusEdytowalny(string? status)
        {
            if (string.IsNullOrWhiteSpace(status)) return true;
            return !string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(status, "Cancelled", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(status, "Released", StringComparison.OrdinalIgnoreCase);
        }
    }
}

