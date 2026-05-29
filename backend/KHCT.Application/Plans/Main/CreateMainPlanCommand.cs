using FluentValidation;
using KHCT.Application.Common.Interfaces;
using KHCT.Domain.Entities;
using KHCT.Domain.Enums;
using MediatR;

namespace KHCT.Application.Plans.Main;

public record CreateMainPlanCommand(int Year, int Month) : IRequest<PlanDetailDto>;

public class CreateMainPlanCommandValidator : AbstractValidator<CreateMainPlanCommand>
{
    public CreateMainPlanCommandValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(2020, 2100);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
    }
}

public class CreateMainPlanHandler : IRequestHandler<CreateMainPlanCommand, PlanDetailDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public CreateMainPlanHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PlanDetailDto> Handle(CreateMainPlanCommand request, CancellationToken ct)
    {
        await PlanSupport.EnsureUniqueMainAsync(_db, request.Year, request.Month, null, ct);

        var plan = new Plan
        {
            Id = Guid.NewGuid(),
            Scope = PlanScope.Main,
            DepartmentId = null,
            Year = request.Year,
            Month = request.Month,
            Status = ApprovalStatus.Draft,
            CreatedById = PlanSupport.RequireActorId(_currentUser)
        };

        _db.Plans.Add(plan);

        await _db.SaveChangesAsync(ct);
        return PlanSupport.ToDetail(plan);
    }
}
