using Data.Data;
using Data.Data.CMS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IntranetWeb.Controllers
{
    public class AktualnoscController : Controller
    {
        private readonly DataContext _context;

        public AktualnoscController(DataContext context)
        {
            _context = context;
        }

        // GET: Aktualnosc
        public async Task<IActionResult> Index()
        {
            return View(await _context.Aktualnosc.ToListAsync());
        }

        // GET: Aktualnosc/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aktualnosc = await _context.Aktualnosc
                .FirstOrDefaultAsync(m => m.IdAktualnosci == id);
            if (aktualnosc == null)
            {
                return NotFound();
            }

            return View(aktualnosc);
        }

        // GET: Aktualnosc/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Aktualnosc/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdAktualnosci,TytulLinku,Nazwa,Tresc,Pozycja")] Aktualnosc aktualnosc)
        {
            if (ModelState.IsValid)
            {
                _context.Add(aktualnosc);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(aktualnosc);
        }

        // GET: Aktualnosc/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aktualnosc = await _context.Aktualnosc.FindAsync(id);
            if (aktualnosc == null)
            {
                return NotFound();
            }
            return View(aktualnosc);
        }

        // POST: Aktualnosc/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdAktualnosci,TytulLinku,Nazwa,Tresc,Pozycja")] Aktualnosc aktualnosc)
        {
            if (id != aktualnosc.IdAktualnosci)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(aktualnosc);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AktualnoscExists(aktualnosc.IdAktualnosci))
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
            return View(aktualnosc);
        }

        // GET: Aktualnosc/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aktualnosc = await _context.Aktualnosc
                .FirstOrDefaultAsync(m => m.IdAktualnosci == id);
            if (aktualnosc == null)
            {
                return NotFound();
            }

            return View(aktualnosc);
        }

        // POST: Aktualnosc/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var aktualnosc = await _context.Aktualnosc.FindAsync(id);
            if (aktualnosc != null)
            {
                _context.Aktualnosc.Remove(aktualnosc);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AktualnoscExists(int id)
        {
            return _context.Aktualnosc.Any(e => e.IdAktualnosci == id);
        }
    }
}
