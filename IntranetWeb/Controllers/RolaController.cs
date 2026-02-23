using Data.Data;
using Data.Data.Magazyn;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IntranetWeb.Controllers
{
    public class RolaController : Controller
    {
        private readonly DataContext _context;

        public RolaController(DataContext context)
        {
            _context = context;
        }

        // GET: Rola
        public async Task<IActionResult> Index()
        {
            return View(await _context.Rola.ToListAsync());
        }

        // GET: Rola/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rola = await _context.Rola
                .FirstOrDefaultAsync(m => m.IdRoli == id);
            if (rola == null)
            {
                return NotFound();
            }

            return View(rola);
        }

        // GET: Rola/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Rola/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdRoli,Nazwa")] Rola rola)
        {
            if (ModelState.IsValid)
            {
                _context.Add(rola);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(rola);
        }

        // GET: Rola/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rola = await _context.Rola.FindAsync(id);
            if (rola == null)
            {
                return NotFound();
            }
            return View(rola);
        }

        // POST: Rola/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdRoli,Nazwa")] Rola rola)
        {
            if (id != rola.IdRoli)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rola);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RolaExists(rola.IdRoli))
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
            return View(rola);
        }

        // GET: Rola/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rola = await _context.Rola
                .FirstOrDefaultAsync(m => m.IdRoli == id);
            if (rola == null)
            {
                return NotFound();
            }

            return View(rola);
        }

        // POST: Rola/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rola = await _context.Rola.FindAsync(id);
            if (rola != null)
            {
                _context.Rola.Remove(rola);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RolaExists(int id)
        {
            return _context.Rola.Any(e => e.IdRoli == id);
        }
    }
}
