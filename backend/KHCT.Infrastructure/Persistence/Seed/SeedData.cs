using KHCT.Domain.Entities;
using KHCT.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Infrastructure.Persistence.Seed;

internal static class SeedData
{
    private static readonly DateTime SeedTime = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static class DeptIds
    {
        public static readonly Guid Ktnb1 = Guid.Parse("11111111-0000-0000-0000-000000000001");
        public static readonly Guid Ktnb2 = Guid.Parse("11111111-0000-0000-0000-000000000002");
        public static readonly Guid Ktnb3 = Guid.Parse("11111111-0000-0000-0000-000000000003");
        public static readonly Guid Kh = Guid.Parse("11111111-0000-0000-0000-000000000004");
        public static readonly Guid Gs = Guid.Parse("11111111-0000-0000-0000-000000000005");
        public static readonly Guid Vpmn = Guid.Parse("11111111-0000-0000-0000-000000000006");
        public static readonly Guid Vpmt = Guid.Parse("11111111-0000-0000-0000-000000000007");
        public static readonly Guid Vptnb = Guid.Parse("11111111-0000-0000-0000-000000000008");
        public static readonly Guid Tkth = Guid.Parse("11111111-0000-0000-0000-000000000009");
    }

    private static class RoleIds
    {
        public static readonly Guid Admin = Guid.Parse("22222222-0000-0000-0000-000000000001");
        public static readonly Guid VanThu = Guid.Parse("22222222-0000-0000-0000-000000000002");
        public static readonly Guid TruongKh = Guid.Parse("22222222-0000-0000-0000-000000000003");
        public static readonly Guid TruongKtnb = Guid.Parse("22222222-0000-0000-0000-000000000004");
        public static readonly Guid PhoTruongKtnb = Guid.Parse("22222222-0000-0000-0000-000000000005");
        public static readonly Guid TruongPhong = Guid.Parse("22222222-0000-0000-0000-000000000006");
        public static readonly Guid TruongNhom = Guid.Parse("22222222-0000-0000-0000-000000000007");
        public static readonly Guid NhanVien = Guid.Parse("22222222-0000-0000-0000-000000000008");
    }

    public static readonly Guid AdminUserId = Guid.Parse("33333333-0000-0000-0000-000000000001");
    public static readonly Guid AdminDepartmentId = DeptIds.Vptnb;
    public static readonly Guid AdminRoleId = RoleIds.Admin;

    public static void Apply(ModelBuilder mb)
    {
        mb.Entity<Department>().HasData(
            new { Id = DeptIds.Ktnb1, Code = "KTNB1", Name = "Kiểm toán nội bộ 1", IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = DeptIds.Ktnb2, Code = "KTNB2", Name = "Kiểm toán nội bộ 2", IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = DeptIds.Ktnb3, Code = "KTNB3", Name = "Kiểm toán nội bộ 3", IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = DeptIds.Kh, Code = "KH", Name = "Phòng Kế hoạch", IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = DeptIds.Gs, Code = "GS", Name = "Phòng Giám sát", IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = DeptIds.Vpmn, Code = "VPMN", Name = "Văn phòng miền Nam", IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = DeptIds.Vpmt, Code = "VPMT", Name = "Văn phòng miền Trung", IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = DeptIds.Vptnb, Code = "VPTNB", Name = "Văn phòng Tây Nam Bộ", IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = DeptIds.Tkth, Code = "TKTH", Name = "Bộ phận Thư ký tổng hợp", IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime }
        );

        mb.Entity<Role>().HasData(
            new { Id = RoleIds.Admin, Code = "ADMIN", Name = "Quản trị hệ thống", CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = RoleIds.VanThu, Code = "VAN_THU", Name = "Văn thư", CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = RoleIds.TruongKh, Code = "TRUONG_KH", Name = "Trưởng phòng Kế hoạch", CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = RoleIds.TruongKtnb, Code = "TRUONG_KTNB", Name = "Trưởng KTNB", CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = RoleIds.PhoTruongKtnb, Code = "PHO_TRUONG_KTNB", Name = "Phó Trưởng KTNB", CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = RoleIds.TruongPhong, Code = "TRUONG_PHONG", Name = "Trưởng phòng", CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = RoleIds.TruongNhom, Code = "TRUONG_NHOM", Name = "Trưởng nhóm", CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = RoleIds.NhanVien, Code = "NHAN_VIEN", Name = "Nhân viên", CreatedAt = SeedTime, UpdatedAt = SeedTime }
        );

        mb.Entity<ApprovalConfig>().HasData(
            new { Id = Guid.Parse("44444444-0000-0000-0000-000000000001"), Scope = PlanScope.Main, Level = 1, DepartmentId = (Guid?)null, RoleId = RoleIds.TruongKh, IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = Guid.Parse("44444444-0000-0000-0000-000000000002"), Scope = PlanScope.Main, Level = 2, DepartmentId = (Guid?)null, RoleId = RoleIds.TruongKtnb, IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = Guid.Parse("44444444-0000-0000-0000-000000000003"), Scope = PlanScope.Sub, Level = 1, DepartmentId = (Guid?)null, RoleId = RoleIds.TruongNhom, IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = Guid.Parse("44444444-0000-0000-0000-000000000004"), Scope = PlanScope.Sub, Level = 2, DepartmentId = (Guid?)null, RoleId = RoleIds.TruongPhong, IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = Guid.Parse("44444444-0000-0000-0000-000000000005"), Scope = PlanScope.Sub, Level = 3, DepartmentId = (Guid?)null, RoleId = RoleIds.PhoTruongKtnb, IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime }
        );
    }
}
