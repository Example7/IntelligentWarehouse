using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Data.Data.Magazyn;

using IntranetWeb.Controllers.Abstrakcja;

namespace IntranetWeb.Controllers
{
    public class DostawcaController : BaseSearchController<Dostawca>
    {

        public DostawcaController(DataContext context) : base(context) { }

        // GET: Dostawca
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var query = _context.Dostawca.AsNoTracking();
            query = ApplySearchAny(query, searchTerm, x => x.Nazwa, x => x.NIP, x => x.Email, x => x.Telefon, x => x.Adres);

            return View(await query.ToListAsync());
        }

        // GET: Dostawca/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dostawca = await _context.Dostawca
                .FirstOrDefaultAsync(m => m.IdDostawcy == id);
            if (dostawca == null)
            {
                return NotFound();
            }

            return View(dostawca);
        }

        // GET: Dostawca/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Dostawca/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdDostawcy,Nazwa,NIP,Email,Telefon,Adres,CzyAktywny,UtworzonoUtc,RowVersion")] Dostawca dostawca)
        {
            if (ModelState.IsValid)
            {
                _context.Add(dostawca);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(dostawca);
        }

        // GET: Dostawca/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dostawca = await _context.Dostawca.FindAsync(id);
            if (dostawca == null)
            {
                return NotFound();
            }
            return View(dostawca);
        }

        // POST: Dostawca/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdDostawcy,Nazwa,NIP,Email,Telefon,Adres,CzyAktywny,UtworzonoUtc,RowVersion")] Dostawca dostawca)
        {
            if (id != dostawca.IdDostawcy)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dostawca);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DostawcaExists(dostawca.IdDostawcy))
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
            return View(dostawca);
        }

        // GET: Dostawca/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dostawca = await _context.Dostawca
                .FirstOrDefaultAsync(m => m.IdDostawcy == id);
            if (dostawca == null)
            {
                return NotFound();
            }

            return View(dostawca);
        }

        // POST: Dostawca/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dostawca = await _context.Dostawca.FindAsync(id);
            if (dostawca != null)
            {
                _context.Dostawca.Remove(dostawca);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DostawcaExists(int id)
        {
            return _context.Dostawca.Any(e => e.IdDostawcy == id);
        }
    }
}
