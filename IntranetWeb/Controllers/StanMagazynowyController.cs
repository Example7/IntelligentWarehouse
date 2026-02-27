using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;
using Interfaces.Magazyn.Dtos;
using IntranetWeb.Security;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using IntranetWeb.Controllers.Abstrakcja;

namespace IntranetWeb.Controllers
{
    [Authorize(Roles = AppRoles.AdminMagazynier)]
    public class StanMagazynowyController : BaseSearchController<StanMagazynowy>
    {
        private readonly IStanMagazynowyService _stanMagazynowyService;

        public StanMagazynowyController(DataContext context, IStanMagazynowyService stanMagazynowyService) : base(context)
        {
            _stanMagazynowyService = stanMagazynowyService;
        }

        // GET: StanMagazynowy
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var model = await _stanMagazynowyService.GetIndexDataAsync(searchTerm);
            return View(model);
        }

        // GET: StanMagazynowy/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stanMagazynowy = await _context.StanMagazynowy
                .Include(s => s.Lokacja)
                    .ThenInclude(l => l.Magazyn)
                .Include(s => s.Produkt)
                    .ThenInclude(p => p.DomyslnaJednostka)
                .FirstOrDefaultAsync(m => m.IdStanu == id);
            if (stanMagazynowy == null)
            {
                return NotFound();
            }

            return View(stanMagazynowy);
        }

        // GET: StanMagazynowy/Create
        public async Task<IActionResult> Create()
        {
            var model = await _stanMagazynowyService.GetCreateFormAsync();
            return View(model);
        }

        // POST: StanMagazynowy/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StanMagazynowyFormDto model)
        {
            var stanMagazynowy = model.StanMagazynowy;
            var existingState = await GetExistingStateForPairAsync(stanMagazynowy.IdProduktu, stanMagazynowy.IdLokacji);
            if (existingState != null)
            {
                SetDuplicateStateContext(existingState);
            }

            if (stanMagazynowy.Ilosc < 0m)
            {
                ModelState.AddModelError(nameof(StanMagazynowy.Ilosc), "Ilość nie może być ujemna.");
            }

            var blockedQty = await CalculateBlockedQtyAsync(stanMagazynowy.IdProduktu, stanMagazynowy.IdLokacji);
            if (stanMagazynowy.Ilosc < blockedQty.Total)
            {
                ModelState.AddModelError(nameof(StanMagazynowy.Ilosc),
                    $"Nie można zapisac stanu {stanMagazynowy.Ilosc:0.###}. Zajęte przez procesy: {blockedQty.Total:0.###} " +
                    $"(rezerwacje aktywne: {blockedQty.ActiveReservations:0.###}, WZ Draft: {blockedQty.DraftWz:0.###}).");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(stanMagazynowy);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex) when (IsUniqueStockConstraintViolation(ex))
                {
                    existingState ??= await GetExistingStateForPairAsync(stanMagazynowy.IdProduktu, stanMagazynowy.IdLokacji);
                    if (existingState != null)
                    {
                        SetDuplicateStateContext(existingState);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Stan dla wybranego produktu i lokacji już istnieje. Użyj edycji istniejącego rekordu.");
                    }
                }
            }

            var formModel = await _stanMagazynowyService.PrepareFormAsync(stanMagazynowy, isEdit: false);
            return View(formModel);
        }

        // GET: StanMagazynowy/Edit/5
        [Authorize(Roles = AppRoles.AdminOnly)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _stanMagazynowyService.GetEditFormAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // POST: StanMagazynowy/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.AdminOnly)]
        public async Task<IActionResult> Edit(int id, StanMagazynowyFormDto model)
        {
            var input = model.StanMagazynowy;
            if (id != input.IdStanu)
            {
                return NotFound();
            }

            if (input.Ilosc < 0m)
            {
                ModelState.AddModelError(nameof(StanMagazynowy.Ilosc), "Ilość nie może być ujemna.");
            }

            var blockedQty = await CalculateBlockedQtyAsync(input.IdProduktu, input.IdLokacji);
            if (input.Ilosc < blockedQty.Total)
            {
                ModelState.AddModelError(nameof(StanMagazynowy.Ilosc),
                    $"Nie można zapisac stanu {input.Ilosc:0.###}. Zajęte przez procesy: {blockedQty.Total:0.###} " +
                    $"(rezerwacje aktywne: {blockedQty.ActiveReservations:0.###}, WZ Draft: {blockedQty.DraftWz:0.###}).");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (await ExistsDuplicateProductLocationAsync(input.IdProduktu, input.IdLokacji, input.IdStanu))
                    {
                        ModelState.AddModelError(string.Empty, "Stan dla wybranego produktu i lokacji już istnieje. Wybierz inną kombinację.");
                        var invalidModel = await _stanMagazynowyService.PrepareFormAsync(input, isEdit: true);
                        return View(invalidModel);
                    }

                    var existingState = await _context.StanMagazynowy
                        .FirstOrDefaultAsync(x => x.IdStanu == id);
                    if (existingState == null)
                    {
                        return NotFound();
                    }

                    // Update tracked entity to preserve original values for audit and real field-level diffs.
                    existingState.IdProduktu = input.IdProduktu;
                    existingState.IdLokacji = input.IdLokacji;
                    existingState.Ilosc = input.Ilosc;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StanMagazynowyExists(input.IdStanu))
                    {
                        return NotFound();
                    }

                    throw;
                }
                catch (DbUpdateException ex) when (IsUniqueStockConstraintViolation(ex))
                {
                    ModelState.AddModelError(string.Empty, "Stan dla wybranego produktu i lokacji już istnieje. Wybierz inną kombinację.");
                    var invalidModel = await _stanMagazynowyService.PrepareFormAsync(input, isEdit: true);
                    return View(invalidModel);
                }

                return RedirectToAction(nameof(Index));
            }

            var formModel = await _stanMagazynowyService.PrepareFormAsync(input, isEdit: true);
            return View(formModel);
        }

        // GET: StanMagazynowy/Delete/5
        [Authorize(Roles = AppRoles.AdminOnly)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stanMagazynowy = await _context.StanMagazynowy
                .Include(s => s.Lokacja)
                    .ThenInclude(l => l.Magazyn)
                .Include(s => s.Produkt)
                    .ThenInclude(p => p.DomyslnaJednostka)
                .FirstOrDefaultAsync(m => m.IdStanu == id);
            if (stanMagazynowy == null)
            {
                return NotFound();
            }

            return View(stanMagazynowy);
        }

        // POST: StanMagazynowy/Delete/5
        [Authorize(Roles = AppRoles.AdminOnly)]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var stanMagazynowy = await _context.StanMagazynowy.FindAsync(id);
            if (stanMagazynowy != null)
            {
                _context.StanMagazynowy.Remove(stanMagazynowy);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StanMagazynowyExists(int id)
        {
            return _context.StanMagazynowy.Any(e => e.IdStanu == id);
        }

        private Task<bool> ExistsDuplicateProductLocationAsync(int idProduktu, int idLokacji, int? excludeId = null)
        {
            var query = _context.StanMagazynowy.AsNoTracking()
                .Where(x => x.IdProduktu == idProduktu && x.IdLokacji == idLokacji);

            if (excludeId.HasValue)
            {
                query = query.Where(x => x.IdStanu != excludeId.Value);
            }

            return query.AnyAsync();
        }

        private static bool IsUniqueStockConstraintViolation(DbUpdateException ex)
        {
            if (ex.InnerException is not SqlException sqlEx)
            {
                return false;
            }

            return (sqlEx.Number == 2601 || sqlEx.Number == 2627) &&
                   sqlEx.Message.Contains("IX_Stock_ProductId_LocationId", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<StanMagazynowy?> GetExistingStateForPairAsync(int idProduktu, int idLokacji)
        {
            if (idProduktu <= 0 || idLokacji <= 0)
            {
                return null;
            }

            return await _context.StanMagazynowy
                .AsNoTracking()
                .Include(x => x.Produkt)
                .FirstOrDefaultAsync(x => x.IdProduktu == idProduktu && x.IdLokacji == idLokacji);
        }

        private void SetDuplicateStateContext(StanMagazynowy existingState)
        {
            ViewData["DuplicateStateEditId"] = existingState.IdStanu;
            ViewData["DuplicateStateProductLabel"] = $"{existingState.Produkt.Kod} - {existingState.Produkt.Nazwa}";
        }

        private async Task<(decimal ActiveReservations, decimal DraftWz, decimal Total)> CalculateBlockedQtyAsync(int idProduktu, int idLokacji)
        {
            if (idProduktu <= 0 || idLokacji <= 0)
            {
                return (0m, 0m, 0m);
            }

            var activeReservations = await _context.PozycjaRezerwacji
                .AsNoTracking()
                .Where(p =>
                    p.IdProduktu == idProduktu &&
                    p.IdLokacji == idLokacji &&
                    p.Rezerwacja.Status == "Active")
                .Select(p => (decimal?)p.Ilosc)
                .SumAsync() ?? 0m;

            var draftWz = await _context.PozycjaWZ
                .AsNoTracking()
                .Where(p =>
                    p.IdProduktu == idProduktu &&
                    p.IdLokacji == idLokacji &&
                    p.Dokument.Status == "Draft")
                .Select(p => (decimal?)p.Ilosc)
                .SumAsync() ?? 0m;

            return (activeReservations, draftWz, activeReservations + draftWz);
        }
    }
}
