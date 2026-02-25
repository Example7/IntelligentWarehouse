using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;
using IntranetWeb.Security;
using Microsoft.AspNetCore.Authorization;

using IntranetWeb.Controllers.Abstrakcja;

namespace IntranetWeb.Controllers
{
    [Authorize(Roles = AppRoles.AdminOnly)]
    public class UstawienieAplikacjiController : BaseSearchController<UstawienieAplikacji>
    {
        private readonly IUstawienieAplikacjiService _ustawienieAplikacjiService;

        public UstawienieAplikacjiController(DataContext context, IUstawienieAplikacjiService ustawienieAplikacjiService) : base(context)
        {
            _ustawienieAplikacjiService = ustawienieAplikacjiService;
        }

        // GET: UstawienieAplikacji
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var model = await _ustawienieAplikacjiService.GetIndexDataAsync(searchTerm);
            return View(model);
        }

        // GET: UstawienieAplikacji/Details/KEY
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var model = await _ustawienieAplikacjiService.GetDetailsDataAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // GET: UstawienieAplikacji/Create
        public IActionResult Create()
        {
            return View(new UstawienieAplikacji());
        }

        // POST: UstawienieAplikacji/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Klucz,Wartosc,Opis,ZmienilUserId")] UstawienieAplikacji ustawienieAplikacji)
        {
            Normalizuj(ustawienieAplikacji, isCreate: true);
            ustawienieAplikacji.ZmienionoUtc = DateTime.UtcNow;

            if (await _context.UstawienieAplikacji.AnyAsync(x => x.Klucz == ustawienieAplikacji.Klucz))
            {
                ModelState.AddModelError(nameof(UstawienieAplikacji.Klucz), $"Ustawienie o kluczu '{ustawienieAplikacji.Klucz}' juz istnieje.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(ustawienieAplikacji);
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError(nameof(UstawienieAplikacji.Klucz), $"Ustawienie o kluczu '{ustawienieAplikacji.Klucz}' juz istnieje.");
                }
            }
            return View(ustawienieAplikacji);
        }

        // GET: UstawienieAplikacji/Edit/KEY
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var ustawienieAplikacji = await _context.UstawienieAplikacji.FindAsync(id);
            if (ustawienieAplikacji == null)
            {
                return NotFound();
            }
            return View(ustawienieAplikacji);
        }

        // POST: UstawienieAplikacji/Edit/KEY
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Klucz,Wartosc,Opis,ZmienilUserId")] UstawienieAplikacji ustawienieAplikacji)
        {
            if (id != ustawienieAplikacji.Klucz)
            {
                return NotFound();
            }

            Normalizuj(ustawienieAplikacji, isCreate: false);

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.UstawienieAplikacji.FirstOrDefaultAsync(x => x.Klucz == id);
                    if (existing == null)
                    {
                        return NotFound();
                    }

                    existing.Wartosc = ustawienieAplikacji.Wartosc;
                    existing.Opis = ustawienieAplikacji.Opis;
                    existing.ZmienilUserId = ustawienieAplikacji.ZmienilUserId;
                    existing.ZmienionoUtc = DateTime.UtcNow;

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UstawienieAplikacjiExists(ustawienieAplikacji.Klucz))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }
            return View(ustawienieAplikacji);
        }

        // GET: UstawienieAplikacji/Delete/KEY
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var model = await _ustawienieAplikacjiService.GetDeleteDataAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // POST: UstawienieAplikacji/Delete/KEY
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var ustawienieAplikacji = await _context.UstawienieAplikacji.FindAsync(id);
            if (ustawienieAplikacji != null)
            {
                _context.UstawienieAplikacji.Remove(ustawienieAplikacji);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UstawienieAplikacjiExists(string id)
        {
            return _context.UstawienieAplikacji.Any(e => e.Klucz == id);
        }

        private static void Normalizuj(UstawienieAplikacji model, bool isCreate)
        {
            model.Klucz = (model.Klucz ?? string.Empty).Trim();
            model.Wartosc = (model.Wartosc ?? string.Empty).Trim();
            model.Opis = string.IsNullOrWhiteSpace(model.Opis) ? null : model.Opis.Trim();

            if (isCreate)
            {
                model.Klucz = model.Klucz.ToUpperInvariant();
            }
        }
    }
}
