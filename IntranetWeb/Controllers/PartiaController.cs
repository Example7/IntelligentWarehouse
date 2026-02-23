using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Data.Data.Magazyn;

using IntranetWeb.Controllers.Abstrakcja;

namespace IntranetWeb.Controllers
{
    public class PartiaController : BaseSearchController<Partia>
    {

        public PartiaController(DataContext context) : base(context) { }

        // GET: Partia
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var query = _context.Partia.Include(p => p.Dostawca).Include(p => p.Produkt).AsNoTracking();
            query = ApplySearchAny(query, searchTerm, x => x.NumerPartii);

            return View(await query.ToListAsync());
        }

        // GET: Partia/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var partia = await _context.Partia
                .Include(p => p.Dostawca)
                .Include(p => p.Produkt)
                .FirstOrDefaultAsync(m => m.IdPartii == id);
            if (partia == null)
            {
                return NotFound();
            }

            return View(partia);
        }

        // GET: Partia/Create
        public IActionResult Create()
        {
            ViewData["IdDostawcy"] = new SelectList(_context.Dostawca, "IdDostawcy", "Nazwa");
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod");
            return View();
        }

        // POST: Partia/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdPartii,IdProduktu,NumerPartii,DataProdukcji,DataWaznosci,IdDostawcy")] Partia partia)
        {
            if (ModelState.IsValid)
            {
                _context.Add(partia);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdDostawcy"] = new SelectList(_context.Dostawca, "IdDostawcy", "Nazwa", partia.IdDostawcy);
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod", partia.IdProduktu);
            return View(partia);
        }

        // GET: Partia/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var partia = await _context.Partia.FindAsync(id);
            if (partia == null)
            {
                return NotFound();
            }
            ViewData["IdDostawcy"] = new SelectList(_context.Dostawca, "IdDostawcy", "Nazwa", partia.IdDostawcy);
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod", partia.IdProduktu);
            return View(partia);
        }

        // POST: Partia/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdPartii,IdProduktu,NumerPartii,DataProdukcji,DataWaznosci,IdDostawcy")] Partia partia)
        {
            if (id != partia.IdPartii)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(partia);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PartiaExists(partia.IdPartii))
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
            ViewData["IdDostawcy"] = new SelectList(_context.Dostawca, "IdDostawcy", "Nazwa", partia.IdDostawcy);
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod", partia.IdProduktu);
            return View(partia);
        }

        // GET: Partia/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var partia = await _context.Partia
                .Include(p => p.Dostawca)
                .Include(p => p.Produkt)
                .FirstOrDefaultAsync(m => m.IdPartii == id);
            if (partia == null)
            {
                return NotFound();
            }

            return View(partia);
        }

        // POST: Partia/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var partia = await _context.Partia.FindAsync(id);
            if (partia != null)
            {
                _context.Partia.Remove(partia);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PartiaExists(int id)
        {
            return _context.Partia.Any(e => e.IdPartii == id);
        }
    }
}
