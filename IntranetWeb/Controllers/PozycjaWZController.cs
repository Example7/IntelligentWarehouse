using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Data.Data.Magazyn;

using IntranetWeb.Controllers.Abstrakcja;

namespace IntranetWeb.Controllers
{
    public class PozycjaWZController : BaseSearchController<PozycjaWZ>
    {

        public PozycjaWZController(DataContext context) : base(context) { }

        // GET: PozycjaWZ
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var query = _context.PozycjaWZ.Include(p => p.Dokument).Include(p => p.Lokacja).Include(p => p.Produkt).AsNoTracking();
            query = ApplySearchAny(query, searchTerm);

            return View(await query.ToListAsync());
        }

        // GET: PozycjaWZ/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pozycjaWZ = await _context.PozycjaWZ
                .Include(p => p.Dokument)
                .Include(p => p.Lokacja)
                .Include(p => p.Produkt)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pozycjaWZ == null)
            {
                return NotFound();
            }

            return View(pozycjaWZ);
        }

        // GET: PozycjaWZ/Create
        public IActionResult Create()
        {
            ViewData["IdDokumentu"] = new SelectList(_context.DokumentWZ, "Id", "Numer");
            ViewData["IdLokacji"] = new SelectList(_context.Lokacja, "IdLokacji", "Kod");
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod");
            return View();
        }

        // POST: PozycjaWZ/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdDokumentu,Lp,IdProduktu,IdLokacji,Ilosc")] PozycjaWZ pozycjaWZ)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pozycjaWZ);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdDokumentu"] = new SelectList(_context.DokumentWZ, "Id", "Numer", pozycjaWZ.IdDokumentu);
            ViewData["IdLokacji"] = new SelectList(_context.Lokacja, "IdLokacji", "Kod", pozycjaWZ.IdLokacji);
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod", pozycjaWZ.IdProduktu);
            return View(pozycjaWZ);
        }

        // GET: PozycjaWZ/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pozycjaWZ = await _context.PozycjaWZ.FindAsync(id);
            if (pozycjaWZ == null)
            {
                return NotFound();
            }
            ViewData["IdDokumentu"] = new SelectList(_context.DokumentWZ, "Id", "Numer", pozycjaWZ.IdDokumentu);
            ViewData["IdLokacji"] = new SelectList(_context.Lokacja, "IdLokacji", "Kod", pozycjaWZ.IdLokacji);
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod", pozycjaWZ.IdProduktu);
            return View(pozycjaWZ);
        }

        // POST: PozycjaWZ/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdDokumentu,Lp,IdProduktu,IdLokacji,Ilosc")] PozycjaWZ pozycjaWZ)
        {
            if (id != pozycjaWZ.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pozycjaWZ);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PozycjaWZExists(pozycjaWZ.Id))
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
            ViewData["IdDokumentu"] = new SelectList(_context.DokumentWZ, "Id", "Numer", pozycjaWZ.IdDokumentu);
            ViewData["IdLokacji"] = new SelectList(_context.Lokacja, "IdLokacji", "Kod", pozycjaWZ.IdLokacji);
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod", pozycjaWZ.IdProduktu);
            return View(pozycjaWZ);
        }

        // GET: PozycjaWZ/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pozycjaWZ = await _context.PozycjaWZ
                .Include(p => p.Dokument)
                .Include(p => p.Lokacja)
                .Include(p => p.Produkt)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pozycjaWZ == null)
            {
                return NotFound();
            }

            return View(pozycjaWZ);
        }

        // POST: PozycjaWZ/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pozycjaWZ = await _context.PozycjaWZ.FindAsync(id);
            if (pozycjaWZ != null)
            {
                _context.PozycjaWZ.Remove(pozycjaWZ);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PozycjaWZExists(int id)
        {
            return _context.PozycjaWZ.Any(e => e.Id == id);
        }
    }
}
