using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Data.Data.Magazyn;

using IntranetWeb.Controllers.Abstrakcja;

namespace IntranetWeb.Controllers
{
    public class AlertController : BaseSearchController<Alert>
    {

        public AlertController(DataContext context) : base(context) { }

        // GET: Alert
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var query = _context.Alert.Include(a => a.Magazyn).Include(a => a.Produkt).Include(a => a.Regula).AsNoTracking();
            query = ApplySearchAny(query, searchTerm, x => x.Waga, x => x.Tresc);

            return View(await query.ToListAsync());
        }

        // GET: Alert/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alert = await _context.Alert
                .Include(a => a.Magazyn)
                .Include(a => a.Produkt)
                .Include(a => a.Regula)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (alert == null)
            {
                return NotFound();
            }

            return View(alert);
        }

        // GET: Alert/Create
        public IActionResult Create()
        {
            ViewData["IdMagazynu"] = new SelectList(_context.Magazyn, "IdMagazynu", "Nazwa");
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod");
            ViewData["IdReguly"] = new SelectList(_context.RegulaAlertu, "Id", "Typ");
            return View();
        }

        // POST: Alert/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdReguly,IdMagazynu,IdProduktu,Waga,Tresc,UtworzonoUtc,CzyPotwierdzony,PotwierdzilUserId,PotwierdzonoUtc")] Alert alert)
        {
            if (ModelState.IsValid)
            {
                _context.Add(alert);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdMagazynu"] = new SelectList(_context.Magazyn, "IdMagazynu", "Nazwa", alert.IdMagazynu);
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod", alert.IdProduktu);
            ViewData["IdReguly"] = new SelectList(_context.RegulaAlertu, "Id", "Typ", alert.IdReguly);
            return View(alert);
        }

        // GET: Alert/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alert = await _context.Alert.FindAsync(id);
            if (alert == null)
            {
                return NotFound();
            }
            ViewData["IdMagazynu"] = new SelectList(_context.Magazyn, "IdMagazynu", "Nazwa", alert.IdMagazynu);
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod", alert.IdProduktu);
            ViewData["IdReguly"] = new SelectList(_context.RegulaAlertu, "Id", "Typ", alert.IdReguly);
            return View(alert);
        }

        // POST: Alert/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,IdReguly,IdMagazynu,IdProduktu,Waga,Tresc,UtworzonoUtc,CzyPotwierdzony,PotwierdzilUserId,PotwierdzonoUtc")] Alert alert)
        {
            if (id != alert.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(alert);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlertExists(alert.Id))
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
            ViewData["IdMagazynu"] = new SelectList(_context.Magazyn, "IdMagazynu", "Nazwa", alert.IdMagazynu);
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod", alert.IdProduktu);
            ViewData["IdReguly"] = new SelectList(_context.RegulaAlertu, "Id", "Typ", alert.IdReguly);
            return View(alert);
        }

        // GET: Alert/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alert = await _context.Alert
                .Include(a => a.Magazyn)
                .Include(a => a.Produkt)
                .Include(a => a.Regula)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (alert == null)
            {
                return NotFound();
            }

            return View(alert);
        }

        // POST: Alert/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var alert = await _context.Alert.FindAsync(id);
            if (alert != null)
            {
                _context.Alert.Remove(alert);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AlertExists(long id)
        {
            return _context.Alert.Any(e => e.Id == id);
        }
    }
}
