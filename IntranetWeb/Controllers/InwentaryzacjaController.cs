using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Data.Data.Magazyn;

using IntranetWeb.Controllers.Abstrakcja;

namespace IntranetWeb.Controllers
{
    public class InwentaryzacjaController : BaseSearchController<Inwentaryzacja>
    {

        public InwentaryzacjaController(DataContext context) : base(context) { }

        // GET: Inwentaryzacja
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var query = _context.Inwentaryzacja.Include(i => i.Magazyn).Include(i => i.Utworzyl).AsNoTracking();
            query = ApplySearchAny(query, searchTerm, x => x.Numer, x => x.Status, x => x.Notatka);

            return View(await query.ToListAsync());
        }

        // GET: Inwentaryzacja/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inwentaryzacja = await _context.Inwentaryzacja
                .Include(i => i.Magazyn)
                .Include(i => i.Utworzyl)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (inwentaryzacja == null)
            {
                return NotFound();
            }

            return View(inwentaryzacja);
        }

        // GET: Inwentaryzacja/Create
        public IActionResult Create()
        {
            ViewData["IdMagazynu"] = new SelectList(_context.Magazyn, "IdMagazynu", "Nazwa");
            ViewData["IdUtworzyl"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email");
            return View();
        }

        // POST: Inwentaryzacja/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Numer,IdMagazynu,Status,StartUtc,KoniecUtc,IdUtworzyl,Notatka,RowVersion")] Inwentaryzacja inwentaryzacja)
        {
            if (ModelState.IsValid)
            {
                _context.Add(inwentaryzacja);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdMagazynu"] = new SelectList(_context.Magazyn, "IdMagazynu", "Nazwa", inwentaryzacja.IdMagazynu);
            ViewData["IdUtworzyl"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email", inwentaryzacja.IdUtworzyl);
            return View(inwentaryzacja);
        }

        // GET: Inwentaryzacja/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inwentaryzacja = await _context.Inwentaryzacja.FindAsync(id);
            if (inwentaryzacja == null)
            {
                return NotFound();
            }
            ViewData["IdMagazynu"] = new SelectList(_context.Magazyn, "IdMagazynu", "Nazwa", inwentaryzacja.IdMagazynu);
            ViewData["IdUtworzyl"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email", inwentaryzacja.IdUtworzyl);
            return View(inwentaryzacja);
        }

        // POST: Inwentaryzacja/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Numer,IdMagazynu,Status,StartUtc,KoniecUtc,IdUtworzyl,Notatka,RowVersion")] Inwentaryzacja inwentaryzacja)
        {
            if (id != inwentaryzacja.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(inwentaryzacja);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InwentaryzacjaExists(inwentaryzacja.Id))
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
            ViewData["IdMagazynu"] = new SelectList(_context.Magazyn, "IdMagazynu", "Nazwa", inwentaryzacja.IdMagazynu);
            ViewData["IdUtworzyl"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email", inwentaryzacja.IdUtworzyl);
            return View(inwentaryzacja);
        }

        // GET: Inwentaryzacja/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inwentaryzacja = await _context.Inwentaryzacja
                .Include(i => i.Magazyn)
                .Include(i => i.Utworzyl)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (inwentaryzacja == null)
            {
                return NotFound();
            }

            return View(inwentaryzacja);
        }

        // POST: Inwentaryzacja/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inwentaryzacja = await _context.Inwentaryzacja.FindAsync(id);
            if (inwentaryzacja != null)
            {
                _context.Inwentaryzacja.Remove(inwentaryzacja);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InwentaryzacjaExists(int id)
        {
            return _context.Inwentaryzacja.Any(e => e.Id == id);
        }
    }
}
