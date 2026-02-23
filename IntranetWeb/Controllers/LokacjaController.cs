using Data.Data;
using Data.Data.Magazyn;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using IntranetWeb.Controllers.Abstrakcja;

namespace IntranetWeb.Controllers
{
    public class LokacjaController : BaseSearchController<Lokacja>
    {

        public LokacjaController(DataContext context) : base(context) { }

        // GET: Lokacja
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var query = _context.Lokacja.Include(l => l.Magazyn).AsNoTracking();
            query = ApplySearchAny(query, searchTerm, x => x.Kod, x => x.Opis);

            return View(await query.ToListAsync());
        }

        // GET: Lokacja/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lokacja = await _context.Lokacja
                .Include(l => l.Magazyn)
                .FirstOrDefaultAsync(m => m.IdLokacji == id);
            if (lokacja == null)
            {
                return NotFound();
            }

            return View(lokacja);
        }

        // GET: Lokacja/Create
        public IActionResult Create()
        {
            ViewData["IdMagazynu"] = new SelectList(_context.Set<Magazyn>(), "IdMagazynu", "Nazwa");
            return View();
        }

        // POST: Lokacja/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdLokacji,IdMagazynu,Kod,Opis,CzyAktywna")] Lokacja lokacja)
        {
            if (ModelState.IsValid)
            {
                _context.Add(lokacja);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdMagazynu"] = new SelectList(_context.Set<Magazyn>(), "IdMagazynu", "Nazwa", lokacja.IdMagazynu);
            return View(lokacja);
        }

        // GET: Lokacja/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lokacja = await _context.Lokacja.FindAsync(id);
            if (lokacja == null)
            {
                return NotFound();
            }
            ViewData["IdMagazynu"] = new SelectList(_context.Set<Magazyn>(), "IdMagazynu", "Nazwa", lokacja.IdMagazynu);
            return View(lokacja);
        }

        // POST: Lokacja/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdLokacji,IdMagazynu,Kod,Opis,CzyAktywna")] Lokacja lokacja)
        {
            if (id != lokacja.IdLokacji)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(lokacja);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LokacjaExists(lokacja.IdLokacji))
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
            ViewData["IdMagazynu"] = new SelectList(_context.Set<Magazyn>(), "IdMagazynu", "Nazwa", lokacja.IdMagazynu);
            return View(lokacja);
        }

        // GET: Lokacja/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lokacja = await _context.Lokacja
                .Include(l => l.Magazyn)
                .FirstOrDefaultAsync(m => m.IdLokacji == id);
            if (lokacja == null)
            {
                return NotFound();
            }

            return View(lokacja);
        }

        // POST: Lokacja/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lokacja = await _context.Lokacja.FindAsync(id);
            if (lokacja != null)
            {
                _context.Lokacja.Remove(lokacja);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LokacjaExists(int id)
        {
            return _context.Lokacja.Any(e => e.IdLokacji == id);
        }
    }
}
