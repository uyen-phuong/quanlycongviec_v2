using FluentValidation;
using KHCT.Application.Common.Interfaces;
using KHCT.Application.Plans;
using KHCT.Domain.Common;
using KHCT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Tasks.Workflow;

public record AssignSingleTaskCommand(Guid TaskId, Guid AssigneeUserId, Guid? ControllerUserId) : IRequest<TaskDetailDto>;

public class AssignSingleTaskCommandValidator : AbstractValidator<AssignSingleTaskCommand>
{
    public AssignSingleTaskCommandValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty();
        RuleFor(x => x.AssigneeUserId).NotEmpty();
    }
}

public class AssignSingleTaskHandler : IRequestHandler<AssignSingleTaskCommand, TaskDetailDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public AssignSingleTaskHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<TaskDetailDto> Handle(AssignSingleTaskCommand request, CancellationToken ct)
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
        
        // Ensure user can assign tasks in this department (TRUONG_PHONG, PHO_TRUONG_KTNB, ADMIN)
        TaskWorkflowSupport.EnsureCanSubmit(_currentUser, task.OwnerDepartmentId);

        if (task.WorkflowStatus is not (TaskWorkflowStatus.New or TaskWorkflowStatus.PendingAssign or TaskWorkflowStatus.InProgress or TaskWorkflowStatus.Returned))
        {
            throw new DomainException("task_workflow_invalid_assign", "Task status is not valid for assignment.");
        }

        var actorId = PlanSupport.RequireActorId(_currentUser);
        var fromStatus = task.WorkflowStatus;

        task.AssigneeUserId = request.AssigneeUserId;
        if (request.ControllerUserId.HasValue)
        {
            task.ControllerUserId = request.ControllerUserId;
        }
        task.WorkflowStatus = TaskWorkflowStatus.InProgress;

        _db.TaskApprovalHistories.Add(TaskWorkflowSupport.CreateHistory(
            task,
            task.OwnerDepartmentId,
            ApprovalAction.Submit, // Map assignment to a Submit/Assign history action
            fromStatus,
            task.WorkflowStatus,
            actorId,
            "Giao việc cho cán bộ"));

        await _db.SaveChangesAsync(ct);
        
        var updatedTask = await _db.Tasks
            .Include(x => x.AssigneeUser)
            .Include(x => x.OwnerDepartment)
            .Include(x => x.SupportingDepts)
                .ThenInclude(x => x.Department)
            .FirstAsync(x => x.Id == task.Id, ct);

        return TaskSupport.ToDetail(updatedTask);
    }
}
