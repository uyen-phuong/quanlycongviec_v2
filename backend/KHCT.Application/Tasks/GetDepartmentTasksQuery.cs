using KHCT.Application.Common.Interfaces;
using KHCT.Application.Common.Support;
using KHCT.Application.Plans;
using KHCT.Domain.Common;
using KHCT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Tasks;

public record GetDepartmentTasksQuery(string? DepartmentCode) : IRequest<IReadOnlyList<TaskListItemDto>>;

public class GetDepartmentTasksHandler : IRequestHandler<GetDepartmentTasksQuery, IReadOnlyList<TaskListItemDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetDepartmentTasksHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<TaskListItemDto>> Handle(GetDepartmentTasksQuery request, CancellationToken ct)
    {
        Guid departmentId;

        if (string.IsNullOrWhiteSpace(request.DepartmentCode))
        {
            if (!_currentUser.DepartmentId.HasValue)
            {
                throw new DomainException("user_department_missing", "User has no department assigned.");
            }
            departmentId = _currentUser.DepartmentId.Value;
        }
        else
        {
            var normalizedCode = request.DepartmentCode.Trim().ToUpperInvariant();
            var department = await _db.Departments.AsNoTracking().FirstOrDefaultAsync(x => x.Code == normalizedCode, ct)
                ?? throw new KeyNotFoundException("Department not found.");
            departmentId = department.Id;
        }

        // Access Control Check
        var isGlobalMonitor = PlanSupport.HasRole(_currentUser, PlanSupport.RoleAdmin) ||
                              PlanSupport.HasRole(_currentUser, PlanSupport.RoleVanThu) ||
                              PlanSupport.HasRole(_currentUser, PlanSupport.RoleTruongKtnb) ||
                              PlanSupport.HasRole(_currentUser, PlanSupport.RolePhoTruongKtnb);

        if (!isGlobalMonitor && _currentUser.DepartmentId != departmentId)
        {
            throw new ForbiddenException("forbidden_department_access", "You do not have access to this department's tasks.");
        }

        var tasks = await _db.Tasks
            .AsNoTracking()
            .Include(x => x.AssigneeUser)
            .Include(x => x.OwnerDepartment)
            .Include(x => x.SupportingDepts)
                .ThenInclude(x => x.Department)
            .Where(x => x.Category == TaskCategory.DepartmentTask && x.OwnerDepartmentId == departmentId)
            .OrderBy(x => x.ParentTaskId.HasValue ? 1 : 0)
            .ThenBy(x => x.ParentTaskId)
            .ThenBy(x => x.DisplayOrder)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(ct);

        return tasks.Select(TaskSupport.ToListItem).ToList();
    }
}
