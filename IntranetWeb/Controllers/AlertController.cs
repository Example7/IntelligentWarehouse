using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;

using IntranetWeb.Controllers.Abstrakcja;

namespace IntranetWeb.Controllers
{
    public class AlertController : BaseSearchController<Alert>
    {
        private readonly IAlertService _alertService;

        public AlertController(DataContext context, IAlertService alertService) : base(context)
        {
            _alertService = alertService;
        }

        // GET: Alert
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var model = await _alertService.GetIndexDataAsync(searchTerm);
            return View(model);
        }

        // GET: Alert/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _alertService.GetDetailsDataAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // GET: Alert/Create
        public IActionResult Create()
        {
            var model = new Alert { Waga = "WARN" };
            UzupelnijDaneFormularza(model);
            return View(model);
        }

        // POST: Alert/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdReguly,IdMagazynu,IdProduktu,Waga,Tresc,CzyPotwierdzony,PotwierdzilUserId,PotwierdzonoUtc")] Alert alert)
        {
            alert.Waga = (alert.Waga ?? string.Empty).Trim().ToUpperInvariant();
            alert.Tresc = (alert.Tresc ?? string.Empty).Trim();
            alert.UtworzonoUtc = DateTime.UtcNow;
            NormalizujPotwierdzenie(alert);

            if (ModelState.IsValid)
            {
                _context.Add(alert);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            UzupelnijDaneFormularza(alert);
            return View(alert);
        }

        // GET: Alert/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alert = await _context.Alert.FindAsync(id);
            if (alert == null)
            {
                return NotFound();
            }

            // Pokazujemy lokalny czas w formularzu.
            if (alert.PotwierdzonoUtc.HasValue)
            {
                alert.PotwierdzonoUtc = DateTime.SpecifyKind(alert.PotwierdzonoUtc.Value, DateTimeKind.Utc).ToLocalTime();
            }

            UzupelnijDaneFormularza(alert);
            return View(alert);
        }

        // POST: Alert/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,IdReguly,IdMagazynu,IdProduktu,Waga,Tresc,CzyPotwierdzony,PotwierdzilUserId,PotwierdzonoUtc")] Alert alert)
        {
            if (id != alert.Id)
            {
                return NotFound();
            }

            alert.Waga = (alert.Waga ?? string.Empty).Trim().ToUpperInvariant();
            alert.Tresc = (alert.Tresc ?? string.Empty).Trim();
            NormalizujPotwierdzenie(alert);

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Alert.FirstOrDefaultAsync(a => a.Id == id);
                    if (existing == null)
                    {
                        return NotFound();
                    }

                    existing.IdReguly = alert.IdReguly;
                    existing.IdMagazynu = alert.IdMagazynu;
                    existing.IdProduktu = alert.IdProduktu;
                    existing.Waga = alert.Waga;
                    existing.Tresc = alert.Tresc;
                    existing.CzyPotwierdzony = alert.CzyPotwierdzony;
                    existing.PotwierdzilUserId = alert.PotwierdzilUserId;
                    existing.PotwierdzonoUtc = alert.PotwierdzonoUtc;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlertExists(alert.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            UzupelnijDaneFormularza(alert);
            return View(alert);
        }

        // GET: Alert/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _alertService.GetDeleteDataAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // POST: Alert/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var alert = await _context.Alert.FindAsync(id);
            if (alert != null)
            {
                _context.Alert.Remove(alert);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AlertExists(long id)
        {
            return _context.Alert.Any(e => e.Id == id);
        }

        private void UzupelnijDaneFormularza(Alert model)
        {
            ViewData["IdMagazynu"] = new SelectList(
                _context.Magazyn.AsNoTracking().OrderBy(m => m.Nazwa).ToList(),
                "IdMagazynu", "Nazwa", model.IdMagazynu);

            var produkty = _context.Produkt.AsNoTracking()
                .Include(p => p.DomyslnaJednostka)
                .OrderBy(p => p.Kod)
                .ThenBy(p => p.Nazwa)
                .Select(p => new
                {
                    p.IdProduktu,
                    Label = $"{p.Kod} - {p.Nazwa} ({(p.DomyslnaJednostka != null ? p.DomyslnaJednostka.Kod : "j.m.")})"
                })
                .ToList();
            ViewData["IdProduktu"] = new SelectList(produkty, "IdProduktu", "Label", model.IdProduktu);

            var reguly = _context.RegulaAlertu.AsNoTracking()
                .Include(r => r.Magazyn)
                .Include(r => r.Produkt)
                .OrderBy(r => r.Typ)
                .ThenBy(r => r.Magazyn.Nazwa)
                .Select(r => new
                {
                    r.Id,
                    Label = $"{r.Typ} | {r.Magazyn.Nazwa} | {(r.Produkt != null ? r.Produkt.Kod : "ogólna")}" 
                })
                .ToList();
            ViewData["IdReguly"] = new SelectList(reguly, "Id", "Label", model.IdReguly);

            ViewData["Severities"] = new SelectList(new[] { "INFO", "WARN", "CRIT" }, model.Waga);

            var users = _context.Uzytkownik.AsNoTracking()
                .OrderBy(u => u.Email)
                .Select(u => new { u.IdUzytkownika, u.Email })
                .ToList();
            var userItems = users.Select(u => new SelectListItem
            {
                Value = u.IdUzytkownika.ToString(),
                Text = u.Email,
                Selected = model.PotwierdzilUserId == u.IdUzytkownika
            }).ToList();
            userItems.Insert(0, new SelectListItem { Value = string.Empty, Text = "(brak)", Selected = model.PotwierdzilUserId == null });
            ViewData["PotwierdzilUserId"] = userItems;
        }

        private static void NormalizujPotwierdzenie(Alert alert)
        {
            if (!alert.CzyPotwierdzony)
            {
                alert.PotwierdzilUserId = null;
                alert.PotwierdzonoUtc = null;
                return;
            }

            if (alert.PotwierdzonoUtc.HasValue)
            {
                alert.PotwierdzonoUtc = DateTime.SpecifyKind(alert.PotwierdzonoUtc.Value, DateTimeKind.Local).ToUniversalTime();
            }
            else
            {
                alert.PotwierdzonoUtc = DateTime.UtcNow;
            }
        }
    }
}
