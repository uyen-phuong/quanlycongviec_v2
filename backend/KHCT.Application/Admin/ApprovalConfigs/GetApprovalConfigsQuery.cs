using KHCT.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Admin.ApprovalConfigs;

public record GetApprovalConfigsQuery() : IRequest<IReadOnlyList<ApprovalConfigDto>>;

public class GetApprovalConfigsHandler : IRequestHandler<GetApprovalConfigsQuery, IReadOnlyList<ApprovalConfigDto>>
{
    private readonly IApplicationDbContext _db;

    public GetApprovalConfigsHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ApprovalConfigDto>> Handle(GetApprovalConfigsQuery request, CancellationToken ct)
    {
        var items = await _db.ApprovalConfigs
            .AsNoTracking()
            .Include(x => x.Department)
            .Include(x => x.Role)
            .OrderBy(x => x.Scope)
            .ThenBy(x => x.Level)
            .ToListAsync(ct);

        return items.Select(AdminSupport.ToDto).ToList();
    }
}
