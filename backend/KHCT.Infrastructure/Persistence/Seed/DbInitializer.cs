using KHCT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KHCT.Infrastructure.Persistence.Seed;

public static class DbInitializer
{
    public static async System.Threading.Tasks.Task InitializeAsync(IServiceProvider services, bool runMigrations = true)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KhctDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");

        if (runMigrations)
        {
            await db.Database.MigrateAsync();
        }

        await EnsureSeedNamesAsync(db);

        var adminExists = await db.Users.AnyAsync(u => u.Id == SeedData.AdminUserId);
        if (!adminExists)
        {
            var admin = new User
            {
                Id = SeedData.AdminUserId,
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123", workFactor: 11),
                FullName = "Quản trị viên",
                Email = "admin@khct.local",
                DepartmentId = SeedData.AdminDepartmentId,
                IsActive = true
            };

            db.Users.Add(admin);
            db.UserRoles.Add(new UserRole { UserId = admin.Id, RoleId = SeedData.AdminRoleId });
            await db.SaveChangesAsync();
            logger.LogInformation("Seeded admin user (username=admin). Đổi mật khẩu ngay sau khi đăng nhập lần đầu.");
        }
    }

    private static async System.Threading.Tasks.Task EnsureSeedNamesAsync(KhctDbContext db)
    {
        var updated = false;

        updated |= await RenameDepartmentAsync(db, "VPTNB", "Văn phòng Tây Nam Bộ");
        updated |= await RenameDepartmentAsync(db, "TKTH", "Bộ phận Thư ký tổng hợp");
        updated |= await RenameRoleAsync(db, "VAN_THU", "Văn thư");

        if (updated)
        {
            await db.SaveChangesAsync();
        }
    }

    private static async System.Threading.Tasks.Task<bool> RenameDepartmentAsync(
        KhctDbContext db,
        string code,
        string expectedName)
    {
        var department = await db.Departments.FirstOrDefaultAsync(x => x.Code == code);
        if (department is null || department.Name == expectedName)
        {
            return false;
        }

        department.Name = expectedName;
        return true;
    }

    private static async System.Threading.Tasks.Task<bool> RenameRoleAsync(
        KhctDbContext db,
        string code,
        string expectedName)
    {
        var role = await db.Roles.FirstOrDefaultAsync(x => x.Code == code);
        if (role is null || role.Name == expectedName)
        {
            return false;
        }

        role.Name = expectedName;
        return true;
    }
}
