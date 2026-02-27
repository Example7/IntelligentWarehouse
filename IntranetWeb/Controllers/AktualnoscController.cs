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
    public class AktualnoscController : BaseSearchController<Aktualnosc>
    {
        private readonly IAktualnoscService _aktualnoscService;

        public AktualnoscController(DataContext context, IAktualnoscService aktualnoscService) : base(context)
        {
            _aktualnoscService = aktualnoscService;
        }

        public async Task<IActionResult> Index(string? searchTerm)
        {
            return View(await _aktualnoscService.GetIndexDataAsync(searchTerm));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dto = await _aktualnoscService.GetDetailsDataAsync(id.Value);
            if (dto == null)
            {
                return NotFound();
            }

            return View(dto);
        }

        public async Task<IActionResult> Create()
        {
            var nextPozycja = await GetNextPozycjaAsync();
            return View(new Aktualnosc
            {
                TytulLinku = string.Empty,
                Nazwa = string.Empty,
                Tresc = string.Empty,
                Pozycja = nextPozycja
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdAktualnosci,TytulLinku,Nazwa,Tresc,Pozycja")] Aktualnosc aktualnosc)
        {
            Normalize(aktualnosc);
            await ValidateUniqueTytulLinkuAsync(aktualnosc.TytulLinku);

            if (!ModelState.IsValid)
            {
                return View(aktualnosc);
            }

            try
            {
                await ShiftPositionsForInsertAsync(aktualnosc.Pozycja);
                _context.Add(aktualnosc);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(nameof(Aktualnosc.TytulLinku), $"Aktualność o tytule odnośnika '{aktualnosc.TytulLinku}' już istnieje.");
                return View(aktualnosc);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aktualnosc = await _context.Aktualnosc.FindAsync(id);
            if (aktualnosc == null)
            {
                return NotFound();
            }

            return View(aktualnosc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdAktualnosci,TytulLinku,Nazwa,Tresc,Pozycja")] Aktualnosc aktualnosc)
        {
            if (id != aktualnosc.IdAktualnosci)
            {
                return NotFound();
            }

            Normalize(aktualnosc);
            await ValidateUniqueTytulLinkuAsync(aktualnosc.TytulLinku, aktualnosc.IdAktualnosci);

            if (!ModelState.IsValid)
            {
                return View(aktualnosc);
            }

            var existing = await _context.Aktualnosc.FirstOrDefaultAsync(x => x.IdAktualnosci == id);
            if (existing == null)
            {
                return NotFound();
            }

            existing.TytulLinku = aktualnosc.TytulLinku;
            existing.Nazwa = aktualnosc.Nazwa;
            existing.Tresc = aktualnosc.Tresc;
            await ShiftPositionsForMoveAsync(existing.IdAktualnosci, existing.Pozycja, aktualnosc.Pozycja);
            existing.Pozycja = aktualnosc.Pozycja;

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Aktualnosc.AnyAsync(e => e.IdAktualnosci == aktualnosc.IdAktualnosci))
                {
                    return NotFound();
                }

                throw;
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(nameof(Aktualnosc.TytulLinku), $"Aktualność o tytule odnośnika '{aktualnosc.TytulLinku}' już istnieje.");
                return View(aktualnosc);
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dto = await _aktualnoscService.GetDeleteDataAsync(id.Value);
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
            var aktualnosc = await _context.Aktualnosc.FindAsync(id);
            if (aktualnosc == null)
            {
                return RedirectToAction(nameof(Index));
            }

            _context.Aktualnosc.Remove(aktualnosc);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private static void Normalize(Aktualnosc aktualnosc)
        {
            aktualnosc.TytulLinku = (aktualnosc.TytulLinku ?? string.Empty).Trim();
            aktualnosc.Nazwa = (aktualnosc.Nazwa ?? string.Empty).Trim();
            aktualnosc.Tresc = (aktualnosc.Tresc ?? string.Empty).Trim();
        }

        private async Task ValidateUniqueTytulLinkuAsync(string tytulLinku, int? excludeId = null)
        {
            if (await _aktualnoscService.TytulLinkuExistsAsync(tytulLinku, excludeId))
            {
                ModelState.AddModelError(nameof(Aktualnosc.TytulLinku), $"Aktualność o tytule odnośnika '{(tytulLinku ?? string.Empty).Trim()}' już istnieje.");
            }
        }
        private async Task<int> GetNextPozycjaAsync()
        {
            var lastPozycja = await _context.Aktualnosc
                .AsNoTracking()
                .Select(x => (int?)x.Pozycja)
                .MaxAsync();

            return (lastPozycja ?? 0) + 1;
        }

        private async Task ShiftPositionsForInsertAsync(int targetPozycja)
        {
            var itemsToShift = await _context.Aktualnosc
                .Where(x => x.Pozycja >= targetPozycja)
                .ToListAsync();

            foreach (var item in itemsToShift)
            {
                item.Pozycja++;
            }
        }

        private async Task ShiftPositionsForMoveAsync(int itemId, int oldPozycja, int newPozycja)
        {
            if (oldPozycja == newPozycja)
            {
                return;
            }

            if (newPozycja < oldPozycja)
            {
                var itemsToShiftUp = await _context.Aktualnosc
                    .Where(x => x.IdAktualnosci != itemId && x.Pozycja >= newPozycja && x.Pozycja < oldPozycja)
                    .ToListAsync();

                foreach (var item in itemsToShiftUp)
                {
                    item.Pozycja++;
                }

                return;
            }

            var itemsToShiftDown = await _context.Aktualnosc
                .Where(x => x.IdAktualnosci != itemId && x.Pozycja > oldPozycja && x.Pozycja <= newPozycja)
                .ToListAsync();

            foreach (var item in itemsToShiftDown)
            {
                item.Pozycja--;
            }
        }
    }
}


