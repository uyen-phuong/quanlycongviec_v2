using KHCT.Application.Common.Interfaces;
using KHCT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Plans.Main;

public record GetMainPlanByIdQuery(Guid Id) : IRequest<PlanDetailDto>;

public class GetMainPlanByIdHandler : IRequestHandler<GetMainPlanByIdQuery, PlanDetailDto>
{
    private readonly IApplicationDbContext _db;

    public GetMainPlanByIdHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PlanDetailDto> Handle(GetMainPlanByIdQuery request, CancellationToken ct)
    {
        var plan = await _db.Plans
            .AsNoTracking()
            .Include(x => x.Department)
            .Include(x => x.CreatedBy)
            .Include(x => x.Tasks)
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.Scope == PlanScope.Main, ct)
            ?? throw new KeyNotFoundException("Plan not found.");

        return PlanSupport.ToDetail(plan);
    }
}
