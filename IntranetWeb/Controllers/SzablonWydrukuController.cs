using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Data.Data.CMS;

using IntranetWeb.Controllers.Abstrakcja;

namespace IntranetWeb.Controllers
{
    public class SzablonWydrukuController : BaseSearchController<SzablonWydruku>
    {

        public SzablonWydrukuController(DataContext context) : base(context) { }

        // GET: SzablonWydruku
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var query = _context.SzablonWydruku.Include(s => s.Wgral).AsNoTracking();
            query = ApplySearchAny(query, searchTerm, x => x.TypDokumentu, x => x.Nazwa, x => x.Wersja, x => x.NazwaPliku, x => x.Sciezka);

            return View(await query.ToListAsync());
        }

        // GET: SzablonWydruku/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var szablonWydruku = await _context.SzablonWydruku
                .Include(s => s.Wgral)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (szablonWydruku == null)
            {
                return NotFound();
            }

            return View(szablonWydruku);
        }

        // GET: SzablonWydruku/Create
        public IActionResult Create()
        {
            ViewData["WgralUserId"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email");
            return View();
        }

        // POST: SzablonWydruku/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TypDokumentu,Nazwa,Wersja,NazwaPliku,Sciezka,CzyAktywny,WgranoUtc,WgralUserId")] SzablonWydruku szablonWydruku)
        {
            if (ModelState.IsValid)
            {
                _context.Add(szablonWydruku);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["WgralUserId"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email", szablonWydruku.WgralUserId);
            return View(szablonWydruku);
        }

        // GET: SzablonWydruku/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var szablonWydruku = await _context.SzablonWydruku.FindAsync(id);
            if (szablonWydruku == null)
            {
                return NotFound();
            }
            ViewData["WgralUserId"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email", szablonWydruku.WgralUserId);
            return View(szablonWydruku);
        }

        // POST: SzablonWydruku/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TypDokumentu,Nazwa,Wersja,NazwaPliku,Sciezka,CzyAktywny,WgranoUtc,WgralUserId")] SzablonWydruku szablonWydruku)
        {
            if (id != szablonWydruku.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(szablonWydruku);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SzablonWydrukuExists(szablonWydruku.Id))
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
            ViewData["WgralUserId"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email", szablonWydruku.WgralUserId);
            return View(szablonWydruku);
        }

        // GET: SzablonWydruku/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var szablonWydruku = await _context.SzablonWydruku
                .Include(s => s.Wgral)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (szablonWydruku == null)
            {
                return NotFound();
            }

            return View(szablonWydruku);
        }

        // POST: SzablonWydruku/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var szablonWydruku = await _context.SzablonWydruku.FindAsync(id);
            if (szablonWydruku != null)
            {
                _context.SzablonWydruku.Remove(szablonWydruku);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SzablonWydrukuExists(int id)
        {
            return _context.SzablonWydruku.Any(e => e.Id == id);
        }
    }
}
