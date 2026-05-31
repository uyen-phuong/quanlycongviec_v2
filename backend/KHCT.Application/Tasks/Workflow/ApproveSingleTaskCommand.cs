using FluentValidation;
using KHCT.Application.Common.Interfaces;
using KHCT.Application.Plans;
using KHCT.Domain.Common;
using KHCT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Tasks.Workflow;

public record ApproveSingleTaskCommand(Guid TaskId, string? Comment) : IRequest<TaskDetailDto>;

public class ApproveSingleTaskCommandValidator : AbstractValidator<ApproveSingleTaskCommand>
{
    public ApproveSingleTaskCommandValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty();
        RuleFor(x => x.Comment).MaximumLength(2000);
    }
}

public class ApproveSingleTaskHandler : IRequestHandler<ApproveSingleTaskCommand, TaskDetailDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public ApproveSingleTaskHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<TaskDetailDto> Handle(ApproveSingleTaskCommand request, CancellationToken ct)
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
        
        // Ensure user can approve based on current workflow status
        var nextStatus = TaskWorkflowSupport.EnsureCanApprove(_currentUser, task.OwnerDepartmentId, task.WorkflowStatus);

        var actorId = PlanSupport.RequireActorId(_currentUser);
        var fromStatus = task.WorkflowStatus;

        task.WorkflowStatus = nextStatus;
        if (nextStatus == TaskWorkflowStatus.Completed)
        {
            task.ApprovedAt = DateTime.UtcNow;
        }

        _db.TaskApprovalHistories.Add(TaskWorkflowSupport.CreateHistory(
            task,
            task.OwnerDepartmentId,
            ApprovalAction.Approve,
            fromStatus,
            task.WorkflowStatus,
            actorId,
            request.Comment));

        await _db.SaveChangesAsync(ct);
        return TaskSupport.ToDetail(task);
    }
}
