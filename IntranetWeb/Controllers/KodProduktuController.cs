using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Data.Data.Magazyn;

using IntranetWeb.Controllers.Abstrakcja;

namespace IntranetWeb.Controllers
{
    public class KodProduktuController : BaseSearchController<KodProduktu>
    {

        public KodProduktuController(DataContext context) : base(context) { }

        // GET: KodProduktu
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var query = _context.KodProduktu.Include(k => k.Produkt).AsNoTracking();
            query = ApplySearchAny(query, searchTerm, x => x.Wartosc, x => x.Typ);

            return View(await query.ToListAsync());
        }

        // GET: KodProduktu/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kodProduktu = await _context.KodProduktu
                .Include(k => k.Produkt)
                .FirstOrDefaultAsync(m => m.IdKodu == id);
            if (kodProduktu == null)
            {
                return NotFound();
            }

            return View(kodProduktu);
        }

        // GET: KodProduktu/Create
        public IActionResult Create()
        {
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod");
            return View();
        }

        // POST: KodProduktu/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdKodu,IdProduktu,Wartosc,Typ,CzyGlowny")] KodProduktu kodProduktu)
        {
            if (ModelState.IsValid)
            {
                _context.Add(kodProduktu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod", kodProduktu.IdProduktu);
            return View(kodProduktu);
        }

        // GET: KodProduktu/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kodProduktu = await _context.KodProduktu.FindAsync(id);
            if (kodProduktu == null)
            {
                return NotFound();
            }
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod", kodProduktu.IdProduktu);
            return View(kodProduktu);
        }

        // POST: KodProduktu/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdKodu,IdProduktu,Wartosc,Typ,CzyGlowny")] KodProduktu kodProduktu)
        {
            if (id != kodProduktu.IdKodu)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(kodProduktu);
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
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod", kodProduktu.IdProduktu);
            return View(kodProduktu);
        }

        // GET: KodProduktu/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kodProduktu = await _context.KodProduktu
                .Include(k => k.Produkt)
                .FirstOrDefaultAsync(m => m.IdKodu == id);
            if (kodProduktu == null)
            {
                return NotFound();
            }

            return View(kodProduktu);
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
    }
}
