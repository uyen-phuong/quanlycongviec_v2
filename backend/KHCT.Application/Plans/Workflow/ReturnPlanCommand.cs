using FluentValidation;
using KHCT.Application.Common.Interfaces;
using KHCT.Application.Common.Support;
using KHCT.Application.Notifications;
using KHCT.Domain.Common;
using KHCT.Domain.Entities;
using KHCT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Plans.Workflow;

public record ReturnTaskCommentItem(Guid TaskId, string Content);

public record ReturnPlanCommand(Guid PlanId, string? Comment, IReadOnlyList<ReturnTaskCommentItem> LineComments) : IRequest<PlanDetailDto>;

public class ReturnPlanCommandValidator : AbstractValidator<ReturnPlanCommand>
{
    public ReturnPlanCommandValidator()
    {
        RuleFor(x => x.PlanId).NotEmpty();
        RuleFor(x => x.Comment).MaximumLength(2000);
        RuleFor(x => x.LineComments).NotNull().Must(x => x.Count > 0).WithMessage("At least one line comment is required.");
        RuleForEach(x => x.LineComments).ChildRules(child =>
        {
            child.RuleFor(x => x.TaskId).NotEmpty();
            child.RuleFor(x => x.Content).NotEmpty().MaximumLength(4000);
        });
    }
}

public class ReturnPlanHandler : IRequestHandler<ReturnPlanCommand, PlanDetailDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public ReturnPlanHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PlanDetailDto> Handle(ReturnPlanCommand request, CancellationToken ct)
    {
        var plan = await _db.Plans
            .Include(x => x.Department)
            .Include(x => x.CreatedBy)
            .Include(x => x.Tasks)
            .FirstOrDefaultAsync(x => x.Id == request.PlanId, ct)
            ?? throw new KeyNotFoundException("Plan not found.");

        WorkflowSupport.EnsureCanReturn(plan, _currentUser);
        if (plan.Status == ApprovalStatus.Returned || plan.Status == ApprovalStatus.Draft)
        {
            throw new DomainException("plan_return_invalid", "Plan cannot be returned at this stage.");
        }

        var taskIds = request.LineComments.Select(x => x.TaskId).Distinct().ToList();
        if (taskIds.Count != request.LineComments.Count)
        {
            throw new DomainException("line_comment_duplicate_task", "Each task can only be commented once per return.");
        }

        var tasks = await _db.Tasks
            .Where(x => taskIds.Contains(x.Id) && x.PlanId == plan.Id)
            .ToListAsync(ct);

        if (tasks.Count != taskIds.Count)
        {
            throw new DomainException("line_comment_task_invalid", "One or more line comments reference invalid tasks.");
        }

        var actorId = PlanSupport.RequireActorId(_currentUser);
        var authorRole = WorkflowSupport.ResolveCommentRole(_currentUser);
        foreach (var item in request.LineComments)
        {
            var task = tasks.First(x => x.Id == item.TaskId);
            task.HasOpenComment = true;
            _db.LineComments.Add(new LineComment
            {
                Id = Guid.NewGuid(),
                TaskId = task.Id,
                AuthorUserId = actorId,
                AuthorRole = authorRole,
                Content = item.Content.Trim(),
                IsResolved = false
            });
        }

        var fromStatus = plan.Status;
        plan.Status = ApprovalStatus.Returned;
        plan.ApprovedAt = null;

        _db.ApprovalHistories.Add(WorkflowSupport.CreateHistory(
            plan,
            ApprovalAction.Return,
            fromStatus,
            ApprovalStatus.Returned,
            actorId,
            request.Comment));

        _db.AuditLogs.Add(ApplicationSupport.CreateAudit(
            "plan",
            plan.Id,
            "return",
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

        await NotificationHelper.OnPlanReturnedAsync(_db, plan, _currentUser.UserId, ct);
        await _db.SaveChangesAsync(ct);
        return PlanSupport.ToDetail(plan);
    }
}
