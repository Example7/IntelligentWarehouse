using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Data.Data.CMS;

using IntranetWeb.Controllers.Abstrakcja;

namespace IntranetWeb.Controllers
{
    public class ZalacznikDokumentuController : BaseSearchController<ZalacznikDokumentu>
    {

        public ZalacznikDokumentuController(DataContext context) : base(context) { }

        // GET: ZalacznikDokumentu
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var query = _context.ZalacznikDokumentu.Include(z => z.Wgral).AsNoTracking();
            query = ApplySearchAny(query, searchTerm, x => x.TypDokumentu, x => x.NazwaPliku, x => x.ContentType, x => x.Sciezka);

            return View(await query.ToListAsync());
        }

        // GET: ZalacznikDokumentu/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var zalacznikDokumentu = await _context.ZalacznikDokumentu
                .Include(z => z.Wgral)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (zalacznikDokumentu == null)
            {
                return NotFound();
            }

            return View(zalacznikDokumentu);
        }

        // GET: ZalacznikDokumentu/Create
        public IActionResult Create()
        {
            ViewData["WgralUserId"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email");
            return View();
        }

        // POST: ZalacznikDokumentu/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TypDokumentu,IdDokumentu,NazwaPliku,ContentType,Sciezka,WgranoUtc,WgralUserId")] ZalacznikDokumentu zalacznikDokumentu)
        {
            if (ModelState.IsValid)
            {
                _context.Add(zalacznikDokumentu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["WgralUserId"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email", zalacznikDokumentu.WgralUserId);
            return View(zalacznikDokumentu);
        }

        // GET: ZalacznikDokumentu/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var zalacznikDokumentu = await _context.ZalacznikDokumentu.FindAsync(id);
            if (zalacznikDokumentu == null)
            {
                return NotFound();
            }
            ViewData["WgralUserId"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email", zalacznikDokumentu.WgralUserId);
            return View(zalacznikDokumentu);
        }

        // POST: ZalacznikDokumentu/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,TypDokumentu,IdDokumentu,NazwaPliku,ContentType,Sciezka,WgranoUtc,WgralUserId")] ZalacznikDokumentu zalacznikDokumentu)
        {
            if (id != zalacznikDokumentu.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(zalacznikDokumentu);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ZalacznikDokumentuExists(zalacznikDokumentu.Id))
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
            ViewData["WgralUserId"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email", zalacznikDokumentu.WgralUserId);
            return View(zalacznikDokumentu);
        }

        // GET: ZalacznikDokumentu/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var zalacznikDokumentu = await _context.ZalacznikDokumentu
                .Include(z => z.Wgral)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (zalacznikDokumentu == null)
            {
                return NotFound();
            }

            return View(zalacznikDokumentu);
        }

        // POST: ZalacznikDokumentu/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var zalacznikDokumentu = await _context.ZalacznikDokumentu.FindAsync(id);
            if (zalacznikDokumentu != null)
            {
                _context.ZalacznikDokumentu.Remove(zalacznikDokumentu);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ZalacznikDokumentuExists(long id)
        {
            return _context.ZalacznikDokumentu.Any(e => e.Id == id);
        }
    }
}
