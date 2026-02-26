using Interfaces.Magazyn;
using IntranetWeb.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Data.Data.Magazyn;
using Data.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

namespace IntranetWeb.Controllers
{
    [Authorize(Roles = AppRoles.AdminMagazynierOperator)]
    public class RaportController : Controller
    {
        private readonly IRaportMagazynowyService _raportMagazynowyService;
        private readonly DataContext _context;

        public RaportController(IRaportMagazynowyService raportMagazynowyService, DataContext context)
        {
            _raportMagazynowyService = raportMagazynowyService;
            _context = context;
        }

        public async Task<IActionResult> StanyMagazynowe(string? searchTerm, int? idMagazynu)
        {
            var model = await _raportMagazynowyService.GetRaportStanowAsync(searchTerm, idMagazynu);
            return View(model);
        }

        public async Task<IActionResult> RuchyMagazynowe(string? searchTerm, int? idMagazynu, TypRuchuMagazynowego? typ, DateTime? dataOd, DateTime? dataDo)
        {
            var model = await _raportMagazynowyService.GetRaportRuchowAsync(searchTerm, idMagazynu, typ, dataOd, dataDo);
            return View(model);
        }

        public async Task<IActionResult> PropozycjeZamowien(string? searchTerm, int? idMagazynu, int? idDostawcy, int? idLokacjiPrzyjecia)
        {
            var model = await _raportMagazynowyService.GetRaportPropozycjiZamowienAsync(searchTerm, idMagazynu, idDostawcy, idLokacjiPrzyjecia);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UtworzPzDraftZPropozycji(
            string? searchTerm,
            int? idMagazynu,
            int? idDostawcy,
            int? idLokacjiPrzyjecia,
            List<string>? selectedRowKeys)
        {
            if (!idDostawcy.HasValue)
            {
                TempData["RaportReorderError"] = "Wybierz dostawcę przed utworzeniem PZ Draft.";
                return RedirectToAction(nameof(PropozycjeZamowien), new { searchTerm, idMagazynu, idDostawcy, idLokacjiPrzyjecia });
            }

            if (!idLokacjiPrzyjecia.HasValue)
            {
                TempData["RaportReorderError"] = "Wybierz lokację przyjęcia dla pozycji PZ.";
                return RedirectToAction(nameof(PropozycjeZamowien), new { searchTerm, idMagazynu, idDostawcy, idLokacjiPrzyjecia });
            }

            if (selectedRowKeys == null || selectedRowKeys.Count == 0)
            {
                TempData["RaportReorderError"] = "Zaznacz co najmniej jedną pozycję z raportu.";
                return RedirectToAction(nameof(PropozycjeZamowien), new { searchTerm, idMagazynu, idDostawcy, idLokacjiPrzyjecia });
            }

            var suggestions = await _raportMagazynowyService.GetRaportPropozycjiZamowienAsync(searchTerm, idMagazynu, idDostawcy, idLokacjiPrzyjecia);
            var selected = new List<Interfaces.Magazyn.Dtos.RaportPropozycjeZamowienRowDto>();

            foreach (var key in selectedRowKeys.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var parts = key.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (parts.Length != 2 ||
                    !int.TryParse(parts[0], out var warehouseId) ||
                    !int.TryParse(parts[1], out var productId))
                {
                    continue;
                }

                var row = suggestions.Rows.FirstOrDefault(r => r.IdMagazynu == warehouseId && r.IdProduktu == productId);
                if (row != null)
                {
                    selected.Add(row);
                }
            }

            if (selected.Count == 0)
            {
                TempData["RaportReorderError"] = "Nie znaleziono zaznaczonych pozycji w aktualnym raporcie (odśwież raport i spróbuj ponownie).";
                return RedirectToAction(nameof(PropozycjeZamowien), new { searchTerm, idMagazynu, idDostawcy, idLokacjiPrzyjecia });
            }

            var distinctWarehouses = selected.Select(x => x.IdMagazynu).Distinct().ToList();
            if (distinctWarehouses.Count != 1)
            {
                TempData["RaportReorderError"] = "Do jednego PZ Draft można wybrać pozycje tylko z jednego magazynu. Ustaw filtr magazynu i zaznacz pozycje ponownie.";
                return RedirectToAction(nameof(PropozycjeZamowien), new { searchTerm, idMagazynu, idDostawcy, idLokacjiPrzyjecia });
            }

            var warehouseIdForPz = distinctWarehouses[0];
            if (idMagazynu.HasValue && idMagazynu.Value != warehouseIdForPz)
            {
                TempData["RaportReorderError"] = "Wybrane pozycje nie należą do aktualnie filtrowanego magazynu.";
                return RedirectToAction(nameof(PropozycjeZamowien), new { searchTerm, idMagazynu, idDostawcy, idLokacjiPrzyjecia });
            }

            var location = await _context.Lokacja
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.IdLokacji == idLokacjiPrzyjecia.Value && l.CzyAktywna);
            if (location == null || location.IdMagazynu != warehouseIdForPz)
            {
                TempData["RaportReorderError"] = "Wybrana lokacja przyjęcia nie należy do magazynu wybranych pozycji.";
                return RedirectToAction(nameof(PropozycjeZamowien), new { searchTerm, idMagazynu, idDostawcy, idLokacjiPrzyjecia });
            }

            var supplierExists = await _context.Dostawca
                .AsNoTracking()
                .AnyAsync(d => d.IdDostawcy == idDostawcy.Value && d.CzyAktywny);
            if (!supplierExists)
            {
                TempData["RaportReorderError"] = "Wybrany dostawca jest nieprawidłowy lub nieaktywny.";
                return RedirectToAction(nameof(PropozycjeZamowien), new { searchTerm, idMagazynu, idDostawcy, idLokacjiPrzyjecia });
            }

            var currentUserId = TryGetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                TempData["RaportReorderError"] = "Nie można ustalić zalogowanego użytkownika.";
                return RedirectToAction(nameof(PropozycjeZamowien), new { searchTerm, idMagazynu, idDostawcy, idLokacjiPrzyjecia });
            }

            var rowsToCreate = selected
                .Where(r => r.ProponowanaIloscZamowienia > 0)
                .OrderBy(r => r.ProduktNazwa)
                .ThenBy(r => r.ProduktKod)
                .ToList();

            if (rowsToCreate.Count == 0)
            {
                TempData["RaportReorderError"] = "Wybrane pozycje nie mają dodatniej proponowanej ilości zamówienia.";
                return RedirectToAction(nameof(PropozycjeZamowien), new { searchTerm, idMagazynu, idDostawcy, idLokacjiPrzyjecia });
            }

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var now = DateTime.UtcNow;
                var pz = new DokumentPZ
                {
                    Numer = await GenerateNextPzNumberAsync(now.Year),
                    IdMagazynu = warehouseIdForPz,
                    IdDostawcy = idDostawcy.Value,
                    Status = "Draft",
                    DataPrzyjeciaUtc = now,
                    IdUtworzyl = currentUserId.Value,
                    ZaksiegowanoUtc = null,
                    Notatka = BuildPzSuggestionNote(rowsToCreate)
                };

                var lp = 1;
                foreach (var row in rowsToCreate)
                {
                    pz.Pozycje.Add(new PozycjaPZ
                    {
                        Lp = lp++,
                        IdProduktu = row.IdProduktu,
                        IdLokacji = idLokacjiPrzyjecia.Value,
                        IdPartii = null,
                        Ilosc = row.ProponowanaIloscZamowienia,
                        CenaJednostkowa = null
                    });
                }

                _context.DokumentPZ.Add(pz);
                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                TempData["DokumentPZCreateSuccess"] = $"Utworzono PZ Draft {pz.Numer} z {rowsToCreate.Count} pozycji raportu ROP/ROQ.";
                return RedirectToAction("Details", "DokumentPZ", new { id = pz.Id });
            }
            catch (DbUpdateException ex)
            {
                await tx.RollbackAsync();
                TempData["RaportReorderError"] = $"Nie udało się utworzyć PZ Draft: {ex.GetBaseException().Message}";
                return RedirectToAction(nameof(PropozycjeZamowien), new { searchTerm, idMagazynu, idDostawcy, idLokacjiPrzyjecia });
            }
        }

        public async Task<IActionResult> EksportStanowExcel(string? searchTerm, int? idMagazynu)
        {
            var bytes = await _raportMagazynowyService.ExportRaportStanowExcelAsync(searchTerm, idMagazynu);
            var date = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Raport_StanyMagazynowe_{date}.xlsx");
        }

        public async Task<IActionResult> EksportStanowPdf(string? searchTerm, int? idMagazynu)
        {
            var bytes = await _raportMagazynowyService.ExportRaportStanowPdfAsync(searchTerm, idMagazynu);
            var date = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return File(bytes, "application/pdf", $"Raport_StanyMagazynowe_{date}.pdf");
        }

        public async Task<IActionResult> EksportRuchowExcel(string? searchTerm, int? idMagazynu, TypRuchuMagazynowego? typ, DateTime? dataOd, DateTime? dataDo)
        {
            var bytes = await _raportMagazynowyService.ExportRaportRuchowExcelAsync(searchTerm, idMagazynu, typ, dataOd, dataDo);
            var date = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Raport_RuchyMagazynowe_{date}.xlsx");
        }

        public async Task<IActionResult> EksportRuchowPdf(string? searchTerm, int? idMagazynu, TypRuchuMagazynowego? typ, DateTime? dataOd, DateTime? dataDo)
        {
            var bytes = await _raportMagazynowyService.ExportRaportRuchowPdfAsync(searchTerm, idMagazynu, typ, dataOd, dataDo);
            var date = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return File(bytes, "application/pdf", $"Raport_RuchyMagazynowe_{date}.pdf");
        }

        private async Task<string> GenerateNextPzNumberAsync(int year)
        {
            var prefix = $"PZ/{year}/";
            var lastNumber = await _context.DokumentPZ
                .AsNoTracking()
                .Where(d => d.Numer.StartsWith(prefix))
                .OrderByDescending(d => d.Id)
                .Select(d => d.Numer)
                .FirstOrDefaultAsync();

            var next = 1;
            if (!string.IsNullOrWhiteSpace(lastNumber))
            {
                var suffix = lastNumber[prefix.Length..];
                if (int.TryParse(suffix, out var parsed))
                {
                    next = parsed + 1;
                }
            }

            return $"{prefix}{next:0000}";
        }

        private static string BuildPzSuggestionNote(IEnumerable<Interfaces.Magazyn.Dtos.RaportPropozycjeZamowienRowDto> rows)
        {
            var list = rows
                .Select(r => $"{r.ProduktKod}: {r.ProponowanaIloscZamowienia.ToString("0.###", CultureInfo.InvariantCulture)} {r.Jednostka}")
                .ToList();

            var joined = string.Join("; ", list);
            return $"Utworzono z raportu propozycji zamówień ROP/ROQ. Pozycje: {joined}";
        }

        private int? TryGetCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return int.TryParse(raw, out var userId) ? userId : null;
        }
    }
}