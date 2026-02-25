using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;
using Interfaces.Magazyn.Dtos;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.Magazyn
{
    public class DokumentWZService : BaseService, IDokumentWZService
    {
        public DokumentWZService(DataContext context) : base(context)
        {
        }

        public async Task<DokumentWZIndexDto> GetIndexDataAsync(string? searchTerm)
        {
            var query = _context.DokumentWZ
                .AsNoTracking()
                .Include(d => d.Klient)
                .Include(d => d.Magazyn)
                .Include(d => d.Utworzyl)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(x =>
                    EF.Functions.Like(x.Numer, $"%{term}%") ||
                    EF.Functions.Like(x.Status, $"%{term}%") ||
                    (x.Notatka != null && EF.Functions.Like(x.Notatka, $"%{term}%")) ||
                    (x.Klient != null && EF.Functions.Like(x.Klient.Nazwa, $"%{term}%")) ||
                    (x.Magazyn != null && EF.Functions.Like(x.Magazyn.Nazwa, $"%{term}%")));
            }

            return new DokumentWZIndexDto
            {
                SearchTerm = searchTerm,
                Items = await query.ToListAsync()
            };
        }

        public async Task<DokumentWZDetailsDto?> GetDetailsDataAsync(int idDokumentu)
        {
            var dokument = await _context.DokumentWZ
                .AsNoTracking()
                .Include(d => d.Klient)
                .Include(d => d.Magazyn)
                .Include(d => d.Utworzyl)
                .FirstOrDefaultAsync(d => d.Id == idDokumentu);
            if (dokument == null)
            {
                return null;
            }

            var pozycje = await _context.PozycjaWZ
                .AsNoTracking()
                .Include(p => p.Produkt)
                    .ThenInclude(p => p.DomyslnaJednostka)
                .Include(p => p.Partia)
                .Include(p => p.Lokacja)
                    .ThenInclude(l => l.Magazyn)
                .Where(p => p.IdDokumentu == idDokumentu)
                .OrderBy(p => p.Lp)
                .ThenBy(p => p.Id)
                .ToListAsync();

            return new DokumentWZDetailsDto
            {
                Dokument = dokument,
                LiczbaPozycji = pozycje.Count,
                SumaIlosci = pozycje.Sum(p => p.Ilosc),
                Pozycje = pozycje
            };
        }

        public async Task<DokumentWZDeleteDto?> GetDeleteDataAsync(int idDokumentu)
        {
            var dokument = await _context.DokumentWZ
                .AsNoTracking()
                .Include(d => d.Klient)
                .Include(d => d.Magazyn)
                .Include(d => d.Utworzyl)
                .FirstOrDefaultAsync(d => d.Id == idDokumentu);
            if (dokument == null)
            {
                return null;
            }

            var liczbaPozycji = await _context.PozycjaWZ.AsNoTracking().CountAsync(p => p.IdDokumentu == idDokumentu);
            return new DokumentWZDeleteDto { Dokument = dokument, LiczbaPozycji = liczbaPozycji };
        }

        public async Task<DokumentWZPostResultDto> PostAsync(int idDokumentu)
        {
            var dokument = await _context.DokumentWZ.FirstOrDefaultAsync(d => d.Id == idDokumentu);
            if (dokument == null)
            {
                return new DokumentWZPostResultDto { Success = false, ErrorMessage = "Nie znaleziono dokumentu WZ." };
            }

            if (!string.Equals(dokument.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            {
                return new DokumentWZPostResultDto { Success = false, ErrorMessage = "Zaksiêgowaæ mo¿na tylko dokument WZ w statusie Draft." };
            }

            var pozycje = await _context.PozycjaWZ
                .Include(p => p.Lokacja)
                .Where(p => p.IdDokumentu == idDokumentu)
                .OrderBy(p => p.Lp)
                .ThenBy(p => p.Id)
                .ToListAsync();

            if (pozycje.Count == 0)
            {
                return new DokumentWZPostResultDto { Success = false, ErrorMessage = "Nie mo¿na zaksiêgowaæ dokumentu WZ bez pozycji." };
            }

            if (pozycje.Any(p => p.Ilosc <= 0))
            {
                return new DokumentWZPostResultDto { Success = false, ErrorMessage = "Wszystkie pozycje WZ musz¹ mieæ iloœæ wiêksz¹ od zera." };
            }

            if (pozycje.Any(p => p.Lokacja == null || p.Lokacja.IdMagazynu != dokument.IdMagazynu))
            {
                return new DokumentWZPostResultDto { Success = false, ErrorMessage = "Wszystkie lokacje pozycji WZ musz¹ nale¿eæ do magazynu dokumentu." };
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var now = DateTime.UtcNow;
                var stockKeys = pozycje
                    .Select(p => new { p.IdProduktu, IdLokacji = p.IdLokacji!.Value })
                    .Distinct()
                    .ToList();

                var productIds = stockKeys.Select(x => x.IdProduktu).Distinct().ToList();
                var locationIds = stockKeys.Select(x => x.IdLokacji).Distinct().ToList();

                var stanyByKey = await _context.StanMagazynowy
                    .Where(s => productIds.Contains(s.IdProduktu) && locationIds.Contains(s.IdLokacji))
                    .ToDictionaryAsync(s => (s.IdProduktu, s.IdLokacji));

                foreach (var pozycja in pozycje)
                {
                    var key = (pozycja.IdProduktu, pozycja.IdLokacji!.Value);
                    stanyByKey.TryGetValue(key, out var stan);

                    if (stan == null)
                    {
                        return new DokumentWZPostResultDto
                        {
                            Success = false,
                            ErrorMessage = $"Brak stanu magazynowego dla produktu {pozycja.IdProduktu} w lokacji {pozycja.Lokacja?.Kod ?? pozycja.IdLokacji?.ToString() ?? "-"}."
                        };
                    }

                    if (stan.Ilosc < pozycja.Ilosc)
                    {
                        return new DokumentWZPostResultDto
                        {
                            Success = false,
                            ErrorMessage = $"Niewystarczaj¹cy stan w lokacji {pozycja.Lokacja?.Kod ?? pozycja.IdLokacji?.ToString() ?? "-"} dla produktu {pozycja.IdProduktu}. Dostêpne: {stan.Ilosc:0.###}."
                        };
                    }

                    stan.Ilosc -= pozycja.Ilosc;

                    _context.RuchMagazynowy.Add(new RuchMagazynowy
                    {
                        Typ = TypRuchuMagazynowego.Wydanie,
                        IdProduktu = pozycja.IdProduktu,
                        IdLokacjiZ = pozycja.IdLokacji,
                        IdLokacjiDo = null,
                        Ilosc = pozycja.Ilosc,
                        Referencja = dokument.Numer,
                        Notatka = $"WZ {dokument.Numer}, lp {pozycja.Lp}",
                        UtworzonoUtc = now,
                        IdUzytkownika = dokument.IdUtworzyl
                    });
                }

                dokument.Status = "Posted";
                dokument.ZaksiegowanoUtc = now;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new DokumentWZPostResultDto { Success = true };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}


