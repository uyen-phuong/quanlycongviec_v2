using FluentValidation;
using KHCT.Application.Common.Interfaces;
using KHCT.Application.Common.Security;
using KHCT.Application.Common.Support;
using KHCT.Domain.Common;
using KHCT.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Admin.Users;

public record CreateUserCommand(
    string Username,
    string Password,
    string FullName,
    string? Email,
    Guid? DepartmentId,
    Guid? PositionId,
    Guid RoleId,
    bool IsActive) : IRequest<AdminUserDetailDto>;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.RoleId).NotEmpty();
    }
}

public class CreateUserHandler : IRequestHandler<CreateUserCommand, AdminUserDetailDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly ICurrentUser _currentUser;

    public CreateUserHandler(IApplicationDbContext db, IPasswordHasher hasher, ICurrentUser currentUser)
    {
        _db = db;
        _hasher = hasher;
        _currentUser = currentUser;
    }

    public async Task<AdminUserDetailDto> Handle(CreateUserCommand request, CancellationToken ct)
    {
        var username = UsernameNormalizer.Normalize(request.Username);
        if (await _db.Users.AnyAsync(x => x.Username == username, ct))
        {
            throw new DomainException("username_taken", "Username already exists.");
        }

        var role = await AdminSupport.RequireRoleAsync(_db, request.RoleId, ct);
        var department = await ApplicationSupport.RequireActiveDepartmentAsync(_db, request.DepartmentId, ct);
        var position = request.PositionId.HasValue
            ? await _db.Positions.FindAsync([request.PositionId.Value], ct)
            : null;

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            PasswordHash = _hasher.Hash(request.Password),
            FullName = request.FullName.Trim(),
            Email = AdminSupport.NormalizeEmail(request.Email),
            DepartmentId = department?.Id,
            PositionId = position?.Id,
            IsActive = request.IsActive
        };

        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id
        };

        user.UserRoles.Add(userRole);
        _db.Users.Add(user);
        _db.UserRoles.Add(userRole);
        await _db.SaveChangesAsync(ct);

        user.Department = department;
        user.Position = position;
        userRole.Role = role;
        return AdminSupport.ToDetail(user);
    }
}
