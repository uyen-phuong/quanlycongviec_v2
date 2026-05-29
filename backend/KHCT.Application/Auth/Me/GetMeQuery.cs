using KHCT.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Auth.Me;

public record GetMeQuery() : IRequest<UserDto>;

public class GetMeHandler : IRequestHandler<GetMeQuery, UserDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _current;

    public GetMeHandler(IApplicationDbContext db, ICurrentUser current)
    {
        _db = db;
        _current = current;
    }

    public async System.Threading.Tasks.Task<UserDto> Handle(GetMeQuery request, CancellationToken ct)
    {
        if (!_current.IsAuthenticated || _current.UserId is null)
            throw new UnauthorizedAccessException("Chưa đăng nhập");

        var user = await _db.Users
            .Include(u => u.Department)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == _current.UserId, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng");

        var roleCodes = user.UserRoles.Select(ur => ur.Role!.Code).ToList();
        return new UserDto(user.Id, user.Username, user.FullName, user.Email,
            user.DepartmentId, user.Department?.Code, roleCodes);
    }
}
