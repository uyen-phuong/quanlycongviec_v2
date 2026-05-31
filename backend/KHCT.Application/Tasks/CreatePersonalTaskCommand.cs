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

public record CreatePersonalTaskCommand(
    string Title,
    DateTime? Deadline,
    string? NoteText,
    string? Priority,
    string? Complexity,
    int DisplayOrder) : IRequest<TaskDetailDto>;

public class CreatePersonalTaskCommandValidator : AbstractValidator<CreatePersonalTaskCommand>
{
    public CreatePersonalTaskCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(500);
        RuleFor(x => x.NoteText).MaximumLength(4000);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public class CreatePersonalTaskHandler : IRequestHandler<CreatePersonalTaskCommand, TaskDetailDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public CreatePersonalTaskHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<TaskDetailDto> Handle(CreatePersonalTaskCommand request, CancellationToken ct)
    {
        var priority = string.IsNullOrWhiteSpace(request.Priority) ? TaskPriority.Normal : TaskSupport.ParsePriority(request.Priority);
        var complexity = string.IsNullOrWhiteSpace(request.Complexity) ? TaskComplexity.Medium : TaskSupport.ParseComplexity(request.Complexity);

        var task = new TaskEntity
        {
            Id = Guid.NewGuid(),
            PlanId = null,
            Category = TaskCategory.PersonalTask,
            ParentTaskId = null,
            OutlineIndex = null,
            DisplayOrder = request.DisplayOrder,
            IsHeader = false,
            Title = request.Title.Trim(),
            WorkType = WorkType.Personal,
            WorkStatus = WorkStatus.NotStarted,
            WorkflowStatus = TaskWorkflowStatus.New,
            Deadline = request.Deadline,
            AssigneeUserId = _currentUser.UserId,
            ControllerUserId = null,
            OwnerDepartmentId = _currentUser.DepartmentId,
            NoteText = string.IsNullOrWhiteSpace(request.NoteText) ? null : request.NoteText.Trim(),
            Priority = priority,
            Complexity = complexity
        };

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync(ct);

        // Reload to map types correctly for detail dto
        await _db.Tasks
            .Include(x => x.AssigneeUser)
            .Include(x => x.OwnerDepartment)
            .Include(x => x.SupportingDepts)
                .ThenInclude(x => x.Department)
            .FirstAsync(x => x.Id == task.Id, ct);

        return TaskSupport.ToDetail(task);
    }
}
