using Data.Data;
using Data.Data.Magazyn;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IntranetWeb.Controllers
{
    public class UzytkownikRolaController : Controller
    {
        private readonly DataContext _context;

        public UzytkownikRolaController(DataContext context)
        {
            _context = context;
        }

        // GET: UzytkownikRola
        public async Task<IActionResult> Index()
        {
            var intranetWebContext = _context.UzytkownikRola.Include(u => u.Rola).Include(u => u.Uzytkownik);
            return View(await intranetWebContext.ToListAsync());
        }

        // GET: UzytkownikRola/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uzytkownikRola = await _context.UzytkownikRola
                .Include(u => u.Rola)
                .Include(u => u.Uzytkownik)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (uzytkownikRola == null)
            {
                return NotFound();
            }

            return View(uzytkownikRola);
        }

        // GET: UzytkownikRola/Create
        public IActionResult Create()
        {
            ViewData["IdRoli"] = new SelectList(_context.Rola, "IdRoli", "Nazwa");
            ViewData["IdUzytkownika"] = new SelectList(_context.Set<Uzytkownik>(), "IdUzytkownika", "Email");
            return View();
        }

        // POST: UzytkownikRola/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdUzytkownika,IdRoli")] UzytkownikRola uzytkownikRola)
        {
            if (ModelState.IsValid)
            {
                _context.Add(uzytkownikRola);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdRoli"] = new SelectList(_context.Rola, "IdRoli", "Nazwa", uzytkownikRola.IdRoli);
            ViewData["IdUzytkownika"] = new SelectList(_context.Set<Uzytkownik>(), "IdUzytkownika", "Email", uzytkownikRola.IdUzytkownika);
            return View(uzytkownikRola);
        }

        // GET: UzytkownikRola/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uzytkownikRola = await _context.UzytkownikRola.FindAsync(id);
            if (uzytkownikRola == null)
            {
                return NotFound();
            }
            ViewData["IdRoli"] = new SelectList(_context.Rola, "IdRoli", "Nazwa", uzytkownikRola.IdRoli);
            ViewData["IdUzytkownika"] = new SelectList(_context.Set<Uzytkownik>(), "IdUzytkownika", "Email", uzytkownikRola.IdUzytkownika);
            return View(uzytkownikRola);
        }

        // POST: UzytkownikRola/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdUzytkownika,IdRoli")] UzytkownikRola uzytkownikRola)
        {
            if (id != uzytkownikRola.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(uzytkownikRola);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UzytkownikRolaExists(uzytkownikRola.Id))
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
            ViewData["IdRoli"] = new SelectList(_context.Rola, "IdRoli", "Nazwa", uzytkownikRola.IdRoli);
            ViewData["IdUzytkownika"] = new SelectList(_context.Set<Uzytkownik>(), "IdUzytkownika", "Email", uzytkownikRola.IdUzytkownika);
            return View(uzytkownikRola);
        }

        // GET: UzytkownikRola/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uzytkownikRola = await _context.UzytkownikRola
                .Include(u => u.Rola)
                .Include(u => u.Uzytkownik)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (uzytkownikRola == null)
            {
                return NotFound();
            }

            return View(uzytkownikRola);
        }

        // POST: UzytkownikRola/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var uzytkownikRola = await _context.UzytkownikRola.FindAsync(id);
            if (uzytkownikRola != null)
            {
                _context.UzytkownikRola.Remove(uzytkownikRola);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UzytkownikRolaExists(int id)
        {
            return _context.UzytkownikRola.Any(e => e.Id == id);
        }
    }
}
