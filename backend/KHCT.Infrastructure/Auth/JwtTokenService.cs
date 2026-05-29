using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using KHCT.Application.Common.Interfaces;
using KHCT.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace KHCT.Infrastructure.Auth;

public class JwtTokenService : ITokenService
{
    private readonly JwtOptions _opt;
    private readonly SigningCredentials _signing;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _opt = options.Value;
        var keyBytes = Encoding.UTF8.GetBytes(_opt.Key);
        _signing = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);
    }

    public AccessToken CreateAccessToken(User user, IReadOnlyList<string> roleCodes)
    {
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(_opt.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new("username", user.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        if (user.DepartmentId.HasValue)
            claims.Add(new Claim("dept", user.DepartmentId.Value.ToString()));
        foreach (var r in roleCodes)
            claims.Add(new Claim(ClaimTypes.Role, r));

        var token = new JwtSecurityToken(
            issuer: _opt.Issuer,
            audience: _opt.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: _signing);

        var serialized = new JwtSecurityTokenHandler().WriteToken(token);
        return new AccessToken(serialized, expires);
    }

    public IssuedRefreshToken CreateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        var raw = Base64UrlEncode(bytes);
        var hash = HashRefreshToken(raw);
        var expires = DateTime.UtcNow.AddDays(_opt.RefreshTokenDays);
        return new IssuedRefreshToken(raw, hash, expires);
    }

    public string HashRefreshToken(string rawToken)
    {
        var bytes = Encoding.UTF8.GetBytes(rawToken);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
