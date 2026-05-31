using FluentValidation;
using KHCT.Application.Common.Interfaces;
using KHCT.Application.Common.Support;
using KHCT.Application.Notifications;
using KHCT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Plans.Workflow;

public record SubmitPlanCommand(Guid PlanId, string? Comment) : IRequest<PlanDetailDto>;

public class SubmitPlanCommandValidator : AbstractValidator<SubmitPlanCommand>
{
    public SubmitPlanCommandValidator()
    {
        RuleFor(x => x.PlanId).NotEmpty();
        RuleFor(x => x.Comment).MaximumLength(2000);
    }
}

public class SubmitPlanHandler : IRequestHandler<SubmitPlanCommand, PlanDetailDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public SubmitPlanHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PlanDetailDto> Handle(SubmitPlanCommand request, CancellationToken ct)
    {
        var plan = await _db.Plans
            .Include(x => x.Department)
            .Include(x => x.CreatedBy)
            .Include(x => x.Tasks)
            .FirstOrDefaultAsync(x => x.Id == request.PlanId, ct)
            ?? throw new KeyNotFoundException("Plan not found.");

        WorkflowSupport.EnsureCanSubmit(plan, _currentUser);

        var fromStatus = plan.Status;
        if (fromStatus == WorkflowStatus.Returned && plan.Tasks.Any(x => x.HasOpenComment))
        {
            throw new Domain.Common.DomainException("plan_has_open_comments", "Resolve all open comments before resubmitting the plan.");
        }

        var action = fromStatus == WorkflowStatus.Returned ? ApprovalAction.Resubmit : ApprovalAction.Submit;
        plan.Status = WorkflowStatus.Pending;
        plan.SubmittedAt = DateTime.UtcNow;
        plan.ApprovedAt = null;

        _db.ApprovalHistories.Add(WorkflowSupport.CreateHistory(
            plan,
            action,
            fromStatus,
            plan.Status,
            PlanSupport.RequireActorId(_currentUser),
            request.Comment));

        _db.AuditLogs.Add(ApplicationSupport.CreateAudit(
            "plan",
            plan.Id,
            action == ApprovalAction.Resubmit ? "resubmit" : "submit",
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

        await NotificationHelper.OnPlanSubmittedAsync(_db, plan, _currentUser.UserId, ct);
        await _db.SaveChangesAsync(ct);
        return PlanSupport.ToDetail(plan);
    }
}
