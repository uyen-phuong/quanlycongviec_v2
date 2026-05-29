using KHCT.Application.Common.Interfaces;
using KHCT.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.PersonalEvaluations.Queries;

public record ScorableUserDto(
    Guid Id,
    string FullName,
    Guid? DepartmentId,
    string? DepartmentCode,
    string? DepartmentName,
    string RoleCode,
    string RoleName);

public record GetScorableUsersQuery() : IRequest<IReadOnlyList<ScorableUserDto>>;

public class GetScorableUsersHandler : IRequestHandler<GetScorableUsersQuery, IReadOnlyList<ScorableUserDto>>
{
    private static readonly string[] StaffRoles = ["NHAN_VIEN", "TRUONG_NHOM"];
    private static readonly string[] DeputyScopeRoles = ["NHAN_VIEN", "TRUONG_NHOM", "PHO_TRUONG_KTNB"];
    private static readonly string[] HeadScopeRoles = ["NHAN_VIEN", "TRUONG_NHOM", "PHO_TRUONG_KTNB"];

    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetScorableUsersHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async System.Threading.Tasks.Task<IReadOnlyList<ScorableUserDto>> Handle(GetScorableUsersQuery request, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAccessException();

        var roles = _currentUser.Roles;
        var query = _db.Users
            .AsNoTracking()
            .Include(x => x.Department)
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .Where(x => x.IsActive);

        if (roles.Contains("TRUONG_KTNB") || roles.Contains("ADMIN"))
        {
            query = query.Where(x => x.UserRoles.Any(ur => ur.Role != null && HeadScopeRoles.Contains(ur.Role.Code)));
        }
        else if (roles.Contains("PHO_TRUONG_KTNB"))
        {
            query = query.Where(x => x.UserRoles.Any(ur => ur.Role != null && DeputyScopeRoles.Contains(ur.Role.Code)));
        }
        else if (roles.Contains("TRUONG_PHONG") || roles.Contains("TRUONG_KH"))
        {
            if (!_currentUser.DepartmentId.HasValue)
                throw new ForbiddenException(PersonalEvaluationSupport.ForbiddenRole, "Không có phòng ban để xem danh sách chấm điểm.");

            var departmentId = _currentUser.DepartmentId.Value;
            query = query.Where(x =>
                x.DepartmentId == departmentId &&
                x.UserRoles.Any(ur => ur.Role != null && StaffRoles.Contains(ur.Role.Code)));
        }
        else if (roles.Contains("TRUONG_NHOM"))
        {
            if (!_currentUser.DepartmentId.HasValue)
                throw new ForbiddenException(PersonalEvaluationSupport.ForbiddenRole, "Không có phòng ban để xem danh sách chấm điểm.");

            var departmentId = _currentUser.DepartmentId.Value;
            query = query.Where(x =>
                x.DepartmentId == departmentId &&
                x.UserRoles.Any(ur => ur.Role != null && ur.Role.Code == "NHAN_VIEN"));
        }
        else
        {
            return [];
        }

        var users = await query
            .OrderBy(x => x.Department != null ? x.Department.Code : "")
            .ThenBy(x => x.FullName)
            .ToListAsync(ct);

        return users
            .Where(x => x.Id != _currentUser.UserId.Value)
            .Select(ToDto)
            .ToList();
    }

    private static ScorableUserDto ToDto(KHCT.Domain.Entities.User user)
    {
        var role = user.UserRoles
            .Select(x => x.Role)
            .Where(x => x != null)
            .OrderBy(x => RoleSort(x!.Code))
            .First();

        return new ScorableUserDto(
            user.Id,
            user.FullName,
            user.DepartmentId,
            user.Department?.Code,
            user.Department?.Name,
            role!.Code,
            role.Name);
    }

    private static int RoleSort(string roleCode) =>
        roleCode switch
        {
            "PHO_TRUONG_KTNB" => 1,
            "TRUONG_NHOM" => 2,
            "NHAN_VIEN" => 3,
            _ => 99
        };
}
