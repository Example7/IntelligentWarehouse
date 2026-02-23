using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Data.Data.Magazyn;

using IntranetWeb.Controllers.Abstrakcja;

namespace IntranetWeb.Controllers
{
    public class PozycjaPZController : BaseSearchController<PozycjaPZ>
    {

        public PozycjaPZController(DataContext context) : base(context) { }

        // GET: PozycjaPZ
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var query = _context.PozycjaPZ.Include(p => p.Dokument).Include(p => p.Lokacja).Include(p => p.Produkt).AsNoTracking();
            query = ApplySearchAny(query, searchTerm);

            return View(await query.ToListAsync());
        }

        // GET: PozycjaPZ/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pozycjaPZ = await _context.PozycjaPZ
                .Include(p => p.Dokument)
                .Include(p => p.Lokacja)
                .Include(p => p.Produkt)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pozycjaPZ == null)
            {
                return NotFound();
            }

            return View(pozycjaPZ);
        }

        // GET: PozycjaPZ/Create
        public IActionResult Create()
        {
            ViewData["IdDokumentu"] = new SelectList(_context.DokumentPZ, "Id", "Numer");
            ViewData["IdLokacji"] = new SelectList(_context.Lokacja, "IdLokacji", "Kod");
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod");
            return View();
        }

        // POST: PozycjaPZ/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdDokumentu,Lp,IdProduktu,IdLokacji,Ilosc,CenaJednostkowa")] PozycjaPZ pozycjaPZ)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pozycjaPZ);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdDokumentu"] = new SelectList(_context.DokumentPZ, "Id", "Numer", pozycjaPZ.IdDokumentu);
            ViewData["IdLokacji"] = new SelectList(_context.Lokacja, "IdLokacji", "Kod", pozycjaPZ.IdLokacji);
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod", pozycjaPZ.IdProduktu);
            return View(pozycjaPZ);
        }

        // GET: PozycjaPZ/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pozycjaPZ = await _context.PozycjaPZ.FindAsync(id);
            if (pozycjaPZ == null)
            {
                return NotFound();
            }
            ViewData["IdDokumentu"] = new SelectList(_context.DokumentPZ, "Id", "Numer", pozycjaPZ.IdDokumentu);
            ViewData["IdLokacji"] = new SelectList(_context.Lokacja, "IdLokacji", "Kod", pozycjaPZ.IdLokacji);
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod", pozycjaPZ.IdProduktu);
            return View(pozycjaPZ);
        }

        // POST: PozycjaPZ/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdDokumentu,Lp,IdProduktu,IdLokacji,Ilosc,CenaJednostkowa")] PozycjaPZ pozycjaPZ)
        {
            if (id != pozycjaPZ.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pozycjaPZ);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PozycjaPZExists(pozycjaPZ.Id))
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
            ViewData["IdDokumentu"] = new SelectList(_context.DokumentPZ, "Id", "Numer", pozycjaPZ.IdDokumentu);
            ViewData["IdLokacji"] = new SelectList(_context.Lokacja, "IdLokacji", "Kod", pozycjaPZ.IdLokacji);
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod", pozycjaPZ.IdProduktu);
            return View(pozycjaPZ);
        }

        // GET: PozycjaPZ/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pozycjaPZ = await _context.PozycjaPZ
                .Include(p => p.Dokument)
                .Include(p => p.Lokacja)
                .Include(p => p.Produkt)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pozycjaPZ == null)
            {
                return NotFound();
            }

            return View(pozycjaPZ);
        }

        // POST: PozycjaPZ/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pozycjaPZ = await _context.PozycjaPZ.FindAsync(id);
            if (pozycjaPZ != null)
            {
                _context.PozycjaPZ.Remove(pozycjaPZ);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PozycjaPZExists(int id)
        {
            return _context.PozycjaPZ.Any(e => e.Id == id);
        }
    }
}
