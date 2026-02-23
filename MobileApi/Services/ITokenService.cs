using Data.Data.Magazyn;

namespace MobileApi.Services;

public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateAccessToken(Uzytkownik user, IReadOnlyCollection<string> roles);
}
