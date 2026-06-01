using KHCT.Application.Common.Interfaces;
using KHCT.Domain.Entities;
using KHCT.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using TaskEntity = KHCT.Domain.Entities.Task;

namespace KHCT.Application.Notifications;

public static class NotificationHelper
{
    public static async System.Threading.Tasks.Task OnPlanSubmittedAsync(
        IApplicationDbContext db, Plan plan, Guid? actorId, CancellationToken ct)
    {
        string title;
        string[] roleCodes;
        Guid? deptId = null;

        if (plan.Scope == PlanScope.Main)
        {
            title = $"Kế hoạch tháng {plan.Month:D2}/{plan.Year} chờ kiểm soát";
            roleCodes = ["TRUONG_PHONG"];
        }
        else
        {
            title = $"Ke hoach phong {plan.Department?.Name ?? ""} thang {plan.Month:D2}/{plan.Year} da duoc gui";
            roleCodes = ["PHO_TRUONG_KTNB", "TRUONG_PHONG"];
            deptId = plan.Scope == PlanScope.Sub ? plan.DepartmentId : null;
        }

        var userIds = await GetUserIdsAsync(db, roleCodes, deptId, actorId, ct);
        CreateMany(db, userIds, title, null, "plan_submitted", plan.Id, null);
    }

    public static async System.Threading.Tasks.Task OnPlanApprovedAsync(
        IApplicationDbContext db, Plan plan, WorkflowStatus newStatus, Guid? actorId, CancellationToken ct)
    {
        string title;
        string[] roleCodes;
        Guid? deptId = null;

        if (plan.Scope == PlanScope.Main)
        {
            if (newStatus == WorkflowStatus.Approved1)
            {
                title = $"Kế hoạch tháng {plan.Month:D2}/{plan.Year} đã qua kiểm soát, chờ phê duyệt";
                roleCodes = ["TRUONG_KTNB"];
            }
            else
            {
                title = $"Kế hoạch tháng {plan.Month:D2}/{plan.Year} đã được phê duyệt";
                roleCodes = ["TRUONG_PHONG", "PHO_PHONG", "NHAN_VIEN"];
            }
        }
        else
        {
            if (newStatus == WorkflowStatus.Approved2)
            {
                title = $"Kế hoạch phòng {plan.Department?.Name ?? ""} chờ phê duyệt cuối";
                roleCodes = ["PHO_TRUONG_KTNB"];
            }
            else
            {
                title = $"Kế hoạch phòng {plan.Department?.Name ?? ""} đã được phê duyệt";
                roleCodes = ["TRUONG_PHONG", "PHO_PHONG", "NHAN_VIEN"];
                deptId = plan.DepartmentId;
            }
        }

        var userIds = await GetUserIdsAsync(db, roleCodes, deptId, actorId, ct);
        CreateMany(db, userIds, title, null, "plan_approved", plan.Id, null);
    }

    public static async System.Threading.Tasks.Task OnPlanReturnedAsync(
        IApplicationDbContext db, Plan plan, Guid? actorId, CancellationToken ct)
    {
        string title;
        string[] roleCodes;
        Guid? deptId = null;

        if (plan.Scope == PlanScope.Main)
        {
            title = $"Kế hoạch tháng {plan.Month:D2}/{plan.Year} bị chuyển trả, vui lòng chỉnh sửa";
            roleCodes = ["VAN_THU"];
        }
        else
        {
            title = $"Kế hoạch phòng {plan.Department?.Name ?? ""} bị chuyển trả, vui lòng chỉnh sửa";
            roleCodes = ["TRUONG_PHONG", "PHO_PHONG", "NHAN_VIEN"];
            deptId = plan.DepartmentId;
        }

        var userIds = await GetUserIdsAsync(db, roleCodes, deptId, actorId, ct);
        CreateMany(db, userIds, title, null, "plan_returned", plan.Id, null);
    }

    public static async System.Threading.Tasks.Task OnTaskCreatedAsync(
        IApplicationDbContext db, TaskEntity task, Plan plan, Guid? actorId, CancellationToken ct)
    {
        var title = $"Cong viec moi: {Truncate(task.Title, 80)}";
        var roleCodes = plan.Scope == PlanScope.Main
            ? new[] { "VAN_THU", "ADMIN" }
            : new[] { "TRUONG_PHONG", "PHO_PHONG" };
        var deptId = plan.Scope == PlanScope.Sub ? plan.DepartmentId : null;

        var userIds = await GetUserIdsAsync(db, roleCodes, deptId, actorId, ct);
        CreateMany(db, userIds, title, null, "task_created", plan.Id, task.Id);
    }

    public static async System.Threading.Tasks.Task OnTaskDeletedAsync(
        IApplicationDbContext db, TaskEntity task, Plan plan, Guid? actorId, CancellationToken ct)
    {
        var title = $"Công việc bị xóa: {Truncate(task.Title, 80)}";
        var roleCodes = plan.Scope == PlanScope.Main
            ? new[] { "VAN_THU", "ADMIN" }
            : new[] { "TRUONG_PHONG", "PHO_PHONG" };
        var deptId = plan.Scope == PlanScope.Sub ? plan.DepartmentId : null;

        var userIds = await GetUserIdsAsync(db, roleCodes, deptId, actorId, ct);
        CreateMany(db, userIds, title, null, "task_deleted", plan.Id, task.Id);
    }

    // ── private helpers ──────────────────────────────────────────────────────

    private static async System.Threading.Tasks.Task<List<Guid>> GetUserIdsAsync(
        IApplicationDbContext db,
        IEnumerable<string> roleCodes,
        Guid? departmentId,
        Guid? excludeUserId,
        CancellationToken ct)
    {
        var codes = roleCodes.ToArray();
        var roleIds = await db.Roles
            .Where(r => codes.Contains(r.Code))
            .Select(r => r.Id)
            .ToListAsync(ct);

        var query = db.Users
            .Where(u => u.IsActive && u.UserRoles.Any(ur => roleIds.Contains(ur.RoleId)));

        if (departmentId.HasValue)
            query = query.Where(u => u.DepartmentId == departmentId);

        if (excludeUserId.HasValue)
            query = query.Where(u => u.Id != excludeUserId);

        return await query.Select(u => u.Id).ToListAsync(ct);
    }

    private static void CreateMany(
        IApplicationDbContext db,
        IEnumerable<Guid> userIds,
        string title,
        string? body,
        string eventType,
        Guid? planId,
        Guid? taskId)
    {
        foreach (var userId in userIds)
        {
            db.Notifications.Add(new Notification
            {
                UserId = userId,
                Title = title,
                Body = body,
                EventType = eventType,
                PlanId = planId,
                TaskId = taskId,
                IsRead = false
            });
        }
    }

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..max] + "...";
}
