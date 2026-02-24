using Data.Data;
using Interfaces.Magazyn;
using Interfaces.Magazyn.Dtos;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.Magazyn
{
    public class MagazynService : BaseService, IMagazynService
    {
        public MagazynService(DataContext context) : base(context)
        {
        }

        public async Task<MagazynIndexDto> GetIndexDataAsync(string? searchTerm)
        {
            var query = _context.Magazyn.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(m =>
                    EF.Functions.Like(m.Nazwa, $"%{term}%") ||
                    (m.Adres != null && EF.Functions.Like(m.Adres, $"%{term}%")));
            }

            var magazyny = await query.ToListAsync();
            var idsMagazynow = magazyny.Select(m => m.IdMagazynu).Distinct().ToList();

            var lokacje = new Dictionary<int, int>();
            var pz = new Dictionary<int, int>();
            var wz = new Dictionary<int, int>();
            var mm = new Dictionary<int, int>();
            var rezerwacje = new Dictionary<int, int>();
            var alerty = new Dictionary<int, int>();

            if (idsMagazynow.Count > 0)
            {
                lokacje = await _context.Lokacja.AsNoTracking()
                    .Where(x => idsMagazynow.Contains(x.IdMagazynu))
                    .GroupBy(x => x.IdMagazynu)
                    .Select(g => new { Id = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Id, x => x.Count);

                pz = await _context.DokumentPZ.AsNoTracking()
                    .Where(x => idsMagazynow.Contains(x.IdMagazynu))
                    .GroupBy(x => x.IdMagazynu)
                    .Select(g => new { Id = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Id, x => x.Count);

                wz = await _context.DokumentWZ.AsNoTracking()
                    .Where(x => idsMagazynow.Contains(x.IdMagazynu))
                    .GroupBy(x => x.IdMagazynu)
                    .Select(g => new { Id = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Id, x => x.Count);

                mm = await _context.DokumentMM.AsNoTracking()
                    .Where(x => idsMagazynow.Contains(x.IdMagazynu))
                    .GroupBy(x => x.IdMagazynu)
                    .Select(g => new { Id = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Id, x => x.Count);

                rezerwacje = await _context.Rezerwacja.AsNoTracking()
                    .Where(x => idsMagazynow.Contains(x.IdMagazynu))
                    .GroupBy(x => x.IdMagazynu)
                    .Select(g => new { Id = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Id, x => x.Count);

                alerty = await _context.Alert.AsNoTracking()
                    .Where(x => idsMagazynow.Contains(x.IdMagazynu))
                    .GroupBy(x => x.IdMagazynu)
                    .Select(g => new { Id = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Id, x => x.Count);
            }

            return new MagazynIndexDto
            {
                SearchTerm = searchTerm,
                Items = magazyny.Select(m => new MagazynIndexItemDto
                {
                    Magazyn = m,
                    LiczbaLokacji = lokacje.TryGetValue(m.IdMagazynu, out var l) ? l : 0,
                    LiczbaDokumentowPz = pz.TryGetValue(m.IdMagazynu, out var pzCount) ? pzCount : 0,
                    LiczbaDokumentowWz = wz.TryGetValue(m.IdMagazynu, out var wzCount) ? wzCount : 0,
                    LiczbaDokumentowMm = mm.TryGetValue(m.IdMagazynu, out var mmCount) ? mmCount : 0,
                    LiczbaRezerwacji = rezerwacje.TryGetValue(m.IdMagazynu, out var r) ? r : 0,
                    LiczbaAlertow = alerty.TryGetValue(m.IdMagazynu, out var a) ? a : 0
                }).ToList()
            };
        }

        public async Task<MagazynDetailsDto?> GetMagazynDetailsData(int idMagazynu)
        {
            var magazyn = await _context.Magazyn
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdMagazynu == idMagazynu);

            if (magazyn == null)
            {
                return null;
            }

            var lokacjeMagazynu = await _context.Lokacja
                .AsNoTracking()
                .Where(l => l.IdMagazynu == idMagazynu)
                .OrderBy(l => l.Kod)
                .ThenBy(l => l.Opis)
                .ToListAsync();

            var regulyAlertowMagazynu = await _context.RegulaAlertu
                .AsNoTracking()
                .Include(r => r.Produkt)
                .Where(r => r.IdMagazynu == idMagazynu)
                .OrderBy(r => r.Typ)
                .ThenBy(r => r.Produkt != null ? r.Produkt.Kod : string.Empty)
                .ToListAsync();

            var ostatnieAlertyMagazynu = await _context.Alert
                .AsNoTracking()
                .Include(a => a.Produkt)
                .Include(a => a.Regula)
                .Where(a => a.IdMagazynu == idMagazynu)
                .OrderByDescending(a => a.UtworzonoUtc)
                .ThenByDescending(a => a.Id)
                .Take(10)
                .ToListAsync();

            var ostatnieDokumentyPz = await _context.DokumentPZ
                .AsNoTracking()
                .Where(d => d.IdMagazynu == idMagazynu)
                .OrderByDescending(d => d.DataPrzyjeciaUtc)
                .ThenByDescending(d => d.Id)
                .Take(5)
                .ToListAsync();

            var ostatnieDokumentyWz = await _context.DokumentWZ
                .AsNoTracking()
                .Where(d => d.IdMagazynu == idMagazynu)
                .OrderByDescending(d => d.DataWydaniaUtc)
                .ThenByDescending(d => d.Id)
                .Take(5)
                .ToListAsync();

            var ostatnieDokumentyMm = await _context.DokumentMM
                .AsNoTracking()
                .Where(d => d.IdMagazynu == idMagazynu)
                .OrderByDescending(d => d.DataUtc)
                .ThenByDescending(d => d.Id)
                .Take(5)
                .ToListAsync();

            var ostatnieRezerwacjeMagazynu = await _context.Rezerwacja
                .AsNoTracking()
                .Where(r => r.IdMagazynu == idMagazynu)
                .OrderByDescending(r => r.UtworzonoUtc)
                .ThenByDescending(r => r.Id)
                .Take(10)
                .ToListAsync();

            var ostatnieInwentaryzacjeMagazynu = await _context.Inwentaryzacja
                .AsNoTracking()
                .Where(i => i.IdMagazynu == idMagazynu)
                .OrderByDescending(i => i.StartUtc)
                .ThenByDescending(i => i.Id)
                .Take(10)
                .ToListAsync();

            var liczbaLokacji = await _context.Lokacja.CountAsync(x => x.IdMagazynu == idMagazynu);
            var liczbaDokumentowPz = await _context.DokumentPZ.CountAsync(x => x.IdMagazynu == idMagazynu);
            var liczbaDokumentowWz = await _context.DokumentWZ.CountAsync(x => x.IdMagazynu == idMagazynu);
            var liczbaDokumentowMm = await _context.DokumentMM.CountAsync(x => x.IdMagazynu == idMagazynu);
            var liczbaInwentaryzacji = await _context.Inwentaryzacja.CountAsync(x => x.IdMagazynu == idMagazynu);
            var liczbaRezerwacji = await _context.Rezerwacja.CountAsync(x => x.IdMagazynu == idMagazynu);
            var liczbaRegulAlertow = await _context.RegulaAlertu.CountAsync(x => x.IdMagazynu == idMagazynu);
            var liczbaAlertow = await _context.Alert.CountAsync(x => x.IdMagazynu == idMagazynu);

            return new MagazynDetailsDto
            {
                Magazyn = magazyn,
                LokacjeMagazynu = lokacjeMagazynu,
                RegulyAlertowMagazynu = regulyAlertowMagazynu,
                OstatnieAlertyMagazynu = ostatnieAlertyMagazynu,
                OstatnieDokumentyPz = ostatnieDokumentyPz,
                OstatnieDokumentyWz = ostatnieDokumentyWz,
                OstatnieDokumentyMm = ostatnieDokumentyMm,
                OstatnieRezerwacjeMagazynu = ostatnieRezerwacjeMagazynu,
                OstatnieInwentaryzacjeMagazynu = ostatnieInwentaryzacjeMagazynu,
                LiczbaLokacji = liczbaLokacji,
                LiczbaDokumentowPz = liczbaDokumentowPz,
                LiczbaDokumentowWz = liczbaDokumentowWz,
                LiczbaDokumentowMm = liczbaDokumentowMm,
                LiczbaInwentaryzacji = liczbaInwentaryzacji,
                LiczbaRezerwacji = liczbaRezerwacji,
                LiczbaRegulAlertow = liczbaRegulAlertow,
                LiczbaAlertow = liczbaAlertow
            };
        }
    }
}
