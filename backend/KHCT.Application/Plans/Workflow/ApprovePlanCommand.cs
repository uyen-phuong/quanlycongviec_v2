using FluentValidation;
using KHCT.Application.Common.Interfaces;
using KHCT.Application.Common.Support;
using KHCT.Application.Notifications;
using KHCT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Plans.Workflow;

public record ApprovePlanCommand(Guid PlanId, string? Comment) : IRequest<PlanDetailDto>;

public class ApprovePlanCommandValidator : AbstractValidator<ApprovePlanCommand>
{
    public ApprovePlanCommandValidator()
    {
        RuleFor(x => x.PlanId).NotEmpty();
        RuleFor(x => x.Comment).MaximumLength(2000);
    }
}

public class ApprovePlanHandler : IRequestHandler<ApprovePlanCommand, PlanDetailDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public ApprovePlanHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PlanDetailDto> Handle(ApprovePlanCommand request, CancellationToken ct)
    {
        var plan = await _db.Plans
            .Include(x => x.Department)
            .Include(x => x.CreatedBy)
            .Include(x => x.Tasks)
            .FirstOrDefaultAsync(x => x.Id == request.PlanId, ct)
            ?? throw new KeyNotFoundException("Plan not found.");

        var fromStatus = plan.Status;
        var nextStatus = WorkflowSupport.EnsureCanApprove(plan, _currentUser);

        var triggersInherit = plan.Scope == PlanScope.Main
            && fromStatus == ApprovalStatus.Approved1
            && nextStatus == ApprovalStatus.Approved2;

        var triggersSync = plan.Scope == PlanScope.Sub
            && fromStatus == ApprovalStatus.Approved2
            && nextStatus == ApprovalStatus.Approved3;

        if (triggersInherit)
        {
            InheritService.EnsureInheritReady(plan);
        }

        if (triggersSync)
        {
            await SyncService.EnsureSyncReadyAsync(_db, plan, ct);
        }

        plan.Status = nextStatus;
        if (nextStatus is ApprovalStatus.Approved2 or ApprovalStatus.Approved3)
        {
            plan.ApprovedAt = DateTime.UtcNow;
        }

        _db.ApprovalHistories.Add(WorkflowSupport.CreateHistory(
            plan,
            ApprovalAction.Approve,
            fromStatus,
            nextStatus,
            PlanSupport.RequireActorId(_currentUser),
            request.Comment));

        _db.AuditLogs.Add(ApplicationSupport.CreateAudit(
            "plan",
            plan.Id,
            "approve",
            _currentUser.UserId,
            PlanSupport.Snapshot(new Domain.Entities.Plan
            {
                Id = plan.Id,
                Scope = plan.Scope,
                DepartmentId = plan.DepartmentId,
                Year = plan.Year,
                Month = plan.Month,
                Status = fromStatus,
                CreatedById = plan.CreatedById
            }),
            PlanSupport.Snapshot(plan)));

        if (triggersInherit)
        {
            await InheritService.RunAsync(_db, plan, _currentUser.UserId, ct);
        }

        if (triggersSync)
        {
            await SyncService.RunAsync(_db, plan, _currentUser.UserId, ct);
        }

        await NotificationHelper.OnPlanApprovedAsync(_db, plan, nextStatus, _currentUser.UserId, ct);
        await _db.SaveChangesAsync(ct);
        return PlanSupport.ToDetail(plan);
    }
}
