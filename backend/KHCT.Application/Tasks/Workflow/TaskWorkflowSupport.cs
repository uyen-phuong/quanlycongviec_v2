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

    public static TaskApprovalStatus AggregateStatus(IEnumerable<TaskEntity> tasks)
    {
        var taskList = tasks.ToList();
        if (taskList.Count == 0)
        {
            return TaskApprovalStatus.Draft;
        }

        if (taskList.Any(x => x.ApprovalStatus == TaskApprovalStatus.Returned))
        {
            return TaskApprovalStatus.Returned;
        }

        if (taskList.All(x => x.ApprovalStatus == TaskApprovalStatus.ApprovedFinal))
        {
            return TaskApprovalStatus.ApprovedFinal;
        }

        if (taskList.All(x => x.ApprovalStatus is TaskApprovalStatus.ApprovedDepartment or TaskApprovalStatus.ApprovedFinal) &&
            taskList.Any(x => x.ApprovalStatus == TaskApprovalStatus.ApprovedDepartment))
        {
            return TaskApprovalStatus.ApprovedDepartment;
        }

        if (taskList.All(x => x.ApprovalStatus is TaskApprovalStatus.ApprovedTeam or TaskApprovalStatus.ApprovedDepartment or TaskApprovalStatus.ApprovedFinal) &&
            taskList.Any(x => x.ApprovalStatus == TaskApprovalStatus.ApprovedTeam))
        {
            return TaskApprovalStatus.ApprovedTeam;
        }

        if (taskList.Any(x => x.ApprovalStatus == TaskApprovalStatus.PendingTeam))
        {
            return TaskApprovalStatus.PendingTeam;
        }

        return TaskApprovalStatus.Draft;
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

    public static TaskApprovalStatus EnsureCanApprove(
        ICurrentUser currentUser,
        Guid? departmentId,
        TaskApprovalStatus currentStatus)
    {
        if (!departmentId.HasValue)
        {
            if (currentStatus == TaskApprovalStatus.PendingTeam && PlanSupport.HasRole(currentUser, PlanSupport.RoleTruongKh))
            {
                return TaskApprovalStatus.ApprovedTeam;
            }

            if (currentStatus == TaskApprovalStatus.ApprovedTeam && PlanSupport.HasRole(currentUser, PlanSupport.RoleTruongKtnb))
            {
                return TaskApprovalStatus.ApprovedFinal;
            }

            throw new ForbiddenException("forbidden_role", "Current role cannot approve this workflow.");
        }

        if (currentStatus == TaskApprovalStatus.PendingTeam && PlanSupport.HasRole(currentUser, PlanSupport.RoleTruongNhom))
        {
            return TaskApprovalStatus.ApprovedTeam;
        }

        if (currentStatus == TaskApprovalStatus.ApprovedTeam &&
            PlanSupport.HasRole(currentUser, PlanSupport.RoleTruongPhong) &&
            currentUser.DepartmentId == departmentId.Value)
        {
            return TaskApprovalStatus.ApprovedDepartment;
        }

        if (currentStatus == TaskApprovalStatus.ApprovedDepartment && PlanSupport.HasRole(currentUser, PlanSupport.RolePhoTruongKtnb))
        {
            return TaskApprovalStatus.ApprovedFinal;
        }

        throw new ForbiddenException("forbidden_role", "Current role cannot approve this workflow.");
    }

    public static void EnsureCanReturn(
        ICurrentUser currentUser,
        Guid? departmentId,
        TaskApprovalStatus currentStatus)
    {
        _ = EnsureCanApprove(currentUser, departmentId, currentStatus);
    }

    public static void EnsureValidForSubmit(IEnumerable<TaskEntity> tasks)
    {
        var invalid = tasks.FirstOrDefault(x => x.ApprovalStatus is not (TaskApprovalStatus.Draft or TaskApprovalStatus.Returned));
        if (invalid is not null)
        {
            throw new DomainException("task_workflow_invalid_submit", "One or more tasks are not ready for submit.");
        }
    }

    public static void EnsureValidForApprove(IEnumerable<TaskEntity> tasks, TaskApprovalStatus expectedStatus)
    {
        var invalid = tasks.FirstOrDefault(x => x.ApprovalStatus != expectedStatus);
        if (invalid is not null)
        {
            throw new DomainException("task_workflow_invalid_approve", "One or more tasks are not on the expected approval step.");
        }
    }

    public static void EnsureValidForReturn(IEnumerable<TaskEntity> tasks)
    {
        var invalid = tasks.FirstOrDefault(x => x.ApprovalStatus is TaskApprovalStatus.Draft or TaskApprovalStatus.Returned);
        if (invalid is not null)
        {
            throw new DomainException("task_workflow_invalid_return", "One or more tasks cannot be returned at this stage.");
        }
    }

    public static TaskApprovalHistory CreateHistory(
        TaskEntity task,
        Guid? departmentId,
        ApprovalAction action,
        TaskApprovalStatus fromStatus,
        TaskApprovalStatus toStatus,
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
            TaskSupport.TaskApprovalStatusCode(history.FromStatus),
            TaskSupport.TaskApprovalStatusCode(history.ToStatus),
            history.ActorUserId,
            history.ActorUser?.FullName,
            history.Comment,
            history.CreatedAt);
}
