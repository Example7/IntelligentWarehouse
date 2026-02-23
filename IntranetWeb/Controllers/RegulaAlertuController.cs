using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Data.Data.Magazyn;

using IntranetWeb.Controllers.Abstrakcja;

namespace IntranetWeb.Controllers
{
    public class RegulaAlertuController : BaseSearchController<RegulaAlertu>
    {

        public RegulaAlertuController(DataContext context) : base(context) { }

        // GET: RegulaAlertu
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var query = _context.RegulaAlertu.Include(r => r.Magazyn).Include(r => r.Produkt).AsNoTracking();
            query = ApplySearchAny(query, searchTerm, x => x.Typ);

            return View(await query.ToListAsync());
        }

        // GET: RegulaAlertu/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var regulaAlertu = await _context.RegulaAlertu
                .Include(r => r.Magazyn)
                .Include(r => r.Produkt)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (regulaAlertu == null)
            {
                return NotFound();
            }

            return View(regulaAlertu);
        }

        // GET: RegulaAlertu/Create
        public IActionResult Create()
        {
            ViewData["IdMagazynu"] = new SelectList(_context.Magazyn, "IdMagazynu", "Nazwa");
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod");
            return View();
        }

        // POST: RegulaAlertu/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdMagazynu,IdProduktu,Typ,Prog,CzyWlaczona,UtworzonoUtc")] RegulaAlertu regulaAlertu)
        {
            if (ModelState.IsValid)
            {
                _context.Add(regulaAlertu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdMagazynu"] = new SelectList(_context.Magazyn, "IdMagazynu", "Nazwa", regulaAlertu.IdMagazynu);
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod", regulaAlertu.IdProduktu);
            return View(regulaAlertu);
        }

        // GET: RegulaAlertu/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var regulaAlertu = await _context.RegulaAlertu.FindAsync(id);
            if (regulaAlertu == null)
            {
                return NotFound();
            }
            ViewData["IdMagazynu"] = new SelectList(_context.Magazyn, "IdMagazynu", "Nazwa", regulaAlertu.IdMagazynu);
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod", regulaAlertu.IdProduktu);
            return View(regulaAlertu);
        }

        // POST: RegulaAlertu/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdMagazynu,IdProduktu,Typ,Prog,CzyWlaczona,UtworzonoUtc")] RegulaAlertu regulaAlertu)
        {
            if (id != regulaAlertu.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(regulaAlertu);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RegulaAlertuExists(regulaAlertu.Id))
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
            ViewData["IdMagazynu"] = new SelectList(_context.Magazyn, "IdMagazynu", "Nazwa", regulaAlertu.IdMagazynu);
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod", regulaAlertu.IdProduktu);
            return View(regulaAlertu);
        }

        // GET: RegulaAlertu/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var regulaAlertu = await _context.RegulaAlertu
                .Include(r => r.Magazyn)
                .Include(r => r.Produkt)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (regulaAlertu == null)
            {
                return NotFound();
            }

            return View(regulaAlertu);
        }

        // POST: RegulaAlertu/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var regulaAlertu = await _context.RegulaAlertu.FindAsync(id);
            if (regulaAlertu != null)
            {
                _context.RegulaAlertu.Remove(regulaAlertu);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RegulaAlertuExists(int id)
        {
            return _context.RegulaAlertu.Any(e => e.Id == id);
        }
    }
}
