using FluentValidation;
using KHCT.Application.Common.Interfaces;
using KHCT.Application.Plans;
using KHCT.Domain.Common;
using KHCT.Domain.Entities;
using KHCT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Tasks.Workflow;

public record ReturnTaskWorkflowCommentItem(Guid TaskId, string Content);

public record ReturnTaskWorkflowCommand(
    Guid PlanId,
    string? DepartmentCode,
    string? Comment,
    IReadOnlyList<ReturnTaskWorkflowCommentItem> LineComments) : IRequest<string>;

public class ReturnTaskWorkflowCommandValidator : AbstractValidator<ReturnTaskWorkflowCommand>
{
    public ReturnTaskWorkflowCommandValidator()
    {
        RuleFor(x => x.PlanId).NotEmpty();
        RuleFor(x => x.Comment).MaximumLength(2000);
        RuleFor(x => x.DepartmentCode).MaximumLength(32);
        RuleFor(x => x.LineComments).NotNull().Must(x => x.Count > 0).WithMessage("At least one line comment is required.");
        RuleForEach(x => x.LineComments).ChildRules(child =>
        {
            child.RuleFor(x => x.TaskId).NotEmpty();
            child.RuleFor(x => x.Content).NotEmpty().MaximumLength(4000);
        });
    }
}

public class ReturnTaskWorkflowHandler : IRequestHandler<ReturnTaskWorkflowCommand, string>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public ReturnTaskWorkflowHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<string> Handle(ReturnTaskWorkflowCommand request, CancellationToken ct)
    {
        var plan = await _db.Plans.FirstOrDefaultAsync(x => x.Id == request.PlanId, ct)
            ?? throw new KeyNotFoundException("Plan not found.");

        var department = await TaskWorkflowSupport.ResolveDepartmentAsync(_db, request.DepartmentCode, ct);
        var workflowTasks = await TaskWorkflowSupport.LoadWorkflowTasksAsync(_db, plan.Id, department?.Id, ct);
        if (workflowTasks.Count == 0)
        {
            throw new DomainException("task_workflow_empty", "No tasks found for this workflow scope.");
        }

        var currentStatus = TaskWorkflowSupport.AggregateStatus(workflowTasks);
        TaskWorkflowSupport.EnsureCanReturn(_currentUser, department?.Id, currentStatus);
        TaskWorkflowSupport.EnsureValidForReturn(workflowTasks);

        var taskMap = workflowTasks.ToDictionary(x => x.Id);
        var commentTaskIds = request.LineComments.Select(x => x.TaskId).Distinct().ToList();
        if (commentTaskIds.Count != request.LineComments.Count)
        {
            throw new DomainException("line_comment_duplicate_task", "Each task can only be commented once per return.");
        }

        if (commentTaskIds.Any(x => !taskMap.ContainsKey(x)))
        {
            throw new DomainException("line_comment_task_invalid", "One or more line comments reference invalid tasks.");
        }

        var actorId = PlanSupport.RequireActorId(_currentUser);
        var authorRole = Plans.Workflow.WorkflowSupport.ResolveCommentRole(_currentUser);
        foreach (var item in request.LineComments)
        {
            var task = taskMap[item.TaskId];
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

        foreach (var task in workflowTasks)
        {
            var fromStatus = task.WorkflowStatus;
            task.WorkflowStatus = TaskWorkflowStatus.Returned;
            task.ApprovedAt = null;
            _db.TaskApprovalHistories.Add(TaskWorkflowSupport.CreateHistory(
                task,
                department?.Id,
                ApprovalAction.Return,
                fromStatus,
                TaskWorkflowStatus.Returned,
                actorId,
                request.Comment));
        }

        await _db.SaveChangesAsync(ct);
        return TaskSupport.TaskWorkflowStatusCode(TaskWorkflowSupport.AggregateStatus(workflowTasks));
    }
}
