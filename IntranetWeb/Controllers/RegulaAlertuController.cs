
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;

using IntranetWeb.Controllers.Abstrakcja;

using IntranetWeb.Security;
using Microsoft.AspNetCore.Authorization;

namespace IntranetWeb.Controllers
{
    [Authorize(Roles = AppRoles.AdminMagazynier)]
    public class RegulaAlertuController : BaseSearchController<RegulaAlertu>
    {
        private readonly IRegulaAlertuService _regulaAlertuService;

        public RegulaAlertuController(DataContext context, IRegulaAlertuService regulaAlertuService) : base(context)
        {
            _regulaAlertuService = regulaAlertuService;
        }

        // GET: RegulaAlertu
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var model = await _regulaAlertuService.GetIndexDataAsync(searchTerm);
            return View(model);
        }

        // GET: RegulaAlertu/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _regulaAlertuService.GetDetailsDataAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // GET: RegulaAlertu/Create
        public IActionResult Create()
        {
            var model = new RegulaAlertu { Typ = "LowStock", CzyWlaczona = true };
            UzupelnijDaneFormularza(model);
            return View(model);
        }

        // POST: RegulaAlertu/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdMagazynu,IdProduktu,Typ,Prog,CzyWlaczona")] RegulaAlertu regulaAlertu)
        {
            regulaAlertu.Typ = (regulaAlertu.Typ ?? string.Empty).Trim();
            regulaAlertu.UtworzonoUtc = DateTime.UtcNow;

            if (ModelState.IsValid)
            {
                _context.Add(regulaAlertu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            UzupelnijDaneFormularza(regulaAlertu);
            return View(regulaAlertu);
        }

        // GET: RegulaAlertu/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var regulaAlertu = await _context.RegulaAlertu.FindAsync(id);
            if (regulaAlertu == null)
            {
                return NotFound();
            }
            UzupelnijDaneFormularza(regulaAlertu);
            return View(regulaAlertu);
        }

        // POST: RegulaAlertu/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdMagazynu,IdProduktu,Typ,Prog,CzyWlaczona")] RegulaAlertu regulaAlertu)
        {
            if (id != regulaAlertu.Id)
            {
                return NotFound();
            }

            regulaAlertu.Typ = (regulaAlertu.Typ ?? string.Empty).Trim();

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.RegulaAlertu.FirstOrDefaultAsync(r => r.Id == id);
                    if (existing == null)
                    {
                        return NotFound();
                    }

                    existing.IdMagazynu = regulaAlertu.IdMagazynu;
                    existing.IdProduktu = regulaAlertu.IdProduktu;
                    existing.Typ = regulaAlertu.Typ;
                    existing.Prog = regulaAlertu.Prog;
                    existing.CzyWlaczona = regulaAlertu.CzyWlaczona;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RegulaAlertuExists(regulaAlertu.Id))
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
            UzupelnijDaneFormularza(regulaAlertu);
            return View(regulaAlertu);
        }

        // GET: RegulaAlertu/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _regulaAlertuService.GetDeleteDataAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // POST: RegulaAlertu/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deleteData = await _regulaAlertuService.GetDeleteDataAsync(id);
            if (deleteData == null)
            {
                return NotFound();
            }

            if (!deleteData.CzyMoznaUsunac)
            {
                ModelState.AddModelError(string.Empty, "Nie można usunąć reguły alertu, ponieważ posiada powiązane alerty.");
                return View("Delete", deleteData);
            }

            var regulaAlertu = await _context.RegulaAlertu.FindAsync(id);
            if (regulaAlertu != null)
            {
                _context.RegulaAlertu.Remove(regulaAlertu);
            }

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Nie udało się usunąć reguły alertu z powodu powiązanych rekordów.");
                var refreshed = await _regulaAlertuService.GetDeleteDataAsync(id);
                return View("Delete", refreshed ?? deleteData);
            }
        }

        private bool RegulaAlertuExists(int id)
        {
            return _context.RegulaAlertu.Any(e => e.Id == id);
        }

        private void UzupelnijDaneFormularza(RegulaAlertu model)
        {
            ViewData["IdMagazynu"] = new SelectList(
                _context.Magazyn.AsNoTracking().OrderBy(m => m.Nazwa).ToList(),
                "IdMagazynu", "Nazwa", model.IdMagazynu);

            var produkty = _context.Produkt.AsNoTracking()
                .Include(p => p.DomyslnaJednostka)
                .OrderBy(p => p.Kod)
                .ThenBy(p => p.Nazwa)
                .Select(p => new { p.IdProduktu, Label = $"{p.Kod} - {p.Nazwa} ({(p.DomyslnaJednostka != null ? p.DomyslnaJednostka.Kod : "j.m.")})" })
                .ToList();
            var produktyItems = produkty.Select(p => new SelectListItem
            {
                Value = p.IdProduktu.ToString(),
                Text = p.Label,
                Selected = model.IdProduktu == p.IdProduktu
            }).ToList();
            produktyItems.Insert(0, new SelectListItem { Value = string.Empty, Text = "(reguła ogólna dla magazynu)", Selected = model.IdProduktu == null });
            ViewData["IdProduktu"] = produktyItems;

            ViewData["RuleTypes"] = new SelectList(new[]
            {
                "LowStock",
                "ReorderPoint",
                "NoStock",
                "Custom"
            }, model.Typ);
        }
    }
}

