using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;
using IntranetWeb.Controllers.Abstrakcja;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IntranetWeb.Controllers
{
    public class RezerwacjaController : BaseSearchController<Rezerwacja>
    {
        private readonly IRezerwacjaService _rezerwacjaService;
        private static readonly string[] DozwoloneStatusyCreate = ["Draft"];
        private static readonly string[] DozwoloneStatusyEdit = ["Draft", "Active", "Completed", "Cancelled"];

        public RezerwacjaController(DataContext context, IRezerwacjaService rezerwacjaService) : base(context)
        {
            _rezerwacjaService = rezerwacjaService;
        }

        public async Task<IActionResult> Index(string? searchTerm)
        {
            var model = await _rezerwacjaService.GetIndexDataAsync(searchTerm);
            return View(model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var model = await _rezerwacjaService.GetDetailsDataAsync(id.Value);
            return model == null ? NotFound() : View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            var result = await _rezerwacjaService.ActivateAsync(id);
            if (result.Success)
            {
                TempData["RezerwacjaStatusSuccess"] = result.Message;
            }
            else
            {
                TempData["RezerwacjaStatusError"] = result.Message;
                if (result.Errors.Count > 0)
                {
                    TempData["RezerwacjaStatusErrorDetails"] = string.Join(" | ", result.Errors.Select(e => e.Error));
                }
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Release(int id)
        {
            var result = await _rezerwacjaService.ReleaseAsync(id);
            if (result.Success)
            {
                TempData["RezerwacjaStatusSuccess"] = result.Message;
            }
            else
            {
                TempData["RezerwacjaStatusError"] = result.Message;
                if (result.Errors.Count > 0)
                {
                    TempData["RezerwacjaStatusErrorDetails"] = string.Join(" | ", result.Errors.Select(e => e.Error));
                }
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        public IActionResult Create()
        {
            var model = new Rezerwacja
            {
                Status = "Draft",
                UtworzonoUtc = DateTime.UtcNow
            };
            UzupelnijDaneFormularza(model);
            ViewData["StatusOptions"] = BuildStatusSelectList(DozwoloneStatusyCreate, model.Status);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Numer,IdMagazynu,WygasaUtc,IdUtworzyl,Notatka")] Rezerwacja rezerwacja)
        {
            rezerwacja.Status = "Draft";
            rezerwacja.UtworzonoUtc = DateTime.UtcNow;
            rezerwacja.Numer = rezerwacja.Numer?.Trim() ?? string.Empty;
            rezerwacja.WygasaUtc = NormalizeLocalInputToUtc(rezerwacja.WygasaUtc);

            if (await CzyNumerRezerwacjiJuzIstniejeAsync(rezerwacja.Numer))
            {
                ModelState.AddModelError(nameof(Rezerwacja.Numer), $"Rezerwacja o numerze '{rezerwacja.Numer}' już istnieje.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(rezerwacja);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError(nameof(Rezerwacja.Numer), $"Rezerwacja o numerze '{rezerwacja.Numer}' już istnieje.");
                }
            }

            UzupelnijDaneFormularza(rezerwacja);
            ViewData["StatusOptions"] = BuildStatusSelectList(DozwoloneStatusyCreate, rezerwacja.Status);
            return View(rezerwacja);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var rezerwacja = await _context.Rezerwacja
                .Include(r => r.Utworzyl)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (rezerwacja == null) return NotFound();

            UzupelnijDaneFormularza(rezerwacja);
            ViewData["StatusOptions"] = BuildStatusSelectList(DozwoloneStatusyEdit, rezerwacja.Status);
            ViewData["AutorDokumentuEmail"] = rezerwacja.Utworzyl?.Email ?? "-";
            return View(rezerwacja);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Numer,IdMagazynu,Status,WygasaUtc,IdUtworzyl,Notatka,RowVersion")] Rezerwacja rezerwacja)
        {
            if (id != rezerwacja.Id) return NotFound();

            rezerwacja.Numer = rezerwacja.Numer?.Trim() ?? string.Empty;
            rezerwacja.WygasaUtc = NormalizeLocalInputToUtc(rezerwacja.WygasaUtc);
            if (!DozwoloneStatusyEdit.Contains(rezerwacja.Status))
            {
                ModelState.AddModelError(nameof(Rezerwacja.Status), "Niedozwolony status rezerwacji.");
            }

            if (await CzyNumerRezerwacjiJuzIstniejeAsync(rezerwacja.Numer, rezerwacja.Id))
            {
                ModelState.AddModelError(nameof(Rezerwacja.Numer), $"Rezerwacja o numerze '{rezerwacja.Numer}' już istnieje.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Rezerwacja.FirstOrDefaultAsync(r => r.Id == id);
                    if (existing == null) return NotFound();

                    existing.Numer = rezerwacja.Numer;
                    existing.IdMagazynu = rezerwacja.IdMagazynu;
                    existing.Status = rezerwacja.Status;
                    existing.WygasaUtc = rezerwacja.WygasaUtc;
                    existing.Notatka = rezerwacja.Notatka;
                    _context.Entry(existing).Property(x => x.RowVersion).OriginalValue = rezerwacja.RowVersion;
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RezerwacjaExists(rezerwacja.Id)) return NotFound();
                    throw;
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError(nameof(Rezerwacja.Numer), $"Rezerwacja o numerze '{rezerwacja.Numer}' już istnieje.");
                }
            }

            return await ReturnEditViewWithLookupsAsync(rezerwacja);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var model = await _rezerwacjaService.GetDeleteDataAsync(id.Value);
            return model == null ? NotFound() : View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deleteData = await _rezerwacjaService.GetDeleteDataAsync(id);
            if (deleteData == null) return NotFound();

            if (!deleteData.CzyMoznaUsunac)
            {
                ModelState.AddModelError(string.Empty, $"Nie można usunąć rezerwacji, ponieważ ma powiązane pozycje ({deleteData.LiczbaPozycji}).");
                return View("Delete", deleteData);
            }

            var rezerwacja = await _context.Rezerwacja.FindAsync(id);
            if (rezerwacja != null) _context.Rezerwacja.Remove(rezerwacja);

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Nie udało się usunąć rezerwacji, ponieważ ma powiązane rekordy (np. pozycje rezerwacji).");
                var refreshed = await _rezerwacjaService.GetDeleteDataAsync(id);
                return refreshed == null ? NotFound() : View("Delete", refreshed);
            }
        }

        private bool RezerwacjaExists(int id) => _context.Rezerwacja.Any(e => e.Id == id);

        private Task<bool> CzyNumerRezerwacjiJuzIstniejeAsync(string numer, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(numer)) return Task.FromResult(false);
            return _context.Rezerwacja.AsNoTracking().AnyAsync(r => r.Numer == numer && (!excludeId.HasValue || r.Id != excludeId.Value));
        }

        private void UzupelnijDaneFormularza(Rezerwacja model)
        {
            ViewData["IdMagazynu"] = new SelectList(_context.Magazyn.AsNoTracking().OrderBy(x => x.Nazwa), "IdMagazynu", "Nazwa", model.IdMagazynu);
            ViewData["IdUtworzyl"] = new SelectList(_context.Uzytkownik.AsNoTracking().OrderBy(x => x.Email), "IdUzytkownika", "Email", model.IdUtworzyl);
        }

        private async Task<IActionResult> ReturnEditViewWithLookupsAsync(Rezerwacja model)
        {
            UzupelnijDaneFormularza(model);
            ViewData["StatusOptions"] = BuildStatusSelectList(DozwoloneStatusyEdit, model.Status);
            ViewData["AutorDokumentuEmail"] = await _context.Uzytkownik
                .Where(u => u.IdUzytkownika == model.IdUtworzyl)
                .Select(u => u.Email)
                .FirstOrDefaultAsync() ?? "-";
            return View(model);
        }

        private static IReadOnlyList<SelectListItem> BuildStatusSelectList(IEnumerable<string> allowedStatuses, string? selected)
            => allowedStatuses.Select(x => new SelectListItem(x, x, string.Equals(x, selected, StringComparison.OrdinalIgnoreCase))).ToList();

        private static DateTime? NormalizeLocalInputToUtc(DateTime? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            var dt = value.Value;
            if (dt.Kind == DateTimeKind.Utc)
            {
                return dt;
            }

            if (dt.Kind == DateTimeKind.Unspecified)
            {
                dt = DateTime.SpecifyKind(dt, DateTimeKind.Local);
            }

            return dt.ToUniversalTime();
        }
    }
}
