
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;
using Interfaces.Magazyn.Dtos;

using IntranetWeb.Controllers.Abstrakcja;

using IntranetWeb.Security;
using Microsoft.AspNetCore.Authorization;

namespace IntranetWeb.Controllers
{
    [Authorize(Roles = AppRoles.AdminMagazynierOperator)]
    public class PartiaController : BaseSearchController<Partia>
    {
        private readonly IPartiaService _partiaService;

        public PartiaController(DataContext context, IPartiaService partiaService) : base(context)
        {
            _partiaService = partiaService;
        }

        // GET: Partia
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var model = await _partiaService.GetIndexDataAsync(searchTerm);
            return View(model);
        }

        // GET: Partia/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _partiaService.GetDetailsDataAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // GET: Partia/Create
        public async Task<IActionResult> Create()
        {
            await PopulateSelectsAsync(null, null);
            return View();
        }

        // POST: Partia/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdPartii,IdProduktu,NumerPartii,DataProdukcji,DataWaznosci,IdDostawcy")] Partia partia)
        {
            await PopulateSelectsAsync(partia.IdProduktu, partia.IdDostawcy);
            Normalize(partia);
            await ValidatePartiaAsync(partia);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(partia);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    if (await ExistsDuplicateBatchAsync(partia.IdProduktu, partia.NumerPartii, null))
                    {
                        ModelState.AddModelError(nameof(Partia.NumerPartii), $"Partia '{partia.NumerPartii}' dla wybranego produktu już istnieje.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Nie udało się zapisać partii.");
                    }
                }
            }
            return View(partia);
        }

        // GET: Partia/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var partia = await _context.Partia.FindAsync(id);
            if (partia == null)
            {
                return NotFound();
            }
            await PopulateSelectsAsync(partia.IdProduktu, partia.IdDostawcy);
            return View(partia);
        }

        // POST: Partia/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdPartii,IdProduktu,NumerPartii,DataProdukcji,DataWaznosci,IdDostawcy")] Partia partia)
        {
            await PopulateSelectsAsync(partia.IdProduktu, partia.IdDostawcy);
            if (id != partia.IdPartii)
            {
                return NotFound();
            }

            Normalize(partia);
            await ValidatePartiaAsync(partia, partia.IdPartii);

            if (ModelState.IsValid)
            {
                var existing = await _context.Partia.FirstOrDefaultAsync(x => x.IdPartii == id);
                if (existing == null)
                {
                    return NotFound();
                }

                try
                {
                    existing.IdProduktu = partia.IdProduktu;
                    existing.NumerPartii = partia.NumerPartii;
                    existing.DataProdukcji = partia.DataProdukcji;
                    existing.DataWaznosci = partia.DataWaznosci;
                    existing.IdDostawcy = partia.IdDostawcy;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PartiaExists(partia.IdPartii))
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
                    if (await ExistsDuplicateBatchAsync(partia.IdProduktu, partia.NumerPartii, partia.IdPartii))
                    {
                        ModelState.AddModelError(nameof(Partia.NumerPartii), $"Partia '{partia.NumerPartii}' dla wybranego produktu już istnieje.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Nie udało się zapisać zmian partii.");
                    }

                    return View(partia);
                }

                return RedirectToAction(nameof(Index));
            }
            return View(partia);
        }

        // GET: Partia/Delete/5
        [Authorize(Roles = AppRoles.AdminOnly)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _partiaService.GetDeleteDataAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // POST: Partia/Delete/5
        [Authorize(Roles = AppRoles.AdminOnly)]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var partia = await _context.Partia.FindAsync(id);
            if (partia != null)
            {
                _context.Partia.Remove(partia);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PartiaExists(int id)
        {
            return _context.Partia.Any(e => e.IdPartii == id);
        }

        private static void Normalize(Partia partia)
        {
            partia.NumerPartii = (partia.NumerPartii ?? string.Empty).Trim();
        }

        private async Task ValidatePartiaAsync(Partia partia, int? excludeId = null)
        {
            if (partia.DataProdukcji.HasValue && partia.DataWaznosci.HasValue &&
                partia.DataProdukcji.Value.Date > partia.DataWaznosci.Value.Date)
            {
                ModelState.AddModelError(nameof(Partia.DataWaznosci), "Data ważności nie może być wcześniejsza niż data produkcji.");
            }

            if (partia.IdProduktu > 0 && !string.IsNullOrWhiteSpace(partia.NumerPartii) &&
                await ExistsDuplicateBatchAsync(partia.IdProduktu, partia.NumerPartii, excludeId))
            {
                ModelState.AddModelError(nameof(Partia.NumerPartii), $"Partia '{partia.NumerPartii}' dla wybranego produktu już istnieje.");
            }
        }

        private Task<bool> ExistsDuplicateBatchAsync(int idProduktu, string numerPartii, int? excludeId)
        {
            var query = _context.Partia.AsNoTracking()
                .Where(x => x.IdProduktu == idProduktu && x.NumerPartii == numerPartii);

            if (excludeId.HasValue)
            {
                query = query.Where(x => x.IdPartii != excludeId.Value);
            }

            return query.AnyAsync();
        }

        private async Task PopulateSelectsAsync(int? selectedProduktId, int? selectedDostawcaId)
        {
            var produkty = await _context.Produkt
                .AsNoTracking()
                .Include(p => p.DomyslnaJednostka)
                .OrderBy(p => p.Kod)
                .ThenBy(p => p.Nazwa)
                .Select(p => new
                {
                    p.IdProduktu,
                    Label = p.Kod + " - " + p.Nazwa + " (" + (p.DomyslnaJednostka != null ? p.DomyslnaJednostka.Kod : "j.m.") + ")"
                })
                .ToListAsync();

            var dostawcy = await _context.Dostawca
                .AsNoTracking()
                .OrderBy(d => d.Nazwa)
                .Select(d => new { d.IdDostawcy, d.Nazwa })
                .ToListAsync();

            ViewData["IdProduktu"] = new SelectList(produkty, "IdProduktu", "Label", selectedProduktId);

            var dostawcyItems = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "(brak)" }
            };
            dostawcyItems.AddRange(dostawcy.Select(d => new SelectListItem
            {
                Value = d.IdDostawcy.ToString(),
                Text = d.Nazwa,
                Selected = selectedDostawcaId.HasValue && d.IdDostawcy == selectedDostawcaId.Value
            }));

            if (!selectedDostawcaId.HasValue)
            {
                dostawcyItems[0].Selected = true;
            }

            ViewData["IdDostawcy"] = dostawcyItems;
        }
    }
}


