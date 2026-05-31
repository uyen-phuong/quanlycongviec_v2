using KHCT.Application.Common.Interfaces;
using KHCT.Application.Plans;
using KHCT.Domain.Common;
using KHCT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Tasks;

public record GetTasksByProjectQuery(Guid ProjectId) : IRequest<IReadOnlyList<TaskListItemDto>>;

public class GetTasksByProjectHandler : IRequestHandler<GetTasksByProjectQuery, IReadOnlyList<TaskListItemDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetTasksByProjectHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<TaskListItemDto>> Handle(GetTasksByProjectQuery request, CancellationToken ct)
    {
        var project = await _db.Projects
            .AsNoTracking()
            .Include(x => x.Members)
            .FirstOrDefaultAsync(x => x.Id == request.ProjectId, ct)
            ?? throw new KeyNotFoundException("Project not found.");

        // Access Control
        var isGlobalMonitor = PlanSupport.HasRole(_currentUser, PlanSupport.RoleAdmin) ||
                              PlanSupport.HasRole(_currentUser, PlanSupport.RoleVanThu) ||
                              PlanSupport.HasRole(_currentUser, PlanSupport.RoleTruongKh) ||
                              PlanSupport.HasRole(_currentUser, PlanSupport.RoleTruongKtnb) ||
                              PlanSupport.HasRole(_currentUser, PlanSupport.RolePhoTruongKtnb);

        var isMember = project.LeaderId == _currentUser.UserId ||
                       project.SubLeaderId == _currentUser.UserId ||
                       project.Members.Any(m => m.UserId == _currentUser.UserId);

        if (!isGlobalMonitor && !isMember)
        {
            throw new ForbiddenException("forbidden_project_access", "You do not have access to this project.");
        }

        var tasks = await _db.Tasks
            .AsNoTracking()
            .Include(x => x.AssigneeUser)
            .Include(x => x.OwnerDepartment)
            .Include(x => x.SupportingDepts)
                .ThenInclude(x => x.Department)
            .Where(x => x.ProjectId == request.ProjectId)
            .OrderBy(x => x.ParentTaskId.HasValue ? 1 : 0)
            .ThenBy(x => x.ParentTaskId)
            .ThenBy(x => x.DisplayOrder)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(ct);

        return tasks.Select(TaskSupport.ToListItem).ToList();
    }
}
