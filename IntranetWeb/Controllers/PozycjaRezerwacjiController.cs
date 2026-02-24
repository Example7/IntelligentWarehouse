using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;
using IntranetWeb.Controllers.Abstrakcja;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace IntranetWeb.Controllers
{
    public class PozycjaRezerwacjiController : BaseSearchController<PozycjaRezerwacji>
    {
        private readonly IPozycjaRezerwacjiService _pozycjaRezerwacjiService;

        public PozycjaRezerwacjiController(DataContext context, IPozycjaRezerwacjiService pozycjaRezerwacjiService) : base(context)
        {
            _pozycjaRezerwacjiService = pozycjaRezerwacjiService;
        }

        public async Task<IActionResult> Index(string? searchTerm)
        {
            var model = await _pozycjaRezerwacjiService.GetIndexDataAsync(searchTerm);
            return View(model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var model = await _pozycjaRezerwacjiService.GetDetailsDataAsync(id.Value);
            return model == null ? NotFound() : View(model);
        }

        public IActionResult Create()
        {
            var model = new PozycjaRezerwacji { Lp = 1 };
            UstawDomyslneLpDlaCreate(model);
            UzupelnijDaneFormularza(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdRezerwacji,Lp,IdProduktu,IdLokacji,Ilosc")] PozycjaRezerwacji pozycja)
        {
            await WalidujPozycjeAsync(pozycja, isEdit: false);
            if (ModelState.IsValid)
            {
                _context.Add(pozycja);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            UzupelnijDaneFormularza(pozycja);
            return View(pozycja);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var pozycja = await _context.PozycjaRezerwacji
                .Include(p => p.Rezerwacja)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (pozycja == null) return NotFound();

            if (!CzyStatusRezerwacjiEdytowalny(pozycja.Rezerwacja?.Status))
            {
                TempData["PozycjaRezerwacjiEditBlocked"] = "Edycja pozycji rezerwacji nie jest dostępna dla zakończonych/anulowanych rezerwacji.";
                return RedirectToAction(nameof(Details), new { id = pozycja.Id });
            }

            UzupelnijDaneFormularza(pozycja);
            return View(pozycja);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdRezerwacji,Lp,IdProduktu,IdLokacji,Ilosc")] PozycjaRezerwacji pozycja)
        {
            if (id != pozycja.Id) return NotFound();

            await WalidujPozycjeAsync(pozycja, isEdit: true);
            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.PozycjaRezerwacji
                        .Include(p => p.Rezerwacja)
                        .FirstOrDefaultAsync(p => p.Id == id);
                    if (existing == null) return NotFound();

                    if (!CzyStatusRezerwacjiEdytowalny(existing.Rezerwacja?.Status))
                    {
                        ModelState.AddModelError(string.Empty, "Edytować można tylko pozycje rezerwacji w statusie edytowalnym.");
                        UzupelnijDaneFormularza(pozycja);
                        return View(pozycja);
                    }

                    existing.IdRezerwacji = pozycja.IdRezerwacji;
                    existing.Lp = pozycja.Lp;
                    existing.IdProduktu = pozycja.IdProduktu;
                    existing.IdLokacji = pozycja.IdLokacji;
                    existing.Ilosc = pozycja.Ilosc;
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PozycjaExists(pozycja.Id)) return NotFound();
                    throw;
                }
            }

            UzupelnijDaneFormularza(pozycja);
            return View(pozycja);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var model = await _pozycjaRezerwacjiService.GetDeleteDataAsync(id.Value);
            return model == null ? NotFound() : View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deleteData = await _pozycjaRezerwacjiService.GetDeleteDataAsync(id);
            if (deleteData == null) return NotFound();

            if (!deleteData.CzyMoznaUsunac)
            {
                ModelState.AddModelError(string.Empty, deleteData.PowodBlokady ?? "Nie można usunąć pozycji rezerwacji.");
                return View("Delete", deleteData);
            }

            var pozycja = await _context.PozycjaRezerwacji.FindAsync(id);
            if (pozycja != null) _context.PozycjaRezerwacji.Remove(pozycja);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PozycjaExists(int id) => _context.PozycjaRezerwacji.Any(e => e.Id == id);

        private void UzupelnijDaneFormularza(PozycjaRezerwacji model)
        {
            ViewData["IdRezerwacji"] = new SelectList(
                _context.Rezerwacja.AsNoTracking().Include(r => r.Magazyn)
                    .OrderByDescending(r => r.UtworzonoUtc).ThenByDescending(r => r.Id)
                    .Select(r => new { r.Id, Label = $"{r.Numer} | {r.Status} | {(r.Magazyn != null ? r.Magazyn.Nazwa : "-")}" })
                    .ToList(),
                "Id", "Label", model.IdRezerwacji);
            UstawMapeLpDlaDokumentow();

            var lokacje = _context.Lokacja.AsNoTracking().Include(l => l.Magazyn)
                .OrderBy(l => l.Magazyn.Nazwa).ThenBy(l => l.Kod)
                .Select(l => new { l.IdLokacji, Label = $"{(l.Magazyn != null ? l.Magazyn.Nazwa : "-")} / {l.Kod}" })
                .ToList();
            ViewData["IdLokacji"] = new SelectList(lokacje, "IdLokacji", "Label", model.IdLokacji);

            ViewData["IdProduktu"] = new SelectList(
                _context.Produkt.AsNoTracking().Include(p => p.DomyslnaJednostka).OrderBy(p => p.Kod).ThenBy(p => p.Nazwa)
                    .Select(p => new { p.IdProduktu, Label = $"{p.Kod} - {p.Nazwa} ({(p.DomyslnaJednostka != null ? p.DomyslnaJednostka.Kod : "j.m.")})" }).ToList(),
                "IdProduktu", "Label", model.IdProduktu);

            var productUomMap = _context.Produkt.AsNoTracking().Include(p => p.DomyslnaJednostka)
                .Select(p => new { p.IdProduktu, Uom = p.DomyslnaJednostka != null ? p.DomyslnaJednostka.Kod : "j.m." })
                .ToDictionary(x => x.IdProduktu, x => x.Uom);
            ViewData["ProductUomMapJson"] = JsonSerializer.Serialize(productUomMap);
        }

        private async Task WalidujPozycjeAsync(PozycjaRezerwacji pozycja, bool isEdit)
        {
            if (pozycja.Ilosc <= 0)
            {
                ModelState.AddModelError(nameof(PozycjaRezerwacji.Ilosc), "Ilość musi być większa od zera.");
            }

            var rezerwacja = await _context.Rezerwacja.AsNoTracking().FirstOrDefaultAsync(r => r.Id == pozycja.IdRezerwacji);
            if (rezerwacja == null)
            {
                ModelState.AddModelError(nameof(PozycjaRezerwacji.IdRezerwacji), "Wybrana rezerwacja nie istnieje.");
                return;
            }

            if (!CzyStatusRezerwacjiEdytowalny(rezerwacja.Status))
            {
                ModelState.AddModelError(nameof(PozycjaRezerwacji.IdRezerwacji), "Pozycje można dodawać/edytować tylko dla rezerwacji w statusie edytowalnym.");
            }

            var produktExists = await _context.Produkt.AsNoTracking().AnyAsync(p => p.IdProduktu == pozycja.IdProduktu);
            if (!produktExists)
            {
                ModelState.AddModelError(nameof(PozycjaRezerwacji.IdProduktu), "Wybrany produkt nie istnieje.");
            }

            if (pozycja.IdLokacji.HasValue)
            {
                var lokacja = await _context.Lokacja.AsNoTracking().FirstOrDefaultAsync(l => l.IdLokacji == pozycja.IdLokacji.Value);
                if (lokacja == null)
                {
                    ModelState.AddModelError(nameof(PozycjaRezerwacji.IdLokacji), "Wybrana lokacja nie istnieje.");
                }
                else if (lokacja.IdMagazynu != rezerwacja.IdMagazynu)
                {
                    ModelState.AddModelError(nameof(PozycjaRezerwacji.IdLokacji), "Lokacja musi należeć do magazynu wybranej rezerwacji.");
                }
                else if (produktExists && pozycja.Ilosc > 0)
                {
                    var stan = await _context.StanMagazynowy.AsNoTracking()
                        .FirstOrDefaultAsync(s => s.IdProduktu == pozycja.IdProduktu && s.IdLokacji == pozycja.IdLokacji.Value);
                    var available = stan?.Ilosc ?? 0m;
                    var sumaInnychPozycji = await _context.PozycjaRezerwacji.AsNoTracking()
                        .Where(p => p.IdRezerwacji == pozycja.IdRezerwacji && p.IdProduktu == pozycja.IdProduktu && p.IdLokacji == pozycja.IdLokacji && (!isEdit || p.Id != pozycja.Id))
                        .SumAsync(p => (decimal?)p.Ilosc) ?? 0m;
                    var sumaPoZapisie = sumaInnychPozycji + pozycja.Ilosc;
                    if (sumaPoZapisie > available && available >= 0)
                    {
                        ModelState.AddModelError(nameof(PozycjaRezerwacji.Ilosc), $"Niewystarczający stan w lokacji. Dostępne: {available:0.###}, suma rezerwacji po zapisie: {sumaPoZapisie:0.###}.");
                    }
                }
            }

            var duplicateLp = await _context.PozycjaRezerwacji.AsNoTracking().AnyAsync(p =>
                p.IdRezerwacji == pozycja.IdRezerwacji && p.Lp == pozycja.Lp && (!isEdit || p.Id != pozycja.Id));
            if (duplicateLp)
            {
                ModelState.AddModelError(nameof(PozycjaRezerwacji.Lp), "Pozycja o takim numerze Lp już istnieje w tej rezerwacji.");
            }

            var hasWarehouseLevelForProduct = await _context.PozycjaRezerwacji.AsNoTracking().AnyAsync(p =>
                p.IdRezerwacji == pozycja.IdRezerwacji &&
                p.IdProduktu == pozycja.IdProduktu &&
                p.IdLokacji == null &&
                (!isEdit || p.Id != pozycja.Id));

            var hasLocationLevelForProduct = await _context.PozycjaRezerwacji.AsNoTracking().AnyAsync(p =>
                p.IdRezerwacji == pozycja.IdRezerwacji &&
                p.IdProduktu == pozycja.IdProduktu &&
                p.IdLokacji != null &&
                (!isEdit || p.Id != pozycja.Id));

            if (!pozycja.IdLokacji.HasValue && hasLocationLevelForProduct)
            {
                ModelState.AddModelError(nameof(PozycjaRezerwacji.IdLokacji),
                    "Dla tego produktu w tej rezerwacji istnieją już pozycje z przypisaną lokacją. Nie mieszaj trybu lokacyjnego i magazynowego.");
            }
            else if (pozycja.IdLokacji.HasValue && hasWarehouseLevelForProduct)
            {
                ModelState.AddModelError(nameof(PozycjaRezerwacji.IdLokacji),
                    "Dla tego produktu w tej rezerwacji istnieje już pozycja bez lokacji. Nie mieszaj trybu magazynowego i lokacyjnego.");
            }
        }

        private void UstawDomyslneLpDlaCreate(PozycjaRezerwacji model)
        {
            if (model.IdRezerwacji == 0)
            {
                var firstId = _context.Rezerwacja.AsNoTracking()
                    .OrderByDescending(r => r.UtworzonoUtc).ThenByDescending(r => r.Id)
                    .Select(r => (int?)r.Id).FirstOrDefault();
                if (firstId.HasValue) model.IdRezerwacji = firstId.Value;
            }

            if (model.IdRezerwacji != 0 && model.Lp <= 1)
            {
                var nextLp = _context.PozycjaRezerwacji.AsNoTracking().Where(p => p.IdRezerwacji == model.IdRezerwacji)
                    .Select(p => (int?)p.Lp).Max();
                model.Lp = (nextLp ?? 0) + 1;
            }
        }

        private void UstawMapeLpDlaDokumentow()
        {
            var nextLpByReservation = _context.Rezerwacja.AsNoTracking()
                .Select(r => new
                {
                    r.Id,
                    NextLp = (_context.PozycjaRezerwacji.Where(p => p.IdRezerwacji == r.Id).Select(p => (int?)p.Lp).Max() ?? 0) + 1
                })
                .ToDictionary(x => x.Id, x => x.NextLp);
            ViewData["NextLpByDocumentJson"] = JsonSerializer.Serialize(nextLpByReservation);
        }

        private static bool CzyStatusRezerwacjiEdytowalny(string? status)
        {
            if (string.IsNullOrWhiteSpace(status)) return true;
            return !string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(status, "Cancelled", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(status, "Released", StringComparison.OrdinalIgnoreCase);
        }
    }
}

