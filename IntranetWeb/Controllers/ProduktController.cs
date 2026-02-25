using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;

using IntranetWeb.Controllers.Abstrakcja;

namespace IntranetWeb.Controllers
{
    public class ProduktController : BaseSearchController<Produkt>
    {
        private readonly IProduktService _produktService;

        public ProduktController(DataContext context, IProduktService produktService) : base(context)
        {
            _produktService = produktService;
        }

        // GET: Produkt
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var query = _context.Produkt.Include(p => p.DomyslnaJednostka).Include(p => p.Kategoria).AsNoTracking();
            query = ApplySearchAny(query, searchTerm, x => x.Kod, x => x.Nazwa, x => x.Opis);

            return View(await query.ToListAsync());
        }

        // GET: Produkt/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detailsData = await _produktService.GetDetailsDataAsync(id.Value);
            if (detailsData == null)
            {
                return NotFound();
            }

            return View(detailsData);
        }

        // GET: Produkt/Create
        public IActionResult Create()
        {
            ViewData["IdDomyslnejJednostki"] = new SelectList(_context.JednostkaMiary, "IdJednostki", "Kod");
            ViewData["IdKategorii"] = new SelectList(_context.Kategoria, "IdKategorii", "Nazwa");
            return View();
        }

        // POST: Produkt/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdProduktu,Kod,Nazwa,Opis,IdKategorii,IdDomyslnejJednostki,StanMinimalny,PunktPonownegoZamowienia,IloscPonownegoZamowienia,CzyAktywny,RowVersion")] Produkt produkt)
        {
            if (ModelState.IsValid)
            {
                produkt.UtworzonoUtc = DateTime.UtcNow;
                _context.Add(produkt);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdDomyslnejJednostki"] = new SelectList(_context.JednostkaMiary, "IdJednostki", "Kod", produkt.IdDomyslnejJednostki);
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
            ViewData["IdDomyslnejJednostki"] = new SelectList(_context.JednostkaMiary, "IdJednostki", "Kod", produkt.IdDomyslnejJednostki);
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
            ViewData["IdDomyslnejJednostki"] = new SelectList(_context.JednostkaMiary, "IdJednostki", "Kod", produkt.IdDomyslnejJednostki);
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

            var data = await _produktService.GetDeleteDataAsync(id.Value);
            if (data == null)
            {
                return NotFound();
            }

            return View(data);
        }

        // POST: Produkt/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deleteData = await _produktService.GetDeleteDataAsync(id);
            if (deleteData == null)
            {
                return NotFound();
            }

            if (!deleteData.CzyMoznaUsunac)
            {
                ModelState.AddModelError(string.Empty, "Nie można usunąć produktu, ponieważ ma powiązane stany lub dokumenty.");
                return View("Delete", deleteData);
            }

            try
            {
                var produkt = await _context.Produkt.FindAsync(id);
                if (produkt != null)
                {
                    _context.Produkt.Remove(produkt);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Nie udało się usunąć produktu. Produkt może być nadal powiązany z innymi rekordami.");
                var refreshed = await _produktService.GetDeleteDataAsync(id);
                if (refreshed == null)
                {
                    return NotFound();
                }
                return View("Delete", refreshed);
            }
        }

        private bool ProduktExists(int id)
        {
            return _context.Produkt.Any(e => e.IdProduktu == id);
        }
    }
}