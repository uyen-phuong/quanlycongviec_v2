using FluentValidation;
using KHCT.Application.Common.Interfaces;
using KHCT.Application.Common.Support;
using KHCT.Application.Notifications;
using KHCT.Application.Plans;
using KHCT.Domain.Common;
using KHCT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Tasks;

public record DeleteTaskCommand(Guid Id) : IRequest<bool>;

public class DeleteTaskCommandValidator : AbstractValidator<DeleteTaskCommand>
{
    public DeleteTaskCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class DeleteTaskHandler : IRequestHandler<DeleteTaskCommand, bool>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public DeleteTaskHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<bool> Handle(DeleteTaskCommand request, CancellationToken ct)
    {
        var task = await _db.Tasks
            .Include(x => x.Plan)
            .Include(x => x.SupportingDepts)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new KeyNotFoundException("Task not found.");

        if (task.Category == TaskCategory.PersonalTask)
        {
            if (task.AssigneeUserId != _currentUser.UserId)
            {
                throw new ForbiddenException("forbidden_personal_task", "You can only delete your own personal tasks.");
            }

            var hasChildren = await _db.Tasks.AnyAsync(x => x.ParentTaskId == task.Id, ct);
            if (hasChildren)
            {
                throw new DomainException("task_has_children", "Cannot delete a task that has children.");
            }

            _db.Tasks.Remove(task);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        if (task.Category == TaskCategory.DepartmentTask || task.Category == TaskCategory.ProjectTask)
        {
            var canDelete = (PlanSupport.HasRole(_currentUser, PlanSupport.RolePhoTruongKtnb) ||
                             PlanSupport.HasRole(_currentUser, PlanSupport.RoleTruongPhong) ||
                             PlanSupport.HasRole(_currentUser, PlanSupport.RolePhoPhong) ||
                             PlanSupport.HasRole(_currentUser, PlanSupport.RoleAdmin)) &&
                            (PlanSupport.HasRole(_currentUser, PlanSupport.RoleAdmin) ||
                             PlanSupport.HasRole(_currentUser, PlanSupport.RolePhoTruongKtnb) ||
                             _currentUser.DepartmentId == task.OwnerDepartmentId);

            if (!canDelete)
            {
                throw new ForbiddenException("forbidden_task_delete", "You do not have access to delete this task.");
            }

            var hasChildren = await _db.Tasks.AnyAsync(x => x.ParentTaskId == task.Id, ct);
            if (hasChildren)
            {
                throw new DomainException("task_has_children", "Cannot delete a task that has children.");
            }

            _db.Tasks.Remove(task);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        TaskSupport.EnsureCanCreateOrDeleteTask(task.Plan!, _currentUser);
        if (task.Plan!.Scope != PlanScope.Sub)
        {
            TaskSupport.EnsureNotLocked(task);
        }

        var hasPlanChildren = await _db.Tasks.AnyAsync(x => x.ParentTaskId == task.Id, ct);
        if (hasPlanChildren)
        {
            throw new DomainException("task_has_children", "Cannot delete a task that has children.");
        }

        await DeleteOwnerMainTaskIfNeededAsync(task, ct);
        _db.Tasks.Remove(task);
        await PlanSupport.ResetWorkflowAsync(_db, task.Plan!, ct);
        await NotificationHelper.OnTaskDeletedAsync(_db, task, task.Plan!, _currentUser.UserId, ct);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private async System.Threading.Tasks.Task DeleteOwnerMainTaskIfNeededAsync(Domain.Entities.Task task, CancellationToken ct)
    {
        var plan = task.Plan!;
        if (plan.Scope != PlanScope.Sub ||
            !plan.DepartmentId.HasValue ||
            !task.InheritedFromTaskId.HasValue)
        {
            return;
        }

        var mainTask = await _db.Tasks
            .Include(x => x.SupportingDepts)
            .FirstOrDefaultAsync(x => x.Id == task.InheritedFromTaskId.Value, ct);
        if (mainTask is null || mainTask.OwnerDepartmentId != plan.DepartmentId)
        {
            return;
        }

        var mainHasChildren = await _db.Tasks.AnyAsync(x => x.ParentTaskId == mainTask.Id, ct);
        if (mainHasChildren)
        {
            throw new DomainException("task_has_children", "Cannot delete a synced main task that has children.");
        }

        var inheritedCopies = await _db.Tasks
            .Where(x => x.InheritedFromTaskId == mainTask.Id && x.Id != task.Id)
            .ToListAsync(ct);
        var inheritedCopyIds = inheritedCopies.Select(x => x.Id).ToHashSet();
        var inheritedCopyChildren = await _db.Tasks
            .Where(x => x.ParentTaskId.HasValue && inheritedCopyIds.Contains(x.ParentTaskId.Value))
            .ToListAsync(ct);
        foreach (var child in inheritedCopyChildren)
        {
            child.ParentTaskId = null;
        }

        foreach (var inheritedCopy in inheritedCopies)
        {
            _db.Tasks.Remove(inheritedCopy);
        }

        _db.Tasks.Remove(mainTask);
        _db.AuditLogs.Add(ApplicationSupport.CreateAudit(
            "task",
            mainTask.Id,
            "delete_from_owner_sub",
            _currentUser.UserId,
            TaskSupport.Snapshot(mainTask),
            null));
    }
}
