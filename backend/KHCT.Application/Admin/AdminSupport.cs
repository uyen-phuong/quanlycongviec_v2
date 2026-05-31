using KHCT.Application.Common.Interfaces;
using KHCT.Domain.Common;
using KHCT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Position = KHCT.Domain.Entities.Position;

namespace KHCT.Application.Admin;

internal static class AdminSupport
{
    public const string AdminRoleCode = "ADMIN";

    public static string? NormalizeEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        return email.Trim();
    }

    public static AdminUserListItemDto ToListItem(User user)
    {
        var role = user.UserRoles.FirstOrDefault()?.Role;
        return new AdminUserListItemDto(
            user.Id,
            user.Username,
            user.FullName,
            user.Email,
            user.IsActive,
            user.LastLoginAt,
            user.LastLogoutAt,
            user.DepartmentId,
            user.Department?.Code,
            user.Department?.Name,
            user.PositionId,
            user.Position?.Code,
            user.Position?.Name,
            role?.Id,
            role?.Code,
            role?.Name);
    }

    public static AdminUserDetailDto ToDetail(User user)
    {
        var role = user.UserRoles.FirstOrDefault()?.Role;
        return new AdminUserDetailDto(
            user.Id,
            user.Username,
            user.FullName,
            user.Email,
            user.IsActive,
            user.LastLoginAt,
            user.LastLogoutAt,
            user.DepartmentId,
            user.Department?.Code,
            user.Department?.Name,
            user.PositionId,
            user.Position?.Code,
            user.Position?.Name,
            role?.Id,
            role?.Code,
            role?.Name,
            user.CreatedAt,
            user.UpdatedAt);
    }

    public static PositionDto ToDto(Position position) =>
        new(position.Id, position.Code, position.Name, position.IsActive, position.SortOrder);

    public static DepartmentDto ToDto(Department department) =>
        new(department.Id, department.Code, department.Name, department.IsActive);

    public static RoleDto ToDto(Role role) =>
        new(role.Id, role.Code, role.Name);

    public static ApprovalConfigDto ToDto(ApprovalConfig config) =>
        new(
            config.Id,
            config.Scope.ToString().ToLowerInvariant(),
            config.Level,
            config.DepartmentId,
            config.Department?.Code,
            config.Department?.Name,
            config.RoleId,
            config.Role?.Code ?? string.Empty,
            config.Role?.Name ?? string.Empty,
            config.IsActive);

    public static object UserSnapshot(User user)
    {
        var role = user.UserRoles.FirstOrDefault()?.Role;
        return new
        {
            user.Id,
            user.Username,
            user.FullName,
            user.Email,
            user.DepartmentId,
            DepartmentCode = user.Department?.Code,
            user.IsActive,
            RoleId = role?.Id,
            RoleCode = role?.Code
        };
    }

    public static object UserRoleSnapshot(User user, Role role) =>
        new
        {
            user.Id,
            user.Username,
            RoleId = role.Id,
            RoleCode = role.Code
        };

    public static object PasswordResetSnapshot(User user) =>
        new
        {
            user.Id,
            user.Username,
            PasswordReset = true
        };

    public static object DepartmentSnapshot(Department department) =>
        new
        {
            department.Id,
            department.Code,
            department.Name,
            department.IsActive
        };

    public static object ApprovalConfigSnapshot(ApprovalConfig config) =>
        new
        {
            config.Id,
            Scope = config.Scope.ToString().ToLowerInvariant(),
            config.Level,
            config.DepartmentId,
            DepartmentCode = config.Department?.Code,
            config.RoleId,
            RoleCode = config.Role?.Code,
            config.IsActive
        };

    public static async System.Threading.Tasks.Task<Role> RequireRoleAsync(
        IApplicationDbContext db,
        Guid roleId,
        CancellationToken ct) =>
        await db.Roles.FirstOrDefaultAsync(x => x.Id == roleId, ct)
        ?? throw new KeyNotFoundException("Role not found.");

    public static async System.Threading.Tasks.Task EnsureNotLastActiveAdminAsync(
        IApplicationDbContext db,
        Guid userId,
        CancellationToken ct)
    {
        var adminUserIds = await db.UserRoles
            .Where(x => x.Role != null && x.Role.Code == AdminRoleCode)
            .Select(x => x.UserId)
            .Distinct()
            .ToListAsync(ct);

        var activeAdminCount = await db.Users
            .Where(x => adminUserIds.Contains(x.Id) && x.IsActive)
            .CountAsync(ct);

        var isProtectedUser = await db.Users
            .AnyAsync(x => x.Id == userId && x.IsActive && adminUserIds.Contains(x.Id), ct);

        if (isProtectedUser && activeAdminCount <= 1)
        {
            throw new DomainException("last_admin_protected", "Cannot change or deactivate the last active admin.");
        }
    }

    public static bool IsAdmin(User user) =>
        user.UserRoles.Any(x => string.Equals(x.Role?.Code, AdminRoleCode, StringComparison.Ordinal));
}
