using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Data.Data.Magazyn;

using IntranetWeb.Controllers.Abstrakcja;

namespace IntranetWeb.Controllers
{
    public class PozycjaRezerwacjiController : BaseSearchController<PozycjaRezerwacji>
    {

        public PozycjaRezerwacjiController(DataContext context) : base(context) { }

        // GET: PozycjaRezerwacji
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var query = _context.PozycjaRezerwacji.Include(p => p.Lokacja).Include(p => p.Produkt).Include(p => p.Rezerwacja).AsNoTracking();
            query = ApplySearchAny(query, searchTerm);

            return View(await query.ToListAsync());
        }

        // GET: PozycjaRezerwacji/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pozycjaRezerwacji = await _context.PozycjaRezerwacji
                .Include(p => p.Lokacja)
                .Include(p => p.Produkt)
                .Include(p => p.Rezerwacja)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pozycjaRezerwacji == null)
            {
                return NotFound();
            }

            return View(pozycjaRezerwacji);
        }

        // GET: PozycjaRezerwacji/Create
        public IActionResult Create()
        {
            ViewData["IdLokacji"] = new SelectList(_context.Lokacja, "IdLokacji", "Kod");
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod");
            ViewData["IdRezerwacji"] = new SelectList(_context.Rezerwacja, "Id", "Numer");
            return View();
        }

        // POST: PozycjaRezerwacji/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdRezerwacji,Lp,IdProduktu,IdLokacji,Ilosc")] PozycjaRezerwacji pozycjaRezerwacji)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pozycjaRezerwacji);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdLokacji"] = new SelectList(_context.Lokacja, "IdLokacji", "Kod", pozycjaRezerwacji.IdLokacji);
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod", pozycjaRezerwacji.IdProduktu);
            ViewData["IdRezerwacji"] = new SelectList(_context.Rezerwacja, "Id", "Numer", pozycjaRezerwacji.IdRezerwacji);
            return View(pozycjaRezerwacji);
        }

        // GET: PozycjaRezerwacji/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pozycjaRezerwacji = await _context.PozycjaRezerwacji.FindAsync(id);
            if (pozycjaRezerwacji == null)
            {
                return NotFound();
            }
            ViewData["IdLokacji"] = new SelectList(_context.Lokacja, "IdLokacji", "Kod", pozycjaRezerwacji.IdLokacji);
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod", pozycjaRezerwacji.IdProduktu);
            ViewData["IdRezerwacji"] = new SelectList(_context.Rezerwacja, "Id", "Numer", pozycjaRezerwacji.IdRezerwacji);
            return View(pozycjaRezerwacji);
        }

        // POST: PozycjaRezerwacji/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdRezerwacji,Lp,IdProduktu,IdLokacji,Ilosc")] PozycjaRezerwacji pozycjaRezerwacji)
        {
            if (id != pozycjaRezerwacji.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pozycjaRezerwacji);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PozycjaRezerwacjiExists(pozycjaRezerwacji.Id))
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
            ViewData["IdLokacji"] = new SelectList(_context.Lokacja, "IdLokacji", "Kod", pozycjaRezerwacji.IdLokacji);
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod", pozycjaRezerwacji.IdProduktu);
            ViewData["IdRezerwacji"] = new SelectList(_context.Rezerwacja, "Id", "Numer", pozycjaRezerwacji.IdRezerwacji);
            return View(pozycjaRezerwacji);
        }

        // GET: PozycjaRezerwacji/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pozycjaRezerwacji = await _context.PozycjaRezerwacji
                .Include(p => p.Lokacja)
                .Include(p => p.Produkt)
                .Include(p => p.Rezerwacja)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pozycjaRezerwacji == null)
            {
                return NotFound();
            }

            return View(pozycjaRezerwacji);
        }

        // POST: PozycjaRezerwacji/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pozycjaRezerwacji = await _context.PozycjaRezerwacji.FindAsync(id);
            if (pozycjaRezerwacji != null)
            {
                _context.PozycjaRezerwacji.Remove(pozycjaRezerwacji);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PozycjaRezerwacjiExists(int id)
        {
            return _context.PozycjaRezerwacji.Any(e => e.Id == id);
        }
    }
}
