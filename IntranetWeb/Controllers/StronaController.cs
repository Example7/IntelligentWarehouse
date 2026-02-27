using Data.Data;
using Data.Data.CMS;
using IntranetWeb.Controllers.Abstrakcja;
using Interfaces.CMS;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using IntranetWeb.Security;
using Microsoft.AspNetCore.Authorization;

namespace IntranetWeb.Controllers
{
    [Authorize(Roles = AppRoles.AdminOnly)]
    public class StronaController : BaseSearchController<Strona>
    {
        private readonly IStronaService _stronaService;

        public StronaController(DataContext context, IStronaService stronaService) : base(context)
        {
            _stronaService = stronaService;
        }

        public async Task<IActionResult> Index(string? searchTerm)
        {
            return View(await _stronaService.GetIndexDataAsync(searchTerm));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dto = await _stronaService.GetDetailsDataAsync(id.Value);
            if (dto == null)
            {
                return NotFound();
            }

            return View(dto);
        }

        public async Task<IActionResult> Create()
        {
            var nextPozycja = await GetNextPozycjaAsync();
            return View(new Strona
            {
                TytulLinku = string.Empty,
                Nazwa = string.Empty,
                Tresc = string.Empty,
                Pozycja = nextPozycja
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdStrony,TytulLinku,Nazwa,Tresc,Pozycja")] Strona strona)
        {
            Normalize(strona);
            await ValidateUniqueTytulLinkuAsync(strona.TytulLinku);

            if (!ModelState.IsValid)
            {
                return View(strona);
            }

            try
            {
                await ShiftPositionsForInsertAsync(strona.Pozycja);
                _context.Add(strona);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(nameof(Strona.TytulLinku), $"Strona o tytule odnośnika '{strona.TytulLinku}' już istnieje.");
                return View(strona);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var strona = await _context.Strona.FindAsync(id);
            if (strona == null)
            {
                return NotFound();
            }

            return View(strona);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdStrony,TytulLinku,Nazwa,Tresc,Pozycja")] Strona strona)
        {
            if (id != strona.IdStrony)
            {
                return NotFound();
            }

            Normalize(strona);
            await ValidateUniqueTytulLinkuAsync(strona.TytulLinku, strona.IdStrony);

            if (!ModelState.IsValid)
            {
                return View(strona);
            }

            var existing = await _context.Strona.FirstOrDefaultAsync(x => x.IdStrony == id);
            if (existing == null)
            {
                return NotFound();
            }

            existing.TytulLinku = strona.TytulLinku;
            existing.Nazwa = strona.Nazwa;
            existing.Tresc = strona.Tresc;
            await ShiftPositionsForMoveAsync(existing.IdStrony, existing.Pozycja, strona.Pozycja);
            existing.Pozycja = strona.Pozycja;

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Strona.AnyAsync(e => e.IdStrony == strona.IdStrony))
                {
                    return NotFound();
                }

                throw;
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(nameof(Strona.TytulLinku), $"Strona o tytule odnośnika '{strona.TytulLinku}' już istnieje.");
                return View(strona);
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dto = await _stronaService.GetDeleteDataAsync(id.Value);
            if (dto == null)
            {
                return NotFound();
            }

            return View(dto);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var strona = await _context.Strona.FindAsync(id);
            if (strona == null)
            {
                return RedirectToAction(nameof(Index));
            }

            _context.Strona.Remove(strona);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private static void Normalize(Strona strona)
        {
            strona.TytulLinku = (strona.TytulLinku ?? string.Empty).Trim();
            strona.Nazwa = (strona.Nazwa ?? string.Empty).Trim();
            strona.Tresc = (strona.Tresc ?? string.Empty).Trim();
        }

        private async Task ValidateUniqueTytulLinkuAsync(string tytulLinku, int? excludeId = null)
        {
            if (await _stronaService.TytulLinkuExistsAsync(tytulLinku, excludeId))
            {
                ModelState.AddModelError(nameof(Strona.TytulLinku), $"Strona o tytule odnośnika '{(tytulLinku ?? string.Empty).Trim()}' już istnieje.");
            }
        }

        private async Task<int> GetNextPozycjaAsync()
        {
            var lastPozycja = await _context.Strona
                .AsNoTracking()
                .Select(x => (int?)x.Pozycja)
                .MaxAsync();

            return (lastPozycja ?? 0) + 1;
        }

        private async Task ShiftPositionsForInsertAsync(int targetPozycja)
        {
            var pagesToShift = await _context.Strona
                .Where(x => x.Pozycja >= targetPozycja)
                .ToListAsync();

            foreach (var page in pagesToShift)
            {
                page.Pozycja++;
            }
        }

        private async Task ShiftPositionsForMoveAsync(int pageId, int oldPozycja, int newPozycja)
        {
            if (oldPozycja == newPozycja)
            {
                return;
            }

            if (newPozycja < oldPozycja)
            {
                var pagesToShiftUp = await _context.Strona
                    .Where(x => x.IdStrony != pageId && x.Pozycja >= newPozycja && x.Pozycja < oldPozycja)
                    .ToListAsync();

                foreach (var page in pagesToShiftUp)
                {
                    page.Pozycja++;
                }

                return;
            }

            var pagesToShiftDown = await _context.Strona
                .Where(x => x.IdStrony != pageId && x.Pozycja > oldPozycja && x.Pozycja <= newPozycja)
                .ToListAsync();

            foreach (var page in pagesToShiftDown)
            {
                page.Pozycja--;
            }
        }
    }
}
