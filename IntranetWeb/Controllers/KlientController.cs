using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;

using IntranetWeb.Controllers.Abstrakcja;

using IntranetWeb.Security;
using IntranetWeb.Models.Klient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;

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

        // GET: Klient
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var model = await _klientService.GetIndexDataAsync(searchTerm);
            return View(model);
        }

        // GET: Klient/Details/5
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
                Details = details,
                KontoMobilne = new UtworzKontoKlientaViewModel
                {
                    KlientId = details.Klient.IdKlienta,
                    Login = BuildSuggestedLogin(details.Klient),
                    Email = details.Klient.Email ?? string.Empty,
                    IsActive = details.Klient.CzyAktywny
                }
            };

            await FillLinkedAccountRoleInfoAsync(pageModel, details.Klient.IdUzytkownika);
            return View(pageModel);
        }

        [Authorize(Roles = AppRoles.AdminOnly)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UtworzKontoMobilne([Bind(Prefix = "KontoMobilne")] UtworzKontoKlientaViewModel model)
        {
            var details = await _klientService.GetDetailsDataAsync(model.KlientId);
            if (details == null)
            {
                return NotFound();
            }

            var klient = await _context.Klient
                .Include(x => x.Uzytkownik)
                .FirstOrDefaultAsync(x => x.IdKlienta == model.KlientId);
            if (klient == null)
            {
                return NotFound();
            }

            if (klient.IdUzytkownika.HasValue)
            {
                TempData["ErrorMessage"] = "Ten klient ma już przypisane konto mobilne.";
                return RedirectToAction(nameof(Details), new { id = model.KlientId });
            }

            model.Login = (model.Login ?? string.Empty).Trim();
            model.Email = (model.Email ?? string.Empty).Trim();

            if (await _context.Uzytkownik.AsNoTracking().AnyAsync(x => x.Login == model.Login))
            {
                ModelState.AddModelError(nameof(UtworzKontoKlientaViewModel.Login), $"Login '{model.Login}' jest już zajęty.");
            }

            if (await _context.Uzytkownik.AsNoTracking().AnyAsync(x => x.Email == model.Email))
            {
                ModelState.AddModelError(nameof(UtworzKontoKlientaViewModel.Email), $"Email '{model.Email}' jest już zajęty.");
            }

            if (!ModelState.IsValid)
            {
                var invalidModel = new KlientDetailsPageViewModel
                {
                    Details = details,
                    KontoMobilne = model
                };
                await FillLinkedAccountRoleInfoAsync(invalidModel, details.Klient.IdUzytkownika);
                return View("Details", invalidModel);
            }

            await using IDbContextTransaction tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = new Uzytkownik
                {
                    Login = model.Login,
                    Email = model.Email,
                    CzyAktywny = model.IsActive
                };

                var hasher = new PasswordHasher<Uzytkownik>();
                user.HashHasla = hasher.HashPassword(user, model.Password);

                _context.Uzytkownik.Add(user);
                await _context.SaveChangesAsync();

                var klientRole = await EnsureClientRoleAsync();
                if (!await _context.UzytkownikRola.AnyAsync(x => x.IdUzytkownika == user.IdUzytkownika && x.IdRoli == klientRole.IdRoli))
                {
                    _context.UzytkownikRola.Add(new UzytkownikRola
                    {
                        IdUzytkownika = user.IdUzytkownika,
                        IdRoli = klientRole.IdRoli
                    });
                }

                klient.IdUzytkownika = user.IdUzytkownika;
                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                TempData["SuccessMessage"] = $"Utworzono konto mobilne '{user.Login}' i przypisano je do klienta.";
                return RedirectToAction(nameof(Details), new { id = model.KlientId });
            }
            catch (DbUpdateException)
            {
                await tx.RollbackAsync();
                ModelState.AddModelError(string.Empty, "Nie udało się utworzyć konta mobilnego. Sprawdź dane i spróbuj ponownie.");

                var failedModel = new KlientDetailsPageViewModel
                {
                    Details = details,
                    KontoMobilne = model
                };
                await FillLinkedAccountRoleInfoAsync(failedModel, details.Klient.IdUzytkownika);
                return View("Details", failedModel);
            }
        }

        [Authorize(Roles = AppRoles.AdminOnly)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OdlaczKontoMobilne(int id)
        {
            var klient = await _context.Klient
                .Include(k => k.Uzytkownik)
                .FirstOrDefaultAsync(k => k.IdKlienta == id);
            if (klient == null)
            {
                return NotFound();
            }

            if (!klient.IdUzytkownika.HasValue)
            {
                TempData["ErrorMessage"] = "Ten klient nie ma przypisanego konta mobilnego.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var detachedLogin = klient.Uzytkownik?.Login ?? klient.IdUzytkownika.Value.ToString();
            klient.IdUzytkownika = null;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Odłączono konto '{detachedLogin}' od klienta.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Klient/Create
        public IActionResult Create()
        {
            return View(new KlientCreateViewModel());
        }

        // POST: Klient/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KlientCreateViewModel model)
        {
            model.Nazwa = (model.Nazwa ?? string.Empty).Trim();
            model.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
            model.Telefon = string.IsNullOrWhiteSpace(model.Telefon) ? null : model.Telefon.Trim();
            model.Adres = string.IsNullOrWhiteSpace(model.Adres) ? null : model.Adres.Trim();

            if (model.UtworzKontoMobilne)
            {
                if (!User.IsInRole(AppRoles.Admin))
                {
                    ModelState.AddModelError(nameof(KlientCreateViewModel.UtworzKontoMobilne), "Tylko administrator może tworzyć konta mobilne.");
                }

                model.LoginMobilny = (model.LoginMobilny ?? string.Empty).Trim();
                model.EmailMobilny = (model.EmailMobilny ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(model.LoginMobilny))
                {
                    ModelState.AddModelError(nameof(KlientCreateViewModel.LoginMobilny), "Login konta mobilnego jest wymagany.");
                }

                if (string.IsNullOrWhiteSpace(model.EmailMobilny))
                {
                    ModelState.AddModelError(nameof(KlientCreateViewModel.EmailMobilny), "Email konta mobilnego jest wymagany.");
                }

                if (string.IsNullOrWhiteSpace(model.HasloMobilne))
                {
                    ModelState.AddModelError(nameof(KlientCreateViewModel.HasloMobilne), "Hasło tymczasowe jest wymagane.");
                }

                if (!string.Equals(model.HasloMobilne, model.PotwierdzHasloMobilne, StringComparison.Ordinal))
                {
                    ModelState.AddModelError(nameof(KlientCreateViewModel.PotwierdzHasloMobilne), "Hasła muszą być takie same.");
                }

                if (!string.IsNullOrWhiteSpace(model.LoginMobilny) &&
                    await _context.Uzytkownik.AsNoTracking().AnyAsync(x => x.Login == model.LoginMobilny))
                {
                    ModelState.AddModelError(nameof(KlientCreateViewModel.LoginMobilny), $"Login '{model.LoginMobilny}' jest już zajęty.");
                }

                if (!string.IsNullOrWhiteSpace(model.EmailMobilny) &&
                    await _context.Uzytkownik.AsNoTracking().AnyAsync(x => x.Email == model.EmailMobilny))
                {
                    ModelState.AddModelError(nameof(KlientCreateViewModel.EmailMobilny), $"Email '{model.EmailMobilny}' jest już zajęty.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await using IDbContextTransaction tx = await _context.Database.BeginTransactionAsync();
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

                if (model.UtworzKontoMobilne)
                {
                    var user = new Uzytkownik
                    {
                        Login = model.LoginMobilny!,
                        Email = model.EmailMobilny!,
                        CzyAktywny = model.CzyAktywneKontoMobilne
                    };

                    var hasher = new PasswordHasher<Uzytkownik>();
                    user.HashHasla = hasher.HashPassword(user, model.HasloMobilne!);
                    _context.Uzytkownik.Add(user);
                    await _context.SaveChangesAsync();

                    var klientRole = await EnsureClientRoleAsync();
                    _context.UzytkownikRola.Add(new UzytkownikRola
                    {
                        IdUzytkownika = user.IdUzytkownika,
                        IdRoli = klientRole.IdRoli
                    });

                    klient.IdUzytkownika = user.IdUzytkownika;
                    await _context.SaveChangesAsync();
                }

                await tx.CommitAsync();
                TempData["SuccessMessage"] = model.UtworzKontoMobilne
                    ? "Dodano klienta i utworzono konto mobilne."
                    : "Dodano klienta.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                await tx.RollbackAsync();
                ModelState.AddModelError(string.Empty, "Nie udało się zapisać klienta. Sprawdź dane i spróbuj ponownie.");
                return View(model);
            }
        }

        // GET: Klient/Edit/5
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

        // POST: Klient/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdKlienta,Nazwa,Email,Telefon,Adres,CzyAktywny,RowVersion")] Klient klient)
        {
            if (id != klient.IdKlienta)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
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
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(klient);
        }

        // GET: Klient/Delete/5
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

        // POST: Klient/Delete/5
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

        private async Task<Rola> EnsureClientRoleAsync()
        {
            var existingRole = await _context.Rola
                .FirstOrDefaultAsync(r => r.Nazwa == "Klient" || r.Nazwa == "Client");
            if (existingRole != null)
            {
                return existingRole;
            }

            var role = new Rola { Nazwa = "Klient" };
            _context.Rola.Add(role);
            await _context.SaveChangesAsync();
            return role;
        }

        private static string BuildSuggestedLogin(Klient klient)
        {
            var baseLogin = !string.IsNullOrWhiteSpace(klient.Email)
                ? klient.Email.Split('@', StringSplitOptions.RemoveEmptyEntries)[0]
                : klient.Nazwa;

            if (string.IsNullOrWhiteSpace(baseLogin))
            {
                baseLogin = "klient";
            }

            var sanitized = new string(baseLogin
                .Trim()
                .Select(ch => char.IsLetterOrDigit(ch) || ch is '.' or '_' or '-' ? ch : '.')
                .ToArray());

            while (sanitized.Contains("..", StringComparison.Ordinal))
            {
                sanitized = sanitized.Replace("..", ".", StringComparison.Ordinal);
            }

            sanitized = sanitized.Trim('.');
            if (string.IsNullOrWhiteSpace(sanitized))
            {
                sanitized = "klient";
            }

            return $"{sanitized}.c{klient.IdKlienta}";
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