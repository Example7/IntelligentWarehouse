using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Data.Data.Magazyn;

using IntranetWeb.Controllers.Abstrakcja;

namespace IntranetWeb.Controllers
{
    public class ProduktJednostkaController : BaseSearchController<ProduktJednostka>
    {

        public ProduktJednostkaController(DataContext context) : base(context) { }

        // GET: ProduktJednostka
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var query = _context.ProduktJednostka.Include(p => p.Jednostka).Include(p => p.Produkt).AsNoTracking();
            query = ApplySearchAny(query, searchTerm);

            return View(await query.ToListAsync());
        }

        // GET: ProduktJednostka/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var produktJednostka = await _context.ProduktJednostka
                .Include(p => p.Jednostka)
                .Include(p => p.Produkt)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (produktJednostka == null)
            {
                return NotFound();
            }

            return View(produktJednostka);
        }

        // GET: ProduktJednostka/Create
        public IActionResult Create()
        {
            ViewData["IdJednostki"] = new SelectList(_context.JednostkaMiary, "IdJednostki", "Kod");
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod");
            return View();
        }

        // POST: ProduktJednostka/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdProduktu,IdJednostki,PrzelicznikDoDomyslnej")] ProduktJednostka produktJednostka)
        {
            if (ModelState.IsValid)
            {
                _context.Add(produktJednostka);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdJednostki"] = new SelectList(_context.JednostkaMiary, "IdJednostki", "Kod", produktJednostka.IdJednostki);
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod", produktJednostka.IdProduktu);
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
            ViewData["IdJednostki"] = new SelectList(_context.JednostkaMiary, "IdJednostki", "Kod", produktJednostka.IdJednostki);
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod", produktJednostka.IdProduktu);
            return View(produktJednostka);
        }

        // POST: ProduktJednostka/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdProduktu,IdJednostki,PrzelicznikDoDomyslnej")] ProduktJednostka produktJednostka)
        {
            if (id != produktJednostka.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(produktJednostka);
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
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdJednostki"] = new SelectList(_context.JednostkaMiary, "IdJednostki", "Kod", produktJednostka.IdJednostki);
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod", produktJednostka.IdProduktu);
            return View(produktJednostka);
        }

        // GET: ProduktJednostka/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var produktJednostka = await _context.ProduktJednostka
                .Include(p => p.Jednostka)
                .Include(p => p.Produkt)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (produktJednostka == null)
            {
                return NotFound();
            }

            return View(produktJednostka);
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
    }
}
