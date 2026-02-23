using Data.Data;
using Data.Data.Magazyn;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IntranetWeb.Controllers
{
    public class JednostkaMiaryController : Controller
    {
        private readonly DataContext _context;

        public JednostkaMiaryController(DataContext context)
        {
            _context = context;
        }

        // GET: JednostkaMiary
        public async Task<IActionResult> Index()
        {
            return View(await _context.JednostkaMiary.ToListAsync());
        }

        // GET: JednostkaMiary/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var jednostkaMiary = await _context.JednostkaMiary
                .FirstOrDefaultAsync(m => m.IdJednostki == id);
            if (jednostkaMiary == null)
            {
                return NotFound();
            }

            return View(jednostkaMiary);
        }

        // GET: JednostkaMiary/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: JednostkaMiary/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdJednostki,Kod,Nazwa,CzyAktywna")] JednostkaMiary jednostkaMiary)
        {
            if (ModelState.IsValid)
            {
                _context.Add(jednostkaMiary);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(jednostkaMiary);
        }

        // GET: JednostkaMiary/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var jednostkaMiary = await _context.JednostkaMiary.FindAsync(id);
            if (jednostkaMiary == null)
            {
                return NotFound();
            }
            return View(jednostkaMiary);
        }

        // POST: JednostkaMiary/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdJednostki,Kod,Nazwa,CzyAktywna")] JednostkaMiary jednostkaMiary)
        {
            if (id != jednostkaMiary.IdJednostki)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(jednostkaMiary);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!JednostkaMiaryExists(jednostkaMiary.IdJednostki))
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
            return View(jednostkaMiary);
        }

        // GET: JednostkaMiary/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var jednostkaMiary = await _context.JednostkaMiary
                .FirstOrDefaultAsync(m => m.IdJednostki == id);
            if (jednostkaMiary == null)
            {
                return NotFound();
            }

            return View(jednostkaMiary);
        }

        // POST: JednostkaMiary/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var jednostkaMiary = await _context.JednostkaMiary.FindAsync(id);
            if (jednostkaMiary != null)
            {
                _context.JednostkaMiary.Remove(jednostkaMiary);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool JednostkaMiaryExists(int id)
        {
            return _context.JednostkaMiary.Any(e => e.IdJednostki == id);
        }
    }
}
