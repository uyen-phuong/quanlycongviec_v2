using KHCT.Application.Common.Interfaces;
using KHCT.Domain.Common;
using KHCT.Domain.Entities;
using KHCT.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Plans;

public static class PlanSupport
{
    public const string RoleAdmin = "ADMIN";
    public const string RoleVanThu = "VAN_THU";
    public const string RoleTruongKh = "TRUONG_KH";
    public const string RoleTruongKtnb = "TRUONG_KTNB";
    public const string RolePhoTruongKtnb = "PHO_TRUONG_KTNB";
    public const string RoleTruongPhong = "TRUONG_PHONG";
    public const string RoleTruongNhom = "TRUONG_NHOM";
    public const string RoleNhanVien = "NHAN_VIEN";

    private static readonly HashSet<string> SubSeeAllRoles = new(StringComparer.Ordinal)
    {
        RoleAdmin,
        RoleTruongKtnb,
        RolePhoTruongKtnb,
        RoleTruongKh,
        RoleVanThu
    };

    private static readonly HashSet<string> SubDeptScopedRoles = new(StringComparer.Ordinal)
    {
        RoleTruongPhong,
        RoleNhanVien
    };

    public static PlanListItemDto ToListItem(Plan plan) =>
        new(
            plan.Id,
            ScopeCode(plan.Scope),
            plan.Year,
            plan.Month,
            StatusCode(plan.Status),
            plan.DepartmentId,
            plan.Department?.Code,
            plan.Department?.Name,
            plan.CreatedById,
            plan.CreatedBy?.FullName,
            plan.SubmittedAt,
            plan.ApprovedAt,
            plan.CreatedAt,
            plan.UpdatedAt);

    public static PlanDetailDto ToDetail(Plan plan) =>
        new(
            plan.Id,
            ScopeCode(plan.Scope),
            plan.Year,
            plan.Month,
            StatusCode(plan.Status),
            plan.DepartmentId,
            plan.Department?.Code,
            plan.Department?.Name,
            plan.CreatedById,
            plan.CreatedBy?.FullName,
            plan.SubmittedAt,
            plan.ApprovedAt,
            plan.Tasks.Count,
            plan.CreatedAt,
            plan.UpdatedAt);

    public static object Snapshot(Plan plan) =>
        new
        {
            plan.Id,
            Scope = ScopeCode(plan.Scope),
            plan.DepartmentId,
            plan.Year,
            plan.Month,
            Status = StatusCode(plan.Status),
            plan.CreatedById
        };

    public static void EnsureDraft(Plan plan)
    {
        if (plan.Status != ApprovalStatus.Draft)
        {
            throw new DomainException("plan_not_editable", "Plan is not editable.");
        }
    }

    public static void EnsureEditable(Plan plan)
    {
        _ = plan;
    }

    public static async System.Threading.Tasks.Task EnsureUniqueMainAsync(
        IApplicationDbContext db,
        int year,
        int month,
        Guid? excludeId,
        CancellationToken ct)
    {
        var exists = await db.Plans.AnyAsync(x =>
            x.Scope == PlanScope.Main &&
            x.DepartmentId == null &&
            x.Year == year &&
            x.Month == month &&
            (!excludeId.HasValue || x.Id != excludeId.Value), ct);

        if (exists)
        {
            throw new DomainException("plan_duplicate", "Plan for this period already exists.");
        }
    }

    public static async System.Threading.Tasks.Task EnsureUniqueSubAsync(
        IApplicationDbContext db,
        Guid departmentId,
        int year,
        int month,
        Guid? excludeId,
        CancellationToken ct)
    {
        var exists = await db.Plans.AnyAsync(x =>
            x.Scope == PlanScope.Sub &&
            x.DepartmentId == departmentId &&
            x.Year == year &&
            x.Month == month &&
            (!excludeId.HasValue || x.Id != excludeId.Value), ct);

        if (exists)
        {
            throw new DomainException("plan_duplicate", "Plan for this period already exists.");
        }
    }

    public static Guid RequireActorId(ICurrentUser currentUser) =>
        currentUser.UserId ?? throw new UnauthorizedAccessException("Chua dang nhap");

    public static bool HasRole(ICurrentUser currentUser, string role) =>
        currentUser.Roles.Any(x => string.Equals(x, role, StringComparison.Ordinal));

    public static void EnsureCanMutateSubDepartment(ICurrentUser currentUser, Guid departmentId)
    {
        if (HasRole(currentUser, RolePhoTruongKtnb))
        {
            return;
        }

        if (HasRole(currentUser, RoleTruongPhong) && currentUser.DepartmentId == departmentId)
        {
            return;
        }

        throw new DomainException("forbidden_department", "You cannot mutate another department's sub plan.");
    }

    public static IQueryable<Plan> ApplySubReadScope(IQueryable<Plan> query, ICurrentUser currentUser)
    {
        if (currentUser.Roles.Any(SubSeeAllRoles.Contains))
        {
            return query;
        }

        if (currentUser.Roles.Any(SubDeptScopedRoles.Contains))
        {
            if (!currentUser.DepartmentId.HasValue)
            {
                throw new ForbiddenException("forbidden_role", "Current user has no department scope.");
            }

            var departmentId = currentUser.DepartmentId.Value;
            return query.Where(x => x.DepartmentId == departmentId);
        }

        throw new ForbiddenException("forbidden_role", "Current role cannot read sub plans.");
    }

    public static bool CanReadSubPlan(Plan plan, ICurrentUser currentUser)
    {
        if (currentUser.Roles.Any(SubSeeAllRoles.Contains))
        {
            return true;
        }

        if (currentUser.Roles.Any(SubDeptScopedRoles.Contains))
        {
            return currentUser.DepartmentId.HasValue && plan.DepartmentId == currentUser.DepartmentId.Value;
        }

        throw new ForbiddenException("forbidden_role", "Current role cannot read sub plans.");
    }

    public static string ScopeCode(PlanScope scope) =>
        scope switch
        {
            PlanScope.Main => "main",
            PlanScope.Sub => "sub",
            _ => scope.ToString().ToLowerInvariant()
        };

    public static string StatusCode(ApprovalStatus status) =>
        status switch
        {
            ApprovalStatus.Draft => "draft",
            ApprovalStatus.Pending => "pending",
            ApprovalStatus.Approved1 => "approved_1",
            ApprovalStatus.Approved2 => "approved_2",
            ApprovalStatus.Approved3 => "approved_3",
            ApprovalStatus.Returned => "returned",
            _ => status.ToString().ToLowerInvariant()
        };

    public static async System.Threading.Tasks.Task ResetWorkflowAsync(
        IApplicationDbContext db,
        Plan plan,
        CancellationToken ct)
    {
        plan.Status = ApprovalStatus.Draft;
        plan.SubmittedAt = null;
        plan.ApprovedAt = null;

        var tasks = await db.Tasks
            .Where(x => x.PlanId == plan.Id)
            .ToListAsync(ct);

        foreach (var task in tasks)
        {
            task.ApprovalStatus = TaskApprovalStatus.Draft;
            task.SubmittedAt = null;
            task.ApprovedAt = null;
        }
    }
}
