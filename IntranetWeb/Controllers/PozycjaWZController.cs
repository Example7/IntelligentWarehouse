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
    public class PozycjaWZController : BaseSearchController<PozycjaWZ>
    {
        private readonly IPozycjaWZService _pozycjaWzService;

        public PozycjaWZController(DataContext context, IPozycjaWZService pozycjaWzService) : base(context)
        {
            _pozycjaWzService = pozycjaWzService;
        }

        public async Task<IActionResult> Index(string? searchTerm)
        {
            var model = await _pozycjaWzService.GetIndexDataAsync(searchTerm);
            return View(model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _pozycjaWzService.GetDetailsDataAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        public IActionResult Create()
        {
            var model = new PozycjaWZ { Lp = 1 };
            UstawDomyslneLpDlaCreate(model);
            UzupelnijDaneFormularza(model, onlyDraftDocuments: true);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdDokumentu,Lp,IdProduktu,IdLokacji,Ilosc")] PozycjaWZ pozycjaWZ)
        {
            await WalidujPozycjeWzAsync(pozycjaWZ, isEdit: false);

            if (ModelState.IsValid)
            {
                _context.Add(pozycjaWZ);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            UzupelnijDaneFormularza(pozycjaWZ, onlyDraftDocuments: true);
            return View(pozycjaWZ);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pozycjaWZ = await _context.PozycjaWZ
                .Include(p => p.Dokument)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (pozycjaWZ == null)
            {
                return NotFound();
            }

            if (!string.Equals(pozycjaWZ.Dokument?.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            {
                TempData["PozycjaWZEditBlocked"] = "Edycja pozycji WZ jest dostępna tylko dla dokumentów WZ w statusie Draft.";
                return RedirectToAction(nameof(Details), new { id = pozycjaWZ.Id });
            }

            UzupelnijDaneFormularza(pozycjaWZ, onlyDraftDocuments: true);
            return View(pozycjaWZ);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdDokumentu,Lp,IdProduktu,IdLokacji,Ilosc")] PozycjaWZ pozycjaWZ)
        {
            if (id != pozycjaWZ.Id)
            {
                return NotFound();
            }

            await WalidujPozycjeWzAsync(pozycjaWZ, isEdit: true);

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.PozycjaWZ
                        .Include(p => p.Dokument)
                        .FirstOrDefaultAsync(p => p.Id == id);
                    if (existing == null)
                    {
                        return NotFound();
                    }

                    if (!string.Equals(existing.Dokument?.Status, "Draft", StringComparison.OrdinalIgnoreCase))
                    {
                        ModelState.AddModelError(string.Empty, "Edytować można tylko pozycje dokumentu WZ w statusie Draft.");
                        UzupelnijDaneFormularza(pozycjaWZ, onlyDraftDocuments: true);
                        return View(pozycjaWZ);
                    }

                    existing.IdDokumentu = pozycjaWZ.IdDokumentu;
                    existing.Lp = pozycjaWZ.Lp;
                    existing.IdProduktu = pozycjaWZ.IdProduktu;
                    existing.IdLokacji = pozycjaWZ.IdLokacji;
                    existing.Ilosc = pozycjaWZ.Ilosc;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PozycjaWZExists(pozycjaWZ.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            UzupelnijDaneFormularza(pozycjaWZ, onlyDraftDocuments: true);
            return View(pozycjaWZ);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _pozycjaWzService.GetDeleteDataAsync(id.Value);
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
            var deleteData = await _pozycjaWzService.GetDeleteDataAsync(id);
            if (deleteData == null)
            {
                return NotFound();
            }

            if (!deleteData.CzyMoznaUsunac)
            {
                ModelState.AddModelError(string.Empty, deleteData.PowodBlokady ?? "Nie można usunąć pozycji WZ.");
                return View("Delete", deleteData);
            }

            var pozycjaWZ = await _context.PozycjaWZ.FindAsync(id);
            if (pozycjaWZ != null)
            {
                _context.PozycjaWZ.Remove(pozycjaWZ);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PozycjaWZExists(int id) => _context.PozycjaWZ.Any(e => e.Id == id);

        private void UzupelnijDaneFormularza(PozycjaWZ model, bool onlyDraftDocuments)
        {
            var dokumentyQuery = _context.DokumentWZ.AsNoTracking().Include(d => d.Magazyn).AsQueryable();
            if (onlyDraftDocuments)
            {
                dokumentyQuery = dokumentyQuery.Where(d => d.Status == "Draft");
            }

            ViewData["IdDokumentu"] = new SelectList(
                dokumentyQuery.OrderByDescending(d => d.DataWydaniaUtc).ThenByDescending(d => d.Id)
                .Select(d => new { d.Id, Label = $"{d.Numer} | {d.Status} | {(d.Magazyn != null ? d.Magazyn.Nazwa : "-")}" })
                .ToList(),
                "Id", "Label", model.IdDokumentu);
            UstawMapeLpDlaDokumentow();

            ViewData["IdLokacji"] = new SelectList(
                _context.Lokacja.AsNoTracking().Include(l => l.Magazyn).OrderBy(l => l.Magazyn.Nazwa).ThenBy(l => l.Kod)
                .Select(l => new { l.IdLokacji, Label = $"{(l.Magazyn != null ? l.Magazyn.Nazwa : "-")} / {l.Kod}" }).ToList(),
                "IdLokacji", "Label", model.IdLokacji);

            ViewData["IdProduktu"] = new SelectList(
                _context.Produkt.AsNoTracking().Include(p => p.DomyslnaJednostka).OrderBy(p => p.Kod).ThenBy(p => p.Nazwa)
                .Select(p => new { p.IdProduktu, Label = $"{p.Kod} - {p.Nazwa} ({(p.DomyslnaJednostka != null ? p.DomyslnaJednostka.Kod : "j.m.")})" }).ToList(),
                "IdProduktu", "Label", model.IdProduktu);

            var productUomMap = _context.Produkt.AsNoTracking().Include(p => p.DomyslnaJednostka)
                .Select(p => new { p.IdProduktu, Uom = p.DomyslnaJednostka != null ? p.DomyslnaJednostka.Kod : "j.m." })
                .ToDictionary(x => x.IdProduktu, x => x.Uom);
            ViewData["ProductUomMapJson"] = JsonSerializer.Serialize(productUomMap);
        }

        private async Task WalidujPozycjeWzAsync(PozycjaWZ pozycjaWZ, bool isEdit)
        {
            if (pozycjaWZ.Ilosc <= 0)
            {
                ModelState.AddModelError(nameof(PozycjaWZ.Ilosc), "Ilość musi być większa od zera.");
            }

            var dokument = await _context.DokumentWZ.AsNoTracking().FirstOrDefaultAsync(d => d.Id == pozycjaWZ.IdDokumentu);
            if (dokument == null)
            {
                ModelState.AddModelError(nameof(PozycjaWZ.IdDokumentu), "Wybrany dokument WZ nie istnieje.");
                return;
            }

            if (!string.Equals(dokument.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(PozycjaWZ.IdDokumentu), "Pozycje można dodawać/edytować tylko dla dokumentów WZ w statusie Draft.");
            }

            if (!pozycjaWZ.IdLokacji.HasValue)
            {
                ModelState.AddModelError(nameof(PozycjaWZ.IdLokacji), "Wybierz lokację wydania.");
            }
            else
            {
                var lokacja = await _context.Lokacja.AsNoTracking().FirstOrDefaultAsync(l => l.IdLokacji == pozycjaWZ.IdLokacji.Value);
                if (lokacja == null)
                {
                    ModelState.AddModelError(nameof(PozycjaWZ.IdLokacji), "Wybrana lokacja nie istnieje.");
                }
                else if (lokacja.IdMagazynu != dokument.IdMagazynu)
                {
                    ModelState.AddModelError(nameof(PozycjaWZ.IdLokacji), "Lokacja musi należeć do magazynu wybranego dokumentu WZ.");
                }
            }

            var duplicateLpExists = await _context.PozycjaWZ.AsNoTracking().AnyAsync(p =>
                p.IdDokumentu == pozycjaWZ.IdDokumentu && p.Lp == pozycjaWZ.Lp && (!isEdit || p.Id != pozycjaWZ.Id));
            if (duplicateLpExists)
            {
                ModelState.AddModelError(nameof(PozycjaWZ.Lp), "Pozycja o takim numerze Lp już istnieje w tym dokumencie WZ.");
            }
        }

        private void UstawDomyslneLpDlaCreate(PozycjaWZ model)
        {
            if (model.IdDokumentu == 0)
            {
                var firstDraftDocumentId = _context.DokumentWZ.AsNoTracking().Where(d => d.Status == "Draft")
                    .OrderByDescending(d => d.DataWydaniaUtc).ThenByDescending(d => d.Id)
                    .Select(d => (int?)d.Id).FirstOrDefault();
                if (firstDraftDocumentId.HasValue)
                {
                    model.IdDokumentu = firstDraftDocumentId.Value;
                }
            }

            if (model.IdDokumentu != 0 && model.Lp <= 1)
            {
                var nextLp = _context.PozycjaWZ.AsNoTracking().Where(p => p.IdDokumentu == model.IdDokumentu)
                    .Select(p => (int?)p.Lp).Max();
                model.Lp = (nextLp ?? 0) + 1;
            }
        }

        private void UstawMapeLpDlaDokumentow()
        {
            var nextLpByDocument = _context.DokumentWZ.AsNoTracking().Where(d => d.Status == "Draft")
                .Select(d => new
                {
                    d.Id,
                    NextLp = (_context.PozycjaWZ.Where(p => p.IdDokumentu == d.Id).Select(p => (int?)p.Lp).Max() ?? 0) + 1
                })
                .ToDictionary(x => x.Id, x => x.NextLp);

            ViewData["NextLpByDocumentJson"] = JsonSerializer.Serialize(nextLpByDocument);
        }
    }
}
