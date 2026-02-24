using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;
using Interfaces.Magazyn.Dtos;

using IntranetWeb.Controllers.Abstrakcja;

namespace IntranetWeb.Controllers
{
    public class KodProduktuController : BaseSearchController<KodProduktu>
    {
        private readonly IKodProduktuService _kodProduktuService;

        public KodProduktuController(DataContext context, IKodProduktuService kodProduktuService) : base(context)
        {
            _kodProduktuService = kodProduktuService;
        }

        // GET: KodProduktu
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var model = await _kodProduktuService.GetIndexDataAsync(searchTerm);
            return View(model);
        }

        // GET: KodProduktu/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _kodProduktuService.GetDetailsDataAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // GET: KodProduktu/Create
        public async Task<IActionResult> Create()
        {
            await PopulateProduktySelectAsync(null);
            return View(new KodProduktu { Typ = "EAN" });
        }

        // POST: KodProduktu/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdKodu,IdProduktu,Wartosc,Typ,CzyGlowny")] KodProduktu kodProduktu)
        {
            await PopulateProduktySelectAsync(kodProduktu.IdProduktu);
            NormalizeKodProduktu(kodProduktu);
            await ValidateKodProduktuAsync(kodProduktu);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(kodProduktu);
                    await EnsureSinglePrimaryCodeAsync(kodProduktu.IdProduktu, null, kodProduktu.CzyGlowny);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    if (await IsDuplicateCodeValueAsync(kodProduktu.Wartosc, null))
                    {
                        ModelState.AddModelError(nameof(KodProduktu.Wartosc), $"Kod produktu o wartości '{kodProduktu.Wartosc}' już istnieje.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Nie udało się zapisać kodu produktu.");
                    }
                }
            }

            return View(kodProduktu);
        }

        // GET: KodProduktu/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kodProduktu = await _context.KodProduktu.AsNoTracking().FirstOrDefaultAsync(x => x.IdKodu == id);
            if (kodProduktu == null)
            {
                return NotFound();
            }
            await PopulateProduktySelectAsync(kodProduktu.IdProduktu);
            return View(kodProduktu);
        }

        // POST: KodProduktu/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdKodu,IdProduktu,Wartosc,Typ,CzyGlowny")] KodProduktu kodProduktu)
        {
            await PopulateProduktySelectAsync(kodProduktu.IdProduktu);
            if (id != kodProduktu.IdKodu)
            {
                return NotFound();
            }

            NormalizeKodProduktu(kodProduktu);
            await ValidateKodProduktuAsync(kodProduktu, kodProduktu.IdKodu);

            if (ModelState.IsValid)
            {
                var existing = await _context.KodProduktu.FirstOrDefaultAsync(x => x.IdKodu == id);
                if (existing == null)
                {
                    return NotFound();
                }

                try
                {
                    existing.IdProduktu = kodProduktu.IdProduktu;
                    existing.Wartosc = kodProduktu.Wartosc;
                    existing.Typ = kodProduktu.Typ;
                    existing.CzyGlowny = kodProduktu.CzyGlowny;

                    await EnsureSinglePrimaryCodeAsync(existing.IdProduktu, existing.IdKodu, existing.CzyGlowny);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KodProduktuExists(kodProduktu.IdKodu))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException)
                {
                    if (await IsDuplicateCodeValueAsync(kodProduktu.Wartosc, kodProduktu.IdKodu))
                    {
                        ModelState.AddModelError(nameof(KodProduktu.Wartosc), $"Kod produktu o wartości '{kodProduktu.Wartosc}' już istnieje.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Nie udało się zapisać zmian kodu produktu.");
                    }

                    return View(kodProduktu);
                }

                return RedirectToAction(nameof(Index));
            }
            return View(kodProduktu);
        }

        // GET: KodProduktu/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _kodProduktuService.GetDeleteDataAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // POST: KodProduktu/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var kodProduktu = await _context.KodProduktu.FindAsync(id);
            if (kodProduktu != null)
            {
                _context.KodProduktu.Remove(kodProduktu);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool KodProduktuExists(int id)
        {
            return _context.KodProduktu.Any(e => e.IdKodu == id);
        }

        private static void NormalizeKodProduktu(KodProduktu kodProduktu)
        {
            kodProduktu.Wartosc = (kodProduktu.Wartosc ?? string.Empty).Trim();
            kodProduktu.Typ = string.IsNullOrWhiteSpace(kodProduktu.Typ) ? "EAN" : kodProduktu.Typ.Trim().ToUpperInvariant();
        }

        private async Task ValidateKodProduktuAsync(KodProduktu kodProduktu, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(kodProduktu.Wartosc))
            {
                return;
            }

            if (await IsDuplicateCodeValueAsync(kodProduktu.Wartosc, excludeId))
            {
                ModelState.AddModelError(nameof(KodProduktu.Wartosc), $"Kod produktu o wartości '{kodProduktu.Wartosc}' już istnieje.");
            }
        }

        private Task<bool> IsDuplicateCodeValueAsync(string wartosc, int? excludeId)
        {
            var query = _context.KodProduktu.AsNoTracking().Where(k => k.Wartosc == wartosc);
            if (excludeId.HasValue)
            {
                query = query.Where(k => k.IdKodu != excludeId.Value);
            }

            return query.AnyAsync();
        }

        private async Task EnsureSinglePrimaryCodeAsync(int idProduktu, int? currentIdKodu, bool czyGlowny)
        {
            if (!czyGlowny)
            {
                return;
            }

            var otherPrimaryCodes = await _context.KodProduktu
                .Where(k => k.IdProduktu == idProduktu && k.CzyGlowny && (!currentIdKodu.HasValue || k.IdKodu != currentIdKodu.Value))
                .ToListAsync();

            foreach (var code in otherPrimaryCodes)
            {
                code.CzyGlowny = false;
            }
        }

        private async Task PopulateProduktySelectAsync(int? selectedId)
        {
            var produkty = await _context.Produkt
                .AsNoTracking()
                .Include(p => p.DomyslnaJednostka)
                .OrderBy(p => p.Kod)
                .ThenBy(p => p.Nazwa)
                .Select(p => new
                {
                    p.IdProduktu,
                    Label = p.Kod + " - " + p.Nazwa + " (" + (p.DomyslnaJednostka != null ? p.DomyslnaJednostka.Kod : "j.m.") + ")"
                })
                .ToListAsync();

            ViewData["IdProduktu"] = new SelectList(produkty, "IdProduktu", "Label", selectedId);
        }
    }
}
