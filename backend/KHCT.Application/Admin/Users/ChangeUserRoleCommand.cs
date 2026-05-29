using FluentValidation;
using KHCT.Application.Common.Interfaces;
using KHCT.Application.Common.Support;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Admin.Users;

public record ChangeUserRoleCommand(Guid Id, Guid RoleId) : IRequest<AdminUserDetailDto>;

public class ChangeUserRoleCommandValidator : AbstractValidator<ChangeUserRoleCommand>
{
    public ChangeUserRoleCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.RoleId).NotEmpty();
    }
}

public class ChangeUserRoleHandler : IRequestHandler<ChangeUserRoleCommand, AdminUserDetailDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public ChangeUserRoleHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<AdminUserDetailDto> Handle(ChangeUserRoleCommand request, CancellationToken ct)
    {
        var user = await _db.Users
            .Include(x => x.Department)
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new KeyNotFoundException("User not found.");

        var role = await AdminSupport.RequireRoleAsync(_db, request.RoleId, ct);
        var currentRole = user.UserRoles.FirstOrDefault()?.Role;

        if (currentRole?.Id == role.Id)
        {
            return AdminSupport.ToDetail(user);
        }

        if (string.Equals(currentRole?.Code, AdminSupport.AdminRoleCode, StringComparison.Ordinal) &&
            !string.Equals(role.Code, AdminSupport.AdminRoleCode, StringComparison.Ordinal))
        {
            await AdminSupport.EnsureNotLastActiveAdminAsync(_db, user.Id, ct);
        }

        var before = AdminSupport.UserSnapshot(user);

        _db.UserRoles.RemoveRange(user.UserRoles);
        user.UserRoles.Clear();

        var userRole = new Domain.Entities.UserRole
        {
            UserId = user.Id,
            RoleId = role.Id,
            Role = role
        };
        user.UserRoles.Add(userRole);
        _db.UserRoles.Add(userRole);

        _db.AuditLogs.Add(ApplicationSupport.CreateAudit(
            "user",
            user.Id,
            "change_role",
            _currentUser.UserId,
            before,
            AdminSupport.UserRoleSnapshot(user, role)));

        await _db.SaveChangesAsync(ct);
        return AdminSupport.ToDetail(user);
    }
}
