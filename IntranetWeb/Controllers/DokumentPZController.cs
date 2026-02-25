using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;
using IntranetWeb.Controllers.Abstrakcja;
using IntranetWeb.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IntranetWeb.Controllers
{
    [Authorize(Roles = AppRoles.AdminMagazynierOperator)]
    public class DokumentPZController : BaseSearchController<DokumentPZ>
    {
        private readonly IDokumentPZService _dokumentPzService;
        private readonly IWydrukDokumentuService _wydrukDokumentuService;
        private static readonly string[] DozwoloneStatusyCreate = ["Draft"];
        private static readonly string[] DozwoloneStatusyEdit = ["Draft", "Cancelled"];

        public DokumentPZController(DataContext context, IDokumentPZService dokumentPzService, IWydrukDokumentuService wydrukDokumentuService) : base(context)
        {
            _dokumentPzService = dokumentPzService;
            _wydrukDokumentuService = wydrukDokumentuService;
        }

        public async Task<IActionResult> Index(string? searchTerm)
        {
            var model = await _dokumentPzService.GetIndexDataAsync(searchTerm);
            return View(model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _dokumentPzService.GetDetailsDataAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        public IActionResult Create()
        {
            var model = new DokumentPZ
            {
                Status = "Draft",
                DataPrzyjeciaUtc = DateTime.UtcNow
            };

            UzupelnijDaneFormularza(model);
            ViewData["StatusOptions"] = BuildStatusSelectList(DozwoloneStatusyCreate, model.Status);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Numer,IdMagazynu,IdDostawcy,DataPrzyjeciaUtc,IdUtworzyl,Notatka")] DokumentPZ dokumentPZ)
        {
            dokumentPZ.Status = "Draft";
            dokumentPZ.Numer = dokumentPZ.Numer?.Trim() ?? string.Empty;

            if (await CzyNumerDokumentuPzJuzIstniejeAsync(dokumentPZ.Numer))
            {
                ModelState.AddModelError(nameof(DokumentPZ.Numer), $"Dokument PZ o numerze '{dokumentPZ.Numer}' już istnieje.");
            }

            if (await CzyNumerDokumentuPzJuzIstniejeAsync(dokumentPZ.Numer, dokumentPZ.Id))
            {
                ModelState.AddModelError(nameof(DokumentPZ.Numer), $"Dokument PZ o numerze '{dokumentPZ.Numer}' juz istnieje.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    dokumentPZ.ZaksiegowanoUtc = null;
                    _context.Add(dokumentPZ);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError(nameof(DokumentPZ.Numer), $"Dokument PZ o numerze '{dokumentPZ.Numer}' już istnieje.");
                }
            }

            UzupelnijDaneFormularza(dokumentPZ);
            ViewData["StatusOptions"] = BuildStatusSelectList(DozwoloneStatusyCreate, dokumentPZ.Status);
            return View(dokumentPZ);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dokumentPZ = await _context.DokumentPZ
                .Include(d => d.Utworzyl)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (dokumentPZ == null)
            {
                return NotFound();
            }

            if (!string.Equals(dokumentPZ.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            {
                TempData["DokumentPZEditBlocked"] = "Edycja jest dostępna tylko dla dokumentow PZ w statusie Draft.";
                return RedirectToAction(nameof(Details), new { id = dokumentPZ.Id });
            }

            UzupelnijDaneFormularza(dokumentPZ);
            ViewData["StatusOptions"] = BuildStatusSelectList(DozwoloneStatusyEdit, dokumentPZ.Status);
            ViewData["AutorDokumentuEmail"] = dokumentPZ.Utworzyl?.Email ?? "-";
            return View(dokumentPZ);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Numer,IdMagazynu,IdDostawcy,Status,DataPrzyjeciaUtc,IdUtworzyl,Notatka,RowVersion")] DokumentPZ dokumentPZ)
        {
            if (id != dokumentPZ.Id)
            {
                return NotFound();
            }

            dokumentPZ.Numer = dokumentPZ.Numer?.Trim() ?? string.Empty;

            if (!DozwoloneStatusyEdit.Contains(dokumentPZ.Status))
            {
                ModelState.AddModelError(nameof(DokumentPZ.Status), "W edycji dozwolone są tylko statusy Draft lub Cancelled. Użyj akcji Zaksięguj dla statusu Posted.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.DokumentPZ.FirstOrDefaultAsync(x => x.Id == id);
                    if (existing == null)
                    {
                        return NotFound();
                    }

                    if (!string.Equals(existing.Status, "Draft", StringComparison.OrdinalIgnoreCase))
                    {
                        ModelState.AddModelError(string.Empty, "Edytować można tylko dokumenty PZ w statusie Draft.");
                        return await ReturnEditViewWithLookupsAsync(dokumentPZ);
                    }

                    existing.Numer = dokumentPZ.Numer;
                    existing.IdMagazynu = dokumentPZ.IdMagazynu;
                    existing.IdDostawcy = dokumentPZ.IdDostawcy;
                    existing.Status = dokumentPZ.Status;
                    existing.DataPrzyjeciaUtc = dokumentPZ.DataPrzyjeciaUtc;
                    existing.Notatka = dokumentPZ.Notatka;

                    _context.Entry(existing).Property(x => x.RowVersion).OriginalValue = dokumentPZ.RowVersion;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DokumentPZExists(dokumentPZ.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError(nameof(DokumentPZ.Numer), $"Dokument PZ o numerze '{dokumentPZ.Numer}' już istnieje.");
                    return await ReturnEditViewWithLookupsAsync(dokumentPZ);
                }

                return RedirectToAction(nameof(Index));
            }

            return await ReturnEditViewWithLookupsAsync(dokumentPZ);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostDocument(int id)
        {
            var result = await _dokumentPzService.PostAsync(id);
            if (result.Success)
            {
                TempData["DokumentPZPostSuccess"] = "Dokument PZ został zaksięgowany.";
            }
            else
            {
                TempData["DokumentPZPostError"] = result.ErrorMessage ?? "Nie udało się zaksięgować dokumentu PZ.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Wydruk(int id, int? idSzablonu)
        {
            try
            {
                var file = await _wydrukDokumentuService.GenerujWydrukPzAsync(id, idSzablonu);
                if (file.UzytoSzablonuAwaryjnego && !string.IsNullOrWhiteSpace(file.KomunikatInformacyjny))
                {
                    TempData["DokumentPZPrintInfo"] = file.KomunikatInformacyjny;
                }
                return File(file.Content, file.ContentType, file.FileName);
            }
            catch (Exception ex)
            {
                TempData["DokumentPZPrintError"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [Authorize(Roles = AppRoles.AdminOnly)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _dokumentPzService.GetDeleteDataAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [Authorize(Roles = AppRoles.AdminOnly)]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deleteData = await _dokumentPzService.GetDeleteDataAsync(id);
            if (deleteData == null)
            {
                return NotFound();
            }

            if (!deleteData.CzyMoznaUsunac)
            {
                ModelState.AddModelError(string.Empty, $"Nie można usunąć dokumentu PZ, ponieważ ma powiązane pozycje ({deleteData.LiczbaPozycji}).");
                return View("Delete", deleteData);
            }

            var dokumentPZ = await _context.DokumentPZ.FindAsync(id);
            if (dokumentPZ != null)
            {
                _context.DokumentPZ.Remove(dokumentPZ);
            }

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Nie udało się usunąć dokumentu PZ, ponieważ ma powiązane rekordy (np. pozycje PZ).");
                deleteData = await _dokumentPzService.GetDeleteDataAsync(id);
                if (deleteData == null)
                {
                    return NotFound();
                }
                return View("Delete", deleteData);
            }
        }

        private bool DokumentPZExists(int id)
        {
            return _context.DokumentPZ.Any(e => e.Id == id);
        }

        private Task<bool> CzyNumerDokumentuPzJuzIstniejeAsync(string numer, int? zWykluczeniemId = null)
        {
            if (string.IsNullOrWhiteSpace(numer))
            {
                return Task.FromResult(false);
            }

            return _context.DokumentPZ.AsNoTracking().AnyAsync(d =>
                d.Numer == numer &&
                (!zWykluczeniemId.HasValue || d.Id != zWykluczeniemId.Value));
        }

        private void UzupelnijDaneFormularza(DokumentPZ dokumentPZ)
        {
            ViewData["IdDostawcy"] = new SelectList(_context.Dostawca.AsNoTracking().OrderBy(x => x.Nazwa), "IdDostawcy", "Nazwa", dokumentPZ.IdDostawcy);
            ViewData["IdMagazynu"] = new SelectList(_context.Magazyn.AsNoTracking().OrderBy(x => x.Nazwa), "IdMagazynu", "Nazwa", dokumentPZ.IdMagazynu);
            ViewData["IdUtworzyl"] = new SelectList(_context.Uzytkownik.AsNoTracking().OrderBy(x => x.Email), "IdUzytkownika", "Email", dokumentPZ.IdUtworzyl);
        }

        private async Task<IActionResult> ReturnEditViewWithLookupsAsync(DokumentPZ dokumentPZ)
        {
            UzupelnijDaneFormularza(dokumentPZ);
            ViewData["StatusOptions"] = BuildStatusSelectList(DozwoloneStatusyEdit, dokumentPZ.Status);
            ViewData["AutorDokumentuEmail"] = await _context.Uzytkownik
                .Where(u => u.IdUzytkownika == dokumentPZ.IdUtworzyl)
                .Select(u => u.Email)
                .FirstOrDefaultAsync() ?? "-";
            return View(dokumentPZ);
        }

        private static IReadOnlyList<SelectListItem> BuildStatusSelectList(IEnumerable<string> allowedStatuses, string? selected)
        {
            return allowedStatuses
                .Select(x => new SelectListItem(x, x, string.Equals(x, selected, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }
    }
}
