using KHCT.Application.Common.Interfaces;
using KHCT.Application.Common.Support;
using KHCT.Application.Plans;
using KHCT.Domain.Common;
using KHCT.Domain.Entities;
using KHCT.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using TaskEntity = KHCT.Domain.Entities.Task;

namespace KHCT.Application.Tasks;

public static class TaskSupport
{
    public static TaskListItemDto ToListItem(TaskEntity task) =>
        new(
            task.Id,
            task.PlanId,
            task.ParentTaskId,
            task.OutlineIndex,
            task.DisplayOrder,
            task.IsHeader,
            task.Title,
            (int)task.WorkType,
            WorkStatusCode(task.WorkStatus),
            task.Deadline,
            task.AssigneeUserId,
            task.AssigneeUser?.FullName,
            task.OwnerDepartmentId,
            task.OwnerDepartment?.Code,
            task.OwnerDepartment?.Name,
            task.BksMemberText,
            task.KtnbLeaderText,
            task.NoteText,
            task.IsLocked,
            task.HasOpenComment,
            TaskApprovalStatusCode(task.ApprovalStatus),
            task.SubmittedAt,
            task.ApprovedAt,
            task.ProgressText,
            task.ReasonNotCompleted,
            task.SupportingDepts.Select(x => x.DepartmentId).OrderBy(x => x).ToList(),
            task.CreatedAt,
            task.UpdatedAt);

    public static TaskDetailDto ToDetail(TaskEntity task) =>
        new(
            task.Id,
            task.PlanId,
            task.ParentTaskId,
            task.OutlineIndex,
            task.DisplayOrder,
            task.IsHeader,
            task.Title,
            (int)task.WorkType,
            WorkStatusCode(task.WorkStatus),
            task.Deadline,
            task.AssigneeUserId,
            task.AssigneeUser?.FullName,
            task.OwnerDepartmentId,
            task.OwnerDepartment?.Code,
            task.OwnerDepartment?.Name,
            task.BksMemberText,
            task.KtnbLeaderText,
            task.NoteText,
            task.IsLocked,
            task.HasOpenComment,
            TaskApprovalStatusCode(task.ApprovalStatus),
            task.SubmittedAt,
            task.ApprovedAt,
            task.ProgressText,
            task.ReasonNotCompleted,
            task.SupportingDepts
                .Where(x => x.Department != null)
                .Select(x => new SupportingDepartmentDto(x.DepartmentId, x.Department!.Code, x.Department.Name))
                .OrderBy(x => x.Code)
                .ToList(),
            task.CreatedAt,
            task.UpdatedAt);

    public static object Snapshot(TaskEntity task) =>
        new
        {
            task.Id,
            task.PlanId,
            task.ParentTaskId,
            task.OutlineIndex,
            task.DisplayOrder,
            task.IsHeader,
            task.Title,
            WorkType = (int)task.WorkType,
            WorkStatus = WorkStatusCode(task.WorkStatus),
            task.Deadline,
            task.AssigneeUserId,
            task.OwnerDepartmentId,
            task.BksMemberText,
            task.KtnbLeaderText,
            task.NoteText,
            task.IsLocked,
            task.HasOpenComment,
            ApprovalStatus = TaskApprovalStatusCode(task.ApprovalStatus),
            task.SubmittedAt,
            task.ApprovedAt,
            task.ProgressText,
            task.ReasonNotCompleted,
            SupportingDepartmentIds = task.SupportingDepts.Select(x => x.DepartmentId).OrderBy(x => x).ToList()
        };

    public static void EnsureCanCreateOrDeleteTask(Plan plan, ICurrentUser currentUser)
    {
        PlanSupport.EnsureEditable(plan);

        if (plan.Scope == PlanScope.Main)
        {
            if (PlanSupport.HasRole(currentUser, PlanSupport.RoleVanThu) ||
                PlanSupport.HasRole(currentUser, PlanSupport.RoleAdmin))
            {
                return;
            }

            throw new ForbiddenException("forbidden_role", "Current role cannot mutate main plan tasks.");
        }

        EnsureSubDepartment(plan);
        if (!PlanSupport.HasRole(currentUser, PlanSupport.RolePhoTruongKtnb) &&
            !PlanSupport.HasRole(currentUser, PlanSupport.RoleTruongPhong) &&
            !PlanSupport.HasRole(currentUser, PlanSupport.RoleTruongNhom))
        {
            throw new ForbiddenException("forbidden_role", "Current role cannot mutate sub plan tasks.");
        }

        PlanSupport.EnsureCanMutateSubDepartment(currentUser, plan.DepartmentId!.Value);
    }

    public static void EnsureCanUpdateTaskFull(Plan plan, TaskEntity task, ICurrentUser currentUser)
    {
        EnsureCanCreateOrDeleteTask(plan, currentUser);
        if (plan.Scope != PlanScope.Sub)
        {
            EnsureNotLocked(task);
        }
    }

    public static void EnsureCanUpdateTaskProgress(Plan plan, TaskEntity task, ICurrentUser currentUser)
    {
        PlanSupport.EnsureEditable(plan);
        EnsureSubDepartment(plan);

        if (!PlanSupport.HasRole(currentUser, PlanSupport.RoleNhanVien) ||
            currentUser.DepartmentId != plan.DepartmentId)
        {
            throw new ForbiddenException("forbidden_role", "Current role cannot update task progress.");
        }
    }

    public static bool CanFullUpdate(Plan plan, ICurrentUser currentUser)
    {
        if (plan.Scope == PlanScope.Main)
        {
            return PlanSupport.HasRole(currentUser, PlanSupport.RoleVanThu) ||
                PlanSupport.HasRole(currentUser, PlanSupport.RoleAdmin);
        }

        return (PlanSupport.HasRole(currentUser, PlanSupport.RolePhoTruongKtnb) ||
                PlanSupport.HasRole(currentUser, PlanSupport.RoleTruongPhong) ||
                PlanSupport.HasRole(currentUser, PlanSupport.RoleTruongNhom)) &&
            (!PlanSupport.HasRole(currentUser, PlanSupport.RoleTruongPhong) ||
                currentUser.DepartmentId == plan.DepartmentId) &&
            (!PlanSupport.HasRole(currentUser, PlanSupport.RoleTruongNhom) ||
                currentUser.DepartmentId == plan.DepartmentId);
    }

    public static void EnsureCanReadPlanTasks(Plan plan, ICurrentUser currentUser)
    {
        if (plan.Scope == PlanScope.Main)
        {
            return;
        }

        if (!PlanSupport.CanReadSubPlan(plan, currentUser))
        {
            throw new KeyNotFoundException("Plan not found.");
        }
    }

    public static void EnsureNotLocked(TaskEntity task)
    {
        if (task.IsLocked)
        {
            throw new ForbiddenException("task_locked", "Task is locked.");
        }
    }

    public static void EnsureHeaderNormalized(TaskEntity task)
    {
        if (!task.IsHeader)
        {
            if (task.WorkType != WorkType.General)
            {
                task.BksMemberText = null;
            }

            return;
        }

        task.Deadline = null;
        task.AssigneeUserId = null;
        task.OwnerDepartmentId = null;
        task.BksMemberText = null;
        task.KtnbLeaderText = null;
        task.NoteText = null;
        task.ProgressText = null;
        task.ReasonNotCompleted = null;
        task.WorkStatus = WorkStatus.NotStarted;
        task.SupportingDepts.Clear();
    }

    public static async System.Threading.Tasks.Task<Guid?> ValidateOwnerDepartmentAsync(
        IApplicationDbContext db,
        Plan plan,
        bool isHeader,
        WorkType workType,
        Guid? ownerDepartmentId,
        CancellationToken ct)
    {
        if (plan.Scope == PlanScope.Sub)
        {
            return ownerDepartmentId;
        }

        if (isHeader || workType != WorkType.General)
        {
            return null;
        }

        if (!ownerDepartmentId.HasValue)
        {
            throw new DomainException("owner_department_required", "Owner department is required for main plan general tasks.");
        }

        await ApplicationSupport.RequireActiveDepartmentAsync(db, ownerDepartmentId, ct);
        return ownerDepartmentId.Value;
    }

    public static void ValidateDeadline(Plan plan, DateTime? deadline)
    {
        if (deadline.HasValue && deadline.Value.Date < plan.CreatedAt.Date)
        {
            throw new DomainException("deadline_before_plan", "Deadline must be greater than or equal to plan created date.");
        }
    }

    public static void ValidateOverdue(WorkStatus workStatus, DateTime? deadline, string? reasonNotCompleted)
    {
        if (workStatus == WorkStatus.Overdue &&
            deadline.HasValue &&
            deadline.Value.Date < DateTime.UtcNow.Date &&
            string.IsNullOrWhiteSpace(reasonNotCompleted))
        {
            throw new DomainException("reason_required", "Reason not completed is required for overdue tasks.");
        }
    }

    public static async System.Threading.Tasks.Task ValidateParentAsync(
        IApplicationDbContext db,
        Guid planId,
        Guid? parentTaskId,
        Guid? taskId,
        CancellationToken ct)
    {
        if (!parentTaskId.HasValue)
        {
            return;
        }

        if (taskId.HasValue && parentTaskId.Value == taskId.Value)
        {
            throw new DomainException("task_cycle", "Task cannot be parent of itself.");
        }

        var parent = await db.Tasks
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == parentTaskId.Value, ct)
            ?? throw new DomainException("parent_task_invalid", "Parent task is invalid.");

        if (parent.PlanId != planId)
        {
            throw new DomainException("parent_task_invalid", "Parent task must belong to the same plan.");
        }

        if (!taskId.HasValue)
        {
            return;
        }

        var cursor = parent.ParentTaskId;
        while (cursor.HasValue)
        {
            if (cursor.Value == taskId.Value)
            {
                throw new DomainException("task_cycle", "Task parent cannot be a descendant.");
            }

            cursor = await db.Tasks
                .AsNoTracking()
                .Where(x => x.Id == cursor.Value)
                .Select(x => x.ParentTaskId)
                .FirstOrDefaultAsync(ct);
        }
    }

    public static async System.Threading.Tasks.Task<List<Guid>> ValidateSupportingDepartmentsAsync(
        IApplicationDbContext db,
        Plan plan,
        IEnumerable<Guid> departmentIds,
        CancellationToken ct)
    {
        var ids = departmentIds.Distinct().ToList();
        if (plan.Scope == PlanScope.Sub && plan.DepartmentId.HasValue && ids.Contains(plan.DepartmentId.Value))
        {
            throw new DomainException("supporting_dept_invalid", "Supporting department cannot be the owner department.");
        }

        foreach (var id in ids)
        {
            await ApplicationSupport.RequireActiveDepartmentAsync(db, id, ct);
        }

        return ids;
    }

    public static void ApplySupportingDepartments(TaskEntity task, IEnumerable<Guid> departmentIds)
    {
        var target = departmentIds.Distinct().ToHashSet();
        var current = task.SupportingDepts.Select(x => x.DepartmentId).ToHashSet();

        foreach (var item in task.SupportingDepts.Where(x => !target.Contains(x.DepartmentId)).ToList())
        {
            task.SupportingDepts.Remove(item);
        }

        foreach (var id in target.Except(current))
        {
            task.SupportingDepts.Add(new TaskSupportingDept
            {
                TaskId = task.Id,
                DepartmentId = id
            });
        }
    }

    public static void EnsureProgressOnlyPayload(TaskEntity task, UpdateTaskValues values)
    {
        if (values.ParentTaskId != task.ParentTaskId ||
            values.OutlineIndex != task.OutlineIndex ||
            values.DisplayOrder != task.DisplayOrder ||
            values.IsHeader != task.IsHeader ||
            values.Title != task.Title ||
            values.WorkType != task.WorkType ||
            values.Deadline != task.Deadline ||
            values.AssigneeUserId != task.AssigneeUserId ||
            values.OwnerDepartmentId != task.OwnerDepartmentId ||
            values.BksMemberText != task.BksMemberText ||
            values.KtnbLeaderText != task.KtnbLeaderText ||
            values.NoteText != task.NoteText ||
            !values.SupportingDepartmentIds.OrderBy(x => x).SequenceEqual(task.SupportingDepts.Select(x => x.DepartmentId).OrderBy(x => x)))
        {
            throw new ForbiddenException("forbidden_field_change", "Only progress fields can be changed.");
        }
    }

    public static string WorkStatusCode(WorkStatus status) =>
        status switch
        {
            WorkStatus.NotStarted => "not_started",
            WorkStatus.InProgress => "in_progress",
            WorkStatus.Done => "done",
            WorkStatus.Overdue => "overdue",
            WorkStatus.Paused => "paused",
            _ => status.ToString().ToLowerInvariant()
        };

    public static WorkStatus ParseWorkStatus(string status) =>
        status.Trim().ToLowerInvariant() switch
        {
            "not_started" => WorkStatus.NotStarted,
            "in_progress" => WorkStatus.InProgress,
            "done" => WorkStatus.Done,
            "overdue" => WorkStatus.Overdue,
            "paused" => WorkStatus.Paused,
            _ => throw new DomainException("work_status_invalid", "Work status is invalid.")
        };

    public static string TaskApprovalStatusCode(TaskApprovalStatus status) =>
        status switch
        {
            TaskApprovalStatus.Draft => "draft",
            TaskApprovalStatus.PendingTeam => "pending_team",
            TaskApprovalStatus.ApprovedTeam => "approved_team",
            TaskApprovalStatus.ApprovedDepartment => "approved_department",
            TaskApprovalStatus.ApprovedFinal => "approved_final",
            TaskApprovalStatus.Returned => "returned",
            _ => status.ToString().ToLowerInvariant()
        };

    public static TaskApprovalStatus ParseTaskApprovalStatus(string status) =>
        status.Trim().ToLowerInvariant() switch
        {
            "draft" => TaskApprovalStatus.Draft,
            "pending_team" => TaskApprovalStatus.PendingTeam,
            "approved_team" => TaskApprovalStatus.ApprovedTeam,
            "approved_department" => TaskApprovalStatus.ApprovedDepartment,
            "approved_final" => TaskApprovalStatus.ApprovedFinal,
            "returned" => TaskApprovalStatus.Returned,
            _ => throw new DomainException("task_approval_status_invalid", "Task approval status is invalid.")
        };

    private static void EnsureSubDepartment(Plan plan)
    {
        if (plan.Scope == PlanScope.Sub && !plan.DepartmentId.HasValue)
        {
            throw new DomainException("plan_department_missing", "Sub plan department is missing.");
        }
    }
}

public record UpdateTaskValues(
    Guid? ParentTaskId,
    string? OutlineIndex,
    int DisplayOrder,
    bool IsHeader,
    string Title,
    WorkType WorkType,
    WorkStatus WorkStatus,
    DateTime? Deadline,
    Guid? AssigneeUserId,
    Guid? OwnerDepartmentId,
    string? BksMemberText,
    string? KtnbLeaderText,
    string? NoteText,
    string? ProgressText,
    string? ReasonNotCompleted,
    IReadOnlyList<Guid> SupportingDepartmentIds);
