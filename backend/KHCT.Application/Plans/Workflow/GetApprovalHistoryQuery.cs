using KHCT.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Plans.Workflow;

public record GetApprovalHistoryQuery(Guid PlanId) : IRequest<IReadOnlyList<ApprovalHistoryDto>>;

public class GetApprovalHistoryHandler : IRequestHandler<GetApprovalHistoryQuery, IReadOnlyList<ApprovalHistoryDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetApprovalHistoryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<ApprovalHistoryDto>> Handle(GetApprovalHistoryQuery request, CancellationToken ct)
    {
        var plan = await WorkflowSupport.RequirePlanAsync(_db, request.PlanId, ct);
        WorkflowSupport.EnsureCanReadWorkflow(plan, _currentUser);

        var items = await _db.ApprovalHistories
            .AsNoTracking()
            .Include(x => x.ActorUser)
            .Where(x => x.PlanId == request.PlanId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(ct);

        return items.Select(WorkflowSupport.ToDto).ToList();
    }
}
