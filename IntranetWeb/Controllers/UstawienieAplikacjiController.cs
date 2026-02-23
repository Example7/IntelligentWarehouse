using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Data.Data.Magazyn;

using IntranetWeb.Controllers.Abstrakcja;

namespace IntranetWeb.Controllers
{
    public class UstawienieAplikacjiController : BaseSearchController<UstawienieAplikacji>
    {

        public UstawienieAplikacjiController(DataContext context) : base(context) { }

        // GET: UstawienieAplikacji
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var query = _context.UstawienieAplikacji.AsNoTracking();
            query = ApplySearchAny(query, searchTerm, x => x.Klucz, x => x.Wartosc, x => x.Opis);

            return View(await query.ToListAsync());
        }

        // GET: UstawienieAplikacji/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ustawienieAplikacji = await _context.UstawienieAplikacji
                .FirstOrDefaultAsync(m => m.Klucz == id);
            if (ustawienieAplikacji == null)
            {
                return NotFound();
            }

            return View(ustawienieAplikacji);
        }

        // GET: UstawienieAplikacji/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: UstawienieAplikacji/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Klucz,Wartosc,Opis,ZmienionoUtc,ZmienilUserId")] UstawienieAplikacji ustawienieAplikacji)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ustawienieAplikacji);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(ustawienieAplikacji);
        }

        // GET: UstawienieAplikacji/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ustawienieAplikacji = await _context.UstawienieAplikacji.FindAsync(id);
            if (ustawienieAplikacji == null)
            {
                return NotFound();
            }
            return View(ustawienieAplikacji);
        }

        // POST: UstawienieAplikacji/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Klucz,Wartosc,Opis,ZmienionoUtc,ZmienilUserId")] UstawienieAplikacji ustawienieAplikacji)
        {
            if (id != ustawienieAplikacji.Klucz)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ustawienieAplikacji);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UstawienieAplikacjiExists(ustawienieAplikacji.Klucz))
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
            return View(ustawienieAplikacji);
        }

        // GET: UstawienieAplikacji/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ustawienieAplikacji = await _context.UstawienieAplikacji
                .FirstOrDefaultAsync(m => m.Klucz == id);
            if (ustawienieAplikacji == null)
            {
                return NotFound();
            }

            return View(ustawienieAplikacji);
        }

        // POST: UstawienieAplikacji/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var ustawienieAplikacji = await _context.UstawienieAplikacji.FindAsync(id);
            if (ustawienieAplikacji != null)
            {
                _context.UstawienieAplikacji.Remove(ustawienieAplikacji);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UstawienieAplikacjiExists(string id)
        {
            return _context.UstawienieAplikacji.Any(e => e.Klucz == id);
        }
    }
}
