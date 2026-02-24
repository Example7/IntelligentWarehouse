using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;
using Interfaces.Magazyn.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using IntranetWeb.Controllers.Abstrakcja;

namespace IntranetWeb.Controllers
{
    public class StanMagazynowyController : BaseSearchController<StanMagazynowy>
    {
        private readonly IStanMagazynowyService _stanMagazynowyService;

        public StanMagazynowyController(DataContext context, IStanMagazynowyService stanMagazynowyService) : base(context)
        {
            _stanMagazynowyService = stanMagazynowyService;
        }

        // GET: StanMagazynowy
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var model = await _stanMagazynowyService.GetIndexDataAsync(searchTerm);
            return View(model);
        }

        // GET: StanMagazynowy/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stanMagazynowy = await _context.StanMagazynowy
                .Include(s => s.Lokacja)
                    .ThenInclude(l => l.Magazyn)
                .Include(s => s.Produkt)
                    .ThenInclude(p => p.DomyslnaJednostka)
                .FirstOrDefaultAsync(m => m.IdStanu == id);
            if (stanMagazynowy == null)
            {
                return NotFound();
            }

            return View(stanMagazynowy);
        }

        // GET: StanMagazynowy/Create
        public async Task<IActionResult> Create()
        {
            var model = await _stanMagazynowyService.GetCreateFormAsync();
            return View(model);
        }

        // POST: StanMagazynowy/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StanMagazynowyFormDto model)
        {
            var stanMagazynowy = model.StanMagazynowy;

            if (ModelState.IsValid)
            {
                _context.Add(stanMagazynowy);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var formModel = await _stanMagazynowyService.PrepareFormAsync(stanMagazynowy, isEdit: false);
            return View(formModel);
        }

        // GET: StanMagazynowy/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _stanMagazynowyService.GetEditFormAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // POST: StanMagazynowy/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StanMagazynowyFormDto model)
        {
            var stanMagazynowy = model.StanMagazynowy;
            if (id != stanMagazynowy.IdStanu)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(stanMagazynowy);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StanMagazynowyExists(stanMagazynowy.IdStanu))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            var formModel = await _stanMagazynowyService.PrepareFormAsync(stanMagazynowy, isEdit: true);
            return View(formModel);
        }

        // GET: StanMagazynowy/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stanMagazynowy = await _context.StanMagazynowy
                .Include(s => s.Lokacja)
                    .ThenInclude(l => l.Magazyn)
                .Include(s => s.Produkt)
                    .ThenInclude(p => p.DomyslnaJednostka)
                .FirstOrDefaultAsync(m => m.IdStanu == id);
            if (stanMagazynowy == null)
            {
                return NotFound();
            }

            return View(stanMagazynowy);
        }

        // POST: StanMagazynowy/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var stanMagazynowy = await _context.StanMagazynowy.FindAsync(id);
            if (stanMagazynowy != null)
            {
                _context.StanMagazynowy.Remove(stanMagazynowy);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StanMagazynowyExists(int id)
        {
            return _context.StanMagazynowy.Any(e => e.IdStanu == id);
        }
    }
}
