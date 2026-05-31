using KHCT.Application.Common.Interfaces;
using KHCT.Domain.Common;
using KHCT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Plans.Workflow;

public record ReInheritCommand(Guid PlanId) : IRequest<ReInheritResult>;

public record ReInheritResult(Guid PlanId, int Year, int Month, string Status);

public class ReInheritCommandHandler : IRequestHandler<ReInheritCommand, ReInheritResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public ReInheritCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ReInheritResult> Handle(ReInheritCommand request, CancellationToken ct)
    {
        var plan = await _db.Plans
            .Include(x => x.Tasks)
            .FirstOrDefaultAsync(x => x.Id == request.PlanId, ct)
            ?? throw new KeyNotFoundException("Plan not found.");

        if (plan.Scope != PlanScope.Main)
            throw new DomainException("invalid_scope", "Re-inherit only applies to main plans.");

        if (plan.Status != WorkflowStatus.Approved2)
            throw new DomainException("invalid_status", "Re-inherit requires main plan status approved_2.");

        InheritService.EnsureInheritReady(plan);
        await InheritService.RunAsync(_db, plan, _currentUser.UserId, ct);
        await _db.SaveChangesAsync(ct);

        return new ReInheritResult(plan.Id, plan.Year, plan.Month, PlanSupport.StatusCode(plan.Status));
    }
}
