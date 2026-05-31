using FluentValidation;
using KHCT.Application.Common.Interfaces;
using KHCT.Application.Plans;
using KHCT.Domain.Common;
using KHCT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Tasks.Workflow;

public record SubmitSingleTaskCommand(Guid TaskId, string? Comment) : IRequest<TaskDetailDto>;

public class SubmitSingleTaskCommandValidator : AbstractValidator<SubmitSingleTaskCommand>
{
    public SubmitSingleTaskCommandValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty();
        RuleFor(x => x.Comment).MaximumLength(2000);
    }
}

public class SubmitSingleTaskHandler : IRequestHandler<SubmitSingleTaskCommand, TaskDetailDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public SubmitSingleTaskHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<TaskDetailDto> Handle(SubmitSingleTaskCommand request, CancellationToken ct)
    {
        var task = await _db.Tasks
            .Include(x => x.Plan)
            .Include(x => x.OwnerDepartment)
            .Include(x => x.SupportingDepts)
                .ThenInclude(x => x.Department)
            .FirstOrDefaultAsync(x => x.Id == request.TaskId, ct)
            ?? throw new KeyNotFoundException("Task not found.");

        if (task.Category == TaskCategory.PlanTask && task.Plan == null)
        {
            throw new KeyNotFoundException("Plan not found.");
        }
        
        // Ensure user can submit task under their department scope
        TaskWorkflowSupport.EnsureCanSubmit(_currentUser, task.OwnerDepartmentId);

        if (task.WorkflowStatus is not (TaskWorkflowStatus.New or TaskWorkflowStatus.Returned or TaskWorkflowStatus.PendingAssign or TaskWorkflowStatus.InProgress))
        {
            throw new DomainException("task_workflow_invalid_submit", "Task is not in a status that can be submitted.");
        }

        var actorId = PlanSupport.RequireActorId(_currentUser);
        var fromStatus = task.WorkflowStatus;
        
        // If task has no assignee, transition to PendingAssign. Else transition to PendingReview
        var targetStatus = task.AssigneeUserId.HasValue ? TaskWorkflowStatus.PendingReview : TaskWorkflowStatus.PendingAssign;
        
        task.WorkflowStatus = targetStatus;
        task.SubmittedAt = DateTime.UtcNow;
        task.ApprovedAt = null;

        _db.TaskApprovalHistories.Add(TaskWorkflowSupport.CreateHistory(
            task,
            task.OwnerDepartmentId,
            fromStatus == TaskWorkflowStatus.Returned ? ApprovalAction.Resubmit : ApprovalAction.Submit,
            fromStatus,
            task.WorkflowStatus,
            actorId,
            request.Comment));

        await _db.SaveChangesAsync(ct);
        return TaskSupport.ToDetail(task);
    }
}
