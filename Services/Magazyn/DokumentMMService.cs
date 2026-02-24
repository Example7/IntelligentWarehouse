using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;
using Interfaces.Magazyn.Dtos;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.Magazyn
{
    public class DokumentMMService : BaseService, IDokumentMMService
    {
        public DokumentMMService(DataContext context) : base(context)
        {
        }

        public async Task<DokumentMMIndexDto> GetIndexDataAsync(string? searchTerm)
        {
            var query = _context.DokumentMM
                .AsNoTracking()
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
                    (x.Magazyn != null && EF.Functions.Like(x.Magazyn.Nazwa, $"%{term}%")) ||
                    (x.Utworzyl != null && EF.Functions.Like(x.Utworzyl.Email, $"%{term}%")));
            }

            return new DokumentMMIndexDto
            {
                SearchTerm = searchTerm,
                Items = await query.ToListAsync()
            };
        }

        public async Task<DokumentMMDetailsDto?> GetDetailsDataAsync(int idDokumentu)
        {
            var dokument = await _context.DokumentMM
                .AsNoTracking()
                .Include(d => d.Magazyn)
                .Include(d => d.Utworzyl)
                .FirstOrDefaultAsync(d => d.Id == idDokumentu);
            if (dokument == null)
            {
                return null;
            }

            var pozycje = await _context.PozycjaMM
                .AsNoTracking()
                .Include(p => p.Produkt)
                    .ThenInclude(p => p.DomyslnaJednostka)
                .Include(p => p.LokacjaZ)
                    .ThenInclude(l => l.Magazyn)
                .Include(p => p.LokacjaDo)
                    .ThenInclude(l => l.Magazyn)
                .Where(p => p.IdDokumentu == idDokumentu)
                .OrderBy(p => p.Lp)
                .ThenBy(p => p.Id)
                .ToListAsync();

            return new DokumentMMDetailsDto
            {
                Dokument = dokument,
                LiczbaPozycji = pozycje.Count,
                SumaIlosci = pozycje.Sum(p => p.Ilosc),
                Pozycje = pozycje
            };
        }

        public async Task<DokumentMMDeleteDto?> GetDeleteDataAsync(int idDokumentu)
        {
            var dokument = await _context.DokumentMM
                .AsNoTracking()
                .Include(d => d.Magazyn)
                .Include(d => d.Utworzyl)
                .FirstOrDefaultAsync(d => d.Id == idDokumentu);
            if (dokument == null)
            {
                return null;
            }

            var liczbaPozycji = await _context.PozycjaMM.AsNoTracking().CountAsync(p => p.IdDokumentu == idDokumentu);
            return new DokumentMMDeleteDto { Dokument = dokument, LiczbaPozycji = liczbaPozycji };
        }

        public async Task<DokumentMMPostResultDto> PostAsync(int idDokumentu)
        {
            var dokument = await _context.DokumentMM.FirstOrDefaultAsync(d => d.Id == idDokumentu);
            if (dokument == null)
            {
                return new DokumentMMPostResultDto { Success = false, ErrorMessage = "Nie znaleziono dokumentu MM." };
            }

            if (!string.Equals(dokument.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            {
                return new DokumentMMPostResultDto { Success = false, ErrorMessage = "Zaksięgować można tylko dokument MM w statusie Draft." };
            }

            var pozycje = await _context.PozycjaMM
                .Include(p => p.LokacjaZ)
                .Include(p => p.LokacjaDo)
                .Where(p => p.IdDokumentu == idDokumentu)
                .OrderBy(p => p.Lp)
                .ThenBy(p => p.Id)
                .ToListAsync();

            if (pozycje.Count == 0)
            {
                return new DokumentMMPostResultDto { Success = false, ErrorMessage = "Nie mozna zaksięgować dokumentu MM bez pozycji." };
            }

            if (pozycje.Any(p => p.Ilosc <= 0))
            {
                return new DokumentMMPostResultDto { Success = false, ErrorMessage = "Wszystkie pozycje MM muszą mieć ilość większą od zera." };
            }

            if (pozycje.Any(p => p.LokacjaZ == null || p.LokacjaDo == null))
            {
                return new DokumentMMPostResultDto { Success = false, ErrorMessage = "Wszystkie pozycje MM muszą mieć lokacje Z i Do." };
            }

            if (pozycje.Any(p => p.IdLokacjiZ == p.IdLokacjiDo))
            {
                return new DokumentMMPostResultDto { Success = false, ErrorMessage = "Lokacja Z i Do w pozycji MM muszą być różne." };
            }

            if (pozycje.Any(p => p.LokacjaZ.IdMagazynu != dokument.IdMagazynu || p.LokacjaDo.IdMagazynu != dokument.IdMagazynu))
            {
                return new DokumentMMPostResultDto { Success = false, ErrorMessage = "Wszystkie lokacje pozycji MM muszą należeć do magazynu dokumentu." };
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var now = DateTime.UtcNow;
                var stockKeys = pozycje
                    .SelectMany(p => new[]
                    {
                        (p.IdProduktu, p.IdLokacjiZ),
                        (p.IdProduktu, p.IdLokacjiDo)
                    })
                    .Distinct()
                    .ToList();

                var productIds = stockKeys.Select(x => x.IdProduktu).Distinct().ToList();
                var locationIds = stockKeys.Select(x => x.Item2).Distinct().ToList();

                var stanyByKey = await _context.StanMagazynowy
                    .Where(s => productIds.Contains(s.IdProduktu) && locationIds.Contains(s.IdLokacji))
                    .ToDictionaryAsync(s => (s.IdProduktu, s.IdLokacji));

                foreach (var pozycja in pozycje)
                {
                    var keyZ = (pozycja.IdProduktu, pozycja.IdLokacjiZ);
                    var keyDo = (pozycja.IdProduktu, pozycja.IdLokacjiDo);

                    stanyByKey.TryGetValue(keyZ, out var stanZ);
                    if (stanZ == null)
                    {
                        return new DokumentMMPostResultDto
                        {
                            Success = false,
                            ErrorMessage = $"Brak stanu magazynowego dla produktu {pozycja.IdProduktu} w lokacji Z {pozycja.LokacjaZ?.Kod ?? pozycja.IdLokacjiZ.ToString()}."
                        };
                    }

                    if (stanZ.Ilosc < pozycja.Ilosc)
                    {
                        return new DokumentMMPostResultDto
                        {
                            Success = false,
                            ErrorMessage = $"Niewystarczajacy stan w lokacji Z {pozycja.LokacjaZ?.Kod ?? pozycja.IdLokacjiZ.ToString()} dla produktu {pozycja.IdProduktu}. Dostępne: {stanZ.Ilosc:0.###}."
                        };
                    }

                    stanZ.Ilosc -= pozycja.Ilosc;

                    if (!stanyByKey.TryGetValue(keyDo, out var stanDo) || stanDo == null)
                    {
                        stanDo = new StanMagazynowy
                        {
                            IdProduktu = pozycja.IdProduktu,
                            IdLokacji = pozycja.IdLokacjiDo,
                            Ilosc = 0m
                        };
                        _context.StanMagazynowy.Add(stanDo);
                        stanyByKey[keyDo] = stanDo;
                    }

                    stanDo.Ilosc += pozycja.Ilosc;

                    _context.RuchMagazynowy.Add(new RuchMagazynowy
                    {
                        Typ = (TypRuchuMagazynowego)3,
                        IdProduktu = pozycja.IdProduktu,
                        IdLokacjiZ = pozycja.IdLokacjiZ,
                        IdLokacjiDo = pozycja.IdLokacjiDo,
                        Ilosc = pozycja.Ilosc,
                        Referencja = dokument.Numer,
                        Notatka = $"MM {dokument.Numer}, lp {pozycja.Lp}",
                        UtworzonoUtc = now,
                        IdUzytkownika = dokument.IdUtworzyl
                    });
                }

                dokument.Status = "Posted";
                dokument.ZaksiegowanoUtc = now;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new DokumentMMPostResultDto { Success = true };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
