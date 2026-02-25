using Data.Data;
using Data.Data.CMS;
using IntranetWeb.Controllers.Abstrakcja;
using Interfaces.CMS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IntranetWeb.Controllers
{
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

        public IActionResult Create()
        {
            return View(new Aktualnosc { TytulLinku = string.Empty, Nazwa = string.Empty, Tresc = string.Empty });
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
    }
}
