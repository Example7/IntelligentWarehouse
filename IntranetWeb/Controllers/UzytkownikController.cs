using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using IntranetWeb.Controllers.Abstrakcja;

namespace IntranetWeb.Controllers
{
    public class UzytkownikController : BaseSearchController<Uzytkownik>
    {
        private readonly IUzytkownikService _uzytkownikService;

        public UzytkownikController(DataContext context, IUzytkownikService uzytkownikService) : base(context)
        {
            _uzytkownikService = uzytkownikService;
        }

        // GET: Uzytkownik
        public async Task<IActionResult> Index(string? searchTerm)
        {
            return View(await _uzytkownikService.GetIndexDataAsync(searchTerm));
        }

        // GET: Uzytkownik/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var data = await _uzytkownikService.GetDetailsDataAsync(id.Value);
            if (data == null)
            {
                return NotFound();
            }

            return View(data);
        }

        // GET: Uzytkownik/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Uzytkownik/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdUzytkownika,Login,HashHasla,Email,CzyAktywny")] Uzytkownik uzytkownik)
        {
            uzytkownik.Login = (uzytkownik.Login ?? string.Empty).Trim();
            uzytkownik.Email = (uzytkownik.Email ?? string.Empty).Trim();
            uzytkownik.HashHasla = (uzytkownik.HashHasla ?? string.Empty).Trim();

            if (await _context.Uzytkownik.AsNoTracking().AnyAsync(x => x.Login == uzytkownik.Login))
            {
                ModelState.AddModelError(nameof(Uzytkownik.Login), $"Użytkownik o loginie '{uzytkownik.Login}' już istnieje.");
            }
            if (await _context.Uzytkownik.AsNoTracking().AnyAsync(x => x.Email == uzytkownik.Email))
            {
                ModelState.AddModelError(nameof(Uzytkownik.Email), $"Użytkownik o e-mail '{uzytkownik.Email}' już istnieje.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(uzytkownik);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError(string.Empty, "Nie udało się zapisać użytkownika. Sprawdź, czy login i e-mail są unikalne.");
                }
            }
            return View(uzytkownik);
        }

        // GET: Uzytkownik/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uzytkownik = await _context.Uzytkownik.FindAsync(id);
            if (uzytkownik == null)
            {
                return NotFound();
            }
            return View(uzytkownik);
        }

        // POST: Uzytkownik/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdUzytkownika,Login,HashHasla,Email,CzyAktywny")] Uzytkownik uzytkownik)
        {
            if (id != uzytkownik.IdUzytkownika)
            {
                return NotFound();
            }

            uzytkownik.Login = (uzytkownik.Login ?? string.Empty).Trim();
            uzytkownik.Email = (uzytkownik.Email ?? string.Empty).Trim();
            uzytkownik.HashHasla = (uzytkownik.HashHasla ?? string.Empty).Trim();

            if (await _context.Uzytkownik.AsNoTracking().AnyAsync(x => x.IdUzytkownika != id && x.Login == uzytkownik.Login))
            {
                ModelState.AddModelError(nameof(Uzytkownik.Login), $"Użytkownik o loginie '{uzytkownik.Login}' już istnieje.");
            }
            if (await _context.Uzytkownik.AsNoTracking().AnyAsync(x => x.IdUzytkownika != id && x.Email == uzytkownik.Email))
            {
                ModelState.AddModelError(nameof(Uzytkownik.Email), $"Użytkownik o e-mail '{uzytkownik.Email}' już istnieje.");
            }

            if (ModelState.IsValid)
            {
                var existing = await _context.Uzytkownik.FirstOrDefaultAsync(x => x.IdUzytkownika == id);
                if (existing == null)
                {
                    return NotFound();
                }

                try
                {
                    existing.Login = uzytkownik.Login;
                    existing.Email = uzytkownik.Email;
                    existing.HashHasla = uzytkownik.HashHasla;
                    existing.CzyAktywny = uzytkownik.CzyAktywny;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UzytkownikExists(uzytkownik.IdUzytkownika))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError(string.Empty, "Nie udało się zapisać zmian. Sprawdź, czy login i e-mail są unikalne.");
                    return View(uzytkownik);
                }
                return RedirectToAction(nameof(Index));
            }
            return View(uzytkownik);
        }

        // GET: Uzytkownik/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var data = await _uzytkownikService.GetDeleteDataAsync(id.Value);
            if (data == null)
            {
                return NotFound();
            }

            return View(data);
        }

        // POST: Uzytkownik/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deleteData = await _uzytkownikService.GetDeleteDataAsync(id);
            if (deleteData == null)
            {
                return NotFound();
            }

            if (!deleteData.CzyMoznaUsunac)
            {
                ModelState.AddModelError(string.Empty, "Nie można usunąć użytkownika, ponieważ ma powiązane rekordy w systemie.");
                return View("Delete", deleteData);
            }

            var uzytkownik = await _context.Uzytkownik.FindAsync(id);
            if (uzytkownik != null)
            {
                _context.Uzytkownik.Remove(uzytkownik);
            }

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Nie udało się usunąć użytkownika, ponieważ jest powiązany z innymi danymi.");
                var refreshDeleteData = await _uzytkownikService.GetDeleteDataAsync(id);
                return refreshDeleteData == null ? NotFound() : View("Delete", refreshDeleteData);
            }
        }

        private bool UzytkownikExists(int id)
        {
            return _context.Uzytkownik.Any(e => e.IdUzytkownika == id);
        }
    }
}