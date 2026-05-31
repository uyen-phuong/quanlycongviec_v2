using FluentValidation;
using KHCT.Application.Common.Interfaces;
using KHCT.Application.Common.Support;
using KHCT.Application.Plans;
using KHCT.Domain.Common;
using KHCT.Domain.Entities;
using KHCT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskEntity = KHCT.Domain.Entities.Task;

namespace KHCT.Application.Tasks;

public record CreateDepartmentTaskCommand(
    Guid? ParentTaskId,
    string? OutlineIndex,
    int DisplayOrder,
    bool IsHeader,
    string Title,
    DateTime? Deadline,
    Guid? AssigneeUserId,
    Guid? ControllerUserId,
    string? NoteText,
    string? Priority,
    string? Complexity,
    IReadOnlyList<Guid> SupportingDepartmentIds) : IRequest<TaskDetailDto>;

public class CreateDepartmentTaskCommandValidator : AbstractValidator<CreateDepartmentTaskCommand>
{
    public CreateDepartmentTaskCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(500);
        RuleFor(x => x.OutlineIndex).MaximumLength(64);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
        RuleFor(x => x.NoteText).MaximumLength(4000);
        RuleFor(x => x.SupportingDepartmentIds).NotNull();
        RuleFor(x => x.SupportingDepartmentIds).Must(x => x.Distinct().Count() == x.Count).WithMessage("Supporting departments must be distinct.");
    }
}

public class CreateDepartmentTaskHandler : IRequestHandler<CreateDepartmentTaskCommand, TaskDetailDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public CreateDepartmentTaskHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<TaskDetailDto> Handle(CreateDepartmentTaskCommand request, CancellationToken ct)
    {
        if (!_currentUser.DepartmentId.HasValue)
        {
            throw new DomainException("user_department_missing", "User has no department assigned.");
        }

        var departmentId = _currentUser.DepartmentId.Value;

        // Ensure parent task belongs to the same department and is in the same category
        if (request.ParentTaskId.HasValue)
        {
            var parent = await _db.Tasks.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.ParentTaskId.Value, ct)
                ?? throw new DomainException("parent_task_invalid", "Parent task is invalid.");
            
            if (parent.Category != TaskCategory.DepartmentTask || parent.OwnerDepartmentId != departmentId)
            {
                throw new DomainException("parent_task_invalid", "Parent task must belong to the same department.");
            }
        }

        var supportingIds = request.SupportingDepartmentIds.Distinct().ToList();
        if (supportingIds.Contains(departmentId))
        {
            throw new DomainException("supporting_dept_invalid", "Supporting department cannot be the owner department.");
        }

        foreach (var id in supportingIds)
        {
            await ApplicationSupport.RequireActiveDepartmentAsync(_db, id, ct);
        }

        var priority = string.IsNullOrWhiteSpace(request.Priority) ? TaskPriority.Normal : TaskSupport.ParsePriority(request.Priority);
        var complexity = string.IsNullOrWhiteSpace(request.Complexity) ? TaskComplexity.Medium : TaskSupport.ParseComplexity(request.Complexity);

        var task = new TaskEntity
        {
            Id = Guid.NewGuid(),
            PlanId = null,
            Category = TaskCategory.DepartmentTask,
            ParentTaskId = request.ParentTaskId,
            OutlineIndex = string.IsNullOrWhiteSpace(request.OutlineIndex) ? null : request.OutlineIndex.Trim(),
            DisplayOrder = request.DisplayOrder,
            IsHeader = request.IsHeader,
            Title = request.Title.Trim(),
            WorkType = WorkType.General,
            WorkStatus = WorkStatus.NotStarted,
            WorkflowStatus = TaskWorkflowStatus.New,
            Deadline = request.Deadline,
            AssigneeUserId = request.AssigneeUserId,
            ControllerUserId = request.ControllerUserId,
            OwnerDepartmentId = departmentId,
            NoteText = string.IsNullOrWhiteSpace(request.NoteText) ? null : request.NoteText.Trim(),
            Priority = priority,
            Complexity = complexity
        };

        TaskSupport.ApplySupportingDepartments(task, supportingIds);
        TaskSupport.EnsureHeaderNormalized(task);

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync(ct);

        // Reload relations for the DetailDto
        await _db.Tasks
            .Include(x => x.AssigneeUser)
            .Include(x => x.OwnerDepartment)
            .Include(x => x.SupportingDepts)
                .ThenInclude(x => x.Department)
            .FirstAsync(x => x.Id == task.Id, ct);

        return TaskSupport.ToDetail(task);
    }
}
