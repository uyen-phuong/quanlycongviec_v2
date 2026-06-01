using KHCT.Application.Common.Interfaces;
using KHCT.Application.Plans;
using KHCT.Domain.Common;
using KHCT.Domain.Entities;
using KHCT.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using TaskEntity = KHCT.Domain.Entities.Task;

namespace KHCT.Application.Tasks.Workflow;

public static class TaskWorkflowSupport
{
    public static async Task<Department?> ResolveDepartmentAsync(
        IApplicationDbContext db,
        string? departmentCode,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(departmentCode))
        {
            return null;
        }

        var normalizedCode = departmentCode.Trim().ToUpperInvariant();
        return await db.Departments.FirstOrDefaultAsync(x => x.Code == normalizedCode, ct);
    }

    public static IQueryable<TaskEntity> ApplyWorkflowScope(
        IQueryable<TaskEntity> query,
        Guid planId,
        Guid? departmentId) =>
        query.Where(x =>
            x.PlanId == planId &&
            !x.IsHeader &&
            x.OwnerDepartmentId.HasValue &&
            (!departmentId.HasValue || x.OwnerDepartmentId == departmentId.Value));

    public static async Task<List<TaskEntity>> LoadWorkflowTasksAsync(
        IApplicationDbContext db,
        Guid planId,
        Guid? departmentId,
        CancellationToken ct) =>
        await ApplyWorkflowScope(
                db.Tasks
                    .Include(x => x.Plan)
                    .Include(x => x.OwnerDepartment)
                    .Include(x => x.SupportingDepts),
                planId,
                departmentId)
            .ToListAsync(ct);

    public static TaskWorkflowStatus AggregateStatus(IEnumerable<TaskEntity> tasks)
    {
        var taskList = tasks.ToList();
        if (taskList.Count == 0)
        {
            return TaskWorkflowStatus.New;
        }

        if (taskList.Any(x => x.WorkflowStatus == TaskWorkflowStatus.Returned))
        {
            return TaskWorkflowStatus.Returned;
        }

        if (taskList.All(x => x.WorkflowStatus == TaskWorkflowStatus.Completed))
        {
            return TaskWorkflowStatus.Completed;
        }

        if (taskList.All(x => x.WorkflowStatus is TaskWorkflowStatus.Completed or TaskWorkflowStatus.Completed) &&
            taskList.Any(x => x.WorkflowStatus == TaskWorkflowStatus.Completed))
        {
            return TaskWorkflowStatus.Completed;
        }

        if (taskList.All(x => x.WorkflowStatus is TaskWorkflowStatus.PendingApproval or TaskWorkflowStatus.Completed or TaskWorkflowStatus.Completed) &&
            taskList.Any(x => x.WorkflowStatus == TaskWorkflowStatus.PendingApproval))
        {
            return TaskWorkflowStatus.PendingApproval;
        }

        if (taskList.Any(x => x.WorkflowStatus == TaskWorkflowStatus.PendingReview))
        {
            return TaskWorkflowStatus.PendingReview;
        }

        return TaskWorkflowStatus.New;
    }

    public static void EnsureCanSubmit(ICurrentUser currentUser, Guid? departmentId)
    {
        if (!departmentId.HasValue)
        {
            if (PlanSupport.HasRole(currentUser, PlanSupport.RoleVanThu) || PlanSupport.HasRole(currentUser, PlanSupport.RoleAdmin))
            {
                return;
            }

            throw new ForbiddenException("forbidden_role", "Current role cannot submit this workflow.");
        }

        if (PlanSupport.HasRole(currentUser, PlanSupport.RolePhoTruongKtnb))
        {
            return;
        }

        if (PlanSupport.HasRole(currentUser, PlanSupport.RoleTruongPhong) && currentUser.DepartmentId == departmentId.Value)
        {
            return;
        }

        throw new ForbiddenException("forbidden_role", "Current role cannot submit this workflow.");
    }

    public static TaskWorkflowStatus EnsureCanApprove(
        ICurrentUser currentUser,
        Guid? departmentId,
        TaskWorkflowStatus currentStatus)
    {
        // Main plan tasks (departmentId = null): VAN_THU tạo, TRUONG_PHONG kiểm soát, TRUONG_KTNB phê duyệt
        if (!departmentId.HasValue)
        {
            if (currentStatus == TaskWorkflowStatus.PendingReview &&
                (PlanSupport.HasRole(currentUser, PlanSupport.RoleTruongPhong) ||
                 PlanSupport.HasRole(currentUser, PlanSupport.RoleAdmin)))
            {
                return TaskWorkflowStatus.PendingApproval;
            }

            if (currentStatus == TaskWorkflowStatus.PendingApproval &&
                (PlanSupport.HasRole(currentUser, PlanSupport.RoleTruongKtnb) ||
                 PlanSupport.HasRole(currentUser, PlanSupport.RolePhoTruongKtnb) ||
                 PlanSupport.HasRole(currentUser, PlanSupport.RoleAdmin)))
            {
                return TaskWorkflowStatus.Completed;
            }

            throw new ForbiddenException("forbidden_role", "Current role cannot approve this workflow.");
        }

        // Sub plan tasks: PHO_PHONG (Phó phòng/Trưởng nhóm) kiểm soát, TRUONG_PHONG phê duyệt
        if (currentStatus == TaskWorkflowStatus.PendingReview &&
            (PlanSupport.HasRole(currentUser, PlanSupport.RolePhoPhong) ||
             PlanSupport.HasRole(currentUser, PlanSupport.RoleTruongPhong) ||
             PlanSupport.HasRole(currentUser, PlanSupport.RoleAdmin)))
        {
            return TaskWorkflowStatus.PendingApproval;
        }

        if (currentStatus == TaskWorkflowStatus.PendingApproval &&
            (PlanSupport.HasRole(currentUser, PlanSupport.RoleTruongPhong) ||
             PlanSupport.HasRole(currentUser, PlanSupport.RoleAdmin)) &&
            (currentUser.DepartmentId == departmentId.Value || PlanSupport.HasRole(currentUser, PlanSupport.RoleAdmin)))
        {
            return TaskWorkflowStatus.Completed;
        }

        // PHO_TRUONG_KTNB có thể phê duyệt cuối cùng (approved_3)
        if (currentStatus == TaskWorkflowStatus.Completed &&
            PlanSupport.HasRole(currentUser, PlanSupport.RolePhoTruongKtnb))
        {
            return TaskWorkflowStatus.Completed;
        }

        throw new ForbiddenException("forbidden_role", "Current role cannot approve this workflow.");
    }

    public static void EnsureCanReturn(
        ICurrentUser currentUser,
        Guid? departmentId,
        TaskWorkflowStatus currentStatus)
    {
        _ = EnsureCanApprove(currentUser, departmentId, currentStatus);
    }

    public static void EnsureValidForSubmit(IEnumerable<TaskEntity> tasks)
    {
        var invalid = tasks.FirstOrDefault(x => x.WorkflowStatus is not (TaskWorkflowStatus.New or TaskWorkflowStatus.Returned));
        if (invalid is not null)
        {
            throw new DomainException("task_workflow_invalid_submit", "One or more tasks are not ready for submit.");
        }
    }

    public static void EnsureValidForApprove(IEnumerable<TaskEntity> tasks, TaskWorkflowStatus expectedStatus)
    {
        var invalid = tasks.FirstOrDefault(x => x.WorkflowStatus != expectedStatus);
        if (invalid is not null)
        {
            throw new DomainException("task_workflow_invalid_approve", "One or more tasks are not on the expected approval step.");
        }
    }

    public static void EnsureValidForReturn(IEnumerable<TaskEntity> tasks)
    {
        var invalid = tasks.FirstOrDefault(x => x.WorkflowStatus is TaskWorkflowStatus.New or TaskWorkflowStatus.Returned);
        if (invalid is not null)
        {
            throw new DomainException("task_workflow_invalid_return", "One or more tasks cannot be returned at this stage.");
        }
    }

    public static TaskApprovalHistory CreateHistory(
        TaskEntity task,
        Guid? departmentId,
        ApprovalAction action,
        TaskWorkflowStatus fromStatus,
        TaskWorkflowStatus toStatus,
        Guid actorUserId,
        string? comment) =>
        new()
        {
            Id = Guid.NewGuid(),
            TaskId = task.Id,
            DepartmentId = departmentId,
            Action = action,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            ActorUserId = actorUserId,
            Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim()
        };

    public static TaskApprovalHistoryDto ToDto(TaskApprovalHistory history) =>
        new(
            history.Id,
            history.TaskId,
            history.DepartmentId,
            history.Action.ToString().ToLowerInvariant(),
            TaskSupport.TaskWorkflowStatusCode(history.FromStatus),
            TaskSupport.TaskWorkflowStatusCode(history.ToStatus),
            history.ActorUserId,
            history.ActorUser?.FullName,
            history.Comment,
            history.CreatedAt);
}
