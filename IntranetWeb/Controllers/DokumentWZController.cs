using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;
using IntranetWeb.Controllers.Abstrakcja;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IntranetWeb.Controllers
{
    public class DokumentWZController : BaseSearchController<DokumentWZ>
    {
        private readonly IDokumentWZService _dokumentWzService;
        private readonly IWydrukDokumentuService _wydrukDokumentuService;
        private static readonly string[] DozwoloneStatusyCreate = ["Draft"];
        private static readonly string[] DozwoloneStatusyEdit = ["Draft", "Cancelled"];

        public DokumentWZController(
            DataContext context,
            IDokumentWZService dokumentWzService,
            IWydrukDokumentuService wydrukDokumentuService) : base(context)
        {
            _dokumentWzService = dokumentWzService;
            _wydrukDokumentuService = wydrukDokumentuService;
        }

        public async Task<IActionResult> Index(string? searchTerm)
        {
            var model = await _dokumentWzService.GetIndexDataAsync(searchTerm);
            return View(model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _dokumentWzService.GetDetailsDataAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        public IActionResult Create()
        {
            var model = new DokumentWZ
            {
                Status = "Draft",
                DataWydaniaUtc = DateTime.UtcNow
            };

            UzupelnijDaneFormularza(model);
            ViewData["StatusOptions"] = BuildStatusSelectList(DozwoloneStatusyCreate, model.Status);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Numer,IdMagazynu,IdKlienta,DataWydaniaUtc,IdUtworzyl,Notatka")] DokumentWZ dokumentWZ)
        {
            dokumentWZ.Status = "Draft";
            dokumentWZ.Numer = dokumentWZ.Numer?.Trim() ?? string.Empty;

            if (await CzyNumerDokumentuWzJuzIstniejeAsync(dokumentWZ.Numer))
            {
                ModelState.AddModelError(nameof(DokumentWZ.Numer), $"Dokument WZ o numerze '{dokumentWZ.Numer}' już istnieje.");
            }

            if (await CzyNumerDokumentuWzJuzIstniejeAsync(dokumentWZ.Numer, dokumentWZ.Id))
            {
                ModelState.AddModelError(nameof(DokumentWZ.Numer), $"Dokument WZ o numerze '{dokumentWZ.Numer}' juz istnieje.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    dokumentWZ.ZaksiegowanoUtc = null;
                    _context.Add(dokumentWZ);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError(nameof(DokumentWZ.Numer), $"Dokument WZ o numerze '{dokumentWZ.Numer}' już istnieje.");
                }
            }

            UzupelnijDaneFormularza(dokumentWZ);
            ViewData["StatusOptions"] = BuildStatusSelectList(DozwoloneStatusyCreate, dokumentWZ.Status);
            return View(dokumentWZ);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dokumentWZ = await _context.DokumentWZ
                .Include(d => d.Utworzyl)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (dokumentWZ == null)
            {
                return NotFound();
            }

            if (!string.Equals(dokumentWZ.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            {
                TempData["DokumentWZEditBlocked"] = "Edycja jest dostępna tylko dla dokumentów WZ w statusie Draft.";
                return RedirectToAction(nameof(Details), new { id = dokumentWZ.Id });
            }

            UzupelnijDaneFormularza(dokumentWZ);
            ViewData["StatusOptions"] = BuildStatusSelectList(DozwoloneStatusyEdit, dokumentWZ.Status);
            ViewData["AutorDokumentuEmail"] = dokumentWZ.Utworzyl?.Email ?? "-";
            return View(dokumentWZ);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Numer,IdMagazynu,IdKlienta,Status,DataWydaniaUtc,IdUtworzyl,Notatka,RowVersion")] DokumentWZ dokumentWZ)
        {
            if (id != dokumentWZ.Id)
            {
                return NotFound();
            }

            dokumentWZ.Numer = dokumentWZ.Numer?.Trim() ?? string.Empty;

            if (!DozwoloneStatusyEdit.Contains(dokumentWZ.Status))
            {
                ModelState.AddModelError(nameof(DokumentWZ.Status), "W edycji dozwolone są tylko statusy Draft lub Cancelled. Użyj akcji Zaksięguj dla statusu Posted.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.DokumentWZ.FirstOrDefaultAsync(x => x.Id == id);
                    if (existing == null)
                    {
                        return NotFound();
                    }

                    if (!string.Equals(existing.Status, "Draft", StringComparison.OrdinalIgnoreCase))
                    {
                        ModelState.AddModelError(string.Empty, "Edytować można tylko dokumenty WZ w statusie Draft.");
                        return await ReturnEditViewWithLookupsAsync(dokumentWZ);
                    }

                    existing.Numer = dokumentWZ.Numer;
                    existing.IdMagazynu = dokumentWZ.IdMagazynu;
                    existing.IdKlienta = dokumentWZ.IdKlienta;
                    existing.Status = dokumentWZ.Status;
                    existing.DataWydaniaUtc = dokumentWZ.DataWydaniaUtc;
                    existing.Notatka = dokumentWZ.Notatka;

                    _context.Entry(existing).Property(x => x.RowVersion).OriginalValue = dokumentWZ.RowVersion;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DokumentWZExists(dokumentWZ.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError(nameof(DokumentWZ.Numer), $"Dokument WZ o numerze '{dokumentWZ.Numer}' juz istnieje.");
                    return await ReturnEditViewWithLookupsAsync(dokumentWZ);
                }

                return RedirectToAction(nameof(Index));
            }

            return await ReturnEditViewWithLookupsAsync(dokumentWZ);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostDocument(int id)
        {
            var result = await _dokumentWzService.PostAsync(id);
            if (result.Success)
            {
                TempData["DokumentWZPostSuccess"] = "Dokument WZ został zaksięgowany.";
            }
            else
            {
                TempData["DokumentWZPostError"] = result.ErrorMessage ?? "Nie udało się zaksięgować dokumentu WZ.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Wydruk(int id, int? idSzablonu)
        {
            try
            {
                var file = await _wydrukDokumentuService.GenerujWydrukWzAsync(id, idSzablonu);
                if (file.UzytoSzablonuAwaryjnego && !string.IsNullOrWhiteSpace(file.KomunikatInformacyjny))
                {
                    TempData["DokumentWZPrintInfo"] = file.KomunikatInformacyjny;
                }
                return File(file.Content, file.ContentType, file.FileName);
            }
            catch (Exception ex)
            {
                TempData["DokumentWZPrintError"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _dokumentWzService.GetDeleteDataAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deleteData = await _dokumentWzService.GetDeleteDataAsync(id);
            if (deleteData == null)
            {
                return NotFound();
            }

            if (!deleteData.CzyMoznaUsunac)
            {
                ModelState.AddModelError(string.Empty, $"Nie można usunąć dokumentu WZ, ponieważ ma powiązane pozycje ({deleteData.LiczbaPozycji}).");
                return View("Delete", deleteData);
            }

            var dokumentWZ = await _context.DokumentWZ.FindAsync(id);
            if (dokumentWZ != null)
            {
                _context.DokumentWZ.Remove(dokumentWZ);
            }

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Nie udało się usunąć dokumentu WZ, ponieważ ma powiązane rekordy (np. pozycje WZ).");
                var refreshed = await _dokumentWzService.GetDeleteDataAsync(id);
                if (refreshed == null)
                {
                    return NotFound();
                }
                return View("Delete", refreshed);
            }
        }

        private bool DokumentWZExists(int id)
        {
            return _context.DokumentWZ.Any(e => e.Id == id);
        }

        private Task<bool> CzyNumerDokumentuWzJuzIstniejeAsync(string numer, int? zWykluczeniemId = null)
        {
            if (string.IsNullOrWhiteSpace(numer))
            {
                return Task.FromResult(false);
            }

            return _context.DokumentWZ.AsNoTracking().AnyAsync(d =>
                d.Numer == numer &&
                (!zWykluczeniemId.HasValue || d.Id != zWykluczeniemId.Value));
        }

        private void UzupelnijDaneFormularza(DokumentWZ dokumentWZ)
        {
            ViewData["IdKlienta"] = new SelectList(_context.Klient.AsNoTracking().OrderBy(x => x.Nazwa), "IdKlienta", "Nazwa", dokumentWZ.IdKlienta);
            ViewData["IdMagazynu"] = new SelectList(_context.Magazyn.AsNoTracking().OrderBy(x => x.Nazwa), "IdMagazynu", "Nazwa", dokumentWZ.IdMagazynu);
            ViewData["IdUtworzyl"] = new SelectList(_context.Uzytkownik.AsNoTracking().OrderBy(x => x.Email), "IdUzytkownika", "Email", dokumentWZ.IdUtworzyl);
        }

        private async Task<IActionResult> ReturnEditViewWithLookupsAsync(DokumentWZ dokumentWZ)
        {
            UzupelnijDaneFormularza(dokumentWZ);
            ViewData["StatusOptions"] = BuildStatusSelectList(DozwoloneStatusyEdit, dokumentWZ.Status);
            ViewData["AutorDokumentuEmail"] = await _context.Uzytkownik
                .Where(u => u.IdUzytkownika == dokumentWZ.IdUtworzyl)
                .Select(u => u.Email)
                .FirstOrDefaultAsync() ?? "-";
            return View(dokumentWZ);
        }

        private static IReadOnlyList<SelectListItem> BuildStatusSelectList(IEnumerable<string> allowedStatuses, string? selected)
        {
            return allowedStatuses
                .Select(x => new SelectListItem(x, x, string.Equals(x, selected, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }
    }
}
