using FluentValidation;
using KHCT.Application.Common.Interfaces;
using KHCT.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Tasks.Workflow;

public record GetTaskWorkflowHistoryQuery(Guid PlanId, string? DepartmentCode) : IRequest<IReadOnlyList<TaskApprovalHistoryDto>>;

public class GetTaskWorkflowHistoryQueryValidator : AbstractValidator<GetTaskWorkflowHistoryQuery>
{
    public GetTaskWorkflowHistoryQueryValidator()
    {
        RuleFor(x => x.PlanId).NotEmpty();
        RuleFor(x => x.DepartmentCode).MaximumLength(32);
    }
}

public class GetTaskWorkflowHistoryHandler : IRequestHandler<GetTaskWorkflowHistoryQuery, IReadOnlyList<TaskApprovalHistoryDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetTaskWorkflowHistoryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<TaskApprovalHistoryDto>> Handle(GetTaskWorkflowHistoryQuery request, CancellationToken ct)
    {
        var plan = await _db.Plans.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.PlanId, ct)
            ?? throw new KeyNotFoundException("Plan not found.");

        TaskSupport.EnsureCanReadPlanTasks(plan, _currentUser);
        var department = await TaskWorkflowSupport.ResolveDepartmentAsync(_db, request.DepartmentCode, ct);
        var departmentId = department?.Id;
        var taskIds = await TaskWorkflowSupport.ApplyWorkflowScope(_db.Tasks.AsNoTracking(), request.PlanId, department?.Id)
            .Select(x => x.Id)
            .ToListAsync(ct);

        if (taskIds.Count == 0)
        {
            return [];
        }

        var items = await _db.TaskApprovalHistories
            .AsNoTracking()
            .Include(x => x.ActorUser)
            .Where(x => taskIds.Contains(x.TaskId) && x.DepartmentId == departmentId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(ct);

        return items.Select(TaskWorkflowSupport.ToDto).ToList();
    }
}
