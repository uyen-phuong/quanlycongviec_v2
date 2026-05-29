using FluentValidation;
using KHCT.Application.Common.Interfaces;
using KHCT.Application.Plans;
using KHCT.Domain.Common;
using KHCT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Tasks.Workflow;

public record SubmitTaskWorkflowCommand(Guid PlanId, string? DepartmentCode, string? Comment) : IRequest<string>;

public class SubmitTaskWorkflowCommandValidator : AbstractValidator<SubmitTaskWorkflowCommand>
{
    public SubmitTaskWorkflowCommandValidator()
    {
        RuleFor(x => x.PlanId).NotEmpty();
        RuleFor(x => x.Comment).MaximumLength(2000);
        RuleFor(x => x.DepartmentCode).MaximumLength(32);
    }
}

public class SubmitTaskWorkflowHandler : IRequestHandler<SubmitTaskWorkflowCommand, string>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public SubmitTaskWorkflowHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<string> Handle(SubmitTaskWorkflowCommand request, CancellationToken ct)
    {
        var plan = await _db.Plans.FirstOrDefaultAsync(x => x.Id == request.PlanId, ct)
            ?? throw new KeyNotFoundException("Plan not found.");

        var department = await TaskWorkflowSupport.ResolveDepartmentAsync(_db, request.DepartmentCode, ct);
        var workflowTasks = await TaskWorkflowSupport.LoadWorkflowTasksAsync(_db, plan.Id, department?.Id, ct);
        if (workflowTasks.Count == 0)
        {
            throw new DomainException("task_workflow_empty", "No tasks found for this workflow scope.");
        }

        TaskWorkflowSupport.EnsureCanSubmit(_currentUser, department?.Id);
        TaskWorkflowSupport.EnsureValidForSubmit(workflowTasks);

        var actorId = PlanSupport.RequireActorId(_currentUser);
        var now = DateTime.UtcNow;
        foreach (var task in workflowTasks)
        {
            var fromStatus = task.ApprovalStatus;
            task.ApprovalStatus = TaskApprovalStatus.PendingTeam;
            task.SubmittedAt = now;
            task.ApprovedAt = null;
            _db.TaskApprovalHistories.Add(TaskWorkflowSupport.CreateHistory(
                task,
                department?.Id,
                fromStatus == TaskApprovalStatus.Returned ? ApprovalAction.Resubmit : ApprovalAction.Submit,
                fromStatus,
                task.ApprovalStatus,
                actorId,
                request.Comment));
        }

        await _db.SaveChangesAsync(ct);
        return TaskSupport.TaskApprovalStatusCode(TaskWorkflowSupport.AggregateStatus(workflowTasks));
    }
}
