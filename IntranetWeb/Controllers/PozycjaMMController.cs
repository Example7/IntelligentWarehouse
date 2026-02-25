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
    public class PozycjaMMController : BaseSearchController<PozycjaMM>
    {
        private readonly IPozycjaMMService _pozycjaMmService;

        public PozycjaMMController(DataContext context, IPozycjaMMService pozycjaMmService) : base(context)
        {
            _pozycjaMmService = pozycjaMmService;
        }

        public async Task<IActionResult> Index(string? searchTerm)
        {
            var model = await _pozycjaMmService.GetIndexDataAsync(searchTerm);
            return View(model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _pozycjaMmService.GetDetailsDataAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        public IActionResult Create()
        {
            var model = new PozycjaMM { Lp = 1 };
            UstawDomyslneLpDlaCreate(model);
            UzupelnijDaneFormularza(model, onlyDraftDocuments: true);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdDokumentu,Lp,IdProduktu,IdPartii,IdLokacjiZ,IdLokacjiDo,Ilosc")] PozycjaMM pozycjaMM)
        {
            await WalidujPozycjeMmAsync(pozycjaMM, isEdit: false);

            if (ModelState.IsValid)
            {
                _context.Add(pozycjaMM);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            UzupelnijDaneFormularza(pozycjaMM, onlyDraftDocuments: true);
            return View(pozycjaMM);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pozycjaMM = await _context.PozycjaMM
                .Include(p => p.Dokument)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (pozycjaMM == null)
            {
                return NotFound();
            }

            if (!string.Equals(pozycjaMM.Dokument?.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            {
                TempData["PozycjaMMEditBlocked"] = "Edycja pozycji MM jest dostepna tylko dla dokumentow MM w statusie Draft.";
                return RedirectToAction(nameof(Details), new { id = pozycjaMM.Id });
            }

            UzupelnijDaneFormularza(pozycjaMM, onlyDraftDocuments: true);
            return View(pozycjaMM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdDokumentu,Lp,IdProduktu,IdPartii,IdLokacjiZ,IdLokacjiDo,Ilosc")] PozycjaMM pozycjaMM)
        {
            if (id != pozycjaMM.Id)
            {
                return NotFound();
            }

            await WalidujPozycjeMmAsync(pozycjaMM, isEdit: true);

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.PozycjaMM
                        .Include(p => p.Dokument)
                        .FirstOrDefaultAsync(p => p.Id == id);
                    if (existing == null)
                    {
                        return NotFound();
                    }

                    if (!string.Equals(existing.Dokument?.Status, "Draft", StringComparison.OrdinalIgnoreCase))
                    {
                        ModelState.AddModelError(string.Empty, "Edytowac mozna tylko pozycje dokumentu MM w statusie Draft.");
                        UzupelnijDaneFormularza(pozycjaMM, onlyDraftDocuments: true);
                        return View(pozycjaMM);
                    }

                    existing.IdDokumentu = pozycjaMM.IdDokumentu;
                    existing.Lp = pozycjaMM.Lp;
                    existing.IdProduktu = pozycjaMM.IdProduktu;
                    existing.IdPartii = pozycjaMM.IdPartii;
                    existing.IdLokacjiZ = pozycjaMM.IdLokacjiZ;
                    existing.IdLokacjiDo = pozycjaMM.IdLokacjiDo;
                    existing.Ilosc = pozycjaMM.Ilosc;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PozycjaMMExists(pozycjaMM.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            UzupelnijDaneFormularza(pozycjaMM, onlyDraftDocuments: true);
            return View(pozycjaMM);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _pozycjaMmService.GetDeleteDataAsync(id.Value);
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
            var deleteData = await _pozycjaMmService.GetDeleteDataAsync(id);
            if (deleteData == null)
            {
                return NotFound();
            }

            if (!deleteData.CzyMoznaUsunac)
            {
                ModelState.AddModelError(string.Empty, deleteData.PowodBlokady ?? "Nie mozna usunac pozycji MM.");
                return View("Delete", deleteData);
            }

            var pozycjaMM = await _context.PozycjaMM.FindAsync(id);
            if (pozycjaMM != null)
            {
                _context.PozycjaMM.Remove(pozycjaMM);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PozycjaMMExists(int id) => _context.PozycjaMM.Any(e => e.Id == id);

        private void UzupelnijDaneFormularza(PozycjaMM model, bool onlyDraftDocuments)
        {
            var dokumentyQuery = _context.DokumentMM.AsNoTracking().Include(d => d.Magazyn).AsQueryable();
            if (onlyDraftDocuments)
            {
                dokumentyQuery = dokumentyQuery.Where(d => d.Status == "Draft");
            }

            ViewData["IdDokumentu"] = new SelectList(
                dokumentyQuery.OrderByDescending(d => d.DataUtc).ThenByDescending(d => d.Id)
                .Select(d => new { d.Id, Label = $"{d.Numer} | {d.Status} | {(d.Magazyn != null ? d.Magazyn.Nazwa : "-")}" })
                .ToList(),
                "Id", "Label", model.IdDokumentu);
            UstawMapeLpDlaDokumentow();

            var lokacje = _context.Lokacja.AsNoTracking()
                .Include(l => l.Magazyn)
                .OrderBy(l => l.Magazyn.Nazwa)
                .ThenBy(l => l.Kod)
                .Select(l => new { l.IdLokacji, Label = $"{(l.Magazyn != null ? l.Magazyn.Nazwa : "-")} / {l.Kod}" })
                .ToList();

            ViewData["IdLokacjiZ"] = new SelectList(lokacje, "IdLokacji", "Label", model.IdLokacjiZ);
            ViewData["IdLokacjiDo"] = new SelectList(lokacje, "IdLokacji", "Label", model.IdLokacjiDo);

            ViewData["IdProduktu"] = new SelectList(
                _context.Produkt.AsNoTracking().Include(p => p.DomyslnaJednostka).OrderBy(p => p.Kod).ThenBy(p => p.Nazwa)
                .Select(p => new { p.IdProduktu, Label = $"{p.Kod} - {p.Nazwa} ({(p.DomyslnaJednostka != null ? p.DomyslnaJednostka.Kod : "j.m.")})" }).ToList(),
                "IdProduktu", "Label", model.IdProduktu);

            var partieItems = _context.Partia.AsNoTracking()
                .Include(b => b.Produkt)
                .OrderBy(b => b.Produkt != null ? b.Produkt.Kod : string.Empty)
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

            var productUomMap = _context.Produkt.AsNoTracking().Include(p => p.DomyslnaJednostka)
                .Select(p => new { p.IdProduktu, Uom = p.DomyslnaJednostka != null ? p.DomyslnaJednostka.Kod : "j.m." })
                .ToDictionary(x => x.IdProduktu, x => x.Uom);
            ViewData["ProductUomMapJson"] = JsonSerializer.Serialize(productUomMap);
        }

        private async Task WalidujPozycjeMmAsync(PozycjaMM pozycjaMM, bool isEdit)
        {
            if (pozycjaMM.Ilosc <= 0)
            {
                ModelState.AddModelError(nameof(PozycjaMM.Ilosc), "Ilosc musi byc wieksza od zera.");
            }

            var dokument = await _context.DokumentMM.AsNoTracking().FirstOrDefaultAsync(d => d.Id == pozycjaMM.IdDokumentu);
            if (dokument == null)
            {
                ModelState.AddModelError(nameof(PozycjaMM.IdDokumentu), "Wybrany dokument MM nie istnieje.");
                return;
            }

            if (!string.Equals(dokument.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(PozycjaMM.IdDokumentu), "Pozycje mozna dodawac/edytowac tylko dla dokumentow MM w statusie Draft.");
            }

            var lokacjaZ = await _context.Lokacja.AsNoTracking().FirstOrDefaultAsync(l => l.IdLokacji == pozycjaMM.IdLokacjiZ);
            var lokacjaDo = await _context.Lokacja.AsNoTracking().FirstOrDefaultAsync(l => l.IdLokacji == pozycjaMM.IdLokacjiDo);

            if (lokacjaZ == null)
            {
                ModelState.AddModelError(nameof(PozycjaMM.IdLokacjiZ), "Wybrana lokacja Z nie istnieje.");
            }
            else if (lokacjaZ.IdMagazynu != dokument.IdMagazynu)
            {
                ModelState.AddModelError(nameof(PozycjaMM.IdLokacjiZ), "Lokacja Z musi nalezec do magazynu wybranego dokumentu MM.");
            }

            if (lokacjaDo == null)
            {
                ModelState.AddModelError(nameof(PozycjaMM.IdLokacjiDo), "Wybrana lokacja Do nie istnieje.");
            }
            else if (lokacjaDo.IdMagazynu != dokument.IdMagazynu)
            {
                ModelState.AddModelError(nameof(PozycjaMM.IdLokacjiDo), "Lokacja Do musi nalezec do magazynu wybranego dokumentu MM.");
            }

            if (pozycjaMM.IdLokacjiZ == pozycjaMM.IdLokacjiDo)
            {
                ModelState.AddModelError(nameof(PozycjaMM.IdLokacjiDo), "Lokacja Z i Do muszą być różne.");
            }

            var produktExists = await _context.Produkt.AsNoTracking().AnyAsync(p => p.IdProduktu == pozycjaMM.IdProduktu);
            if (!produktExists)
            {
                ModelState.AddModelError(nameof(PozycjaMM.IdProduktu), "Wybrany produkt nie istnieje.");
            }

            if (pozycjaMM.IdPartii.HasValue)
            {
                var partia = await _context.Partia.AsNoTracking()
                    .FirstOrDefaultAsync(b => b.IdPartii == pozycjaMM.IdPartii.Value);

                if (partia == null)
                {
                    ModelState.AddModelError(nameof(PozycjaMM.IdPartii), "Wybrana partia nie istnieje.");
                }
                else if (partia.IdProduktu != pozycjaMM.IdProduktu)
                {
                    ModelState.AddModelError(nameof(PozycjaMM.IdPartii), "Wybrana partia nie nalezy do wskazanego produktu.");
                }
            }

            var duplicateLpExists = await _context.PozycjaMM.AsNoTracking().AnyAsync(p =>
                p.IdDokumentu == pozycjaMM.IdDokumentu && p.Lp == pozycjaMM.Lp && (!isEdit || p.Id != pozycjaMM.Id));
            if (duplicateLpExists)
            {
                ModelState.AddModelError(nameof(PozycjaMM.Lp), "Pozycja o takim numerze Lp już istnieje w tym dokumencie MM.");
            }

            if (lokacjaZ != null && produktExists && pozycjaMM.Ilosc > 0)
            {
                var stanZ = await _context.StanMagazynowy.AsNoTracking()
                    .FirstOrDefaultAsync(s => s.IdProduktu == pozycjaMM.IdProduktu && s.IdLokacji == pozycjaMM.IdLokacjiZ);

                if (stanZ == null || stanZ.Ilosc <= 0)
                {
                    ModelState.AddModelError(nameof(PozycjaMM.IdLokacjiZ), "W lokacji Z nie ma stanu dla wybranego produktu.");
                }
                else
                {
                    var sumaPozycjiDlaProduktuIZrodla = await _context.PozycjaMM.AsNoTracking()
                        .Where(p =>
                            p.IdDokumentu == pozycjaMM.IdDokumentu &&
                            p.IdProduktu == pozycjaMM.IdProduktu &&
                            p.IdLokacjiZ == pozycjaMM.IdLokacjiZ &&
                            (!isEdit || p.Id != pozycjaMM.Id))
                        .SumAsync(p => (decimal?)p.Ilosc) ?? 0m;

                    var sumaPoZmianie = sumaPozycjiDlaProduktuIZrodla + pozycjaMM.Ilosc;
                    if (sumaPoZmianie > stanZ.Ilosc)
                    {
                        ModelState.AddModelError(
                            nameof(PozycjaMM.Ilosc),
                            $"Niewystarczajacy stan w lokacji Z. Dostępne: {stanZ.Ilosc:0.###}, suma pozycji MM po zapisie: {sumaPoZmianie:0.###}.");
                    }
                }
            }
        }

        private void UstawDomyslneLpDlaCreate(PozycjaMM model)
        {
            if (model.IdDokumentu == 0)
            {
                var firstDraftDocumentId = _context.DokumentMM.AsNoTracking().Where(d => d.Status == "Draft")
                    .OrderByDescending(d => d.DataUtc).ThenByDescending(d => d.Id)
                    .Select(d => (int?)d.Id).FirstOrDefault();
                if (firstDraftDocumentId.HasValue)
                {
                    model.IdDokumentu = firstDraftDocumentId.Value;
                }
            }

            if (model.IdDokumentu != 0 && model.Lp <= 1)
            {
                var nextLp = _context.PozycjaMM.AsNoTracking().Where(p => p.IdDokumentu == model.IdDokumentu)
                    .Select(p => (int?)p.Lp).Max();
                model.Lp = (nextLp ?? 0) + 1;
            }
        }

        private void UstawMapeLpDlaDokumentow()
        {
            var nextLpByDocument = _context.DokumentMM.AsNoTracking().Where(d => d.Status == "Draft")
                .Select(d => new
                {
                    d.Id,
                    NextLp = (_context.PozycjaMM.Where(p => p.IdDokumentu == d.Id).Select(p => (int?)p.Lp).Max() ?? 0) + 1
                })
                .ToDictionary(x => x.Id, x => x.NextLp);

            ViewData["NextLpByDocumentJson"] = JsonSerializer.Serialize(nextLpByDocument);
        }
    }
}
