using KHCT.Application.Common.Interfaces;
using KHCT.Application.Plans;
using KHCT.Domain.Common;
using KHCT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Tasks;

public record GetPersonalTasksQuery : IRequest<IReadOnlyList<TaskListItemDto>>;

public class GetPersonalTasksHandler : IRequestHandler<GetPersonalTasksQuery, IReadOnlyList<TaskListItemDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetPersonalTasksHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<TaskListItemDto>> Handle(GetPersonalTasksQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId;

        var tasks = await _db.Tasks
            .AsNoTracking()
            .Include(x => x.AssigneeUser)
            .Include(x => x.OwnerDepartment)
            .Include(x => x.SupportingDepts)
                .ThenInclude(x => x.Department)
            .Where(x => x.Category == TaskCategory.PersonalTask && x.AssigneeUserId == userId)
            .OrderBy(x => x.ParentTaskId.HasValue ? 1 : 0)
            .ThenBy(x => x.ParentTaskId)
            .ThenBy(x => x.DisplayOrder)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(ct);

        return tasks.Select(TaskSupport.ToListItem).ToList();
    }
}
