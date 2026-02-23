using Data.Data;
using Data.Data.Magazyn;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using IntranetWeb.Controllers.Abstrakcja;

namespace IntranetWeb.Controllers
{
    public class StanMagazynowyController : BaseSearchController<StanMagazynowy>
    {

        public StanMagazynowyController(DataContext context) : base(context) { }

        // GET: StanMagazynowy
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var query = _context.StanMagazynowy.Include(s => s.Lokacja).Include(s => s.Produkt).AsNoTracking();
            query = ApplySearchAny(query, searchTerm);

            return View(await query.ToListAsync());
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
                .Include(s => s.Produkt)
                .FirstOrDefaultAsync(m => m.IdStanu == id);
            if (stanMagazynowy == null)
            {
                return NotFound();
            }

            return View(stanMagazynowy);
        }

        // GET: StanMagazynowy/Create
        public IActionResult Create()
        {
            ViewData["IdLokacji"] = new SelectList(_context.Lokacja, "IdLokacji", "Kod");
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod");
            return View();
        }

        // POST: StanMagazynowy/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdStanu,IdProduktu,IdLokacji,Ilosc,RowVersion")] StanMagazynowy stanMagazynowy)
        {
            if (ModelState.IsValid)
            {
                _context.Add(stanMagazynowy);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdLokacji"] = new SelectList(_context.Lokacja, "IdLokacji", "Kod", stanMagazynowy.IdLokacji);
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod", stanMagazynowy.IdProduktu);
            return View(stanMagazynowy);
        }

        // GET: StanMagazynowy/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stanMagazynowy = await _context.StanMagazynowy.FindAsync(id);
            if (stanMagazynowy == null)
            {
                return NotFound();
            }
            ViewData["IdLokacji"] = new SelectList(_context.Lokacja, "IdLokacji", "Kod", stanMagazynowy.IdLokacji);
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod", stanMagazynowy.IdProduktu);
            return View(stanMagazynowy);
        }

        // POST: StanMagazynowy/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdStanu,IdProduktu,IdLokacji,Ilosc,RowVersion")] StanMagazynowy stanMagazynowy)
        {
            if (id != stanMagazynowy.IdStanu)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(stanMagazynowy);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StanMagazynowyExists(stanMagazynowy.IdStanu))
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
            ViewData["IdLokacji"] = new SelectList(_context.Lokacja, "IdLokacji", "Kod", stanMagazynowy.IdLokacji);
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod", stanMagazynowy.IdProduktu);
            return View(stanMagazynowy);
        }

        // GET: StanMagazynowy/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stanMagazynowy = await _context.StanMagazynowy
                .Include(s => s.Lokacja)
                .Include(s => s.Produkt)
                .FirstOrDefaultAsync(m => m.IdStanu == id);
            if (stanMagazynowy == null)
            {
                return NotFound();
            }

            return View(stanMagazynowy);
        }

        // POST: StanMagazynowy/Delete/5
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
    }
}
