using Data.Data;
using Data.Data.Magazyn;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IntranetWeb.Controllers
{
    public class MagazynController : Controller
    {
        private readonly DataContext _context;

        public MagazynController(DataContext context)
        {
            _context = context;
        }

        // GET: Magazyn
        public async Task<IActionResult> Index()
        {
            return View(await _context.Magazyn.ToListAsync());
        }

        // GET: Magazyn/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var magazyn = await _context.Magazyn
                .FirstOrDefaultAsync(m => m.IdMagazynu == id);
            if (magazyn == null)
            {
                return NotFound();
            }

            return View(magazyn);
        }

        // GET: Magazyn/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Magazyn/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdMagazynu,Nazwa,Adres,CzyAktywny")] Magazyn magazyn)
        {
            if (ModelState.IsValid)
            {
                _context.Add(magazyn);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(magazyn);
        }

        // GET: Magazyn/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var magazyn = await _context.Magazyn.FindAsync(id);
            if (magazyn == null)
            {
                return NotFound();
            }
            return View(magazyn);
        }

        // POST: Magazyn/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdMagazynu,Nazwa,Adres,CzyAktywny")] Magazyn magazyn)
        {
            if (id != magazyn.IdMagazynu)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(magazyn);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MagazynExists(magazyn.IdMagazynu))
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
            return View(magazyn);
        }

        // GET: Magazyn/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var magazyn = await _context.Magazyn
                .FirstOrDefaultAsync(m => m.IdMagazynu == id);
            if (magazyn == null)
            {
                return NotFound();
            }

            return View(magazyn);
        }

        // POST: Magazyn/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var magazyn = await _context.Magazyn.FindAsync(id);
            if (magazyn != null)
            {
                _context.Magazyn.Remove(magazyn);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MagazynExists(int id)
        {
            return _context.Magazyn.Any(e => e.IdMagazynu == id);
        }
    }
}
