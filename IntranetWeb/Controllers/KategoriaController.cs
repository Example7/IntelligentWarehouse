using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Data.Data.Magazyn;

using IntranetWeb.Controllers.Abstrakcja;

namespace IntranetWeb.Controllers
{
    public class KategoriaController : BaseSearchController<Kategoria>
    {

        public KategoriaController(DataContext context) : base(context) { }

        // GET: Kategoria
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var query = _context.Kategoria.Include(k => k.NadrzednaKategoria).AsNoTracking();
            query = ApplySearchAny(query, searchTerm, x => x.Nazwa, x => x.Sciezka);

            return View(await query.ToListAsync());
        }

        // GET: Kategoria/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kategoria = await _context.Kategoria
                .Include(k => k.NadrzednaKategoria)
                .FirstOrDefaultAsync(m => m.IdKategorii == id);
            if (kategoria == null)
            {
                return NotFound();
            }

            return View(kategoria);
        }

        // GET: Kategoria/Create
        public IActionResult Create()
        {
            ViewData["IdNadrzednejKategorii"] = new SelectList(_context.Kategoria, "IdKategorii", "Nazwa");
            return View();
        }

        // POST: Kategoria/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdKategorii,IdNadrzednejKategorii,Nazwa,Sciezka")] Kategoria kategoria)
        {
            if (ModelState.IsValid)
            {
                _context.Add(kategoria);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdNadrzednejKategorii"] = new SelectList(_context.Kategoria, "IdKategorii", "Nazwa", kategoria.IdNadrzednejKategorii);
            return View(kategoria);
        }

        // GET: Kategoria/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kategoria = await _context.Kategoria.FindAsync(id);
            if (kategoria == null)
            {
                return NotFound();
            }
            ViewData["IdNadrzednejKategorii"] = new SelectList(_context.Kategoria, "IdKategorii", "Nazwa", kategoria.IdNadrzednejKategorii);
            return View(kategoria);
        }

        // POST: Kategoria/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdKategorii,IdNadrzednejKategorii,Nazwa,Sciezka")] Kategoria kategoria)
        {
            if (id != kategoria.IdKategorii)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(kategoria);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KategoriaExists(kategoria.IdKategorii))
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
            ViewData["IdNadrzednejKategorii"] = new SelectList(_context.Kategoria, "IdKategorii", "Nazwa", kategoria.IdNadrzednejKategorii);
            return View(kategoria);
        }

        // GET: Kategoria/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kategoria = await _context.Kategoria
                .Include(k => k.NadrzednaKategoria)
                .FirstOrDefaultAsync(m => m.IdKategorii == id);
            if (kategoria == null)
            {
                return NotFound();
            }

            return View(kategoria);
        }

        // POST: Kategoria/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var kategoria = await _context.Kategoria.FindAsync(id);
            if (kategoria != null)
            {
                _context.Kategoria.Remove(kategoria);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool KategoriaExists(int id)
        {
            return _context.Kategoria.Any(e => e.IdKategorii == id);
        }
    }
}
