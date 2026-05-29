using FluentValidation;
using FluentValidation.Results;
using KHCT.Application.Common.Interfaces;
using KHCT.Application.Common.Support;
using KHCT.Application.Tasks;
using KHCT.Domain.Common;
using KHCT.Domain.Entities;
using KHCT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskEntity = KHCT.Domain.Entities.Task;

namespace KHCT.Application.Plans.Import;

public record ImportMainPlanExcelCommand(Guid PlanId, string FileName, byte[] Content) : IRequest<ImportMainPlanExcelResultDto>;

public class ImportMainPlanExcelCommandValidator : AbstractValidator<ImportMainPlanExcelCommand>
{
    public ImportMainPlanExcelCommandValidator()
    {
        RuleFor(x => x.PlanId).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Content).NotNull().Must(x => x.Length > 0).WithMessage("Excel content is required.");
    }
}

public class ImportMainPlanExcelHandler : IRequestHandler<ImportMainPlanExcelCommand, ImportMainPlanExcelResultDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IMainPlanExcelImportService _importService;

    public ImportMainPlanExcelHandler(
        IApplicationDbContext db,
        ICurrentUser currentUser,
        IMainPlanExcelImportService importService)
    {
        _db = db;
        _currentUser = currentUser;
        _importService = importService;
    }

    public async Task<ImportMainPlanExcelResultDto> Handle(ImportMainPlanExcelCommand request, CancellationToken ct)
    {
        var plan = await _db.Plans
            .Include(x => x.Tasks)
            .FirstOrDefaultAsync(x => x.Id == request.PlanId && x.Scope == PlanScope.Main, ct)
            ?? throw new KeyNotFoundException("Plan not found.");

        PlanSupport.EnsureEditable(plan);
        await EnsureNoDownstreamInheritedTasks(plan.Id, ct);

        var importData = await _importService.ParseAsync(request.FileName, request.Content, ct);
        var validationErrors = ValidateRows(plan, importData.Rows);
        if (validationErrors.Count > 0)
        {
            throw new ValidationException(validationErrors);
        }

        var actorId = PlanSupport.RequireActorId(_currentUser);
        var oldTaskIds = await _db.Tasks
            .Where(x => x.PlanId == plan.Id)
            .Select(x => x.Id)
            .ToListAsync(ct);

        var beforeAudit = new
        {
            request.FileName,
            OldTaskCount = oldTaskIds.Count,
            DownstreamBlocked = false
        };

        var stagedTasks = BuildTasks(plan, importData.Rows);

        await using var transaction = await _db.Database.BeginTransactionAsync(ct);
        var existingTasks = await _db.Tasks.Where(x => x.PlanId == plan.Id).ToListAsync(ct);
        _db.Tasks.RemoveRange(existingTasks);
        _db.Tasks.AddRange(stagedTasks);
        await PlanSupport.ResetWorkflowAsync(_db, plan, ct);
        _db.AuditLogs.Add(ApplicationSupport.CreateAudit(
            "plan",
            plan.Id,
            "import_excel",
            actorId,
            beforeAudit,
            new
            {
                request.FileName,
                importData.SheetName,
                importData.HeaderRowNumber,
                NewTaskCount = stagedTasks.Count,
                HeaderCount = stagedTasks.Count(x => x.IsHeader),
                TaskCount = stagedTasks.Count(x => !x.IsHeader)
            }));

        await _db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return new ImportMainPlanExcelResultDto(
            true,
            request.FileName,
            importData.SheetName,
            importData.HeaderRowNumber,
            importData.Rows.Count,
            stagedTasks.Count(x => x.IsHeader),
            stagedTasks.Count(x => !x.IsHeader),
            stagedTasks.Count);
    }

    private async System.Threading.Tasks.Task EnsureNoDownstreamInheritedTasks(Guid mainPlanId, CancellationToken ct)
    {
        var mainTaskIds = await _db.Tasks
            .Where(x => x.PlanId == mainPlanId)
            .Select(x => x.Id)
            .ToListAsync(ct);

        if (mainTaskIds.Count == 0)
        {
            return;
        }

        var hasDownstream = await _db.Tasks.AnyAsync(x =>
            x.InheritedFromTaskId.HasValue &&
            mainTaskIds.Contains(x.InheritedFromTaskId.Value), ct);

        if (hasDownstream)
        {
            throw new DomainException("plan_has_downstream", "Main plan already has downstream inherited tasks.");
        }
    }

    private static List<ValidationFailure> ValidateRows(Plan plan, IReadOnlyList<MainPlanExcelImportRow> rows)
    {
        var failures = new List<ValidationFailure>();
        if (rows.Count == 0)
        {
            failures.Add(new ValidationFailure("file", "Excel file does not contain any importable rows."));
            return failures;
        }

        var levelStack = new Stack<(int Level, int RowNumber)>();
        foreach (var row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.Title))
            {
                failures.Add(new ValidationFailure($"rows[{row.RowNumber}].title", "Title is required."));
            }

            while (levelStack.Count > 0 && levelStack.Peek().Level >= row.Level)
            {
                levelStack.Pop();
            }

            if (row.Level > 1 && levelStack.Count == 0)
            {
                failures.Add(new ValidationFailure($"rows[{row.RowNumber}].outlineIndex", "Missing parent outline."));
            }

            levelStack.Push((row.Level, row.RowNumber));

            if (!row.IsHeader)
            {
                var inferredStatus = string.IsNullOrWhiteSpace(row.ProgressText) && string.IsNullOrWhiteSpace(row.ReasonNotCompleted)
                    ? WorkStatus.NotStarted
                    : WorkStatus.InProgress;
                TaskSupport.ValidateOverdue(inferredStatus, null, row.ReasonNotCompleted);
                TaskSupport.ValidateDeadline(plan, null);
            }
        }

        return failures;
    }

    private static List<TaskEntity> BuildTasks(Plan plan, IReadOnlyList<MainPlanExcelImportRow> rows)
    {
        var tasks = new List<TaskEntity>();
        var parentStack = new Stack<(int Level, Guid TaskId)>();
        var siblingCounters = new Dictionary<Guid, int>();
        var rootSiblingCount = 0;

        foreach (var row in rows)
        {
            while (parentStack.Count > 0 && parentStack.Peek().Level >= row.Level)
            {
                parentStack.Pop();
            }

            Guid? parentTaskId = parentStack.Count > 0 ? parentStack.Peek().TaskId : null;
            int siblingIndex;
            if (parentTaskId.HasValue)
            {
                siblingCounters.TryGetValue(parentTaskId.Value, out siblingIndex);
                siblingIndex++;
                siblingCounters[parentTaskId.Value] = siblingIndex;
            }
            else
            {
                rootSiblingCount++;
                siblingIndex = rootSiblingCount;
            }

            var task = new TaskEntity
            {
                Id = Guid.NewGuid(),
                PlanId = plan.Id,
                ParentTaskId = parentTaskId,
                OutlineIndex = row.OutlineIndex,
                DisplayOrder = siblingIndex * 10,
                IsHeader = row.IsHeader,
                Title = row.Title,
                WorkType = WorkType.General,
                WorkStatus = row.IsHeader
                    ? WorkStatus.NotStarted
                    : (string.IsNullOrWhiteSpace(row.ProgressText) && string.IsNullOrWhiteSpace(row.ReasonNotCompleted)
                        ? WorkStatus.NotStarted
                        : WorkStatus.InProgress),
                Deadline = null,
                AssigneeUserId = null,
                OwnerDepartmentId = null,
                BksMemberText = row.BksMemberText,
                KtnbLeaderText = row.KtnbLeaderText,
                NoteText = row.NoteText,
                ProgressText = row.ProgressText,
                ReasonNotCompleted = row.ReasonNotCompleted,
                IsLocked = false,
                InheritedFromTaskId = null,
                HasOpenComment = false
            };

            TaskSupport.EnsureHeaderNormalized(task);
            tasks.Add(task);
            parentStack.Push((row.Level, task.Id));
        }

        return tasks;
    }
}
