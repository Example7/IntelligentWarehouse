using System.Security.Claims;
using Data.Data;
using Data.Data.Magazyn;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MobileApi.Models.Auth;
using MobileApi.Services;

namespace MobileApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly DataContext _context;
    private readonly ITokenService _tokenService;

    public AuthController(DataContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.LoginOrEmail) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Login/email i hasło są wymagane.");
        }

        var lookup = request.LoginOrEmail.Trim();
        var userQuery = _context.Uzytkownik
            .Include(u => u.RoleUzytkownika)
            .ThenInclude(ur => ur.Rola)
            .AsNoTracking()
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

        if (user is null || !VerifyPassword(user, request.Password))
        {
            return Unauthorized("Nieprawidłowe dane logowania.");
        }

        var roles = user.RoleUzytkownika
            .Select(x => x.Rola.Nazwa)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var (token, expiresAtUtc) = _tokenService.CreateAccessToken(user, roles);

        return Ok(new LoginResponseDto
        {
            AccessToken = token,
            ExpiresAtUtc = expiresAtUtc,
            UserId = user.IdUzytkownika,
            Login = user.Login,
            Email = user.Email,
            Roles = roles
        });
    }

    [HttpPost("register-client")]
    [AllowAnonymous]
    public async Task<ActionResult<RegisterClientResponseDto>> RegisterClient([FromBody] RegisterClientRequestDto request)
    {
        var name = (request.Name ?? string.Empty).Trim();
        var login = (request.Login ?? string.Empty).Trim();
        var email = (request.Email ?? string.Empty).Trim();
        var phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        var address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest("Nazwa klienta jest wymagana.");
        }

        if (string.IsNullOrWhiteSpace(login))
        {
            return BadRequest("Login jest wymagany.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest("Email jest wymagany.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Hasło jest wymagane.");
        }

        if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
        {
            return BadRequest("Hasło i potwierdzenie hasła muszą być takie same.");
        }

        if (request.Password.Length < 8)
        {
            return BadRequest("Hasło musi mieć co najmniej 8 znaków.");
        }

        if (await _context.Uzytkownik.AsNoTracking().AnyAsync(x => x.Login == login))
        {
            return Conflict($"Login '{login}' jest już zajęty.");
        }

        if (await _context.Uzytkownik.AsNoTracking().AnyAsync(x => x.Email == email))
        {
            return Conflict($"Email '{email}' jest już zajęty.");
        }

        await using IDbContextTransaction tx = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = new Uzytkownik
            {
                Login = login,
                Email = email,
                CzyAktywny = true
            };

            user.HashHasla = HashPassword(user, request.Password);
            _context.Uzytkownik.Add(user);
            await _context.SaveChangesAsync();

            var klientRole = await EnsureClientRoleAsync();
            _context.UzytkownikRola.Add(new UzytkownikRola
            {
                IdUzytkownika = user.IdUzytkownika,
                IdRoli = klientRole.IdRoli
            });

            var klient = new Klient
            {
                Nazwa = name,
                Email = email,
                Telefon = phone,
                Adres = address,
                IdUzytkownika = user.IdUzytkownika,
                CzyAktywny = true,
                UtworzonoUtc = DateTime.UtcNow
            };

            _context.Klient.Add(klient);
            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return Ok(new RegisterClientResponseDto
            {
                UserId = user.IdUzytkownika,
                CustomerId = klient.IdKlienta,
                Message = "Konto zostało utworzone."
            });
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync();
            return BadRequest("Nie udało się utworzyć konta. Sprawdź dane i spróbuj ponownie.");
        }
    }

    [HttpGet("me")]
    [Authorize(Roles = "Client,Klient")]
    public ActionResult<CurrentUserDto> Me()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _ = int.TryParse(userIdValue, out var userId);

        var roles = User.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Ok(new CurrentUserDto
        {
            UserId = userId,
            Login = User.Identity?.Name ?? string.Empty,
            Email = User.FindFirstValue(ClaimTypes.Email),
            Roles = roles
        });
    }

    [HttpPost("change-password")]
    [Authorize(Roles = "Client,Klient")]
    public async Task<ActionResult<ChangePasswordResponseDto>> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword) ||
            string.IsNullOrWhiteSpace(request.NewPassword) ||
            string.IsNullOrWhiteSpace(request.ConfirmNewPassword))
        {
            return BadRequest("Wszystkie pola hasła są wymagane.");
        }

        if (!string.Equals(request.NewPassword, request.ConfirmNewPassword, StringComparison.Ordinal))
        {
            return BadRequest("Nowe hasło i potwierdzenie muszą być takie same.");
        }

        if (request.NewPassword.Length < 8)
        {
            return BadRequest("Nowe hasło musi mieć co najmniej 8 znaków.");
        }

        if (string.Equals(request.CurrentPassword, request.NewPassword, StringComparison.Ordinal))
        {
            return BadRequest("Nowe hasło musi być inne niż obecne.");
        }

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdValue, out var userId) || userId <= 0)
        {
            return Unauthorized("Nie można ustalić użytkownika.");
        }

        var user = await _context.Uzytkownik.FirstOrDefaultAsync(u => u.IdUzytkownika == userId && u.CzyAktywny);
        if (user is null)
        {
            return Unauthorized("Użytkownik nie istnieje lub jest nieaktywny.");
        }

        if (!VerifyPassword(user, request.CurrentPassword))
        {
            return BadRequest("Obecne hasło jest nieprawidłowe.");
        }

        user.HashHasla = HashPassword(user, request.NewPassword);
        await _context.SaveChangesAsync();

        return Ok(new ChangePasswordResponseDto
        {
            Message = "Hasło zostało zmienione."
        });
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

    private static string HashPassword(Uzytkownik user, string password)
    {
        var identityHasher = new PasswordHasher<Uzytkownik>();
        return identityHasher.HashPassword(user, password);
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
}
