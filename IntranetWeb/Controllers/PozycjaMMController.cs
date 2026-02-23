using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Data.Data.Magazyn;

using IntranetWeb.Controllers.Abstrakcja;

namespace IntranetWeb.Controllers
{
    public class PozycjaMMController : BaseSearchController<PozycjaMM>
    {

        public PozycjaMMController(DataContext context) : base(context) { }

        // GET: PozycjaMM
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var query = _context.PozycjaMM.Include(p => p.Dokument).Include(p => p.LokacjaDo).Include(p => p.LokacjaZ).Include(p => p.Produkt).AsNoTracking();
            query = ApplySearchAny(query, searchTerm);

            return View(await query.ToListAsync());
        }

        // GET: PozycjaMM/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pozycjaMM = await _context.PozycjaMM
                .Include(p => p.Dokument)
                .Include(p => p.LokacjaDo)
                .Include(p => p.LokacjaZ)
                .Include(p => p.Produkt)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pozycjaMM == null)
            {
                return NotFound();
            }

            return View(pozycjaMM);
        }

        // GET: PozycjaMM/Create
        public IActionResult Create()
        {
            ViewData["IdDokumentu"] = new SelectList(_context.DokumentMM, "Id", "Numer");
            ViewData["IdLokacjiDo"] = new SelectList(_context.Lokacja, "IdLokacji", "Kod");
            ViewData["IdLokacjiZ"] = new SelectList(_context.Lokacja, "IdLokacji", "Kod");
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod");
            return View();
        }

        // POST: PozycjaMM/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdDokumentu,Lp,IdProduktu,IdLokacjiZ,IdLokacjiDo,Ilosc")] PozycjaMM pozycjaMM)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pozycjaMM);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdDokumentu"] = new SelectList(_context.DokumentMM, "Id", "Numer", pozycjaMM.IdDokumentu);
            ViewData["IdLokacjiDo"] = new SelectList(_context.Lokacja, "IdLokacji", "Kod", pozycjaMM.IdLokacjiDo);
            ViewData["IdLokacjiZ"] = new SelectList(_context.Lokacja, "IdLokacji", "Kod", pozycjaMM.IdLokacjiZ);
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod", pozycjaMM.IdProduktu);
            return View(pozycjaMM);
        }

        // GET: PozycjaMM/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pozycjaMM = await _context.PozycjaMM.FindAsync(id);
            if (pozycjaMM == null)
            {
                return NotFound();
            }
            ViewData["IdDokumentu"] = new SelectList(_context.DokumentMM, "Id", "Numer", pozycjaMM.IdDokumentu);
            ViewData["IdLokacjiDo"] = new SelectList(_context.Lokacja, "IdLokacji", "Kod", pozycjaMM.IdLokacjiDo);
            ViewData["IdLokacjiZ"] = new SelectList(_context.Lokacja, "IdLokacji", "Kod", pozycjaMM.IdLokacjiZ);
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod", pozycjaMM.IdProduktu);
            return View(pozycjaMM);
        }

        // POST: PozycjaMM/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdDokumentu,Lp,IdProduktu,IdLokacjiZ,IdLokacjiDo,Ilosc")] PozycjaMM pozycjaMM)
        {
            if (id != pozycjaMM.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pozycjaMM);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PozycjaMMExists(pozycjaMM.Id))
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
            ViewData["IdDokumentu"] = new SelectList(_context.DokumentMM, "Id", "Numer", pozycjaMM.IdDokumentu);
            ViewData["IdLokacjiDo"] = new SelectList(_context.Lokacja, "IdLokacji", "Kod", pozycjaMM.IdLokacjiDo);
            ViewData["IdLokacjiZ"] = new SelectList(_context.Lokacja, "IdLokacji", "Kod", pozycjaMM.IdLokacjiZ);
            ViewData["IdProduktu"] = new SelectList(_context.Produkt, "IdProduktu", "Kod", pozycjaMM.IdProduktu);
            return View(pozycjaMM);
        }

        // GET: PozycjaMM/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pozycjaMM = await _context.PozycjaMM
                .Include(p => p.Dokument)
                .Include(p => p.LokacjaDo)
                .Include(p => p.LokacjaZ)
                .Include(p => p.Produkt)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pozycjaMM == null)
            {
                return NotFound();
            }

            return View(pozycjaMM);
        }

        // POST: PozycjaMM/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pozycjaMM = await _context.PozycjaMM.FindAsync(id);
            if (pozycjaMM != null)
            {
                _context.PozycjaMM.Remove(pozycjaMM);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PozycjaMMExists(int id)
        {
            return _context.PozycjaMM.Any(e => e.Id == id);
        }
    }
}
