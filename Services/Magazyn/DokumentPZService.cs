using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;
using Interfaces.Magazyn.Dtos;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.Magazyn
{
    public class DokumentPZService : BaseService, IDokumentPZService
    {
        public DokumentPZService(DataContext context) : base(context)
        {
        }

        public async Task<DokumentPZIndexDto> GetIndexDataAsync(string? searchTerm)
        {
            var query = _context.DokumentPZ
                .AsNoTracking()
                .Include(d => d.Dostawca)
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
                    (x.Dostawca != null && EF.Functions.Like(x.Dostawca.Nazwa, $"%{term}%")) ||
                    (x.Magazyn != null && EF.Functions.Like(x.Magazyn.Nazwa, $"%{term}%")));
            }

            return new DokumentPZIndexDto
            {
                SearchTerm = searchTerm,
                Items = await query.ToListAsync()
            };
        }

        public async Task<DokumentPZDetailsDto?> GetDetailsDataAsync(int idDokumentu)
        {
            var dokument = await _context.DokumentPZ
                .AsNoTracking()
                .Include(d => d.Dostawca)
                .Include(d => d.Magazyn)
                .Include(d => d.Utworzyl)
                .FirstOrDefaultAsync(d => d.Id == idDokumentu);
            if (dokument == null)
            {
                return null;
            }

            var pozycje = await _context.PozycjaPZ
                .AsNoTracking()
                .Include(p => p.Produkt)
                    .ThenInclude(p => p.DomyslnaJednostka)
                .Include(p => p.Lokacja)
                    .ThenInclude(l => l.Magazyn)
                .Where(p => p.IdDokumentu == idDokumentu)
                .OrderBy(p => p.Lp)
                .ThenBy(p => p.Id)
                .ToListAsync();

            return new DokumentPZDetailsDto
            {
                Dokument = dokument,
                LiczbaPozycji = pozycje.Count,
                SumaIlosci = pozycje.Sum(p => p.Ilosc),
                SumaWartosci = pozycje.Sum(p => p.Ilosc * (p.CenaJednostkowa ?? 0m)),
                Pozycje = pozycje
            };
        }

        public async Task<DokumentPZDeleteDto?> GetDeleteDataAsync(int idDokumentu)
        {
            var dokument = await _context.DokumentPZ
                .AsNoTracking()
                .Include(d => d.Dostawca)
                .Include(d => d.Magazyn)
                .Include(d => d.Utworzyl)
                .FirstOrDefaultAsync(d => d.Id == idDokumentu);
            if (dokument == null)
            {
                return null;
            }

            var liczbaPozycji = await _context.PozycjaPZ
                .AsNoTracking()
                .CountAsync(p => p.IdDokumentu == idDokumentu);

            return new DokumentPZDeleteDto
            {
                Dokument = dokument,
                LiczbaPozycji = liczbaPozycji
            };
        }

        public async Task<DokumentPZPostResultDto> PostAsync(int idDokumentu)
        {
            var dokument = await _context.DokumentPZ
                .FirstOrDefaultAsync(d => d.Id == idDokumentu);
            if (dokument == null)
            {
                return new DokumentPZPostResultDto { Success = false, ErrorMessage = "Nie znaleziono dokumentu PZ." };
            }

            if (!string.Equals(dokument.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            {
                return new DokumentPZPostResultDto { Success = false, ErrorMessage = "Zaksięgować można tylko dokument PZ w statusie Draft." };
            }

            var pozycje = await _context.PozycjaPZ
                .Include(p => p.Lokacja)
                .Where(p => p.IdDokumentu == idDokumentu)
                .OrderBy(p => p.Lp)
                .ThenBy(p => p.Id)
                .ToListAsync();

            if (pozycje.Count == 0)
            {
                return new DokumentPZPostResultDto { Success = false, ErrorMessage = "Nie można zaksięgować dokumentu PZ bez pozycji." };
            }

            if (pozycje.Any(p => p.Ilosc <= 0))
            {
                return new DokumentPZPostResultDto { Success = false, ErrorMessage = "Wszystkie pozycje PZ muszą mieć ilość większą od zera." };
            }

            if (pozycje.Any(p => p.Lokacja == null || p.Lokacja.IdMagazynu != dokument.IdMagazynu))
            {
                return new DokumentPZPostResultDto { Success = false, ErrorMessage = "Wszystkie lokacje pozycji PZ muszą należeć do magazynu dokumentu." };
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var now = DateTime.UtcNow;
                var stockKeys = pozycje
                    .Select(p => new { p.IdProduktu, p.IdLokacji })
                    .Distinct()
                    .ToList();

                var productIds = stockKeys.Select(x => x.IdProduktu).Distinct().ToList();
                var locationIds = stockKeys.Select(x => x.IdLokacji).Distinct().ToList();

                var stanyByKey = await _context.StanMagazynowy
                    .Where(s => productIds.Contains(s.IdProduktu) && locationIds.Contains(s.IdLokacji))
                    .ToDictionaryAsync(s => (s.IdProduktu, s.IdLokacji));

                foreach (var pozycja in pozycje)
                {
                    var key = (pozycja.IdProduktu, pozycja.IdLokacji);
                    stanyByKey.TryGetValue(key, out var stan);

                    if (stan == null)
                    {
                        stan = new StanMagazynowy
                        {
                            IdProduktu = pozycja.IdProduktu,
                            IdLokacji = pozycja.IdLokacji,
                            Ilosc = 0m
                        };
                        _context.StanMagazynowy.Add(stan);
                        stanyByKey[key] = stan;
                    }
                    stan.Ilosc += pozycja.Ilosc;

                    _context.RuchMagazynowy.Add(new RuchMagazynowy
                    {
                        Typ = TypRuchuMagazynowego.Przyjęcie,
                        IdProduktu = pozycja.IdProduktu,
                        IdLokacjiDo = pozycja.IdLokacji,
                        IdLokacjiZ = null,
                        Ilosc = pozycja.Ilosc,
                        Referencja = dokument.Numer,
                        Notatka = $"PZ {dokument.Numer}, lp {pozycja.Lp}",
                        UtworzonoUtc = now,
                        IdUzytkownika = dokument.IdUtworzyl
                    });
                }

                dokument.Status = "Posted";
                dokument.ZaksiegowanoUtc = now;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new DokumentPZPostResultDto { Success = true };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
