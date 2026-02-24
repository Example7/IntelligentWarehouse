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
    public class ProduktJednostkaController : BaseSearchController<ProduktJednostka>
    {
        private readonly IProduktJednostkaService _produktJednostkaService;

        public ProduktJednostkaController(DataContext context, IProduktJednostkaService produktJednostkaService) : base(context)
        {
            _produktJednostkaService = produktJednostkaService;
        }

        // GET: ProduktJednostka
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var model = await _produktJednostkaService.GetIndexDataAsync(searchTerm);
            return View(model);
        }

        // GET: ProduktJednostka/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _produktJednostkaService.GetDetailsDataAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // GET: ProduktJednostka/Create
        public async Task<IActionResult> Create()
        {
            await PopulateSelectsAsync(null, null);
            return View(new ProduktJednostka { PrzelicznikDoDomyslnej = 1m });
        }

        // POST: ProduktJednostka/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdProduktu,IdJednostki,PrzelicznikDoDomyslnej")] ProduktJednostka produktJednostka)
        {
            await PopulateSelectsAsync(produktJednostka.IdProduktu, produktJednostka.IdJednostki);
            Normalize(produktJednostka);
            await ValidateProduktJednostkaAsync(produktJednostka);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(produktJednostka);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    if (await ExistsPairAsync(produktJednostka.IdProduktu, produktJednostka.IdJednostki, null))
                    {
                        ModelState.AddModelError(nameof(ProduktJednostka.IdJednostki), "To powiązanie produktu z jednostką miary już istnieje.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Nie udało się zapisać jednostki produktu.");
                    }
                }
            }
            return View(produktJednostka);
        }

        // GET: ProduktJednostka/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var produktJednostka = await _context.ProduktJednostka.FindAsync(id);
            if (produktJednostka == null)
            {
                return NotFound();
            }
            await PopulateSelectsAsync(produktJednostka.IdProduktu, produktJednostka.IdJednostki);
            return View(produktJednostka);
        }

        // POST: ProduktJednostka/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdProduktu,IdJednostki,PrzelicznikDoDomyslnej")] ProduktJednostka produktJednostka)
        {
            await PopulateSelectsAsync(produktJednostka.IdProduktu, produktJednostka.IdJednostki);
            if (id != produktJednostka.Id)
            {
                return NotFound();
            }

            Normalize(produktJednostka);
            await ValidateProduktJednostkaAsync(produktJednostka, produktJednostka.Id);

            if (ModelState.IsValid)
            {
                var existing = await _context.ProduktJednostka.FirstOrDefaultAsync(x => x.Id == id);
                if (existing == null)
                {
                    return NotFound();
                }

                try
                {
                    existing.IdProduktu = produktJednostka.IdProduktu;
                    existing.IdJednostki = produktJednostka.IdJednostki;
                    existing.PrzelicznikDoDomyslnej = produktJednostka.PrzelicznikDoDomyslnej;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProduktJednostkaExists(produktJednostka.Id))
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
                    if (await ExistsPairAsync(produktJednostka.IdProduktu, produktJednostka.IdJednostki, produktJednostka.Id))
                    {
                        ModelState.AddModelError(nameof(ProduktJednostka.IdJednostki), "To powiązanie produktu z jednostką miary już istnieje.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Nie udało się zapisać zmian jednostki produktu.");
                    }

                    return View(produktJednostka);
                }

                return RedirectToAction(nameof(Index));
            }
            return View(produktJednostka);
        }

        // GET: ProduktJednostka/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _produktJednostkaService.GetDeleteDataAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // POST: ProduktJednostka/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var produktJednostka = await _context.ProduktJednostka.FindAsync(id);
            if (produktJednostka != null)
            {
                _context.ProduktJednostka.Remove(produktJednostka);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProduktJednostkaExists(int id)
        {
            return _context.ProduktJednostka.Any(e => e.Id == id);
        }

        private static void Normalize(ProduktJednostka produktJednostka)
        {
            if (produktJednostka.PrzelicznikDoDomyslnej < 0)
            {
                produktJednostka.PrzelicznikDoDomyslnej = Math.Abs(produktJednostka.PrzelicznikDoDomyslnej);
            }
        }

        private async Task ValidateProduktJednostkaAsync(ProduktJednostka produktJednostka, int? excludeId = null)
        {
            if (produktJednostka.PrzelicznikDoDomyslnej <= 0m)
            {
                ModelState.AddModelError(nameof(ProduktJednostka.PrzelicznikDoDomyslnej), "Przelicznik musi być większy od 0.");
            }

            if (produktJednostka.IdProduktu > 0 && produktJednostka.IdJednostki > 0 &&
                await ExistsPairAsync(produktJednostka.IdProduktu, produktJednostka.IdJednostki, excludeId))
            {
                ModelState.AddModelError(nameof(ProduktJednostka.IdJednostki), "To powiązanie produktu z jednostką miary już istnieje.");
            }
        }

        private Task<bool> ExistsPairAsync(int idProduktu, int idJednostki, int? excludeId)
        {
            var query = _context.ProduktJednostka.AsNoTracking()
                .Where(x => x.IdProduktu == idProduktu && x.IdJednostki == idJednostki);

            if (excludeId.HasValue)
            {
                query = query.Where(x => x.Id != excludeId.Value);
            }

            return query.AnyAsync();
        }

        private async Task PopulateSelectsAsync(int? selectedProduktId, int? selectedJednostkaId)
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

            var jednostki = await _context.JednostkaMiary
                .AsNoTracking()
                .OrderBy(j => j.Kod)
                .ThenBy(j => j.Nazwa)
                .Select(j => new
                {
                    j.IdJednostki,
                    Label = j.Kod + " - " + j.Nazwa
                })
                .ToListAsync();

            ViewData["IdProduktu"] = new SelectList(produkty, "IdProduktu", "Label", selectedProduktId);
            ViewData["IdJednostki"] = new SelectList(jednostki, "IdJednostki", "Label", selectedJednostkaId);
        }
    }
}