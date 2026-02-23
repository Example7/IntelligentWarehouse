using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Data.Data.Magazyn;

using IntranetWeb.Controllers.Abstrakcja;

namespace IntranetWeb.Controllers
{
    public class DokumentMMController : BaseSearchController<DokumentMM>
    {

        public DokumentMMController(DataContext context) : base(context) { }

        // GET: DokumentMM
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var query = _context.DokumentMM.Include(d => d.Magazyn).Include(d => d.Utworzyl).AsNoTracking();
            query = ApplySearchAny(query, searchTerm, x => x.Numer, x => x.Status, x => x.Notatka);

            return View(await query.ToListAsync());
        }

        // GET: DokumentMM/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dokumentMM = await _context.DokumentMM
                .Include(d => d.Magazyn)
                .Include(d => d.Utworzyl)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dokumentMM == null)
            {
                return NotFound();
            }

            return View(dokumentMM);
        }

        // GET: DokumentMM/Create
        public IActionResult Create()
        {
            ViewData["IdMagazynu"] = new SelectList(_context.Magazyn, "IdMagazynu", "Nazwa");
            ViewData["IdUtworzyl"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email");
            return View();
        }

        // POST: DokumentMM/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Numer,IdMagazynu,Status,DataUtc,IdUtworzyl,ZaksiegowanoUtc,Notatka,RowVersion")] DokumentMM dokumentMM)
        {
            if (ModelState.IsValid)
            {
                _context.Add(dokumentMM);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdMagazynu"] = new SelectList(_context.Magazyn, "IdMagazynu", "Nazwa", dokumentMM.IdMagazynu);
            ViewData["IdUtworzyl"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email", dokumentMM.IdUtworzyl);
            return View(dokumentMM);
        }

        // GET: DokumentMM/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dokumentMM = await _context.DokumentMM.FindAsync(id);
            if (dokumentMM == null)
            {
                return NotFound();
            }
            ViewData["IdMagazynu"] = new SelectList(_context.Magazyn, "IdMagazynu", "Nazwa", dokumentMM.IdMagazynu);
            ViewData["IdUtworzyl"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email", dokumentMM.IdUtworzyl);
            return View(dokumentMM);
        }

        // POST: DokumentMM/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Numer,IdMagazynu,Status,DataUtc,IdUtworzyl,ZaksiegowanoUtc,Notatka,RowVersion")] DokumentMM dokumentMM)
        {
            if (id != dokumentMM.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dokumentMM);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DokumentMMExists(dokumentMM.Id))
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
            ViewData["IdMagazynu"] = new SelectList(_context.Magazyn, "IdMagazynu", "Nazwa", dokumentMM.IdMagazynu);
            ViewData["IdUtworzyl"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email", dokumentMM.IdUtworzyl);
            return View(dokumentMM);
        }

        // GET: DokumentMM/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dokumentMM = await _context.DokumentMM
                .Include(d => d.Magazyn)
                .Include(d => d.Utworzyl)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dokumentMM == null)
            {
                return NotFound();
            }

            return View(dokumentMM);
        }

        // POST: DokumentMM/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dokumentMM = await _context.DokumentMM.FindAsync(id);
            if (dokumentMM != null)
            {
                _context.DokumentMM.Remove(dokumentMM);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DokumentMMExists(int id)
        {
            return _context.DokumentMM.Any(e => e.Id == id);
        }
    }
}
