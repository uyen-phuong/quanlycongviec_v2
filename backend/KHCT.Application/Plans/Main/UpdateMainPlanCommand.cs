using FluentValidation;
using KHCT.Application.Common.Interfaces;
using KHCT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Plans.Main;

public record UpdateMainPlanCommand(Guid Id, string Name, int Year, int Month, string ReportingPeriodType, Guid? KtnbLeaderId) : IRequest<PlanDetailDto>;

public class UpdateMainPlanCommandValidator : AbstractValidator<UpdateMainPlanCommand>
{
    public UpdateMainPlanCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Year).InclusiveBetween(2020, 2100);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
        RuleFor(x => x.ReportingPeriodType).NotEmpty();
    }
}

public class UpdateMainPlanHandler : IRequestHandler<UpdateMainPlanCommand, PlanDetailDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public UpdateMainPlanHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PlanDetailDto> Handle(UpdateMainPlanCommand request, CancellationToken ct)
    {
        var plan = await _db.Plans
            .Include(x => x.Department)
            .Include(x => x.CreatedBy)
            .Include(x => x.KtnbLeader)
            .Include(x => x.Tasks)
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.Scope == PlanScope.Main, ct)
            ?? throw new KeyNotFoundException("Plan not found.");

        PlanSupport.EnsureEditable(plan);
        await PlanSupport.EnsureUniqueMainAsync(_db, request.Year, request.Month, plan.Id, ct);
        var periodType = PlanSupport.ParseReportingPeriodType(request.ReportingPeriodType);

        plan.Name = request.Name;
        plan.Year = request.Year;
        plan.Month = request.Month;
        plan.ReportingPeriodType = periodType;
        plan.KtnbLeaderId = request.KtnbLeaderId;
        
        await PlanSupport.ResetWorkflowAsync(_db, plan, ct);

        await _db.SaveChangesAsync(ct);
        return PlanSupport.ToDetail(plan);
    }
}
