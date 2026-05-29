using KHCT.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Admin.Roles;

public record GetRolesQuery() : IRequest<IReadOnlyList<RoleDto>>;

public class GetRolesHandler : IRequestHandler<GetRolesQuery, IReadOnlyList<RoleDto>>
{
    private readonly IApplicationDbContext _db;

    public GetRolesHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<RoleDto>> Handle(GetRolesQuery request, CancellationToken ct)
    {
        var items = await _db.Roles
            .AsNoTracking()
            .OrderBy(x => x.Code)
            .ToListAsync(ct);

        return items.Select(AdminSupport.ToDto).ToList();
    }
}
