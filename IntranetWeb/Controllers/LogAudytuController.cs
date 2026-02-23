using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Data.Data.Magazyn;

using IntranetWeb.Controllers.Abstrakcja;

namespace IntranetWeb.Controllers
{
    public class LogAudytuController : BaseSearchController<LogAudytu>
    {

        public LogAudytuController(DataContext context) : base(context) { }

        // GET: LogAudytu
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var query = _context.LogAudytu.Include(l => l.Uzytkownik).AsNoTracking();
            query = ApplySearchAny(query, searchTerm, x => x.Akcja, x => x.Encja, x => x.IdEncji);

            return View(await query.ToListAsync());
        }

        // GET: LogAudytu/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var logAudytu = await _context.LogAudytu
                .Include(l => l.Uzytkownik)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (logAudytu == null)
            {
                return NotFound();
            }

            return View(logAudytu);
        }

        // GET: LogAudytu/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email");
            return View();
        }

        // POST: LogAudytu/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,Akcja,Encja,IdEncji,KiedyUtc,StareJson,NoweJson")] LogAudytu logAudytu)
        {
            if (ModelState.IsValid)
            {
                _context.Add(logAudytu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email", logAudytu.UserId);
            return View(logAudytu);
        }

        // GET: LogAudytu/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var logAudytu = await _context.LogAudytu.FindAsync(id);
            if (logAudytu == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email", logAudytu.UserId);
            return View(logAudytu);
        }

        // POST: LogAudytu/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,UserId,Akcja,Encja,IdEncji,KiedyUtc,StareJson,NoweJson")] LogAudytu logAudytu)
        {
            if (id != logAudytu.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(logAudytu);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LogAudytuExists(logAudytu.Id))
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
            ViewData["UserId"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email", logAudytu.UserId);
            return View(logAudytu);
        }

        // GET: LogAudytu/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var logAudytu = await _context.LogAudytu
                .Include(l => l.Uzytkownik)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (logAudytu == null)
            {
                return NotFound();
            }

            return View(logAudytu);
        }

        // POST: LogAudytu/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var logAudytu = await _context.LogAudytu.FindAsync(id);
            if (logAudytu != null)
            {
                _context.LogAudytu.Remove(logAudytu);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LogAudytuExists(long id)
        {
            return _context.LogAudytu.Any(e => e.Id == id);
        }
    }
}
