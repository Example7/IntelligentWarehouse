using Data.Data;
using Data.Data.Magazyn;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using IntranetWeb.Controllers.Abstrakcja;

namespace IntranetWeb.Controllers
{
    public class RuchMagazynowyController : BaseSearchController<RuchMagazynowy>
    {

        public RuchMagazynowyController(DataContext context) : base(context) { }

        // GET: RuchMagazynowy
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var query = _context.RuchMagazynowy.Include(r => r.LokacjaDo).Include(r => r.LokacjaZ).Include(r => r.Produkt).Include(r => r.Uzytkownik).AsNoTracking();
            query = ApplySearchAny(query, searchTerm, x => x.Referencja, x => x.Notatka);

            return View(await query.ToListAsync());
        }

        // GET: RuchMagazynowy/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ruchMagazynowy = await _context.RuchMagazynowy
                .Include(r => r.LokacjaDo)
                .Include(r => r.LokacjaZ)
                .Include(r => r.Produkt)
                .Include(r => r.Uzytkownik)
                .FirstOrDefaultAsync(m => m.IdRuchu == id);
            if (ruchMagazynowy == null)
            {
                return NotFound();
            }

            return View(ruchMagazynowy);
        }

        // GET: RuchMagazynowy/Create
        public IActionResult Create()
        {
            ViewData["IdLokacjiDo"] = new SelectList(_context.Lokacja, "IdLokacji", "Kod");
            ViewData["IdLokacjiZ"] = new SelectList(_context.Lokacja, "IdLokacji", "Kod");
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod");
            ViewData["IdUzytkownika"] = new SelectList(_context.Set<Uzytkownik>(), "IdUzytkownika", "Email");
            return View();
        }

        // POST: RuchMagazynowy/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdRuchu,Typ,IdProduktu,IdLokacjiZ,IdLokacjiDo,Ilosc,Referencja,Notatka,UtworzonoUtc,IdUzytkownika")] RuchMagazynowy ruchMagazynowy)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ruchMagazynowy);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdLokacjiDo"] = new SelectList(_context.Lokacja, "IdLokacji", "Kod", ruchMagazynowy.IdLokacjiDo);
            ViewData["IdLokacjiZ"] = new SelectList(_context.Lokacja, "IdLokacji", "Kod", ruchMagazynowy.IdLokacjiZ);
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod", ruchMagazynowy.IdProduktu);
            ViewData["IdUzytkownika"] = new SelectList(_context.Set<Uzytkownik>(), "IdUzytkownika", "Email", ruchMagazynowy.IdUzytkownika);
            return View(ruchMagazynowy);
        }

        // GET: RuchMagazynowy/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ruchMagazynowy = await _context.RuchMagazynowy.FindAsync(id);
            if (ruchMagazynowy == null)
            {
                return NotFound();
            }
            ViewData["IdLokacjiDo"] = new SelectList(_context.Lokacja, "IdLokacji", "Kod", ruchMagazynowy.IdLokacjiDo);
            ViewData["IdLokacjiZ"] = new SelectList(_context.Lokacja, "IdLokacji", "Kod", ruchMagazynowy.IdLokacjiZ);
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod", ruchMagazynowy.IdProduktu);
            ViewData["IdUzytkownika"] = new SelectList(_context.Set<Uzytkownik>(), "IdUzytkownika", "Email", ruchMagazynowy.IdUzytkownika);
            return View(ruchMagazynowy);
        }

        // POST: RuchMagazynowy/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdRuchu,Typ,IdProduktu,IdLokacjiZ,IdLokacjiDo,Ilosc,Referencja,Notatka,UtworzonoUtc,IdUzytkownika")] RuchMagazynowy ruchMagazynowy)
        {
            if (id != ruchMagazynowy.IdRuchu)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ruchMagazynowy);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RuchMagazynowyExists(ruchMagazynowy.IdRuchu))
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
            ViewData["IdLokacjiDo"] = new SelectList(_context.Lokacja, "IdLokacji", "Kod", ruchMagazynowy.IdLokacjiDo);
            ViewData["IdLokacjiZ"] = new SelectList(_context.Lokacja, "IdLokacji", "Kod", ruchMagazynowy.IdLokacjiZ);
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod", ruchMagazynowy.IdProduktu);
            ViewData["IdUzytkownika"] = new SelectList(_context.Set<Uzytkownik>(), "IdUzytkownika", "Email", ruchMagazynowy.IdUzytkownika);
            return View(ruchMagazynowy);
        }

        // GET: RuchMagazynowy/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ruchMagazynowy = await _context.RuchMagazynowy
                .Include(r => r.LokacjaDo)
                .Include(r => r.LokacjaZ)
                .Include(r => r.Produkt)
                .Include(r => r.Uzytkownik)
                .FirstOrDefaultAsync(m => m.IdRuchu == id);
            if (ruchMagazynowy == null)
            {
                return NotFound();
            }

            return View(ruchMagazynowy);
        }

        // POST: RuchMagazynowy/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ruchMagazynowy = await _context.RuchMagazynowy.FindAsync(id);
            if (ruchMagazynowy != null)
            {
                _context.RuchMagazynowy.Remove(ruchMagazynowy);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RuchMagazynowyExists(int id)
        {
            return _context.RuchMagazynowy.Any(e => e.IdRuchu == id);
        }
    }
}
