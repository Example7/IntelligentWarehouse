using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;
using IntranetWeb.Controllers.Abstrakcja;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IntranetWeb.Controllers
{
    public class DokumentMMController : BaseSearchController<DokumentMM>
    {
        private readonly IDokumentMMService _dokumentMmService;
        private readonly IWydrukDokumentuService _wydrukDokumentuService;
        private static readonly string[] DozwoloneStatusyCreate = ["Draft"];
        private static readonly string[] DozwoloneStatusyEdit = ["Draft", "Cancelled"];

        public DokumentMMController(DataContext context, IDokumentMMService dokumentMmService, IWydrukDokumentuService wydrukDokumentuService) : base(context)
        {
            _dokumentMmService = dokumentMmService;
            _wydrukDokumentuService = wydrukDokumentuService;
        }

        public async Task<IActionResult> Index(string? searchTerm)
        {
            var model = await _dokumentMmService.GetIndexDataAsync(searchTerm);
            return View(model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _dokumentMmService.GetDetailsDataAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        public IActionResult Create()
        {
            var model = new DokumentMM
            {
                Status = "Draft",
                DataUtc = DateTime.UtcNow
            };

            UzupelnijDaneFormularza(model);
            ViewData["StatusOptions"] = BuildStatusSelectList(DozwoloneStatusyCreate, model.Status);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Numer,IdMagazynu,DataUtc,IdUtworzyl,Notatka")] DokumentMM dokumentMM)
        {
            dokumentMM.Status = "Draft";
            dokumentMM.Numer = dokumentMM.Numer?.Trim() ?? string.Empty;

            if (await CzyNumerDokumentuMMJuzIstniejeAsync(dokumentMM.Numer))
            {
                ModelState.AddModelError(nameof(DokumentMM.Numer), $"Dokument MM o numerze '{dokumentMM.Numer}' już istnieje.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    dokumentMM.ZaksiegowanoUtc = null;
                    _context.Add(dokumentMM);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError(nameof(DokumentMM.Numer), $"Dokument MM o numerze '{dokumentMM.Numer}' już istnieje.");
                }
            }

            UzupelnijDaneFormularza(dokumentMM);
            ViewData["StatusOptions"] = BuildStatusSelectList(DozwoloneStatusyCreate, dokumentMM.Status);
            return View(dokumentMM);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dokumentMM = await _context.DokumentMM
                .Include(d => d.Utworzyl)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (dokumentMM == null)
            {
                return NotFound();
            }

            if (!string.Equals(dokumentMM.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            {
                TempData["DokumentMMEditBlocked"] = "Edycja jest dostepna tylko dla dokumentow MM w statusie Draft.";
                return RedirectToAction(nameof(Details), new { id = dokumentMM.Id });
            }

            UzupelnijDaneFormularza(dokumentMM);
            ViewData["StatusOptions"] = BuildStatusSelectList(DozwoloneStatusyEdit, dokumentMM.Status);
            ViewData["AutorDokumentuEmail"] = dokumentMM.Utworzyl?.Email ?? "-";
            return View(dokumentMM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Numer,IdMagazynu,Status,DataUtc,IdUtworzyl,Notatka,RowVersion")] DokumentMM dokumentMM)
        {
            if (id != dokumentMM.Id)
            {
                return NotFound();
            }

            dokumentMM.Numer = dokumentMM.Numer?.Trim() ?? string.Empty;

            if (!DozwoloneStatusyEdit.Contains(dokumentMM.Status))
            {
                ModelState.AddModelError(nameof(DokumentMM.Status), "W edycji dozwolone są tylko statusy Draft lub Cancelled. Użyj akcji Zaksięguj dla statusu Posted.");
            }

            if (await CzyNumerDokumentuMMJuzIstniejeAsync(dokumentMM.Numer, dokumentMM.Id))
            {
                ModelState.AddModelError(nameof(DokumentMM.Numer), $"Dokument MM o numerze '{dokumentMM.Numer}' już istnieje.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.DokumentMM.FirstOrDefaultAsync(x => x.Id == id);
                    if (existing == null)
                    {
                        return NotFound();
                    }

                    if (!string.Equals(existing.Status, "Draft", StringComparison.OrdinalIgnoreCase))
                    {
                        ModelState.AddModelError(string.Empty, "Edytować mozna tylko dokumenty MM w statusie Draft.");
                        return await ReturnEditViewWithLookupsAsync(dokumentMM);
                    }

                    existing.Numer = dokumentMM.Numer;
                    existing.IdMagazynu = dokumentMM.IdMagazynu;
                    existing.Status = dokumentMM.Status;
                    existing.DataUtc = dokumentMM.DataUtc;
                    existing.Notatka = dokumentMM.Notatka;

                    _context.Entry(existing).Property(x => x.RowVersion).OriginalValue = dokumentMM.RowVersion;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DokumentMMExists(dokumentMM.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError(nameof(DokumentMM.Numer), $"Dokument MM o numerze '{dokumentMM.Numer}' już istnieje.");
                    return await ReturnEditViewWithLookupsAsync(dokumentMM);
                }

                return RedirectToAction(nameof(Index));
            }

            return await ReturnEditViewWithLookupsAsync(dokumentMM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostDocument(int id)
        {
            var result = await _dokumentMmService.PostAsync(id);
            if (result.Success)
            {
                TempData["DokumentMMPostSuccess"] = "Dokument MM został zaksięgowany.";
            }
            else
            {
                TempData["DokumentMMPostError"] = result.ErrorMessage ?? "Nie udało się zaksięgować dokumentu MM.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Wydruk(int id, int? idSzablonu)
        {
            try
            {
                var file = await _wydrukDokumentuService.GenerujWydrukMmAsync(id, idSzablonu);
                if (file.UzytoSzablonuAwaryjnego && !string.IsNullOrWhiteSpace(file.KomunikatInformacyjny))
                {
                    TempData["DokumentMMPrintInfo"] = file.KomunikatInformacyjny;
                }
                return File(file.Content, file.ContentType, file.FileName);
            }
            catch (Exception ex)
            {
                TempData["DokumentMMPrintError"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _dokumentMmService.GetDeleteDataAsync(id.Value);
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
            var deleteData = await _dokumentMmService.GetDeleteDataAsync(id);
            if (deleteData == null)
            {
                return NotFound();
            }

            if (!deleteData.CzyMoznaUsunac)
            {
                ModelState.AddModelError(string.Empty, $"Nie można usunąć dokumentu MM, ponieważ ma powiązane pozycje ({deleteData.LiczbaPozycji}).");
                return View("Delete", deleteData);
            }

            var dokumentMM = await _context.DokumentMM.FindAsync(id);
            if (dokumentMM != null)
            {
                _context.DokumentMM.Remove(dokumentMM);
            }

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Nie udało się usunąć dokumentu MM, ponieważ ma powiązane rekordy (np. pozycje MM).");
                var refreshed = await _dokumentMmService.GetDeleteDataAsync(id);
                if (refreshed == null)
                {
                    return NotFound();
                }
                return View("Delete", refreshed);
            }
        }

        private bool DokumentMMExists(int id) => _context.DokumentMM.Any(e => e.Id == id);

        private Task<bool> CzyNumerDokumentuMMJuzIstniejeAsync(string numer, int? zWykluczeniemId = null)
        {
            if (string.IsNullOrWhiteSpace(numer))
            {
                return Task.FromResult(false);
            }

            return _context.DokumentMM.AsNoTracking().AnyAsync(d =>
                d.Numer == numer &&
                (!zWykluczeniemId.HasValue || d.Id != zWykluczeniemId.Value));
        }

        private void UzupelnijDaneFormularza(DokumentMM dokumentMM)
        {
            ViewData["IdMagazynu"] = new SelectList(_context.Magazyn.AsNoTracking().OrderBy(x => x.Nazwa), "IdMagazynu", "Nazwa", dokumentMM.IdMagazynu);
            ViewData["IdUtworzyl"] = new SelectList(_context.Uzytkownik.AsNoTracking().OrderBy(x => x.Email), "IdUzytkownika", "Email", dokumentMM.IdUtworzyl);
        }

        private async Task<IActionResult> ReturnEditViewWithLookupsAsync(DokumentMM dokumentMM)
        {
            UzupelnijDaneFormularza(dokumentMM);
            ViewData["StatusOptions"] = BuildStatusSelectList(DozwoloneStatusyEdit, dokumentMM.Status);
            ViewData["AutorDokumentuEmail"] = await _context.Uzytkownik
                .Where(u => u.IdUzytkownika == dokumentMM.IdUtworzyl)
                .Select(u => u.Email)
                .FirstOrDefaultAsync() ?? "-";
            return View(dokumentMM);
        }

        private static IReadOnlyList<SelectListItem> BuildStatusSelectList(IEnumerable<string> allowedStatuses, string? selected)
        {
            return allowedStatuses
                .Select(x => new SelectListItem(x, x, string.Equals(x, selected, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }
    }
}

