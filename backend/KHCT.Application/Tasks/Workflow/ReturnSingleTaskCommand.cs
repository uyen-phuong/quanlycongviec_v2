using FluentValidation;
using KHCT.Application.Common.Interfaces;
using KHCT.Application.Plans;
using KHCT.Domain.Common;
using KHCT.Domain.Entities;
using KHCT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Tasks.Workflow;

public record ReturnSingleTaskCommand(Guid TaskId, string Comment) : IRequest<TaskDetailDto>;

public class ReturnSingleTaskCommandValidator : AbstractValidator<ReturnSingleTaskCommand>
{
    public ReturnSingleTaskCommandValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty();
        RuleFor(x => x.Comment).NotEmpty().MaximumLength(2000);
    }
}

public class ReturnSingleTaskHandler : IRequestHandler<ReturnSingleTaskCommand, TaskDetailDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public ReturnSingleTaskHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<TaskDetailDto> Handle(ReturnSingleTaskCommand request, CancellationToken ct)
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
        
        // Ensure user can return (same role permissions as approve)
        TaskWorkflowSupport.EnsureCanReturn(_currentUser, task.OwnerDepartmentId, task.WorkflowStatus);

        var actorId = PlanSupport.RequireActorId(_currentUser);
        var fromStatus = task.WorkflowStatus;

        // Transition back to Returned
        task.WorkflowStatus = TaskWorkflowStatus.Returned;
        task.SubmittedAt = null;
        task.ApprovedAt = null;

        _db.TaskApprovalHistories.Add(TaskWorkflowSupport.CreateHistory(
            task,
            task.OwnerDepartmentId,
            ApprovalAction.Return,
            fromStatus,
            task.WorkflowStatus,
            actorId,
            request.Comment));

        // Create line comment automatically when returning a task to explain what to fix (PRD requirement!)
        var comment = new LineComment
        {
            Id = Guid.NewGuid(),
            TaskId = task.Id,
            AuthorUserId = actorId,
            AuthorRole = PlanSupport.HasRole(_currentUser, PlanSupport.RoleNhanVien) ? CommentRole.Creator :
                         PlanSupport.HasRole(_currentUser, PlanSupport.RoleTruongPhong) ? CommentRole.Approver : CommentRole.Controller,
            Content = request.Comment.Trim(),
            IsResolved = false
        };
        _db.LineComments.Add(comment);
        task.HasOpenComment = true;

        await _db.SaveChangesAsync(ct);
        return TaskSupport.ToDetail(task);
    }
}
