using KHCT.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Projects;

public record GetProjectsQuery : IRequest<IReadOnlyList<ProjectListItemDto>>;

public class GetProjectsHandler : IRequestHandler<GetProjectsQuery, IReadOnlyList<ProjectListItemDto>>
{
    private readonly IApplicationDbContext _db;

    public GetProjectsHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ProjectListItemDto>> Handle(GetProjectsQuery request, CancellationToken ct)
    {
        var projects = await _db.Projects
            .AsNoTracking()
            .Include(x => x.Leader)
            .Include(x => x.SubLeader)
            .Include(x => x.Members)
            .Include(x => x.Tasks)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

        var list = new List<ProjectListItemDto>();
        foreach (var p in projects)
        {
            var totalTasks = p.Tasks.Count;
            var completedTasks = p.Tasks.Count(t => t.WorkStatus == Domain.Enums.WorkStatus.Done);
            var completionPercent = totalTasks > 0 ? (completedTasks * 100) / totalTasks : 0;

            list.Add(new ProjectListItemDto(
                p.Id,
                p.Name,
                p.Description,
                p.LeaderId,
                p.Leader?.FullName ?? "Tổ trưởng",
                p.SubLeaderId,
                p.SubLeader?.FullName,
                p.Status.ToString().ToLowerInvariant(),
                p.SubmittedAt,
                p.ApprovedAt,
                totalTasks,
                completedTasks,
                completionPercent,
                p.Members.Select(m => m.UserId).ToList(),
                p.CreatedAt,
                p.UpdatedAt));
        }

        return list;
    }
}
