using KHCT.Application.Common.Interfaces;
using KHCT.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Admin.Users;

public record GetUsersQuery(
    int Page = 1,
    int PageSize = 20,
    Guid? DepartmentId = null,
    string? RoleCode = null,
    bool? IsActive = null,
    string? Keyword = null) : IRequest<PagedResult<AdminUserListItemDto>>;

public class GetUsersHandler : IRequestHandler<GetUsersQuery, PagedResult<AdminUserListItemDto>>
{
    private readonly IApplicationDbContext _db;

    public GetUsersHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<AdminUserListItemDto>> Handle(GetUsersQuery request, CancellationToken ct)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 200);

        var query = _db.Users
            .AsNoTracking()
            .Include(x => x.Department)
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .AsQueryable();

        if (request.DepartmentId.HasValue)
        {
            query = query.Where(x => x.DepartmentId == request.DepartmentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.RoleCode))
        {
            var roleCode = request.RoleCode.Trim().ToUpperInvariant();
            query = query.Where(x => x.UserRoles.Any(ur => ur.Role != null && ur.Role.Code == roleCode));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim();
            query = query.Where(x =>
                x.Username.Contains(keyword) ||
                x.FullName.Contains(keyword) ||
                (x.Email != null && x.Email.Contains(keyword)));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(x => x.Username)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<AdminUserListItemDto>(
            items.Select(AdminSupport.ToListItem).ToList(),
            page,
            pageSize,
            total);
    }
}
