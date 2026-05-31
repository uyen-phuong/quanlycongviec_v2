using FluentValidation;
using KHCT.Application.Common.Interfaces;
using KHCT.Application.Common.Support;
using KHCT.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Admin.Users;

public record UpdateUserCommand(
    Guid Id,
    string FullName,
    string? Email,
    Guid? DepartmentId,
    Guid? PositionId,
    bool IsActive) : IRequest<AdminUserDetailDto>;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}

public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, AdminUserDetailDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public UpdateUserHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<AdminUserDetailDto> Handle(UpdateUserCommand request, CancellationToken ct)
    {
        var user = await _db.Users
            .Include(x => x.Department)
            .Include(x => x.Position)
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new KeyNotFoundException("User not found.");

        if (!request.IsActive && user.IsActive && AdminSupport.IsAdmin(user))
        {
            await AdminSupport.EnsureNotLastActiveAdminAsync(_db, user.Id, ct);
        }

        var department = await ApplicationSupport.RequireActiveDepartmentAsync(_db, request.DepartmentId, ct);
        var position = request.PositionId.HasValue
            ? await _db.Positions.FindAsync([request.PositionId.Value], ct)
            : null;

        user.FullName = request.FullName.Trim();
        user.Email = AdminSupport.NormalizeEmail(request.Email);
        user.DepartmentId = department?.Id;
        user.Department = department;
        user.PositionId = position?.Id;
        user.Position = position;
        user.IsActive = request.IsActive;

        await _db.SaveChangesAsync(ct);
        return AdminSupport.ToDetail(user);
    }
}
