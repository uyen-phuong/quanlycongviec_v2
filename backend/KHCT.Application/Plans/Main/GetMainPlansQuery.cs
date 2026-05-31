using KHCT.Application.Common.Interfaces;
using KHCT.Application.Common.Models;
using KHCT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Plans.Main;

public record GetMainPlansQuery(
    int Page,
    int PageSize,
    int? Year,
    int? Month,
    WorkflowStatus? Status) : IRequest<PagedResult<PlanListItemDto>>;

public class GetMainPlansHandler : IRequestHandler<GetMainPlansQuery, PagedResult<PlanListItemDto>>
{
    private readonly IApplicationDbContext _db;

    public GetMainPlansHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<PlanListItemDto>> Handle(GetMainPlansQuery request, CancellationToken ct)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var query = _db.Plans
            .AsNoTracking()
            .Include(x => x.Department)
            .Include(x => x.CreatedBy)
            .Where(x => x.Scope == PlanScope.Main);

        if (request.Year.HasValue)
        {
            query = query.Where(x => x.Year == request.Year.Value);
        }

        if (request.Month.HasValue)
        {
            query = query.Where(x => x.Month == request.Month.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .ThenByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<PlanListItemDto>(items.Select(PlanSupport.ToListItem).ToList(), page, pageSize, total);
    }
}
