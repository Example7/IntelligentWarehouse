using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Data.Data.Magazyn;

using IntranetWeb.Controllers.Abstrakcja;

namespace IntranetWeb.Controllers
{
    public class DokumentWZController : BaseSearchController<DokumentWZ>
    {

        public DokumentWZController(DataContext context) : base(context) { }

        // GET: DokumentWZ
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var query = _context.DokumentWZ.Include(d => d.Klient).Include(d => d.Magazyn).Include(d => d.Utworzyl).AsNoTracking();
            query = ApplySearchAny(query, searchTerm, x => x.Numer, x => x.Status, x => x.Notatka);

            return View(await query.ToListAsync());
        }

        // GET: DokumentWZ/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dokumentWZ = await _context.DokumentWZ
                .Include(d => d.Klient)
                .Include(d => d.Magazyn)
                .Include(d => d.Utworzyl)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dokumentWZ == null)
            {
                return NotFound();
            }

            return View(dokumentWZ);
        }

        // GET: DokumentWZ/Create
        public IActionResult Create()
        {
            ViewData["IdKlienta"] = new SelectList(_context.Klient, "IdKlienta", "Nazwa");
            ViewData["IdMagazynu"] = new SelectList(_context.Magazyn, "IdMagazynu", "Nazwa");
            ViewData["IdUtworzyl"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email");
            return View();
        }

        // POST: DokumentWZ/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Numer,IdMagazynu,IdKlienta,Status,DataWydaniaUtc,IdUtworzyl,ZaksiegowanoUtc,Notatka,RowVersion")] DokumentWZ dokumentWZ)
        {
            if (ModelState.IsValid)
            {
                _context.Add(dokumentWZ);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdKlienta"] = new SelectList(_context.Klient, "IdKlienta", "Nazwa", dokumentWZ.IdKlienta);
            ViewData["IdMagazynu"] = new SelectList(_context.Magazyn, "IdMagazynu", "Nazwa", dokumentWZ.IdMagazynu);
            ViewData["IdUtworzyl"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email", dokumentWZ.IdUtworzyl);
            return View(dokumentWZ);
        }

        // GET: DokumentWZ/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dokumentWZ = await _context.DokumentWZ.FindAsync(id);
            if (dokumentWZ == null)
            {
                return NotFound();
            }
            ViewData["IdKlienta"] = new SelectList(_context.Klient, "IdKlienta", "Nazwa", dokumentWZ.IdKlienta);
            ViewData["IdMagazynu"] = new SelectList(_context.Magazyn, "IdMagazynu", "Nazwa", dokumentWZ.IdMagazynu);
            ViewData["IdUtworzyl"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email", dokumentWZ.IdUtworzyl);
            return View(dokumentWZ);
        }

        // POST: DokumentWZ/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Numer,IdMagazynu,IdKlienta,Status,DataWydaniaUtc,IdUtworzyl,ZaksiegowanoUtc,Notatka,RowVersion")] DokumentWZ dokumentWZ)
        {
            if (id != dokumentWZ.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dokumentWZ);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DokumentWZExists(dokumentWZ.Id))
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
            ViewData["IdKlienta"] = new SelectList(_context.Klient, "IdKlienta", "Nazwa", dokumentWZ.IdKlienta);
            ViewData["IdMagazynu"] = new SelectList(_context.Magazyn, "IdMagazynu", "Nazwa", dokumentWZ.IdMagazynu);
            ViewData["IdUtworzyl"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email", dokumentWZ.IdUtworzyl);
            return View(dokumentWZ);
        }

        // GET: DokumentWZ/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dokumentWZ = await _context.DokumentWZ
                .Include(d => d.Klient)
                .Include(d => d.Magazyn)
                .Include(d => d.Utworzyl)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dokumentWZ == null)
            {
                return NotFound();
            }

            return View(dokumentWZ);
        }

        // POST: DokumentWZ/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dokumentWZ = await _context.DokumentWZ.FindAsync(id);
            if (dokumentWZ != null)
            {
                _context.DokumentWZ.Remove(dokumentWZ);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DokumentWZExists(int id)
        {
            return _context.DokumentWZ.Any(e => e.Id == id);
        }
    }
}
