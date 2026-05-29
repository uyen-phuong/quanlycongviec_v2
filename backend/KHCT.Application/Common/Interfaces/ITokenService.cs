using KHCT.Domain.Entities;

namespace KHCT.Application.Common.Interfaces;

public record AccessToken(string Token, DateTime ExpiresAt);
public record IssuedRefreshToken(string RawToken, string TokenHash, DateTime ExpiresAt);

public interface ITokenService
{
    AccessToken CreateAccessToken(User user, IReadOnlyList<string> roleCodes);
    IssuedRefreshToken CreateRefreshToken();
    string HashRefreshToken(string rawToken);
}
