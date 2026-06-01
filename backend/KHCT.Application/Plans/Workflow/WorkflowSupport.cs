using KHCT.Application.Common.Interfaces;
using KHCT.Domain.Common;
using KHCT.Domain.Entities;
using KHCT.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Plans.Workflow;

public static class WorkflowSupport
{
    public static void EnsureCanSubmit(Plan plan, ICurrentUser currentUser)
    {
        PlanSupport.EnsureEditable(plan);

        if (plan.Scope == PlanScope.Main)
        {
            if (PlanSupport.HasRole(currentUser, PlanSupport.RoleVanThu))
            {
                return;
            }

            throw new ForbiddenException("forbidden_role", "Current role cannot submit main plan.");
        }

        if (!plan.DepartmentId.HasValue)
        {
            throw new DomainException("plan_department_missing", "Sub plan department is missing.");
        }

        if (PlanSupport.HasRole(currentUser, PlanSupport.RolePhoTruongKtnb))
        {
            return;
        }

        if ((PlanSupport.HasRole(currentUser, PlanSupport.RoleTruongPhong) ||
             PlanSupport.HasRole(currentUser, PlanSupport.RolePhoPhong) ||
             PlanSupport.HasRole(currentUser, PlanSupport.RoleNhanVien)) &&
            currentUser.DepartmentId == plan.DepartmentId.Value)
        {
            return;
        }

        throw new ForbiddenException("forbidden_role", "Current role cannot submit sub plan.");
    }

    public static WorkflowStatus EnsureCanApprove(Plan plan, ICurrentUser currentUser)
    {
        if (plan.Scope == PlanScope.Main)
        {
            if (plan.Status == WorkflowStatus.Pending &&
                (PlanSupport.HasRole(currentUser, PlanSupport.RoleTruongPhong) ||
                 PlanSupport.HasRole(currentUser, PlanSupport.RoleAdmin)))
            {
                return WorkflowStatus.Approved1;
            }

            if (plan.Status == WorkflowStatus.Approved1 && PlanSupport.HasRole(currentUser, PlanSupport.RoleTruongKtnb))
            {
                return WorkflowStatus.Approved2;
            }

            throw new ForbiddenException("forbidden_role", "Current role cannot approve main plan at this stage.");
        }

        if (plan.Status == WorkflowStatus.Pending &&
            plan.DepartmentId.HasValue &&
            PlanSupport.HasRole(currentUser, PlanSupport.RoleTruongPhong) &&
            currentUser.DepartmentId == plan.DepartmentId.Value)
        {
            return WorkflowStatus.Approved2;
        }

        if (plan.Status == WorkflowStatus.Approved2 && PlanSupport.HasRole(currentUser, PlanSupport.RolePhoTruongKtnb))
        {
            return WorkflowStatus.Approved3;
        }

        throw new ForbiddenException("forbidden_role", "Current role cannot approve sub plan at this stage.");
    }

    public static void EnsureCanReturn(Plan plan, ICurrentUser currentUser)
    {
        _ = EnsureCanApprove(plan, currentUser);
    }

    public static void EnsureCanResolveComment(Plan plan, ICurrentUser currentUser)
    {
        if (plan.Scope == PlanScope.Main)
        {
            if (PlanSupport.HasRole(currentUser, PlanSupport.RoleVanThu) ||
                PlanSupport.HasRole(currentUser, PlanSupport.RoleAdmin))
            {
                return;
            }

            throw new ForbiddenException("forbidden_role", "Current role cannot resolve main plan comments.");
        }

        if (!plan.DepartmentId.HasValue)
        {
            throw new DomainException("plan_department_missing", "Sub plan department is missing.");
        }

        if (PlanSupport.HasRole(currentUser, PlanSupport.RolePhoTruongKtnb))
        {
            return;
        }

        if ((PlanSupport.HasRole(currentUser, PlanSupport.RoleTruongPhong) ||
            PlanSupport.HasRole(currentUser, PlanSupport.RoleNhanVien)) &&
            currentUser.DepartmentId == plan.DepartmentId.Value)
        {
            return;
        }

        throw new ForbiddenException("forbidden_role", "Current role cannot resolve sub plan comments.");
    }

    public static void EnsureCanReadWorkflow(Plan plan, ICurrentUser currentUser)
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

    public static ApprovalHistory CreateHistory(
        Plan plan,
        ApprovalAction action,
        WorkflowStatus fromStatus,
        WorkflowStatus toStatus,
        Guid actorUserId,
        string? comment) =>
        new()
        {
            Id = Guid.NewGuid(),
            PlanId = plan.Id,
            Action = action,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            ActorUserId = actorUserId,
            Comment = Normalize(comment)
        };

    public static CommentRole ResolveCommentRole(ICurrentUser currentUser)
    {
        if (PlanSupport.HasRole(currentUser, PlanSupport.RoleTruongPhong) ||
            PlanSupport.HasRole(currentUser, PlanSupport.RolePhoPhong))
        {
            return CommentRole.Controller;
        }

        if (PlanSupport.HasRole(currentUser, PlanSupport.RoleTruongKtnb) ||
            PlanSupport.HasRole(currentUser, PlanSupport.RolePhoTruongKtnb))
        {
            return CommentRole.Approver;
        }

        return CommentRole.Creator;
    }

    public static string ApprovalActionCode(ApprovalAction action) =>
        action switch
        {
            ApprovalAction.Submit => "submit",
            ApprovalAction.Approve => "approve",
            ApprovalAction.Return => "return",
            ApprovalAction.Resubmit => "resubmit",
            _ => action.ToString().ToLowerInvariant()
        };

    public static string CommentRoleCode(CommentRole role) =>
        role switch
        {
            CommentRole.Controller => "controller",
            CommentRole.Approver => "approver",
            CommentRole.Creator => "creator",
            _ => role.ToString().ToLowerInvariant()
        };

    public static ApprovalHistoryDto ToDto(ApprovalHistory history) =>
        new(
            history.Id,
            history.PlanId,
            ApprovalActionCode(history.Action),
            PlanSupport.StatusCode(history.FromStatus),
            PlanSupport.StatusCode(history.ToStatus),
            history.ActorUserId,
            history.ActorUser?.FullName,
            history.Comment,
            history.CreatedAt);

    public static LineCommentDto ToDto(LineComment comment) =>
        new(
            comment.Id,
            comment.TaskId,
            comment.Task?.Title ?? string.Empty,
            comment.Task?.OutlineIndex,
            comment.AuthorUserId,
            comment.AuthorUser?.FullName,
            CommentRoleCode(comment.AuthorRole),
            comment.Content,
            comment.IsResolved,
            comment.ResolvedAt,
            comment.ResolvedByUserId,
            comment.ResolvedByUser?.FullName,
            comment.CreatedAt);

    public static async Task<Plan> RequirePlanAsync(IApplicationDbContext db, Guid planId, CancellationToken ct) =>
        await db.Plans
            .Include(x => x.Department)
            .FirstOrDefaultAsync(x => x.Id == planId, ct)
        ?? throw new KeyNotFoundException("Plan not found.");

    public static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
