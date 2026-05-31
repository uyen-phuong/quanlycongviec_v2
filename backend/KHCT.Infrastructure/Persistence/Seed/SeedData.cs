using KHCT.Domain.Entities;
using KHCT.Domain.Enums;
using KHCT.Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Infrastructure.Persistence.Seed;

internal static class SeedData
{
    private static readonly DateTime SeedTime = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static class DeptIds
    {
        public static readonly Guid LanhDaoKtnb = Guid.Parse("11111111-0000-0000-0000-000000000010");
        public static readonly Guid Ktnb1 = Guid.Parse("11111111-0000-0000-0000-000000000001");
        public static readonly Guid Ktnb2 = Guid.Parse("11111111-0000-0000-0000-000000000002");
        public static readonly Guid Ktnb3 = Guid.Parse("11111111-0000-0000-0000-000000000003");
        public static readonly Guid Kh = Guid.Parse("11111111-0000-0000-0000-000000000004");
        public static readonly Guid Gs = Guid.Parse("11111111-0000-0000-0000-000000000005");
        public static readonly Guid Vpmn = Guid.Parse("11111111-0000-0000-0000-000000000006");
        public static readonly Guid Vpmt = Guid.Parse("11111111-0000-0000-0000-000000000007");
        public static readonly Guid Vptnb = Guid.Parse("11111111-0000-0000-0000-000000000008");
        public static readonly Guid Tkth = Guid.Parse("11111111-0000-0000-0000-000000000009");
        public static readonly Guid BanKs = Guid.Parse("11111111-0000-0000-0000-000000000011");
        public static readonly Guid VangLai = Guid.Parse("11111111-0000-0000-0000-000000000012");
    }

    private static class RoleIds
    {
        public static readonly Guid Admin = Guid.Parse("22222222-0000-0000-0000-000000000001");
        public static readonly Guid VanThu = Guid.Parse("22222222-0000-0000-0000-000000000002");
        public static readonly Guid TruongKtnb = Guid.Parse("22222222-0000-0000-0000-000000000004");
        public static readonly Guid PhoTruongKtnb = Guid.Parse("22222222-0000-0000-0000-000000000005");
        public static readonly Guid TruongPhong = Guid.Parse("22222222-0000-0000-0000-000000000006");
        public static readonly Guid PhoPhong = Guid.Parse("22222222-0000-0000-0000-000000000007");
        public static readonly Guid NhanVien = Guid.Parse("22222222-0000-0000-0000-000000000008");
        public static readonly Guid Guest = Guid.Parse("22222222-0000-0000-0000-000000000009");
    }

    private static class PositionIds
    {
        public static readonly Guid TruongKtnb = Guid.Parse("55555555-0000-0000-0000-000000000001");
        public static readonly Guid PhoTruongKtnb = Guid.Parse("55555555-0000-0000-0000-000000000002");
        public static readonly Guid TruongBks = Guid.Parse("55555555-0000-0000-0000-000000000003");
        public static readonly Guid ThanhVienBks = Guid.Parse("55555555-0000-0000-0000-000000000004");
        public static readonly Guid TruongPhong = Guid.Parse("55555555-0000-0000-0000-000000000005");
        public static readonly Guid PhoPhong = Guid.Parse("55555555-0000-0000-0000-000000000006");
        public static readonly Guid TruongNhom = Guid.Parse("55555555-0000-0000-0000-000000000007");
        public static readonly Guid NhanVien = Guid.Parse("55555555-0000-0000-0000-000000000008");
    }

    public static readonly Guid AdminUserId = Guid.Parse("33333333-0000-0000-0000-000000000001");
    public static readonly Guid AdminDepartmentId = DeptIds.LanhDaoKtnb;
    public static readonly Guid AdminRoleId = RoleIds.Admin;

    public static void Apply(ModelBuilder mb)
    {
        mb.Entity<Department>().HasData(
            new { Id = DeptIds.LanhDaoKtnb, Code = "LDKTNB", Name = "Lãnh đạo KTNB", IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = DeptIds.Ktnb1, Code = "KTNB1", Name = "Phòng KTNB1", IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = DeptIds.Ktnb2, Code = "KTNB2", Name = "Phòng KTNB2", IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = DeptIds.Ktnb3, Code = "KTNB3", Name = "Phòng KTNB3", IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = DeptIds.Kh, Code = "KH", Name = "Phòng KH", IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = DeptIds.Tkth, Code = "TKTH", Name = "Phòng TKTH", IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = DeptIds.Vpmn, Code = "VPMN", Name = "Phòng MN", IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = DeptIds.Vpmt, Code = "VPMT", Name = "Phòng MT", IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = DeptIds.Vptnb, Code = "VPTNB", Name = "Phòng TNB", IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = DeptIds.Gs, Code = "GS", Name = "Phòng Giám sát", IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = DeptIds.BanKs, Code = "BKS", Name = "Ban KS", IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = DeptIds.VangLai, Code = "VL", Name = "Vãng lai", IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime }
        );

        mb.Entity<Position>().HasData(
            new { Id = PositionIds.TruongKtnb, Code = "TRUONG_KTNB", Name = "Trưởng KTNB", IsActive = true, SortOrder = 1, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = PositionIds.PhoTruongKtnb, Code = "PHO_TRUONG_KTNB", Name = "Phó Trưởng KTNB", IsActive = true, SortOrder = 2, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = PositionIds.TruongBks, Code = "TRUONG_BKS", Name = "Trưởng BKS", IsActive = true, SortOrder = 3, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = PositionIds.ThanhVienBks, Code = "THANH_VIEN_BKS", Name = "Thành viên BKS", IsActive = true, SortOrder = 4, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = PositionIds.TruongPhong, Code = "TRUONG_PHONG", Name = "Trưởng Phòng", IsActive = true, SortOrder = 5, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = PositionIds.PhoPhong, Code = "PHO_PHONG", Name = "Phó Phòng", IsActive = true, SortOrder = 6, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = PositionIds.TruongNhom, Code = "TRUONG_NHOM", Name = "Trưởng nhóm", IsActive = true, SortOrder = 7, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = PositionIds.NhanVien, Code = "NHAN_VIEN", Name = "Nhân viên", IsActive = true, SortOrder = 8, CreatedAt = SeedTime, UpdatedAt = SeedTime }
        );

        mb.Entity<Role>().HasData(
            new { Id = RoleIds.Admin, Code = RoleConstants.Admin, Name = "Quản trị hệ thống", CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = RoleIds.TruongKtnb, Code = RoleConstants.TruongKtnb, Name = "Phê duyệt 1 – Trưởng KTNB", CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = RoleIds.PhoTruongKtnb, Code = RoleConstants.PhoTruongKtnb, Name = "Phê duyệt 2 – Phó Trưởng KTNB", CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = RoleIds.TruongPhong, Code = RoleConstants.TruongPhong, Name = "Kiểm soát 1 – Trưởng phòng", CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = RoleIds.PhoPhong, Code = RoleConstants.PhoPhong, Name = "Kiểm soát 2 – Phó phòng / Trưởng nhóm", CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = RoleIds.NhanVien, Code = RoleConstants.NhanVien, Name = "Nhân viên", CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = RoleIds.VanThu, Code = RoleConstants.VanThu, Name = "Văn thư", CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = RoleIds.Guest, Code = RoleConstants.Guest, Name = "Giám sát 2 – Guest", CreatedAt = SeedTime, UpdatedAt = SeedTime }
        );

        mb.Entity<ApprovalConfig>().HasData(
            new { Id = Guid.Parse("44444444-0000-0000-0000-000000000001"), Scope = PlanScope.Main, Level = 1, DepartmentId = (Guid?)DeptIds.Kh, RoleId = RoleIds.TruongPhong, IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = Guid.Parse("44444444-0000-0000-0000-000000000002"), Scope = PlanScope.Main, Level = 2, DepartmentId = (Guid?)null, RoleId = RoleIds.TruongKtnb, IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = Guid.Parse("44444444-0000-0000-0000-000000000003"), Scope = PlanScope.Sub, Level = 1, DepartmentId = (Guid?)null, RoleId = RoleIds.PhoPhong, IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = Guid.Parse("44444444-0000-0000-0000-000000000004"), Scope = PlanScope.Sub, Level = 2, DepartmentId = (Guid?)null, RoleId = RoleIds.TruongPhong, IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new { Id = Guid.Parse("44444444-0000-0000-0000-000000000005"), Scope = PlanScope.Sub, Level = 3, DepartmentId = (Guid?)null, RoleId = RoleIds.PhoTruongKtnb, IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime }
        );
    }
}
