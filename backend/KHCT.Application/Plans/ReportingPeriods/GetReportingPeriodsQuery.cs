using KHCT.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Plans.ReportingPeriods;

public record GetReportingPeriodsQuery(Guid PlanId) : IRequest<List<ReportingPeriodDto>>;

public class GetReportingPeriodsHandler : IRequestHandler<GetReportingPeriodsQuery, List<ReportingPeriodDto>>
{
    private readonly IApplicationDbContext _db;

    public GetReportingPeriodsHandler(IApplicationDbContext db) => _db = db;

    public async Task<List<ReportingPeriodDto>> Handle(GetReportingPeriodsQuery request, CancellationToken ct)
    {
        var periods = await _db.PlanReportingPeriods
            .AsNoTracking()
            .Include(x => x.ApprovedByUser)
            .Where(x => x.PlanId == request.PlanId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(ct);

        return periods.Select(p => new ReportingPeriodDto(
            p.Id,
            p.PlanId,
            p.PeriodLabel,
            p.ProgressText,
            p.CompletionPercent,
            p.Status.ToString().ToLowerInvariant(),
            p.ApprovedByUserId,
            p.ApprovedByUser?.FullName,
            p.ApprovedAt,
            p.CreatedAt,
            p.UpdatedAt
        )).ToList();
    }
}
