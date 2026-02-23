using Data.Data;
using Data.Data.Magazyn;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IntranetWeb.Controllers
{
    public class ProduktController : Controller
    {
        private readonly DataContext _context;

        public ProduktController(DataContext context)
        {
            _context = context;
        }

        // GET: Produkt
        public async Task<IActionResult> Index()
        {
            var intranetWebContext = _context.Produkt.Include(p => p.Kategoria);
            return View(await intranetWebContext.ToListAsync());
        }

        // GET: Produkt/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var produkt = await _context.Produkt
                .Include(p => p.Kategoria)
                .FirstOrDefaultAsync(m => m.IdProduktu == id);
            if (produkt == null)
            {
                return NotFound();
            }

            return View(produkt);
        }

        // GET: Produkt/Create
        public IActionResult Create()
        {
            ViewData["IdKategorii"] = new SelectList(_context.Kategoria, "IdKategorii", "Nazwa");
            return View();
        }

        // POST: Produkt/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Kod,Nazwa,Opis,IdKategorii,IdDomyslnejJednostki,StanMinimalny,PunktPonownegoZamowienia,IloscPonownegoZamowienia,CzyAktywny")] Produkt produkt)
        {
            produkt.UtworzonoUtc = DateTime.UtcNow;

            if (ModelState.IsValid)
            {
                _context.Add(produkt);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["IdKategorii"] = new SelectList(_context.Kategoria, "IdKategorii", "Nazwa", produkt.IdKategorii);
            return View(produkt);
        }

        // GET: Produkt/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var produkt = await _context.Produkt.FindAsync(id);
            if (produkt == null)
            {
                return NotFound();
            }
            ViewData["IdKategorii"] = new SelectList(_context.Kategoria, "IdKategorii", "Nazwa", produkt.IdKategorii);
            return View(produkt);
        }

        // POST: Produkt/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdProduktu,Kod,Nazwa,Opis,IdKategorii,IdDomyslnejJednostki,StanMinimalny,PunktPonownegoZamowienia,IloscPonownegoZamowienia,CzyAktywny,UtworzonoUtc,RowVersion")] Produkt produkt)
        {
            if (id != produkt.IdProduktu)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(produkt);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProduktExists(produkt.IdProduktu))
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
            ViewData["IdKategorii"] = new SelectList(_context.Kategoria, "IdKategorii", "Nazwa", produkt.IdKategorii);
            return View(produkt);
        }

        // GET: Produkt/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var produkt = await _context.Produkt
                .Include(p => p.Kategoria)
                .FirstOrDefaultAsync(m => m.IdProduktu == id);
            if (produkt == null)
            {
                return NotFound();
            }

            return View(produkt);
        }

        // POST: Produkt/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var produkt = await _context.Produkt.FindAsync(id);
            if (produkt != null)
            {
                _context.Produkt.Remove(produkt);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProduktExists(int id)
        {
            return _context.Produkt.Any(e => e.IdProduktu == id);
        }
    }
}
