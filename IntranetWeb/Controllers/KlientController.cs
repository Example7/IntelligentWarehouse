using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;
using IntranetWeb.Controllers.Abstrakcja;
using IntranetWeb.Models.Klient;
using IntranetWeb.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IntranetWeb.Controllers
{
    [Authorize(Roles = AppRoles.AdminMagazynierOperator)]
    public class KlientController : BaseSearchController<Klient>
    {
        private readonly IKlientService _klientService;

        public KlientController(DataContext context, IKlientService klientService) : base(context)
        {
            _klientService = klientService;
        }

        public async Task<IActionResult> Index(string? searchTerm)
        {
            var model = await _klientService.GetIndexDataAsync(searchTerm);
            return View(model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var details = await _klientService.GetDetailsDataAsync(id.Value);
            if (details == null)
            {
                return NotFound();
            }

            var pageModel = new KlientDetailsPageViewModel
            {
                Details = details
            };

            await FillLinkedAccountRoleInfoAsync(pageModel, details.Klient.IdUzytkownika);
            return View(pageModel);
        }

        public IActionResult Create()
        {
            return View(new KlientCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KlientCreateViewModel model)
        {
            model.Nazwa = (model.Nazwa ?? string.Empty).Trim();
            model.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
            model.Telefon = string.IsNullOrWhiteSpace(model.Telefon) ? null : model.Telefon.Trim();
            model.Adres = string.IsNullOrWhiteSpace(model.Adres) ? null : model.Adres.Trim();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var klient = new Klient
                {
                    Nazwa = model.Nazwa,
                    Email = model.Email,
                    Telefon = model.Telefon,
                    Adres = model.Adres,
                    CzyAktywny = model.CzyAktywny,
                    UtworzonoUtc = DateTime.UtcNow
                };

                _context.Klient.Add(klient);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Dodano klienta.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Nie udało się zapisać klienta. Sprawdź dane i spróbuj ponownie.");
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var klient = await _context.Klient.FindAsync(id);
            if (klient == null)
            {
                return NotFound();
            }

            return View(klient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdKlienta,Nazwa,Email,Telefon,Adres,CzyAktywny,RowVersion")] Klient klient)
        {
            if (id != klient.IdKlienta)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(klient);
            }

            try
            {
                var existing = await _context.Klient.FirstOrDefaultAsync(x => x.IdKlienta == id);
                if (existing == null)
                {
                    return NotFound();
                }

                existing.Nazwa = klient.Nazwa;
                existing.Email = klient.Email;
                existing.Telefon = klient.Telefon;
                existing.Adres = klient.Adres;
                existing.CzyAktywny = klient.CzyAktywny;

                _context.Entry(existing).Property(x => x.RowVersion).OriginalValue = klient.RowVersion;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!KlientExists(klient.IdKlienta))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = AppRoles.AdminOnly)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _klientService.GetDeleteDataAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [Authorize(Roles = AppRoles.AdminOnly)]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deleteData = await _klientService.GetDeleteDataAsync(id);
            if (deleteData == null)
            {
                return NotFound();
            }

            if (!deleteData.CzyMoznaUsunac)
            {
                ModelState.AddModelError(string.Empty, $"Nie można usunąć klienta, ponieważ ma powiązane dokumenty WZ: {deleteData.LiczbaDokumentowWz}.");
                return View("Delete", deleteData);
            }

            var klient = await _context.Klient.FindAsync(id);
            if (klient != null)
            {
                _context.Klient.Remove(klient);
            }

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Nie udało się usunąć klienta, ponieważ ma powiązane dokumenty WZ.");
                deleteData = await _klientService.GetDeleteDataAsync(id);
                if (deleteData == null)
                {
                    return NotFound();
                }

                return View("Delete", deleteData);
            }
        }

        private bool KlientExists(int id)
        {
            return _context.Klient.Any(e => e.IdKlienta == id);
        }

        private async Task FillLinkedAccountRoleInfoAsync(KlientDetailsPageViewModel pageModel, int? userId)
        {
            if (!userId.HasValue)
            {
                return;
            }

            var roleNames = await _context.UzytkownikRola
                .AsNoTracking()
                .Where(x => x.IdUzytkownika == userId.Value)
                .Select(x => x.Rola.Nazwa)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            pageModel.RoleKontaMobilnego = roleNames;
            pageModel.CzyKontoMobilneMaRoleKlienta = roleNames.Any(IsClientRoleName);
            pageModel.CzyKontoMobilneMaRolePracownicze = roleNames.Any(IsStaffRoleName);
        }

        private static bool IsClientRoleName(string? roleName)
        {
            return string.Equals(roleName, "Klient", StringComparison.OrdinalIgnoreCase)
                || string.Equals(roleName, "Client", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsStaffRoleName(string? roleName)
        {
            return string.Equals(roleName, "Admin", StringComparison.OrdinalIgnoreCase)
                || string.Equals(roleName, "Magazynier", StringComparison.OrdinalIgnoreCase)
                || string.Equals(roleName, "Operator", StringComparison.OrdinalIgnoreCase);
        }
    }
}