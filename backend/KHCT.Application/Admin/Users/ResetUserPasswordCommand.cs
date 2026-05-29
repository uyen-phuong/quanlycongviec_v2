using FluentValidation;
using KHCT.Application.Common.Interfaces;
using KHCT.Application.Common.Support;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Admin.Users;

public record ResetUserPasswordCommand(Guid Id, string Password) : IRequest<Unit>;

public class ResetUserPasswordCommandValidator : AbstractValidator<ResetUserPasswordCommand>
{
    public ResetUserPasswordCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
    }
}

public class ResetUserPasswordHandler : IRequestHandler<ResetUserPasswordCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly ICurrentUser _currentUser;

    public ResetUserPasswordHandler(IApplicationDbContext db, IPasswordHasher hasher, ICurrentUser currentUser)
    {
        _db = db;
        _hasher = hasher;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(ResetUserPasswordCommand request, CancellationToken ct)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new KeyNotFoundException("User not found.");

        user.PasswordHash = _hasher.Hash(request.Password);
        _db.AuditLogs.Add(ApplicationSupport.CreateAudit(
            "user",
            user.Id,
            "reset_password",
            _currentUser.UserId,
            null,
            AdminSupport.PasswordResetSnapshot(user)));

        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
