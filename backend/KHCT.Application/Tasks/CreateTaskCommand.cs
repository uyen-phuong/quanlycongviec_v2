using FluentValidation;
using KHCT.Application.Common.Interfaces;
using KHCT.Application.Notifications;
using KHCT.Application.Plans;
using KHCT.Application.Plans.Workflow;
using KHCT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskEntity = KHCT.Domain.Entities.Task;

namespace KHCT.Application.Tasks;

public record CreateTaskCommand(
    Guid PlanId,
    Guid? ParentTaskId,
    string? OutlineIndex,
    int DisplayOrder,
    bool IsHeader,
    string Title,
    int WorkType,
    string WorkStatus,
    DateTime? Deadline,
    Guid? AssigneeUserId,
    Guid? ControllerUserId,
    Guid? OwnerDepartmentId,
    string? BksMemberText,
    string? KtnbLeaderText,
    string? NoteText,
    string? ProgressText,
    string? ReasonNotCompleted,
    string? Priority,
    string? Complexity,
    IReadOnlyList<Guid> SupportingDepartmentIds) : IRequest<TaskDetailDto>;

public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.PlanId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(500);
        RuleFor(x => x.OutlineIndex).MaximumLength(64);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
        RuleFor(x => x.WorkType).Must(x => Enum.IsDefined(typeof(WorkType), x)).WithMessage("Work type is invalid.");
        RuleFor(x => x.WorkStatus).NotEmpty();
        RuleFor(x => x.BksMemberText).MaximumLength(255);
        RuleFor(x => x.KtnbLeaderText).MaximumLength(255);
        RuleFor(x => x.NoteText).MaximumLength(4000);
        RuleFor(x => x.ProgressText).MaximumLength(2000);
        RuleFor(x => x.ReasonNotCompleted).MaximumLength(2000);
        RuleFor(x => x.SupportingDepartmentIds).NotNull();
        RuleFor(x => x.SupportingDepartmentIds).Must(x => x.Distinct().Count() == x.Count).WithMessage("Supporting departments must be distinct.");
        RuleFor(x => x.ReasonNotCompleted).NotEmpty().When(x =>
            string.Equals(x.WorkStatus, "overdue", StringComparison.OrdinalIgnoreCase) && x.Deadline.HasValue && x.Deadline.Value.Date < DateTime.UtcNow.Date);
    }
}

public class CreateTaskHandler : IRequestHandler<CreateTaskCommand, TaskDetailDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public CreateTaskHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<TaskDetailDto> Handle(CreateTaskCommand request, CancellationToken ct)
    {
        var plan = await _db.Plans.FirstOrDefaultAsync(x => x.Id == request.PlanId, ct)
            ?? throw new KeyNotFoundException("Plan not found.");

        TaskSupport.EnsureCanCreateOrDeleteTask(plan, _currentUser);
        await TaskSupport.ValidateParentAsync(_db, plan.Id, request.ParentTaskId, null, ct);
        TaskSupport.ValidateDeadline(plan, request.Deadline);

        var workStatus = TaskSupport.ParseWorkStatus(request.WorkStatus);
        TaskSupport.ValidateOverdue(workStatus, request.Deadline, request.ReasonNotCompleted);
        var supportingIds = await TaskSupport.ValidateSupportingDepartmentsAsync(_db, plan, request.SupportingDepartmentIds, ct);
        var ownerDepartmentId = await TaskSupport.ValidateOwnerDepartmentAsync(_db, plan, request.IsHeader, (WorkType)request.WorkType, request.OwnerDepartmentId, ct);

        var priority = string.IsNullOrWhiteSpace(request.Priority) ? TaskPriority.Normal : TaskSupport.ParsePriority(request.Priority);
        var complexity = string.IsNullOrWhiteSpace(request.Complexity) ? TaskComplexity.Medium : TaskSupport.ParseComplexity(request.Complexity);

        var task = new TaskEntity
        {
            Id = Guid.NewGuid(),
            PlanId = plan.Id,
            ParentTaskId = request.ParentTaskId,
            OutlineIndex = string.IsNullOrWhiteSpace(request.OutlineIndex) ? null : request.OutlineIndex.Trim(),
            DisplayOrder = request.DisplayOrder,
            IsHeader = request.IsHeader,
            Title = request.Title.Trim(),
            WorkType = (WorkType)request.WorkType,
            WorkStatus = workStatus,
            Deadline = request.Deadline,
            AssigneeUserId = request.AssigneeUserId,
            ControllerUserId = request.ControllerUserId,
            OwnerDepartmentId = ownerDepartmentId,
            BksMemberText = string.IsNullOrWhiteSpace(request.BksMemberText) ? null : request.BksMemberText.Trim(),
            KtnbLeaderText = string.IsNullOrWhiteSpace(request.KtnbLeaderText) ? null : request.KtnbLeaderText.Trim(),
            NoteText = string.IsNullOrWhiteSpace(request.NoteText) ? null : request.NoteText.Trim(),
            ProgressText = string.IsNullOrWhiteSpace(request.ProgressText) ? null : request.ProgressText.Trim(),
            ReasonNotCompleted = string.IsNullOrWhiteSpace(request.ReasonNotCompleted) ? null : request.ReasonNotCompleted.Trim(),
            Priority = priority,
            Complexity = complexity
        };

        TaskSupport.ApplySupportingDepartments(task, supportingIds);
        TaskSupport.EnsureHeaderNormalized(task);

        _db.Tasks.Add(task);
        await PlanSupport.ResetWorkflowAsync(_db, plan, ct);
        await NotificationHelper.OnTaskCreatedAsync(_db, task, plan, _currentUser.UserId, ct);
        await _db.SaveChangesAsync(ct);

        if (plan.Scope == PlanScope.Main)
        {
            await InheritService.RunAsync(_db, plan, _currentUser.UserId, ct);
            await _db.SaveChangesAsync(ct);
        }

        await LoadTaskRelations(task.Id, ct);
        return TaskSupport.ToDetail(task);
    }

    private async Task LoadTaskRelations(Guid taskId, CancellationToken ct)
    {
        await _db.Tasks
            .Include(x => x.AssigneeUser)
            .Include(x => x.OwnerDepartment)
            .Include(x => x.SupportingDepts)
                .ThenInclude(x => x.Department)
            .FirstAsync(x => x.Id == taskId, ct);
    }
}
