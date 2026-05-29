using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using KHCT.Domain.Entities;
using KHCT.Infrastructure.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace KHCT.Tests.Auth;

public class JwtTokenServiceTests
{
    private static JwtTokenService BuildService(out JwtOptions opt)
    {
        opt = new JwtOptions
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            Key = "this-is-a-test-signing-key-with-min-32-chars-1234",
            AccessTokenMinutes = 15,
            RefreshTokenDays = 7
        };
        return new JwtTokenService(Options.Create(opt));
    }

    [Fact]
    public void CreateAccessToken_emits_expected_claims()
    {
        var svc = BuildService(out var opt);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "alice",
            DepartmentId = Guid.NewGuid(),
            FullName = "Alice"
        };
        var access = svc.CreateAccessToken(user, new[] { "ADMIN", "VAN_THU" });

        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = opt.Issuer,
            ValidAudience = opt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opt.Key))
        };
        var handler = new JwtSecurityTokenHandler();
        handler.InboundClaimTypeMap.Clear();
        var principal = handler.ValidateToken(access.Token, parameters, out _);

        principal.FindFirst(JwtRegisteredClaimNames.Sub)!.Value.Should().Be(user.Id.ToString());
        principal.FindFirst("username")!.Value.Should().Be("alice");
        principal.FindFirst("dept")!.Value.Should().Be(user.DepartmentId!.Value.ToString());
        principal.FindAll(ClaimTypes.Role).Select(c => c.Value).Should().BeEquivalentTo(new[] { "ADMIN", "VAN_THU" });
        access.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void CreateRefreshToken_produces_unique_tokens_with_matching_hash()
    {
        var svc = BuildService(out _);
        var a = svc.CreateRefreshToken();
        var b = svc.CreateRefreshToken();

        a.RawToken.Should().NotBe(b.RawToken);
        a.TokenHash.Should().NotBe(b.TokenHash);
        svc.HashRefreshToken(a.RawToken).Should().Be(a.TokenHash);
        svc.HashRefreshToken(b.RawToken).Should().Be(b.TokenHash);
        a.ExpiresAt.Should().BeAfter(DateTime.UtcNow.AddDays(6));
    }
}
