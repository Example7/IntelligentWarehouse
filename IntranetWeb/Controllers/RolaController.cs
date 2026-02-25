using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;
using IntranetWeb.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using IntranetWeb.Controllers.Abstrakcja;

namespace IntranetWeb.Controllers
{
    [Authorize(Roles = AppRoles.AdminOnly)]
    public class RolaController : BaseSearchController<Rola>
    {
        private readonly IRolaService _rolaService;

        public RolaController(DataContext context, IRolaService rolaService) : base(context)
        {
            _rolaService = rolaService;
        }

        // GET: Rola
        public async Task<IActionResult> Index(string? searchTerm)
        {
            return View(await _rolaService.GetIndexDataAsync(searchTerm));
        }

        // GET: Rola/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var data = await _rolaService.GetDetailsDataAsync(id.Value);
            if (data == null)
            {
                return NotFound();
            }

            return View(data);
        }

        // GET: Rola/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Rola/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdRoli,Nazwa")] Rola rola)
        {
            rola.Nazwa = (rola.Nazwa ?? string.Empty).Trim();

            if (await _context.Rola.AnyAsync(x => x.Nazwa == rola.Nazwa))
            {
                ModelState.AddModelError(nameof(Rola.Nazwa), $"Rola o nazwie '{rola.Nazwa}' już istnieje.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(rola);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError(nameof(Rola.Nazwa), $"Nie udało się zapisać roli '{rola.Nazwa}'. Sprawdź, czy nazwa jest unikalna.");
                }
            }
            return View(rola);
        }

        // GET: Rola/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rola = await _context.Rola.FindAsync(id);
            if (rola == null)
            {
                return NotFound();
            }
            return View(rola);
        }

        // POST: Rola/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdRoli,Nazwa")] Rola rola)
        {
            if (id != rola.IdRoli)
            {
                return NotFound();
            }

            rola.Nazwa = (rola.Nazwa ?? string.Empty).Trim();

            if (await _context.Rola.AnyAsync(x => x.IdRoli != rola.IdRoli && x.Nazwa == rola.Nazwa))
            {
                ModelState.AddModelError(nameof(Rola.Nazwa), $"Rola o nazwie '{rola.Nazwa}' już istnieje.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Rola.FirstOrDefaultAsync(x => x.IdRoli == id);
                    if (existing == null)
                    {
                        return NotFound();
                    }

                    existing.Nazwa = rola.Nazwa;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RolaExists(rola.IdRoli))
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
                    ModelState.AddModelError(nameof(Rola.Nazwa), $"Nie udało się zapisać roli '{rola.Nazwa}'. Sprawdź, czy nazwa jest unikalna.");
                    return View(rola);
                }
                return RedirectToAction(nameof(Index));
            }
            return View(rola);
        }

        // GET: Rola/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var data = await _rolaService.GetDeleteDataAsync(id.Value);
            if (data == null)
            {
                return NotFound();
            }

            return View(data);
        }

        // POST: Rola/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deleteData = await _rolaService.GetDeleteDataAsync(id);
            if (deleteData == null)
            {
                return NotFound();
            }

            if (!deleteData.CzyMoznaUsunac)
            {
                ModelState.AddModelError(string.Empty, "Nie można usunąć roli, ponieważ jest przypisana do użytkowników.");
                return View("Delete", deleteData);
            }

            try
            {
                var rola = await _context.Rola.FindAsync(id);
                if (rola != null)
                {
                    _context.Rola.Remove(rola);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Nie udało się usunąć roli. Rola może być nadal powiązana z użytkownikami.");
                var refreshed = await _rolaService.GetDeleteDataAsync(id);
                if (refreshed == null)
                {
                    return NotFound();
                }
                return View("Delete", refreshed);
            }
        }

        private bool RolaExists(int id)
        {
            return _context.Rola.Any(e => e.IdRoli == id);
        }
    }
}
