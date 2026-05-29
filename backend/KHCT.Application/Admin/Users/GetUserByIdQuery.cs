using KHCT.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Admin.Users;

public record GetUserByIdQuery(Guid Id) : IRequest<AdminUserDetailDto>;

public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, AdminUserDetailDto>
{
    private readonly IApplicationDbContext _db;

    public GetUserByIdHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<AdminUserDetailDto> Handle(GetUserByIdQuery request, CancellationToken ct)
    {
        var user = await _db.Users
            .AsNoTracking()
            .Include(x => x.Department)
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new KeyNotFoundException("User not found.");

        return AdminSupport.ToDetail(user);
    }
}
