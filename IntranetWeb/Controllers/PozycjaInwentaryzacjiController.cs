using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;
using IntranetWeb.Controllers.Abstrakcja;
using IntranetWeb.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace IntranetWeb.Controllers
{
    [Authorize(Roles = AppRoles.AdminMagazynier)]
    public class PozycjaInwentaryzacjiController : BaseSearchController<PozycjaInwentaryzacji>
    {
        private readonly IPozycjaInwentaryzacjiService _pozycjaInwentaryzacjiService;

        public PozycjaInwentaryzacjiController(DataContext context, IPozycjaInwentaryzacjiService pozycjaInwentaryzacjiService) : base(context)
        {
            _pozycjaInwentaryzacjiService = pozycjaInwentaryzacjiService;
        }

        public async Task<IActionResult> Index(string? searchTerm)
        {
            var model = await _pozycjaInwentaryzacjiService.GetIndexDataAsync(searchTerm);
            return View(model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _pozycjaInwentaryzacjiService.GetDetailsDataAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        public IActionResult Create()
        {
            var model = new PozycjaInwentaryzacji { Lp = 1 };
            UstawDomyslneLpDlaCreate(model);
            UzupelnijDaneFormularza(model, onlyDraftDocuments: true);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdDokumentu,Lp,IdProduktu,IdLokacji,IloscOczekiwana,IloscPoliczona")] PozycjaInwentaryzacji pozycja)
        {
            await WalidujPozycjeAsync(pozycja, isEdit: false);

            if (ModelState.IsValid)
            {
                _context.Add(pozycja);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            UzupelnijDaneFormularza(pozycja, onlyDraftDocuments: true);
            return View(pozycja);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pozycja = await _context.PozycjaInwentaryzacji
                .Include(p => p.Dokument)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (pozycja == null)
            {
                return NotFound();
            }

            if (!string.Equals(pozycja.Dokument?.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            {
                TempData["PozycjaInwentaryzacjiEditBlocked"] = "Edycja pozycji inwentaryzacji jest dostępna tylko dla dokumentów w statusie Draft.";
                return RedirectToAction(nameof(Details), new { id = pozycja.Id });
            }

            UzupelnijDaneFormularza(pozycja, onlyDraftDocuments: true);
            return View(pozycja);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdDokumentu,Lp,IdProduktu,IdLokacji,IloscOczekiwana,IloscPoliczona")] PozycjaInwentaryzacji pozycja)
        {
            if (id != pozycja.Id)
            {
                return NotFound();
            }

            await WalidujPozycjeAsync(pozycja, isEdit: true);

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.PozycjaInwentaryzacji
                        .Include(p => p.Dokument)
                        .FirstOrDefaultAsync(p => p.Id == id);
                    if (existing == null)
                    {
                        return NotFound();
                    }

                    if (!string.Equals(existing.Dokument?.Status, "Draft", StringComparison.OrdinalIgnoreCase))
                    {
                        ModelState.AddModelError(string.Empty, "Edytować można tylko pozycje inwentaryzacji w statusie Draft.");
                        UzupelnijDaneFormularza(pozycja, onlyDraftDocuments: true);
                        return View(pozycja);
                    }

                    existing.IdDokumentu = pozycja.IdDokumentu;
                    existing.Lp = pozycja.Lp;
                    existing.IdProduktu = pozycja.IdProduktu;
                    existing.IdLokacji = pozycja.IdLokacji;
                    existing.IloscOczekiwana = pozycja.IloscOczekiwana;
                    existing.IloscPoliczona = pozycja.IloscPoliczona;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PozycjaExists(pozycja.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }
            }

            if (!ModelState.IsValid)
            {
                UzupelnijDaneFormularza(pozycja, onlyDraftDocuments: true);
                return View(pozycja);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _pozycjaInwentaryzacjiService.GetDeleteDataAsync(id.Value);
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
            var deleteData = await _pozycjaInwentaryzacjiService.GetDeleteDataAsync(id);
            if (deleteData == null)
            {
                return NotFound();
            }

            if (!deleteData.CzyMoznaUsunac)
            {
                ModelState.AddModelError(string.Empty, deleteData.PowodBlokady ?? "Nie można usunąć pozycji inwentaryzacji.");
                return View("Delete", deleteData);
            }

            var pozycja = await _context.PozycjaInwentaryzacji.FindAsync(id);
            if (pozycja != null)
            {
                _context.PozycjaInwentaryzacji.Remove(pozycja);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PozycjaExists(int id) => _context.PozycjaInwentaryzacji.Any(e => e.Id == id);

        private void UzupelnijDaneFormularza(PozycjaInwentaryzacji model, bool onlyDraftDocuments)
        {
            var dokumentyQuery = _context.Inwentaryzacja.AsNoTracking().Include(d => d.Magazyn).AsQueryable();
            if (onlyDraftDocuments)
            {
                dokumentyQuery = dokumentyQuery.Where(d => d.Status == "Draft");
            }

            ViewData["IdDokumentu"] = new SelectList(
                dokumentyQuery.OrderByDescending(d => d.StartUtc).ThenByDescending(d => d.Id)
                    .Select(d => new { d.Id, Label = $"{d.Numer} | {d.Status} | {(d.Magazyn != null ? d.Magazyn.Nazwa : "-")}" })
                    .ToList(),
                "Id", "Label", model.IdDokumentu);
            UstawMapeLpDlaDokumentow();

            ViewData["IdLokacji"] = new SelectList(
                _context.Lokacja.AsNoTracking().Include(l => l.Magazyn)
                    .OrderBy(l => l.Magazyn.Nazwa).ThenBy(l => l.Kod)
                    .Select(l => new { l.IdLokacji, Label = $"{(l.Magazyn != null ? l.Magazyn.Nazwa : "-")} / {l.Kod}" })
                    .ToList(),
                "IdLokacji", "Label", model.IdLokacji);

            ViewData["IdProduktu"] = new SelectList(
                _context.Produkt.AsNoTracking().Include(p => p.DomyslnaJednostka)
                    .OrderBy(p => p.Kod).ThenBy(p => p.Nazwa)
                    .Select(p => new { p.IdProduktu, Label = $"{p.Kod} - {p.Nazwa} ({(p.DomyslnaJednostka != null ? p.DomyslnaJednostka.Kod : "j.m.")})" })
                    .ToList(),
                "IdProduktu", "Label", model.IdProduktu);

            var productUomMap = _context.Produkt.AsNoTracking().Include(p => p.DomyslnaJednostka)
                .Select(p => new { p.IdProduktu, Uom = p.DomyslnaJednostka != null ? p.DomyslnaJednostka.Kod : "j.m." })
                .ToDictionary(x => x.IdProduktu, x => x.Uom);
            ViewData["ProductUomMapJson"] = JsonSerializer.Serialize(productUomMap);

            var stockQtyMap = _context.StanMagazynowy.AsNoTracking()
                .Select(s => new { s.IdProduktu, s.IdLokacji, s.Ilosc })
                .ToDictionary(
                    x => $"{x.IdProduktu}:{x.IdLokacji}",
                    x => x.Ilosc);
            ViewData["StockQtyMapJson"] = JsonSerializer.Serialize(stockQtyMap);
        }

        private async Task WalidujPozycjeAsync(PozycjaInwentaryzacji pozycja, bool isEdit)
        {
            if (pozycja.IloscOczekiwana < 0)
            {
                ModelState.AddModelError(nameof(PozycjaInwentaryzacji.IloscOczekiwana), "Ilość oczekiwana nie może być ujemna.");
            }

            if (pozycja.IloscPoliczona < 0)
            {
                ModelState.AddModelError(nameof(PozycjaInwentaryzacji.IloscPoliczona), "Ilość policzona nie może być ujemna.");
            }

            var dokument = await _context.Inwentaryzacja.AsNoTracking().FirstOrDefaultAsync(d => d.Id == pozycja.IdDokumentu);
            if (dokument == null)
            {
                ModelState.AddModelError(nameof(PozycjaInwentaryzacji.IdDokumentu), "Wybrana inwentaryzacja nie istnieje.");
                return;
            }

            if (!string.Equals(dokument.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(PozycjaInwentaryzacji.IdDokumentu), "Pozycje można dodawać/edytować tylko dla inwentaryzacji w statusie Draft.");
            }

            var lokacja = await _context.Lokacja.AsNoTracking().FirstOrDefaultAsync(l => l.IdLokacji == pozycja.IdLokacji);
            if (lokacja == null)
            {
                ModelState.AddModelError(nameof(PozycjaInwentaryzacji.IdLokacji), "Wybrana lokacja nie istnieje.");
            }
            else if (lokacja.IdMagazynu != dokument.IdMagazynu)
            {
                ModelState.AddModelError(nameof(PozycjaInwentaryzacji.IdLokacji), "Lokacja musi należeć do magazynu wybranej inwentaryzacji.");
            }

            var produktExists = await _context.Produkt.AsNoTracking().AnyAsync(p => p.IdProduktu == pozycja.IdProduktu);
            if (!produktExists)
            {
                ModelState.AddModelError(nameof(PozycjaInwentaryzacji.IdProduktu), "Wybrany produkt nie istnieje.");
            }

            var duplicateLpExists = await _context.PozycjaInwentaryzacji.AsNoTracking().AnyAsync(p =>
                p.IdDokumentu == pozycja.IdDokumentu &&
                p.Lp == pozycja.Lp &&
                (!isEdit || p.Id != pozycja.Id));
            if (duplicateLpExists)
            {
                ModelState.AddModelError(nameof(PozycjaInwentaryzacji.Lp), "Pozycja o takim numerze Lp już istnieje w tej inwentaryzacji.");
            }

            var duplicateProductLocationExists = await _context.PozycjaInwentaryzacji.AsNoTracking().AnyAsync(p =>
                p.IdDokumentu == pozycja.IdDokumentu &&
                p.IdProduktu == pozycja.IdProduktu &&
                p.IdLokacji == pozycja.IdLokacji &&
                (!isEdit || p.Id != pozycja.Id));
            if (duplicateProductLocationExists)
            {
                ModelState.AddModelError(nameof(PozycjaInwentaryzacji.IdProduktu), "Pozycja dla tego produktu i lokacji już istnieje w tej inwentaryzacji.");
            }
        }

        private void UstawDomyslneLpDlaCreate(PozycjaInwentaryzacji model)
        {
            if (model.IdDokumentu == 0)
            {
                var firstDraftDocumentId = _context.Inwentaryzacja.AsNoTracking()
                    .Where(d => d.Status == "Draft")
                    .OrderByDescending(d => d.StartUtc)
                    .ThenByDescending(d => d.Id)
                    .Select(d => (int?)d.Id)
                    .FirstOrDefault();
                if (firstDraftDocumentId.HasValue)
                {
                    model.IdDokumentu = firstDraftDocumentId.Value;
                }
            }

            if (model.IdDokumentu != 0 && model.Lp <= 1)
            {
                var nextLp = _context.PozycjaInwentaryzacji.AsNoTracking()
                    .Where(p => p.IdDokumentu == model.IdDokumentu)
                    .Select(p => (int?)p.Lp)
                    .Max();
                model.Lp = (nextLp ?? 0) + 1;
            }
        }

        private void UstawMapeLpDlaDokumentow()
        {
            var nextLpByDocument = _context.Inwentaryzacja.AsNoTracking()
                .Where(d => d.Status == "Draft")
                .Select(d => new
                {
                    d.Id,
                    NextLp = (_context.PozycjaInwentaryzacji.Where(p => p.IdDokumentu == d.Id).Select(p => (int?)p.Lp).Max() ?? 0) + 1
                })
                .ToDictionary(x => x.Id, x => x.NextLp);

            ViewData["NextLpByDocumentJson"] = JsonSerializer.Serialize(nextLpByDocument);
        }
    }
}
