using KHCT.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Admin.Positions;

public record GetPositionsQuery : IRequest<List<PositionDto>>;

public class GetPositionsHandler : IRequestHandler<GetPositionsQuery, List<PositionDto>>
{
    private readonly IApplicationDbContext _db;

    public GetPositionsHandler(IApplicationDbContext db) => _db = db;

    public async Task<List<PositionDto>> Handle(GetPositionsQuery request, CancellationToken ct)
    {
        var items = await _db.Positions
            .AsNoTracking()
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(ct);

        return items.Select(AdminSupport.ToDto).ToList();
    }
}
