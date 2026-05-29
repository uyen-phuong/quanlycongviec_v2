using FluentAssertions;

namespace KHCT.Tests.Integration;

public class DepartmentsIntegrationTests : IClassFixture<KhctApiFactory>
{
    private readonly KhctApiFactory _factory;

    public DepartmentsIntegrationTests(KhctApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Authenticated_User_Can_List_Active_Departments()
    {
        await _factory.CreateUserAsync("dept.reader", "Password123!", "Department Reader", "VAN_THU", "VPTNB");
        var client = await _factory.CreateAuthenticatedClientAsync("dept.reader", "Password123!");

        var response = await client.GetAsync("/api/departments");
        var json = await _factory.DownloadJsonAsync(response);

        response.EnsureSuccessStatusCode();
        json.GetProperty("data").EnumerateArray().Should().NotBeEmpty();
        json.GetProperty("data").EnumerateArray().Any(x => x.GetProperty("code").GetString() == "KTNB1").Should().BeTrue();
    }
}
