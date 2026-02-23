using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Data.Data;
using Data.Data.Magazyn;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        var user = await _context.Uzytkownik
            .Include(u => u.RoleUzytkownika)
            .ThenInclude(ur => ur.Rola)
            .AsNoTracking()
            .FirstOrDefaultAsync(u =>
                u.CzyAktywny &&
                (u.Login == lookup || u.Email == lookup));

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

    [HttpGet("me")]
    [Authorize]
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

    private static bool VerifyPassword(Uzytkownik user, string providedPassword)
    {
        // Obsługa hashy Identity + fallback do plain text na czas migracji starego modelu.
        // Docelowo usuń fallback plain text i trzymaj jeden algorytm hashowania.
        var identityHasher = new PasswordHasher<Uzytkownik>();
        var result = identityHasher.VerifyHashedPassword(user, user.HashHasla, providedPassword);
        if (result != PasswordVerificationResult.Failed)
        {
            return true;
        }

        return FixedTimeEquals(user.HashHasla, providedPassword);
    }

    private static bool FixedTimeEquals(string a, string b)
    {
        var left = Encoding.UTF8.GetBytes(a);
        var right = Encoding.UTF8.GetBytes(b);
        return CryptographicOperations.FixedTimeEquals(left, right);
    }
}
