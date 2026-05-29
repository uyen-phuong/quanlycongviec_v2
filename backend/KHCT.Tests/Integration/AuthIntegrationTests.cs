using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace KHCT.Tests.Integration;

public class AuthIntegrationTests : IClassFixture<KhctApiFactory>
{
    private readonly KhctApiFactory _factory;

    public AuthIntegrationTests(KhctApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_And_Me_Should_Work_ForSeedAdmin()
    {
        var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin@123");

        var response = await client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var root = await _factory.DownloadJsonAsync(response);
        root.GetProperty("data").GetProperty("username").GetString().Should().Be("admin");
    }

    [Fact]
    public async Task Inactive_User_Should_Be_Blocked_From_Login()
    {
        await _factory.CreateUserAsync("blocked.user", "Password123!", "Blocked User", "TRUONG_PHONG", "KTNB1", isActive: false);
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            username = "blocked.user",
            password = "Password123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
