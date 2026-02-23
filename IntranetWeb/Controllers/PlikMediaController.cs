using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Data.Data.CMS;

using IntranetWeb.Controllers.Abstrakcja;

namespace IntranetWeb.Controllers
{
    public class PlikMediaController : BaseSearchController<PlikMedia>
    {

        public PlikMediaController(DataContext context) : base(context) { }

        // GET: PlikMedia
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var query = _context.PlikMedia.Include(p => p.Wgral).AsNoTracking();
            query = ApplySearchAny(query, searchTerm, x => x.NazwaPliku, x => x.ContentType, x => x.Sciezka, x => x.Opis);

            return View(await query.ToListAsync());
        }

        // GET: PlikMedia/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plikMedia = await _context.PlikMedia
                .Include(p => p.Wgral)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (plikMedia == null)
            {
                return NotFound();
            }

            return View(plikMedia);
        }

        // GET: PlikMedia/Create
        public IActionResult Create()
        {
            ViewData["WgralUserId"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email");
            return View();
        }

        // POST: PlikMedia/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,NazwaPliku,ContentType,Sciezka,RozmiarBajty,Opis,WgranoUtc,WgralUserId")] PlikMedia plikMedia)
        {
            if (ModelState.IsValid)
            {
                _context.Add(plikMedia);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["WgralUserId"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email", plikMedia.WgralUserId);
            return View(plikMedia);
        }

        // GET: PlikMedia/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plikMedia = await _context.PlikMedia.FindAsync(id);
            if (plikMedia == null)
            {
                return NotFound();
            }
            ViewData["WgralUserId"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email", plikMedia.WgralUserId);
            return View(plikMedia);
        }

        // POST: PlikMedia/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,NazwaPliku,ContentType,Sciezka,RozmiarBajty,Opis,WgranoUtc,WgralUserId")] PlikMedia plikMedia)
        {
            if (id != plikMedia.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(plikMedia);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlikMediaExists(plikMedia.Id))
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
            ViewData["WgralUserId"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email", plikMedia.WgralUserId);
            return View(plikMedia);
        }

        // GET: PlikMedia/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plikMedia = await _context.PlikMedia
                .Include(p => p.Wgral)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (plikMedia == null)
            {
                return NotFound();
            }

            return View(plikMedia);
        }

        // POST: PlikMedia/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var plikMedia = await _context.PlikMedia.FindAsync(id);
            if (plikMedia != null)
            {
                _context.PlikMedia.Remove(plikMedia);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PlikMediaExists(long id)
        {
            return _context.PlikMedia.Any(e => e.Id == id);
        }
    }
}
