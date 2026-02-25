using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;
using Interfaces.Magazyn.Dtos;
using IntranetWeb.Controllers.Abstrakcja;
using IntranetWeb.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IntranetWeb.Controllers
{
    [Authorize(Roles = AppRoles.AdminMagazynier)]
    public class InwentaryzacjaController : BaseSearchController<Inwentaryzacja>
    {
        private readonly IInwentaryzacjaService _inwentaryzacjaService;
        private static readonly string[] DozwoloneStatusyCreate = ["Draft"];
        private static readonly string[] DozwoloneStatusyEdit = ["Draft", "Cancelled"];

        public InwentaryzacjaController(DataContext context, IInwentaryzacjaService inwentaryzacjaService) : base(context)
        {
            _inwentaryzacjaService = inwentaryzacjaService;
        }

        public async Task<IActionResult> Index(string? searchTerm)
        {
            var model = await _inwentaryzacjaService.GetIndexDataAsync(searchTerm);
            return View(model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _inwentaryzacjaService.GetDetailsDataAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        public IActionResult Create()
        {
            var model = new Inwentaryzacja
            {
                Status = "Draft",
                StartUtc = DateTime.UtcNow
            };

            UzupelnijDaneFormularza(model);
            ViewData["StatusOptions"] = BuildStatusSelectList(DozwoloneStatusyCreate, model.Status);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Numer,IdMagazynu,StartUtc,IdUtworzyl,Notatka")] Inwentaryzacja inwentaryzacja)
        {
            inwentaryzacja.Status = "Draft";
            inwentaryzacja.KoniecUtc = null;
            inwentaryzacja.Numer = inwentaryzacja.Numer?.Trim() ?? string.Empty;

            if (await CzyNumerInwentaryzacjiJuzIstniejeAsync(inwentaryzacja.Numer))
            {
                ModelState.AddModelError(nameof(Inwentaryzacja.Numer), $"Inwentaryzacja o numerze '{inwentaryzacja.Numer}' już istnieje.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(inwentaryzacja);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError(nameof(Inwentaryzacja.Numer), $"Inwentaryzacja o numerze '{inwentaryzacja.Numer}' już istnieje.");
                }
            }

            UzupelnijDaneFormularza(inwentaryzacja);
            ViewData["StatusOptions"] = BuildStatusSelectList(DozwoloneStatusyCreate, inwentaryzacja.Status);
            return View(inwentaryzacja);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inwentaryzacja = await _context.Inwentaryzacja
                .Include(i => i.Utworzyl)
                .FirstOrDefaultAsync(i => i.Id == id);
            if (inwentaryzacja == null)
            {
                return NotFound();
            }

            if (!string.Equals(inwentaryzacja.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            {
                TempData["InwentaryzacjaEditBlocked"] = "Edycja jest dostepna tylko dla inwentaryzacji w statusie Draft.";
                return RedirectToAction(nameof(Details), new { id = inwentaryzacja.Id });
            }

            UzupelnijDaneFormularza(inwentaryzacja);
            ViewData["StatusOptions"] = BuildStatusSelectList(DozwoloneStatusyEdit, inwentaryzacja.Status);
            ViewData["AutorDokumentuEmail"] = inwentaryzacja.Utworzyl?.Email ?? "-";
            return View(inwentaryzacja);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Numer,IdMagazynu,Status,StartUtc,IdUtworzyl,Notatka,RowVersion")] Inwentaryzacja inwentaryzacja)
        {
            if (id != inwentaryzacja.Id)
            {
                return NotFound();
            }

            inwentaryzacja.Numer = inwentaryzacja.Numer?.Trim() ?? string.Empty;

            if (!DozwoloneStatusyEdit.Contains(inwentaryzacja.Status))
            {
                ModelState.AddModelError(nameof(Inwentaryzacja.Status), "W edycji dozwolone są tylko statusy Draft lub Cancelled. Użyj akcji Zamknij dla zamknięcia inwentaryzacji.");
            }

            if (await CzyNumerInwentaryzacjiJuzIstniejeAsync(inwentaryzacja.Numer, inwentaryzacja.Id))
            {
                ModelState.AddModelError(nameof(Inwentaryzacja.Numer), $"Inwentaryzacja o numerze '{inwentaryzacja.Numer}' już istnieje.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Inwentaryzacja.FirstOrDefaultAsync(x => x.Id == id);
                    if (existing == null)
                    {
                        return NotFound();
                    }

                    if (!string.Equals(existing.Status, "Draft", StringComparison.OrdinalIgnoreCase))
                    {
                        ModelState.AddModelError(string.Empty, "Edytować można tylko inwentaryzacje w statusie Draft.");
                        return await ReturnEditViewWithLookupsAsync(inwentaryzacja);
                    }

                    existing.Numer = inwentaryzacja.Numer;
                    existing.IdMagazynu = inwentaryzacja.IdMagazynu;
                    existing.Status = inwentaryzacja.Status;
                    existing.StartUtc = inwentaryzacja.StartUtc;
                    existing.Notatka = inwentaryzacja.Notatka;
                    existing.KoniecUtc = string.Equals(inwentaryzacja.Status, "Cancelled", StringComparison.OrdinalIgnoreCase)
                        ? (existing.KoniecUtc ?? DateTime.UtcNow)
                        : null;

                    _context.Entry(existing).Property(x => x.RowVersion).OriginalValue = inwentaryzacja.RowVersion;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InwentaryzacjaExists(inwentaryzacja.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError(nameof(Inwentaryzacja.Numer), $"Inwentaryzacja o numerze '{inwentaryzacja.Numer}' już istnieje.");
                    return await ReturnEditViewWithLookupsAsync(inwentaryzacja);
                }

                return RedirectToAction(nameof(Index));
            }

            return await ReturnEditViewWithLookupsAsync(inwentaryzacja);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseDocument(int id)
        {
            var result = await _inwentaryzacjaService.CloseAsync(id);
            if (result.Success)
            {
                TempData["InwentaryzacjaCloseSuccess"] = "Inwentaryzacja zostala zamknieta i przeliczona.";
            }
            else
            {
                TempData["InwentaryzacjaCloseError"] = result.ErrorMessage ?? "Nie udalo sie zamknac inwentaryzacji.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _inwentaryzacjaService.GetDeleteDataAsync(id.Value);
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
            var deleteData = await _inwentaryzacjaService.GetDeleteDataAsync(id);
            if (deleteData == null)
            {
                return NotFound();
            }

            if (!deleteData.CzyMoznaUsunac)
            {
                ModelState.AddModelError(string.Empty, $"Nie można usunąć inwentaryzacji, ponieważ ma powiązane pozycje ({deleteData.LiczbaPozycji}).");
                return View("Delete", deleteData);
            }

            var inwentaryzacja = await _context.Inwentaryzacja.FindAsync(id);
            if (inwentaryzacja != null)
            {
                _context.Inwentaryzacja.Remove(inwentaryzacja);
            }

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Nie udało się usunąć inwentaryzacji, ponieważ ma powiązane rekordy (np. pozycje inwentaryzacji).");
                var refreshed = await _inwentaryzacjaService.GetDeleteDataAsync(id);
                if (refreshed == null)
                {
                    return NotFound();
                }
                return View("Delete", refreshed);
            }
        }

        private bool InwentaryzacjaExists(int id) => _context.Inwentaryzacja.Any(e => e.Id == id);

        private Task<bool> CzyNumerInwentaryzacjiJuzIstniejeAsync(string numer, int? zWykluczeniemId = null)
        {
            if (string.IsNullOrWhiteSpace(numer))
            {
                return Task.FromResult(false);
            }

            return _context.Inwentaryzacja.AsNoTracking().AnyAsync(d =>
                d.Numer == numer &&
                (!zWykluczeniemId.HasValue || d.Id != zWykluczeniemId.Value));
        }

        private void UzupelnijDaneFormularza(Inwentaryzacja model)
        {
            ViewData["IdMagazynu"] = new SelectList(_context.Magazyn.AsNoTracking().OrderBy(x => x.Nazwa), "IdMagazynu", "Nazwa", model.IdMagazynu);
            ViewData["IdUtworzyl"] = new SelectList(_context.Uzytkownik.AsNoTracking().OrderBy(x => x.Email), "IdUzytkownika", "Email", model.IdUtworzyl);
        }

        private async Task<IActionResult> ReturnEditViewWithLookupsAsync(Inwentaryzacja inwentaryzacja)
        {
            UzupelnijDaneFormularza(inwentaryzacja);
            ViewData["StatusOptions"] = BuildStatusSelectList(DozwoloneStatusyEdit, inwentaryzacja.Status);
            ViewData["AutorDokumentuEmail"] = await _context.Uzytkownik
                .Where(u => u.IdUzytkownika == inwentaryzacja.IdUtworzyl)
                .Select(u => u.Email)
                .FirstOrDefaultAsync() ?? "-";
            return View(inwentaryzacja);
        }

        private static IReadOnlyList<SelectListItem> BuildStatusSelectList(IEnumerable<string> allowedStatuses, string? selected)
        {
            return allowedStatuses
                .Select(x => new SelectListItem(x, x, string.Equals(x, selected, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }
    }
}
