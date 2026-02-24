using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;
using Interfaces.Magazyn.Dtos;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.Magazyn
{
    public class InwentaryzacjaService : BaseService, IInwentaryzacjaService
    {
        public InwentaryzacjaService(DataContext context) : base(context)
        {
        }

        public async Task<InwentaryzacjaIndexDto> GetIndexDataAsync(string? searchTerm)
        {
            var query = _context.Inwentaryzacja
                .AsNoTracking()
                .Include(i => i.Magazyn)
                .Include(i => i.Utworzyl)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(i =>
                    EF.Functions.Like(i.Numer, $"%{term}%") ||
                    EF.Functions.Like(i.Status, $"%{term}%") ||
                    (i.Notatka != null && EF.Functions.Like(i.Notatka, $"%{term}%")) ||
                    (i.Magazyn != null && EF.Functions.Like(i.Magazyn.Nazwa, $"%{term}%")) ||
                    (i.Utworzyl != null && EF.Functions.Like(i.Utworzyl.Email, $"%{term}%")));
            }

            return new InwentaryzacjaIndexDto
            {
                SearchTerm = searchTerm,
                Items = await query.ToListAsync()
            };
        }

        public async Task<InwentaryzacjaDetailsDto?> GetDetailsDataAsync(int idDokumentu)
        {
            var dokument = await _context.Inwentaryzacja
                .AsNoTracking()
                .Include(i => i.Magazyn)
                .Include(i => i.Utworzyl)
                .FirstOrDefaultAsync(i => i.Id == idDokumentu);
            if (dokument == null)
            {
                return null;
            }

            var pozycje = await _context.PozycjaInwentaryzacji
                .AsNoTracking()
                .Include(p => p.Produkt).ThenInclude(p => p.DomyslnaJednostka)
                .Include(p => p.Lokacja).ThenInclude(l => l.Magazyn)
                .Where(p => p.IdDokumentu == idDokumentu)
                .OrderBy(p => p.Lp)
                .ThenBy(p => p.Id)
                .ToListAsync();

            return new InwentaryzacjaDetailsDto
            {
                Dokument = dokument,
                LiczbaPozycji = pozycje.Count,
                SumaOczekiwana = pozycje.Sum(p => p.IloscOczekiwana),
                SumaPoliczona = pozycje.Sum(p => p.IloscPoliczona),
                SumaRoznic = pozycje.Sum(p => p.IloscPoliczona - p.IloscOczekiwana),
                Pozycje = pozycje
            };
        }

        public async Task<InwentaryzacjaDeleteDto?> GetDeleteDataAsync(int idDokumentu)
        {
            var dokument = await _context.Inwentaryzacja
                .AsNoTracking()
                .Include(i => i.Magazyn)
                .Include(i => i.Utworzyl)
                .FirstOrDefaultAsync(i => i.Id == idDokumentu);
            if (dokument == null)
            {
                return null;
            }

            var liczbaPozycji = await _context.PozycjaInwentaryzacji.AsNoTracking().CountAsync(p => p.IdDokumentu == idDokumentu);
            return new InwentaryzacjaDeleteDto { Dokument = dokument, LiczbaPozycji = liczbaPozycji };
        }

        public async Task<InwentaryzacjaCloseResultDto> CloseAsync(int idDokumentu)
        {
            var dokument = await _context.Inwentaryzacja.FirstOrDefaultAsync(i => i.Id == idDokumentu);
            if (dokument == null)
            {
                return new InwentaryzacjaCloseResultDto { Success = false, ErrorMessage = "Nie znaleziono dokumentu inwentaryzacji." };
            }

            if (!string.Equals(dokument.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            {
                return new InwentaryzacjaCloseResultDto { Success = false, ErrorMessage = "Zamknąć można tylko inwentaryzacje w statusie Draft." };
            }

            var pozycje = await _context.PozycjaInwentaryzacji
                .Include(p => p.Lokacja)
                .Where(p => p.IdDokumentu == idDokumentu)
                .OrderBy(p => p.Lp)
                .ThenBy(p => p.Id)
                .ToListAsync();

            if (pozycje.Count == 0)
            {
                return new InwentaryzacjaCloseResultDto { Success = false, ErrorMessage = "Nie można zamknąć inwentaryzacji bez pozycji." };
            }

            if (pozycje.Any(p => p.IloscOczekiwana < 0 || p.IloscPoliczona < 0))
            {
                return new InwentaryzacjaCloseResultDto { Success = false, ErrorMessage = "Ilości oczekiwane i policzone nie mogą być ujemne." };
            }

            if (pozycje.Any(p => p.Lokacja == null || p.Lokacja.IdMagazynu != dokument.IdMagazynu))
            {
                return new InwentaryzacjaCloseResultDto { Success = false, ErrorMessage = "Wszystkie lokacje pozycji inwentaryzacji muszą należeć do magazynu dokumentu." };
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var now = DateTime.UtcNow;
                var keys = pozycje.Select(p => (p.IdProduktu, p.IdLokacji)).Distinct().ToList();
                var productIds = keys.Select(x => x.IdProduktu).Distinct().ToList();
                var locationIds = keys.Select(x => x.IdLokacji).Distinct().ToList();

                var stanyByKey = await _context.StanMagazynowy
                    .Where(s => productIds.Contains(s.IdProduktu) && locationIds.Contains(s.IdLokacji))
                    .ToDictionaryAsync(s => (s.IdProduktu, s.IdLokacji));

                foreach (var p in pozycje)
                {
                    var key = (p.IdProduktu, p.IdLokacji);
                    if (!stanyByKey.TryGetValue(key, out var stan) || stan == null)
                    {
                        stan = new StanMagazynowy
                        {
                            IdProduktu = p.IdProduktu,
                            IdLokacji = p.IdLokacji,
                            Ilosc = 0m
                        };
                        _context.StanMagazynowy.Add(stan);
                        stanyByKey[key] = stan;
                    }

                    var roznica = p.IloscPoliczona - p.IloscOczekiwana;
                    stan.Ilosc = p.IloscPoliczona;

                    if (roznica == 0)
                    {
                        continue;
                    }

                    _context.RuchMagazynowy.Add(new RuchMagazynowy
                    {
                        Typ = TypRuchuMagazynowego.Inwentaryzacja,
                        IdProduktu = p.IdProduktu,
                        IdLokacjiZ = roznica < 0 ? p.IdLokacji : null,
                        IdLokacjiDo = roznica > 0 ? p.IdLokacji : null,
                        Ilosc = Math.Abs(roznica),
                        Referencja = dokument.Numer,
                        Notatka = $"INV {dokument.Numer}, lp {p.Lp}",
                        UtworzonoUtc = now,
                        IdUzytkownika = dokument.IdUtworzyl
                    });
                }

                dokument.Status = "Closed";
                dokument.KoniecUtc = now;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new InwentaryzacjaCloseResultDto { Success = true };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
