using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Data.Data.Magazyn;

using IntranetWeb.Controllers.Abstrakcja;

namespace IntranetWeb.Controllers
{
    public class PozycjaInwentaryzacjiController : BaseSearchController<PozycjaInwentaryzacji>
    {

        public PozycjaInwentaryzacjiController(DataContext context) : base(context) { }

        // GET: PozycjaInwentaryzacji
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var query = _context.PozycjaInwentaryzacji.Include(p => p.Dokument).Include(p => p.Lokacja).Include(p => p.Produkt).AsNoTracking();
            query = ApplySearchAny(query, searchTerm);

            return View(await query.ToListAsync());
        }

        // GET: PozycjaInwentaryzacji/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pozycjaInwentaryzacji = await _context.PozycjaInwentaryzacji
                .Include(p => p.Dokument)
                .Include(p => p.Lokacja)
                .Include(p => p.Produkt)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pozycjaInwentaryzacji == null)
            {
                return NotFound();
            }

            return View(pozycjaInwentaryzacji);
        }

        // GET: PozycjaInwentaryzacji/Create
        public IActionResult Create()
        {
            ViewData["IdDokumentu"] = new SelectList(_context.Inwentaryzacja, "Id", "Numer");
            ViewData["IdLokacji"] = new SelectList(_context.Lokacja, "IdLokacji", "Kod");
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod");
            return View();
        }

        // POST: PozycjaInwentaryzacji/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdDokumentu,Lp,IdProduktu,IdLokacji,IloscOczekiwana,IloscPoliczona")] PozycjaInwentaryzacji pozycjaInwentaryzacji)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pozycjaInwentaryzacji);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdDokumentu"] = new SelectList(_context.Inwentaryzacja, "Id", "Numer", pozycjaInwentaryzacji.IdDokumentu);
            ViewData["IdLokacji"] = new SelectList(_context.Lokacja, "IdLokacji", "Kod", pozycjaInwentaryzacji.IdLokacji);
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod", pozycjaInwentaryzacji.IdProduktu);
            return View(pozycjaInwentaryzacji);
        }

        // GET: PozycjaInwentaryzacji/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pozycjaInwentaryzacji = await _context.PozycjaInwentaryzacji.FindAsync(id);
            if (pozycjaInwentaryzacji == null)
            {
                return NotFound();
            }
            ViewData["IdDokumentu"] = new SelectList(_context.Inwentaryzacja, "Id", "Numer", pozycjaInwentaryzacji.IdDokumentu);
            ViewData["IdLokacji"] = new SelectList(_context.Lokacja, "IdLokacji", "Kod", pozycjaInwentaryzacji.IdLokacji);
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod", pozycjaInwentaryzacji.IdProduktu);
            return View(pozycjaInwentaryzacji);
        }

        // POST: PozycjaInwentaryzacji/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdDokumentu,Lp,IdProduktu,IdLokacji,IloscOczekiwana,IloscPoliczona")] PozycjaInwentaryzacji pozycjaInwentaryzacji)
        {
            if (id != pozycjaInwentaryzacji.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pozycjaInwentaryzacji);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PozycjaInwentaryzacjiExists(pozycjaInwentaryzacji.Id))
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
            ViewData["IdDokumentu"] = new SelectList(_context.Inwentaryzacja, "Id", "Numer", pozycjaInwentaryzacji.IdDokumentu);
            ViewData["IdLokacji"] = new SelectList(_context.Lokacja, "IdLokacji", "Kod", pozycjaInwentaryzacji.IdLokacji);
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod", pozycjaInwentaryzacji.IdProduktu);
            return View(pozycjaInwentaryzacji);
        }

        // GET: PozycjaInwentaryzacji/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pozycjaInwentaryzacji = await _context.PozycjaInwentaryzacji
                .Include(p => p.Dokument)
                .Include(p => p.Lokacja)
                .Include(p => p.Produkt)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pozycjaInwentaryzacji == null)
            {
                return NotFound();
            }

            return View(pozycjaInwentaryzacji);
        }

        // POST: PozycjaInwentaryzacji/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pozycjaInwentaryzacji = await _context.PozycjaInwentaryzacji.FindAsync(id);
            if (pozycjaInwentaryzacji != null)
            {
                _context.PozycjaInwentaryzacji.Remove(pozycjaInwentaryzacji);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PozycjaInwentaryzacjiExists(int id)
        {
            return _context.PozycjaInwentaryzacji.Any(e => e.Id == id);
        }
    }
}
