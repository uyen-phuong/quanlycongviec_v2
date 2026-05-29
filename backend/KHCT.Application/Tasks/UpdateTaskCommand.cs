using FluentValidation;
using KHCT.Application.Common.Interfaces;
using KHCT.Application.Plans;
using KHCT.Application.Plans.Workflow;
using KHCT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Tasks;

public record UpdateTaskCommand(
    Guid Id,
    Guid? ParentTaskId,
    string? OutlineIndex,
    int DisplayOrder,
    bool IsHeader,
    string Title,
    int WorkType,
    string WorkStatus,
    DateTime? Deadline,
    Guid? AssigneeUserId,
    Guid? OwnerDepartmentId,
    string? BksMemberText,
    string? KtnbLeaderText,
    string? NoteText,
    string? ProgressText,
    string? ReasonNotCompleted,
    IReadOnlyList<Guid> SupportingDepartmentIds) : IRequest<TaskDetailDto>;

public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
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

public class UpdateTaskHandler : IRequestHandler<UpdateTaskCommand, TaskDetailDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public UpdateTaskHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<TaskDetailDto> Handle(UpdateTaskCommand request, CancellationToken ct)
    {
        var task = await _db.Tasks
            .Include(x => x.Plan)
            .Include(x => x.AssigneeUser)
            .Include(x => x.OwnerDepartment)
            .Include(x => x.SupportingDepts)
                .ThenInclude(x => x.Department)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new KeyNotFoundException("Task not found.");

        var plan = task.Plan ?? throw new KeyNotFoundException("Plan not found.");
        var workStatus = TaskSupport.ParseWorkStatus(request.WorkStatus);
        var values = ToValues(request, workStatus);
        var canFullUpdate = TaskSupport.CanFullUpdate(plan, _currentUser);

        if (canFullUpdate && (!task.IsLocked || plan.Scope == PlanScope.Sub))
        {
            TaskSupport.EnsureCanUpdateTaskFull(plan, task, _currentUser);
            await ApplyFullUpdate(task, plan, values, ct);

            if (plan.Scope == PlanScope.Main)
            {
                await InheritService.RunAsync(_db, plan, _currentUser.UserId, ct);
            }
        }
        else
        {
            // canFullUpdate + locked: inherited task in sub plan — allow progress fields only
            if (canFullUpdate)
                PlanSupport.EnsureEditable(plan);
            else
                TaskSupport.EnsureCanUpdateTaskProgress(plan, task, _currentUser);

            TaskSupport.EnsureProgressOnlyPayload(task, values);
            TaskSupport.ValidateOverdue(workStatus, task.Deadline, values.ReasonNotCompleted);

            task.WorkStatus = workStatus;
            task.ProgressText = NormalizeText(values.ProgressText);
            task.ReasonNotCompleted = NormalizeText(values.ReasonNotCompleted);
            await PlanSupport.ResetWorkflowAsync(_db, plan, ct);
            await _db.SaveChangesAsync(ct);
            return await LoadTaskDetail(task.Id, ct);
        }

        await PlanSupport.ResetWorkflowAsync(_db, plan, ct);
        await _db.SaveChangesAsync(ct);
        return await LoadTaskDetail(task.Id, ct);
    }

    private async Task ApplyFullUpdate(Domain.Entities.Task task, Domain.Entities.Plan plan, UpdateTaskValues values, CancellationToken ct)
    {
        await TaskSupport.ValidateParentAsync(_db, plan.Id, values.ParentTaskId, task.Id, ct);
        TaskSupport.ValidateDeadline(plan, values.Deadline);
        TaskSupport.ValidateOverdue(values.WorkStatus, values.Deadline, values.ReasonNotCompleted);
        var supportingIds = await TaskSupport.ValidateSupportingDepartmentsAsync(_db, plan, values.SupportingDepartmentIds, ct);
        var ownerDepartmentId = await TaskSupport.ValidateOwnerDepartmentAsync(_db, plan, values.IsHeader, values.WorkType, values.OwnerDepartmentId, ct);

        task.ParentTaskId = values.ParentTaskId;
        task.OutlineIndex = NormalizeText(values.OutlineIndex);
        task.DisplayOrder = values.DisplayOrder;
        task.IsHeader = values.IsHeader;
        task.Title = values.Title.Trim();
        task.WorkType = values.WorkType;
        task.WorkStatus = values.WorkStatus;
        task.Deadline = values.Deadline;
        task.AssigneeUserId = values.AssigneeUserId;
        task.OwnerDepartmentId = ownerDepartmentId;
        task.BksMemberText = NormalizeText(values.BksMemberText);
        task.KtnbLeaderText = NormalizeText(values.KtnbLeaderText);
        task.NoteText = NormalizeText(values.NoteText);
        task.ProgressText = NormalizeText(values.ProgressText);
        task.ReasonNotCompleted = NormalizeText(values.ReasonNotCompleted);

        TaskSupport.ApplySupportingDepartments(task, supportingIds);
        TaskSupport.EnsureHeaderNormalized(task);
    }

    private static UpdateTaskValues ToValues(UpdateTaskCommand request, WorkStatus workStatus) =>
        new(
            request.ParentTaskId,
            NormalizeText(request.OutlineIndex),
            request.DisplayOrder,
            request.IsHeader,
            request.Title.Trim(),
            (WorkType)request.WorkType,
            workStatus,
            request.Deadline,
            request.AssigneeUserId,
            request.OwnerDepartmentId,
            NormalizeText(request.BksMemberText),
            NormalizeText(request.KtnbLeaderText),
            NormalizeText(request.NoteText),
            NormalizeText(request.ProgressText),
            NormalizeText(request.ReasonNotCompleted),
            request.SupportingDepartmentIds);

    private static string? NormalizeText(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private async Task<TaskDetailDto> LoadTaskDetail(Guid taskId, CancellationToken ct)
    {
        var task = await _db.Tasks
            .AsNoTracking()
            .Include(x => x.AssigneeUser)
            .Include(x => x.OwnerDepartment)
            .Include(x => x.SupportingDepts)
                .ThenInclude(x => x.Department)
            .FirstAsync(x => x.Id == taskId, ct);

        return TaskSupport.ToDetail(task);
    }
}
