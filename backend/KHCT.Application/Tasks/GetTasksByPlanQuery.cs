using KHCT.Application.Common.Interfaces;
using KHCT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Tasks;

public record GetTasksByPlanQuery(Guid PlanId, int? WorkType, string? DepartmentCode) : IRequest<IReadOnlyList<TaskListItemDto>>;

public class GetTasksByPlanHandler : IRequestHandler<GetTasksByPlanQuery, IReadOnlyList<TaskListItemDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetTasksByPlanHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<TaskListItemDto>> Handle(GetTasksByPlanQuery request, CancellationToken ct)
    {
        var plan = await _db.Plans.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.PlanId, ct)
            ?? throw new KeyNotFoundException("Plan not found.");

        TaskSupport.EnsureCanReadPlanTasks(plan, _currentUser);

        var query = _db.Tasks
            .AsNoTracking()
            .Include(x => x.AssigneeUser)
            .Include(x => x.OwnerDepartment)
            .Include(x => x.SupportingDepts)
                .ThenInclude(x => x.Department)
            .Where(x => x.PlanId == plan.Id);

        var departmentCode = string.IsNullOrWhiteSpace(request.DepartmentCode)
            ? null
            : request.DepartmentCode.Trim().ToUpperInvariant();

        if (plan.Scope == PlanScope.Main && departmentCode is null)
        {
            query = query.Where(x => x.WorkType == WorkType.General);
        }
        else if (request.WorkType.HasValue)
        {
            query = query.Where(x => x.WorkType == (WorkType)request.WorkType.Value);
        }

        var items = await query
            .OrderBy(x => x.ParentTaskId.HasValue ? 1 : 0)
            .ThenBy(x => x.ParentTaskId)
            .ThenBy(x => x.DisplayOrder)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(ct);

        if (departmentCode is not null && plan.Scope == PlanScope.Main)
        {
            var selectedIds = items
                .Where(x => !x.IsHeader && x.OwnerDepartment?.Code == departmentCode)
                .Select(x => x.Id)
                .ToHashSet();

            var includedIds = new HashSet<Guid>(selectedIds);
            foreach (var item in items.Where(x => selectedIds.Contains(x.Id)))
            {
                var cursor = item.ParentTaskId;
                while (cursor.HasValue)
                {
                    if (!includedIds.Add(cursor.Value))
                    {
                        break;
                    }

                    cursor = items.FirstOrDefault(x => x.Id == cursor.Value)?.ParentTaskId;
                }
            }

            items = items
                .Where(x => x.IsHeader ? includedIds.Contains(x.Id) : selectedIds.Contains(x.Id))
                .ToList();
        }

        return items.Select(TaskSupport.ToListItem).ToList();
    }
}
