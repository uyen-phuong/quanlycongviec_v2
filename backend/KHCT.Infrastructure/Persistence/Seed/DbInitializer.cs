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

        // Sync department names
        updated |= await RenameDepartmentAsync(db, "KTNB1", "Phòng KTNB1");
        updated |= await RenameDepartmentAsync(db, "KTNB2", "Phòng KTNB2");
        updated |= await RenameDepartmentAsync(db, "KTNB3", "Phòng KTNB3");
        updated |= await RenameDepartmentAsync(db, "KH", "Phòng Kế hoạch");
        updated |= await RenameDepartmentAsync(db, "GS", "Phòng Giám sát");
        updated |= await RenameDepartmentAsync(db, "VPMN", "Phòng KTNB Miền Nam");
        updated |= await RenameDepartmentAsync(db, "VPMT", "Phòng KTNB Miền Trung");
        updated |= await RenameDepartmentAsync(db, "VPTNB", "Phòng KTNB Miền Tây");
        updated |= await RenameDepartmentAsync(db, "TKTH", "Phòng Thư ký");
        updated |= await RenameDepartmentAsync(db, "LDKTNB", "Lãnh đạo KTNB");
        updated |= await RenameDepartmentAsync(db, "BKS", "Ban Kiểm sát");
        updated |= await RenameDepartmentAsync(db, "VL", "Vãng lai");

        // Sync role names to PRD role names
        updated |= await RenameRoleAsync(db, "TRUONG_KTNB", "Phê duyệt 1 – Trưởng KTNB");
        updated |= await RenameRoleAsync(db, "PHO_TRUONG_KTNB", "Phê duyệt 2 – Phó Trưởng KTNB");
        updated |= await RenameRoleAsync(db, "TRUONG_PHONG", "Kiểm soát 1 – Trưởng phòng");
        updated |= await RenameRoleAsync(db, "PHO_PHONG", "Kiểm soát 2 – Phó phòng / Trưởng nhóm");
        updated |= await RenameRoleAsync(db, "NHAN_VIEN", "Nhân viên");
        updated |= await RenameRoleAsync(db, "VAN_THU", "Văn thư");
        updated |= await RenameRoleAsync(db, "GUEST", "Giám sát 2 – Guest");

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
