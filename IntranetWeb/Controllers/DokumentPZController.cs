using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Data.Data.Magazyn;

using IntranetWeb.Controllers.Abstrakcja;

namespace IntranetWeb.Controllers
{
    public class DokumentPZController : BaseSearchController<DokumentPZ>
    {

        public DokumentPZController(DataContext context) : base(context) { }

        // GET: DokumentPZ
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var query = _context.DokumentPZ.Include(d => d.Dostawca).Include(d => d.Magazyn).Include(d => d.Utworzyl).AsNoTracking();
            query = ApplySearchAny(query, searchTerm, x => x.Numer, x => x.Status, x => x.Notatka);

            return View(await query.ToListAsync());
        }

        // GET: DokumentPZ/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dokumentPZ = await _context.DokumentPZ
                .Include(d => d.Dostawca)
                .Include(d => d.Magazyn)
                .Include(d => d.Utworzyl)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dokumentPZ == null)
            {
                return NotFound();
            }

            return View(dokumentPZ);
        }

        // GET: DokumentPZ/Create
        public IActionResult Create()
        {
            ViewData["IdDostawcy"] = new SelectList(_context.Dostawca, "IdDostawcy", "Nazwa");
            ViewData["IdMagazynu"] = new SelectList(_context.Magazyn, "IdMagazynu", "Nazwa");
            ViewData["IdUtworzyl"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email");
            return View();
        }

        // POST: DokumentPZ/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Numer,IdMagazynu,IdDostawcy,Status,DataPrzyjeciaUtc,IdUtworzyl,ZaksiegowanoUtc,Notatka,RowVersion")] DokumentPZ dokumentPZ)
        {
            if (ModelState.IsValid)
            {
                _context.Add(dokumentPZ);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdDostawcy"] = new SelectList(_context.Dostawca, "IdDostawcy", "Nazwa", dokumentPZ.IdDostawcy);
            ViewData["IdMagazynu"] = new SelectList(_context.Magazyn, "IdMagazynu", "Nazwa", dokumentPZ.IdMagazynu);
            ViewData["IdUtworzyl"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email", dokumentPZ.IdUtworzyl);
            return View(dokumentPZ);
        }

        // GET: DokumentPZ/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dokumentPZ = await _context.DokumentPZ.FindAsync(id);
            if (dokumentPZ == null)
            {
                return NotFound();
            }
            ViewData["IdDostawcy"] = new SelectList(_context.Dostawca, "IdDostawcy", "Nazwa", dokumentPZ.IdDostawcy);
            ViewData["IdMagazynu"] = new SelectList(_context.Magazyn, "IdMagazynu", "Nazwa", dokumentPZ.IdMagazynu);
            ViewData["IdUtworzyl"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email", dokumentPZ.IdUtworzyl);
            return View(dokumentPZ);
        }

        // POST: DokumentPZ/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Numer,IdMagazynu,IdDostawcy,Status,DataPrzyjeciaUtc,IdUtworzyl,ZaksiegowanoUtc,Notatka,RowVersion")] DokumentPZ dokumentPZ)
        {
            if (id != dokumentPZ.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dokumentPZ);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DokumentPZExists(dokumentPZ.Id))
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
            ViewData["IdDostawcy"] = new SelectList(_context.Dostawca, "IdDostawcy", "Nazwa", dokumentPZ.IdDostawcy);
            ViewData["IdMagazynu"] = new SelectList(_context.Magazyn, "IdMagazynu", "Nazwa", dokumentPZ.IdMagazynu);
            ViewData["IdUtworzyl"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email", dokumentPZ.IdUtworzyl);
            return View(dokumentPZ);
        }

        // GET: DokumentPZ/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dokumentPZ = await _context.DokumentPZ
                .Include(d => d.Dostawca)
                .Include(d => d.Magazyn)
                .Include(d => d.Utworzyl)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dokumentPZ == null)
            {
                return NotFound();
            }

            return View(dokumentPZ);
        }

        // POST: DokumentPZ/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dokumentPZ = await _context.DokumentPZ.FindAsync(id);
            if (dokumentPZ != null)
            {
                _context.DokumentPZ.Remove(dokumentPZ);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DokumentPZExists(int id)
        {
            return _context.DokumentPZ.Any(e => e.Id == id);
        }
    }
}
