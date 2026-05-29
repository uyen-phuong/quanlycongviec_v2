using KHCT.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Plans.Workflow;

public record GetPlanLineCommentsQuery(Guid PlanId) : IRequest<IReadOnlyList<LineCommentDto>>;

public class GetPlanLineCommentsHandler : IRequestHandler<GetPlanLineCommentsQuery, IReadOnlyList<LineCommentDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetPlanLineCommentsHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<LineCommentDto>> Handle(GetPlanLineCommentsQuery request, CancellationToken ct)
    {
        var plan = await WorkflowSupport.RequirePlanAsync(_db, request.PlanId, ct);
        WorkflowSupport.EnsureCanReadWorkflow(plan, _currentUser);

        var items = await _db.LineComments
            .AsNoTracking()
            .Include(x => x.Task)
            .Include(x => x.AuthorUser)
            .Include(x => x.ResolvedByUser)
            .Where(x => x.Task != null && x.Task.PlanId == request.PlanId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

        return items.Select(WorkflowSupport.ToDto).ToList();
    }
}
