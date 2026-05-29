using FluentValidation;
using KHCT.Application.Common.Interfaces;
using KHCT.Domain.Common;
using KHCT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Plans.Main;

public record DeleteMainPlanCommand(Guid Id) : IRequest<bool>;

public class DeleteMainPlanCommandValidator : AbstractValidator<DeleteMainPlanCommand>
{
    public DeleteMainPlanCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class DeleteMainPlanHandler : IRequestHandler<DeleteMainPlanCommand, bool>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public DeleteMainPlanHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<bool> Handle(DeleteMainPlanCommand request, CancellationToken ct)
    {
        var plan = await _db.Plans
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.Scope == PlanScope.Main, ct)
            ?? throw new KeyNotFoundException("Plan not found.");

        PlanSupport.EnsureDraft(plan);

        var hasTasks = await _db.Tasks.AnyAsync(x => x.PlanId == plan.Id, ct);
        if (hasTasks)
        {
            throw new DomainException("plan_has_tasks", "Cannot delete a plan that already has tasks.");
        }

        _db.Plans.Remove(plan);

        await _db.SaveChangesAsync(ct);
        return true;
    }
}
