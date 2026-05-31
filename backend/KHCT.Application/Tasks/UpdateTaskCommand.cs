using FluentValidation;
using KHCT.Application.Common.Interfaces;
using KHCT.Application.Common.Support;
using KHCT.Application.Plans;
using KHCT.Application.Plans.Workflow;
using KHCT.Domain.Common;
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

        if (task.Category == TaskCategory.DepartmentTask || task.Category == TaskCategory.ProjectTask || task.Category == TaskCategory.PersonalTask)
        {
            return await HandleNonPlanTaskAsync(task, request, ct);
        }

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

        var priority = string.IsNullOrWhiteSpace(values.Priority) ? TaskPriority.Normal : TaskSupport.ParsePriority(values.Priority);
        var complexity = string.IsNullOrWhiteSpace(values.Complexity) ? TaskComplexity.Medium : TaskSupport.ParseComplexity(values.Complexity);

        task.ParentTaskId = values.ParentTaskId;
        task.OutlineIndex = NormalizeText(values.OutlineIndex);
        task.DisplayOrder = values.DisplayOrder;
        task.IsHeader = values.IsHeader;
        task.Title = values.Title.Trim();
        task.WorkType = values.WorkType;
        task.WorkStatus = values.WorkStatus;
        task.Deadline = values.Deadline;
        task.AssigneeUserId = values.AssigneeUserId;
        task.ControllerUserId = values.ControllerUserId;
        task.OwnerDepartmentId = ownerDepartmentId;
        task.BksMemberText = NormalizeText(values.BksMemberText);
        task.KtnbLeaderText = NormalizeText(values.KtnbLeaderText);
        task.NoteText = NormalizeText(values.NoteText);
        task.ProgressText = NormalizeText(values.ProgressText);
        task.ReasonNotCompleted = NormalizeText(values.ReasonNotCompleted);
        task.Priority = priority;
        task.Complexity = complexity;

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
            request.ControllerUserId,
            request.OwnerDepartmentId,
            NormalizeText(request.BksMemberText),
            NormalizeText(request.KtnbLeaderText),
            NormalizeText(request.NoteText),
            NormalizeText(request.ProgressText),
            NormalizeText(request.ReasonNotCompleted),
            request.Priority,
            request.Complexity,
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

    private async Task<TaskDetailDto> HandleNonPlanTaskAsync(Domain.Entities.Task task, UpdateTaskCommand request, CancellationToken ct)
    {
        if (task.Category == TaskCategory.PersonalTask)
        {
            if (task.AssigneeUserId != _currentUser.UserId)
            {
                throw new ForbiddenException("forbidden_personal_task", "You can only edit your own personal tasks.");
            }

            var pWorkStatus = TaskSupport.ParseWorkStatus(request.WorkStatus);
            if (pWorkStatus != WorkStatus.NotStarted && pWorkStatus != WorkStatus.InProgress && pWorkStatus != WorkStatus.Done)
            {
                throw new DomainException("invalid_personal_status", "Personal tasks only support NotStarted, InProgress, or Done status.");
            }

            task.Title = request.Title.Trim();
            task.DisplayOrder = request.DisplayOrder;
            task.WorkStatus = pWorkStatus;
            task.Deadline = request.Deadline;
            task.NoteText = string.IsNullOrWhiteSpace(request.NoteText) ? null : request.NoteText.Trim();
            task.Priority = string.IsNullOrWhiteSpace(request.Priority) ? TaskPriority.Normal : TaskSupport.ParsePriority(request.Priority);
            task.Complexity = string.IsNullOrWhiteSpace(request.Complexity) ? TaskComplexity.Medium : TaskSupport.ParseComplexity(request.Complexity);
            task.ProgressText = string.IsNullOrWhiteSpace(request.ProgressText) ? null : request.ProgressText.Trim();

            // Personal tasks also automatically sync WorkflowStatus to match WorkStatus transitions
            task.WorkflowStatus = pWorkStatus switch
            {
                WorkStatus.NotStarted => TaskWorkflowStatus.New,
                WorkStatus.InProgress => TaskWorkflowStatus.InProgress,
                WorkStatus.Done => TaskWorkflowStatus.Completed,
                _ => TaskWorkflowStatus.New
            };

            await _db.SaveChangesAsync(ct);
            return await LoadTaskDetail(task.Id, ct);
        }

        // DepartmentTask or ProjectTask
        var canFullUpdate = (PlanSupport.HasRole(_currentUser, PlanSupport.RolePhoTruongKtnb) ||
                             PlanSupport.HasRole(_currentUser, PlanSupport.RoleTruongPhong) ||
                             PlanSupport.HasRole(_currentUser, PlanSupport.RoleTruongNhom) ||
                             PlanSupport.HasRole(_currentUser, PlanSupport.RoleAdmin)) &&
                            (PlanSupport.HasRole(_currentUser, PlanSupport.RoleAdmin) ||
                             PlanSupport.HasRole(_currentUser, PlanSupport.RolePhoTruongKtnb) ||
                             _currentUser.DepartmentId == task.OwnerDepartmentId);

        var workStatus = TaskSupport.ParseWorkStatus(request.WorkStatus);

        if (canFullUpdate)
        {
            if (request.ParentTaskId.HasValue)
            {
                var parent = await _db.Tasks.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.ParentTaskId.Value, ct)
                    ?? throw new DomainException("parent_task_invalid", "Parent task is invalid.");
                if (parent.Category != task.Category || parent.OwnerDepartmentId != task.OwnerDepartmentId)
                {
                    throw new DomainException("parent_task_invalid", "Parent task must belong to the same department/category.");
                }
            }

            var supportingIds = (request.SupportingDepartmentIds ?? Array.Empty<Guid>()).Distinct().ToList();
            if (task.OwnerDepartmentId.HasValue && supportingIds.Contains(task.OwnerDepartmentId.Value))
            {
                throw new DomainException("supporting_dept_invalid", "Supporting department cannot be the owner department.");
            }
            foreach (var id in supportingIds)
            {
                await ApplicationSupport.RequireActiveDepartmentAsync(_db, id, ct);
            }

            task.ParentTaskId = request.ParentTaskId;
            task.OutlineIndex = string.IsNullOrWhiteSpace(request.OutlineIndex) ? null : request.OutlineIndex.Trim();
            task.DisplayOrder = request.DisplayOrder;
            task.IsHeader = request.IsHeader;
            task.Title = request.Title.Trim();
            task.WorkStatus = workStatus;
            task.Deadline = request.Deadline;
            task.AssigneeUserId = request.AssigneeUserId;
            task.ControllerUserId = request.ControllerUserId;
            task.NoteText = string.IsNullOrWhiteSpace(request.NoteText) ? null : request.NoteText.Trim();
            task.Priority = string.IsNullOrWhiteSpace(request.Priority) ? TaskPriority.Normal : TaskSupport.ParsePriority(request.Priority);
            task.Complexity = string.IsNullOrWhiteSpace(request.Complexity) ? TaskComplexity.Medium : TaskSupport.ParseComplexity(request.Complexity);
            task.ProgressText = string.IsNullOrWhiteSpace(request.ProgressText) ? null : request.ProgressText.Trim();
            task.ReasonNotCompleted = string.IsNullOrWhiteSpace(request.ReasonNotCompleted) ? null : request.ReasonNotCompleted.Trim();

            TaskSupport.ApplySupportingDepartments(task, supportingIds);
            TaskSupport.EnsureHeaderNormalized(task);
            TaskSupport.ValidateOverdue(workStatus, task.Deadline, task.ReasonNotCompleted);
        }
        else
        {
            // Staff progress update
            if (task.AssigneeUserId != _currentUser.UserId)
            {
                throw new ForbiddenException("forbidden_task_update", "You do not have access to update this task.");
            }

            task.WorkStatus = workStatus;
            task.ProgressText = string.IsNullOrWhiteSpace(request.ProgressText) ? null : request.ProgressText.Trim();
            task.ReasonNotCompleted = string.IsNullOrWhiteSpace(request.ReasonNotCompleted) ? null : request.ReasonNotCompleted.Trim();

            TaskSupport.ValidateOverdue(workStatus, task.Deadline, task.ReasonNotCompleted);
        }

        await _db.SaveChangesAsync(ct);
        return await LoadTaskDetail(task.Id, ct);
    }
}
