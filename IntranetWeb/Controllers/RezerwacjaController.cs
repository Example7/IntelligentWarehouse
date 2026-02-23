using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Data.Data.Magazyn;

using IntranetWeb.Controllers.Abstrakcja;

namespace IntranetWeb.Controllers
{
    public class RezerwacjaController : BaseSearchController<Rezerwacja>
    {

        public RezerwacjaController(DataContext context) : base(context) { }

        // GET: Rezerwacja
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var query = _context.Rezerwacja.Include(r => r.Magazyn).Include(r => r.Utworzyl).AsNoTracking();
            query = ApplySearchAny(query, searchTerm, x => x.Numer, x => x.Status, x => x.Notatka);

            return View(await query.ToListAsync());
        }

        // GET: Rezerwacja/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rezerwacja = await _context.Rezerwacja
                .Include(r => r.Magazyn)
                .Include(r => r.Utworzyl)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rezerwacja == null)
            {
                return NotFound();
            }

            return View(rezerwacja);
        }

        // GET: Rezerwacja/Create
        public IActionResult Create()
        {
            ViewData["IdMagazynu"] = new SelectList(_context.Magazyn, "IdMagazynu", "Nazwa");
            ViewData["IdUtworzyl"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email");
            return View();
        }

        // POST: Rezerwacja/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Numer,IdMagazynu,Status,UtworzonoUtc,WygasaUtc,IdUtworzyl,Notatka,RowVersion")] Rezerwacja rezerwacja)
        {
            if (ModelState.IsValid)
            {
                _context.Add(rezerwacja);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdMagazynu"] = new SelectList(_context.Magazyn, "IdMagazynu", "Nazwa", rezerwacja.IdMagazynu);
            ViewData["IdUtworzyl"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email", rezerwacja.IdUtworzyl);
            return View(rezerwacja);
        }

        // GET: Rezerwacja/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rezerwacja = await _context.Rezerwacja.FindAsync(id);
            if (rezerwacja == null)
            {
                return NotFound();
            }
            ViewData["IdMagazynu"] = new SelectList(_context.Magazyn, "IdMagazynu", "Nazwa", rezerwacja.IdMagazynu);
            ViewData["IdUtworzyl"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email", rezerwacja.IdUtworzyl);
            return View(rezerwacja);
        }

        // POST: Rezerwacja/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Numer,IdMagazynu,Status,UtworzonoUtc,WygasaUtc,IdUtworzyl,Notatka,RowVersion")] Rezerwacja rezerwacja)
        {
            if (id != rezerwacja.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rezerwacja);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RezerwacjaExists(rezerwacja.Id))
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
            ViewData["IdMagazynu"] = new SelectList(_context.Magazyn, "IdMagazynu", "Nazwa", rezerwacja.IdMagazynu);
            ViewData["IdUtworzyl"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email", rezerwacja.IdUtworzyl);
            return View(rezerwacja);
        }

        // GET: Rezerwacja/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rezerwacja = await _context.Rezerwacja
                .Include(r => r.Magazyn)
                .Include(r => r.Utworzyl)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rezerwacja == null)
            {
                return NotFound();
            }

            return View(rezerwacja);
        }

        // POST: Rezerwacja/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rezerwacja = await _context.Rezerwacja.FindAsync(id);
            if (rezerwacja != null)
            {
                _context.Rezerwacja.Remove(rezerwacja);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RezerwacjaExists(int id)
        {
            return _context.Rezerwacja.Any(e => e.Id == id);
        }
    }
}
