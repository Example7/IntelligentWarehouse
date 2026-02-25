using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using IntranetWeb.Controllers.Abstrakcja;

namespace IntranetWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UzytkownikRolaController : BaseSearchController<UzytkownikRola>
    {
        private readonly IUzytkownikRolaService _uzytkownikRolaService;

        public UzytkownikRolaController(DataContext context, IUzytkownikRolaService uzytkownikRolaService) : base(context)
        {
            _uzytkownikRolaService = uzytkownikRolaService;
        }

        // GET: UzytkownikRola
        public async Task<IActionResult> Index(string? searchTerm)
        {
            return View(await _uzytkownikRolaService.GetIndexDataAsync(searchTerm));
        }

        // GET: UzytkownikRola/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var data = await _uzytkownikRolaService.GetDetailsDataAsync(id.Value);
            if (data == null)
            {
                return NotFound();
            }

            return View(data);
        }

        // GET: UzytkownikRola/Create
        public IActionResult Create()
        {
            PopulateSelects();
            return View();
        }

        // POST: UzytkownikRola/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdUzytkownika,IdRoli")] UzytkownikRola uzytkownikRola)
        {
            if (await _context.UzytkownikRola.AnyAsync(x => x.IdUzytkownika == uzytkownikRola.IdUzytkownika && x.IdRoli == uzytkownikRola.IdRoli))
            {
                ModelState.AddModelError(string.Empty, "Takie przypisanie roli do użytkownika już istnieje.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(uzytkownikRola);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError(string.Empty, "Nie udało się zapisać przypisania. Sprawdź, czy przypisanie nie istnieje już w systemie.");
                }
            }
            PopulateSelects(uzytkownikRola.IdUzytkownika, uzytkownikRola.IdRoli);
            return View(uzytkownikRola);
        }

        // GET: UzytkownikRola/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uzytkownikRola = await _context.UzytkownikRola.FindAsync(id);
            if (uzytkownikRola == null)
            {
                return NotFound();
            }
            PopulateSelects(uzytkownikRola.IdUzytkownika, uzytkownikRola.IdRoli);
            return View(uzytkownikRola);
        }

        // POST: UzytkownikRola/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdUzytkownika,IdRoli")] UzytkownikRola uzytkownikRola)
        {
            if (id != uzytkownikRola.Id)
            {
                return NotFound();
            }

            if (await _context.UzytkownikRola.AnyAsync(x =>
                    x.Id != uzytkownikRola.Id &&
                    x.IdUzytkownika == uzytkownikRola.IdUzytkownika &&
                    x.IdRoli == uzytkownikRola.IdRoli))
            {
                ModelState.AddModelError(string.Empty, "Takie przypisanie roli do użytkownika już istnieje.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.UzytkownikRola.FirstOrDefaultAsync(x => x.Id == id);
                    if (existing == null)
                    {
                        return NotFound();
                    }

                    existing.IdUzytkownika = uzytkownikRola.IdUzytkownika;
                    existing.IdRoli = uzytkownikRola.IdRoli;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UzytkownikRolaExists(uzytkownikRola.Id))
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
                    ModelState.AddModelError(string.Empty, "Nie udało się zapisać przypisania. Sprawdź, czy przypisanie nie istnieje już w systemie.");
                    PopulateSelects(uzytkownikRola.IdUzytkownika, uzytkownikRola.IdRoli);
                    return View(uzytkownikRola);
                }
                return RedirectToAction(nameof(Index));
            }
            PopulateSelects(uzytkownikRola.IdUzytkownika, uzytkownikRola.IdRoli);
            return View(uzytkownikRola);
        }

        // GET: UzytkownikRola/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var data = await _uzytkownikRolaService.GetDeleteDataAsync(id.Value);
            if (data == null)
            {
                return NotFound();
            }

            return View(data);
        }

        // POST: UzytkownikRola/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var uzytkownikRola = await _context.UzytkownikRola.FindAsync(id);
            if (uzytkownikRola != null)
            {
                _context.UzytkownikRola.Remove(uzytkownikRola);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UzytkownikRolaExists(int id)
        {
            return _context.UzytkownikRola.Any(e => e.Id == id);
        }

        private void PopulateSelects(int? selectedUserId = null, int? selectedRoleId = null)
        {
            ViewData["IdRoli"] = new SelectList(
                _context.Rola
                    .AsNoTracking()
                    .OrderBy(x => x.Nazwa)
                    .Select(x => new { x.IdRoli, Label = x.Nazwa }),
                "IdRoli",
                "Label",
                selectedRoleId);

            ViewData["IdUzytkownika"] = new SelectList(
                _context.Set<Uzytkownik>()
                    .AsNoTracking()
                    .OrderBy(x => x.Login)
                    .Select(x => new
                    {
                        x.IdUzytkownika,
                        Label = x.Login + " | " + x.Email
                    }),
                "IdUzytkownika",
                "Label",
                selectedUserId);
        }
    }
}
