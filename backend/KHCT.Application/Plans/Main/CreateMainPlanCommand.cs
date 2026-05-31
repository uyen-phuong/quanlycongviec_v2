using FluentValidation;
using KHCT.Application.Common.Interfaces;
using KHCT.Domain.Entities;
using KHCT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Plans.Main;

public record CreateMainPlanCommand(string Name, int Year, int Month, string ReportingPeriodType, Guid? KtnbLeaderId) : IRequest<PlanDetailDto>;

public class CreateMainPlanCommandValidator : AbstractValidator<CreateMainPlanCommand>
{
    public CreateMainPlanCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Year).InclusiveBetween(2020, 2100);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
        RuleFor(x => x.ReportingPeriodType).NotEmpty();
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
        var periodType = PlanSupport.ParseReportingPeriodType(request.ReportingPeriodType);

        var plan = new Plan
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Scope = PlanScope.Main,
            DepartmentId = null,
            Year = request.Year,
            Month = request.Month,
            ReportingPeriodType = periodType,
            CurrentPeriodIndex = 0,
            Status = WorkflowStatus.Draft,
            CreatedById = PlanSupport.RequireActorId(_currentUser),
            KtnbLeaderId = request.KtnbLeaderId
        };

        _db.Plans.Add(plan);

        await _db.SaveChangesAsync(ct);
        
        // Fetch full plan with navigation properties for ToDetail
        var fullPlan = await _db.Plans
            .Include(x => x.Department)
            .Include(x => x.CreatedBy)
            .Include(x => x.KtnbLeader)
            .Include(x => x.Tasks)
            .FirstAsync(x => x.Id == plan.Id, ct);

        return PlanSupport.ToDetail(fullPlan);
    }
}
