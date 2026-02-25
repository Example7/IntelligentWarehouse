using System.Security.Claims;
using Data.Data;
using Data.Data.Magazyn;
using IntranetWeb.Models.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IntranetWeb.Controllers;

public class KontoController : Controller
{
    private readonly DataContext _context;

    public KontoController(DataContext context)
    {
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToLocal(returnUrl);
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var lookup = model.LoginOrEmail.Trim();
        var userQuery = _context.Uzytkownik
            .Include(u => u.RoleUzytkownika)
            .ThenInclude(ur => ur.Rola)
            .Where(u => u.CzyAktywny);

        var user = await userQuery
            .FirstOrDefaultAsync(u => u.Login == lookup || u.Email == lookup);

        if (user is null)
        {
            var activeUsers = await userQuery.ToListAsync();
            user = activeUsers.FirstOrDefault(u =>
                string.Equals((u.Login ?? string.Empty).Trim(), lookup, StringComparison.OrdinalIgnoreCase) ||
                string.Equals((u.Email ?? string.Empty).Trim(), lookup, StringComparison.OrdinalIgnoreCase));
        }

        if (user is null || !VerifyPassword(user, model.Password))
        {
            ModelState.AddModelError(string.Empty, "Nieprawidłowe dane logowania.");
            return View(model);
        }

        var roleNames = user.RoleUzytkownika
            .Select(x => x.Rola?.Nazwa)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Cast<string>()
            .ToList();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.IdUzytkownika.ToString()),
            new(ClaimTypes.Name, user.Login),
            new(ClaimTypes.Email, user.Email),
            new("display_name", string.IsNullOrWhiteSpace(user.Email) ? user.Login : user.Email)
        };

        claims.AddRange(roleNames.Select(role => new Claim(ClaimTypes.Role, role)));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(model.RememberMe ? 24 : 8)
            });

        return RedirectToLocal(model.ReturnUrl);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Wyloguj()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [Authorize]
    [HttpGet]
    public IActionResult ZmienHaslo()
    {
        return View(new ChangePasswordViewModel());
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ZmienHaslo(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdValue, out var userId))
        {
            return Challenge();
        }

        var user = await _context.Uzytkownik.FirstOrDefaultAsync(x => x.IdUzytkownika == userId && x.CzyAktywny);
        if (user == null)
        {
            return Challenge();
        }

        if (!VerifyPassword(user, model.CurrentPassword))
        {
            ModelState.AddModelError(nameof(ChangePasswordViewModel.CurrentPassword), "Nieprawidlowe obecne haslo.");
            return View(model);
        }

        if (VerifyPassword(user, model.NewPassword))
        {
            ModelState.AddModelError(nameof(ChangePasswordViewModel.NewPassword), "Nowe haslo musi byc inne niz obecne.");
            return View(model);
        }

        var hasher = new PasswordHasher<Uzytkownik>();
        user.HashHasla = hasher.HashPassword(user, model.NewPassword);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Haslo zostalo zmienione.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult BrakDostepu()
    {
        return View();
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    private static bool VerifyPassword(Uzytkownik user, string providedPassword)
    {
        var identityHasher = new PasswordHasher<Uzytkownik>();
        try
        {
            var result = identityHasher.VerifyHashedPassword(user, user.HashHasla, providedPassword);
            return result != PasswordVerificationResult.Failed;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
