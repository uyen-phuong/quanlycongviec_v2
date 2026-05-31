using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using KHCT.Application.Common.Interfaces;
using KHCT.Domain.Enums;
using KHCT.Infrastructure;
using KHCT.Infrastructure.Persistence;
using KHCT.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UserEntity = KHCT.Domain.Entities.User;
using UserRoleEntity = KHCT.Domain.Entities.UserRole;

namespace KHCT.Tests.Integration;

public sealed class KhctApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        Environment.SetEnvironmentVariable("KHCT_AUTO_MIGRATE", "false");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<KhctDbContext>>();
            services.RemoveAll<KhctDbContext>();
            services.AddDbContext<KhctDbContext>(options => options.UseSqlite(_connection));
            services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<KhctDbContext>());
        });
    }

    public async System.Threading.Tasks.Task InitializeAsync()
    {
        _connection.CreateCollation("ascii_general_ci", (x, y) => string.Compare(x, y, StringComparison.OrdinalIgnoreCase));
        _connection.CreateCollation("utf8mb4_0900_ai_ci", (x, y) => string.Compare(x, y, StringComparison.OrdinalIgnoreCase));
        _connection.CreateCollation("utf8mb4_general_ci", (x, y) => string.Compare(x, y, StringComparison.OrdinalIgnoreCase));
        _connection.CreateCollation("utf8mb4_bin", (x, y) => string.Compare(x, y, StringComparison.Ordinal));

        await _connection.OpenAsync();
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KhctDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        await DbInitializer.InitializeAsync(scope.ServiceProvider, runMigrations: false);
    }

    public new async System.Threading.Tasks.Task DisposeAsync()
    {
        await _connection.DisposeAsync();
        Dispose();
    }

    public async System.Threading.Tasks.Task<(Guid Id, string Username, string Password)> CreateUserAsync(
        string username,
        string password,
        string fullName,
        string roleCode,
        string departmentCode,
        bool isActive = true)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KhctDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var role = await db.Roles.FirstAsync(x => x.Code == roleCode);
        var department = await db.Departments.FirstAsync(x => x.Code == departmentCode);
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Username = username.Trim().ToLowerInvariant(),
            PasswordHash = hasher.Hash(password),
            FullName = fullName,
            DepartmentId = department.Id,
            IsActive = isActive
        };
        db.Users.Add(user);
        db.UserRoles.Add(new UserRoleEntity
        {
            UserId = user.Id,
            RoleId = role.Id
        });
        await db.SaveChangesAsync();

        return (user.Id, user.Username, password);
    }

    public async System.Threading.Tasks.Task<HttpClient> CreateAuthenticatedClientAsync(string username, string password)
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            username,
            password
        });
        response.EnsureSuccessStatusCode();

        var root = await response.Content.ReadFromJsonAsync<JsonElement>();
        var token = root.GetProperty("data").GetProperty("accessToken").GetString()
            ?? throw new InvalidOperationException("Missing access token.");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public async System.Threading.Tasks.Task<Guid> CreateMainPlanAsync(HttpClient client, int year = 2026, int month = 5)
    {
        var response = await client.PostAsJsonAsync("/api/plans/main", new { name = $"Kế hoạch {year}", year, month, reportingPeriodType = "monthly", ktnbLeaderId = (Guid?)null });
        response.EnsureSuccessStatusCode();
        var root = await response.Content.ReadFromJsonAsync<JsonElement>();
        return root.GetProperty("data").GetProperty("id").GetGuid();
    }

    public async System.Threading.Tasks.Task<Guid> CreateTaskAsync(HttpClient client, Guid planId, string title, Guid? ownerDepartmentId = null)
    {
        var response = await client.PostAsJsonAsync("/api/tasks", new
        {
            planId,
            parentTaskId = (Guid?)null,
            outlineIndex = "1",
            displayOrder = 10,
            isHeader = false,
            title,
            workType = 0,
            workStatus = "not_started",
            deadline = (DateTime?)null,
            assigneeUserId = (Guid?)null,
            ownerDepartmentId,
            bksMemberText = (string?)null,
            ktnbLeaderText = (string?)null,
            noteText = (string?)null,
            progressText = (string?)null,
            reasonNotCompleted = (string?)null,
            supportingDepartmentIds = Array.Empty<Guid>()
        });
        response.EnsureSuccessStatusCode();
        var root = await response.Content.ReadFromJsonAsync<JsonElement>();
        return root.GetProperty("data").GetProperty("id").GetGuid();
    }

    public async System.Threading.Tasks.Task<Guid> GetDepartmentIdAsync(string departmentCode)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KhctDbContext>();
        return await db.Departments.Where(x => x.Code == departmentCode).Select(x => x.Id).FirstAsync();
    }

    public async System.Threading.Tasks.Task<(Guid Id, string Status)> GetPlanAsync(Guid planId)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KhctDbContext>();
        var plan = await db.Plans.FirstAsync(x => x.Id == planId);
        return (plan.Id, plan.Status.ToString());
    }

    public async System.Threading.Tasks.Task SetTaskProgressAsync(Guid taskId, string progressText, WorkStatus workStatus, string? reasonNotCompleted = null)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KhctDbContext>();
        var task = await db.Tasks.FirstAsync(x => x.Id == taskId);
        task.ProgressText = progressText;
        task.WorkStatus = workStatus;
        task.ReasonNotCompleted = reasonNotCompleted;
        await db.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task<int> CountAuditAsync(string entityName, string action)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KhctDbContext>();
        return await db.AuditLogs.CountAsync(x => x.EntityName == entityName && x.Action == action);
    }

    public async System.Threading.Tasks.Task<JsonElement> DownloadJsonAsync(HttpResponseMessage response)
    {
        var text = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(text);
        return document.RootElement.Clone();
    }

    public static MultipartFormDataContent CreateFileContent(string fileName, byte[] content, string contentType)
    {
        var multipart = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(content);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        multipart.Add(fileContent, "file", fileName);
        return multipart;
    }
}
