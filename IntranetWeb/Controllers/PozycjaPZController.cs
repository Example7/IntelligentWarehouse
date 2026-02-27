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
    [Authorize(Roles = AppRoles.AdminMagazynierOperator)]
    public class PozycjaPZController : BaseSearchController<PozycjaPZ>
    {
        private readonly IPozycjaPZService _pozycjaPzService;

        public PozycjaPZController(DataContext context, IPozycjaPZService pozycjaPzService) : base(context)
        {
            _pozycjaPzService = pozycjaPzService;
        }

        public async Task<IActionResult> Index(string? searchTerm)
        {
            var model = await _pozycjaPzService.GetIndexDataAsync(searchTerm);
            return View(model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _pozycjaPzService.GetDetailsDataAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        public IActionResult Create()
        {
            var model = new PozycjaPZ
            {
                Lp = 1
            };
            UstawDomyslneLpDlaCreate(model);
            UzupelnijDaneFormularza(model, onlyDraftDocuments: true);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdDokumentu,Lp,IdProduktu,IdLokacji,IdPartii,Ilosc,CenaJednostkowa")] PozycjaPZ pozycjaPZ)
        {
            await WalidujPozycjePzAsync(pozycjaPZ, isEdit: false);

            if (ModelState.IsValid)
            {
                _context.Add(pozycjaPZ);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            UzupelnijDaneFormularza(pozycjaPZ, onlyDraftDocuments: true);
            return View(pozycjaPZ);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pozycjaPZ = await _context.PozycjaPZ
                .Include(p => p.Dokument)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (pozycjaPZ == null)
            {
                return NotFound();
            }

            if (!string.Equals(pozycjaPZ.Dokument?.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            {
                TempData["PozycjaPZEditBlocked"] = "Edycja pozycji PZ jest dostępna tylko dla dokumentow PZ w statusie Draft.";
                return RedirectToAction(nameof(Details), new { id = pozycjaPZ.Id });
            }

            UzupelnijDaneFormularza(pozycjaPZ, onlyDraftDocuments: true);
            return View(pozycjaPZ);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdDokumentu,Lp,IdProduktu,IdLokacji,IdPartii,Ilosc,CenaJednostkowa")] PozycjaPZ pozycjaPZ)
        {
            if (id != pozycjaPZ.Id)
            {
                return NotFound();
            }

            await WalidujPozycjePzAsync(pozycjaPZ, isEdit: true);

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.PozycjaPZ
                        .Include(p => p.Dokument)
                        .FirstOrDefaultAsync(p => p.Id == id);
                    if (existing == null)
                    {
                        return NotFound();
                    }

                    if (!string.Equals(existing.Dokument?.Status, "Draft", StringComparison.OrdinalIgnoreCase))
                    {
                        ModelState.AddModelError(string.Empty, "Edytować można tylko pozycje dokumentu PZ w statusie Draft.");
                        UzupelnijDaneFormularza(pozycjaPZ, onlyDraftDocuments: true);
                        return View(pozycjaPZ);
                    }

                    existing.IdDokumentu = pozycjaPZ.IdDokumentu;
                    existing.Lp = pozycjaPZ.Lp;
                    existing.IdProduktu = pozycjaPZ.IdProduktu;
                    existing.IdLokacji = pozycjaPZ.IdLokacji;
                    existing.IdPartii = pozycjaPZ.IdPartii;
                    existing.Ilosc = pozycjaPZ.Ilosc;
                    existing.CenaJednostkowa = pozycjaPZ.CenaJednostkowa;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PozycjaPZExists(pozycjaPZ.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            UzupelnijDaneFormularza(pozycjaPZ, onlyDraftDocuments: true);
            return View(pozycjaPZ);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _pozycjaPzService.GetDeleteDataAsync(id.Value);
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
            var deleteData = await _pozycjaPzService.GetDeleteDataAsync(id);
            if (deleteData == null)
            {
                return NotFound();
            }

            if (!deleteData.CzyMoznaUsunac)
            {
                ModelState.AddModelError(string.Empty, deleteData.PowodBlokady ?? "Nie można usunąć pozycji PZ.");
                return View("Delete", deleteData);
            }

            var pozycjaPZ = await _context.PozycjaPZ.FindAsync(id);
            if (pozycjaPZ != null)
            {
                _context.PozycjaPZ.Remove(pozycjaPZ);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PozycjaPZExists(int id)
        {
            return _context.PozycjaPZ.Any(e => e.Id == id);
        }

        private void UzupelnijDaneFormularza(PozycjaPZ model, bool onlyDraftDocuments)
        {
            var dokumentyQuery = _context.DokumentPZ
                .AsNoTracking()
                .Include(d => d.Magazyn)
                .AsQueryable();

            if (onlyDraftDocuments)
            {
                dokumentyQuery = dokumentyQuery.Where(d => d.Status == "Draft");
            }

            ViewData["IdDokumentu"] = new SelectList(
                dokumentyQuery
                    .OrderByDescending(d => d.DataPrzyjeciaUtc)
                    .ThenByDescending(d => d.Id)
                    .Select(d => new
                    {
                        d.Id,
                        Label = $"{d.Numer} | {d.Status} | {(d.Magazyn != null ? d.Magazyn.Nazwa : "-")}"
                    })
                    .ToList(),
                "Id",
                "Label",
                model.IdDokumentu);
            UstawMapeLpDlaDokumentow();

            ViewData["IdLokacji"] = new SelectList(
                _context.Lokacja
                    .AsNoTracking()
                    .Include(l => l.Magazyn)
                    .OrderBy(l => l.Magazyn.Nazwa)
                    .ThenBy(l => l.Kod)
                    .Select(l => new
                    {
                        l.IdLokacji,
                        Label = $"{(l.Magazyn != null ? l.Magazyn.Nazwa : "-")} / {l.Kod}"
                    })
                    .ToList(),
                "IdLokacji",
                "Label",
                model.IdLokacji);

            ViewData["IdProduktu"] = new SelectList(
                _context.Produkt
                    .AsNoTracking()
                    .Include(p => p.DomyslnaJednostka)
                    .OrderBy(p => p.Kod)
                    .ThenBy(p => p.Nazwa)
                    .Select(p => new
                    {
                        p.IdProduktu,
                        Label = $"{p.Kod} - {p.Nazwa} ({(p.DomyslnaJednostka != null ? p.DomyslnaJednostka.Kod : "j.m.")})"
                    })
                    .ToList(),
                "IdProduktu",
                "Label",
                model.IdProduktu);

            var partieItems = _context.Partia
                .AsNoTracking()
                .Include(b => b.Produkt)
                .OrderBy(b => b.Produkt.Kod)
                .ThenBy(b => b.NumerPartii)
                .Select(b => new SelectListItem
                {
                    Value = b.IdPartii.ToString(),
                    Text = $"{(b.Produkt != null ? b.Produkt.Kod : "-")} / {b.NumerPartii}",
                    Selected = model.IdPartii == b.IdPartii
                })
                .ToList();
            partieItems.Insert(0, new SelectListItem
            {
                Value = string.Empty,
                Text = "(brak partii)",
                Selected = !model.IdPartii.HasValue
            });
            ViewData["IdPartii"] = partieItems;
        }

        private async Task WalidujPozycjePzAsync(PozycjaPZ pozycjaPZ, bool isEdit)
        {
            if (pozycjaPZ.Ilosc <= 0)
            {
                ModelState.AddModelError(nameof(PozycjaPZ.Ilosc), "Ilość musi być większa od zera.");
            }

            if (pozycjaPZ.CenaJednostkowa.HasValue && pozycjaPZ.CenaJednostkowa.Value < 0m)
            {
                ModelState.AddModelError(nameof(PozycjaPZ.CenaJednostkowa), "Cena jednostkowa nie może być ujemna.");
            }

            var dokument = await _context.DokumentPZ
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == pozycjaPZ.IdDokumentu);
            if (dokument == null)
            {
                ModelState.AddModelError(nameof(PozycjaPZ.IdDokumentu), "Wybrany dokument PZ nie istnieje.");
                return;
            }

            if (!string.Equals(dokument.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(PozycjaPZ.IdDokumentu), "Pozycje można dodawać/edytować tylko dla dokumentów PZ w statusie Draft.");
            }

            var lokacja = await _context.Lokacja
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.IdLokacji == pozycjaPZ.IdLokacji);
            if (lokacja == null)
            {
                ModelState.AddModelError(nameof(PozycjaPZ.IdLokacji), "Wybrana lokacja nie istnieje.");
            }
            else if (lokacja.IdMagazynu != dokument.IdMagazynu)
            {
                ModelState.AddModelError(nameof(PozycjaPZ.IdLokacji), "Lokacja musi należeć do magazynu wybranego dokumentu PZ.");
            }

            var duplicateLpExists = await _context.PozycjaPZ
                .AsNoTracking()
                .AnyAsync(p => p.IdDokumentu == pozycjaPZ.IdDokumentu &&
                               p.Lp == pozycjaPZ.Lp &&
                               (!isEdit || p.Id != pozycjaPZ.Id));
            if (duplicateLpExists)
            {
                ModelState.AddModelError(nameof(PozycjaPZ.Lp), "Pozycja o takim numerze Lp już istnieje w tym dokumencie PZ.");
            }
            if (pozycjaPZ.IdPartii.HasValue)
            {
                var partia = await _context.Partia
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.IdPartii == pozycjaPZ.IdPartii.Value);

                if (partia == null)
                {
                    ModelState.AddModelError(nameof(PozycjaPZ.IdPartii), "Wybrana partia nie istnieje.");
                }
                else if (partia.IdProduktu != pozycjaPZ.IdProduktu)
                {
                    ModelState.AddModelError(nameof(PozycjaPZ.IdPartii), "Wybrana partia musi należeć do wybranego produktu.");
                }
            }
        }

        private void UstawDomyslneLpDlaCreate(PozycjaPZ model)
        {
            if (model.IdDokumentu == 0)
            {
                var firstDraftDocumentId = _context.DokumentPZ
                    .AsNoTracking()
                    .Where(d => d.Status == "Draft")
                    .OrderByDescending(d => d.DataPrzyjeciaUtc)
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
                var nextLp = _context.PozycjaPZ
                    .AsNoTracking()
                    .Where(p => p.IdDokumentu == model.IdDokumentu)
                    .Select(p => (int?)p.Lp)
                    .Max();

                model.Lp = (nextLp ?? 0) + 1;
            }
        }

        private void UstawMapeLpDlaDokumentow()
        {
            var nextLpByDocument = _context.DokumentPZ
                .AsNoTracking()
                .Where(d => d.Status == "Draft")
                .Select(d => new
                {
                    d.Id,
                    NextLp = (_context.PozycjaPZ.Where(p => p.IdDokumentu == d.Id).Select(p => (int?)p.Lp).Max() ?? 0) + 1
                })
                .ToDictionary(x => x.Id, x => x.NextLp);

            ViewData["NextLpByDocumentJson"] = JsonSerializer.Serialize(nextLpByDocument);
        }
    }
}

