using FluentValidation;
using KHCT.Application.Common.Interfaces;
using KHCT.Application.Common.Security;
using KHCT.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Auth.Login;

public record LoginCommand(string Username, string Password) : IRequest<AuthResultDto>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).MaximumLength(128);
    }
}

public class LoginHandler : IRequestHandler<LoginCommand, AuthResultDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokens;

    public LoginHandler(IApplicationDbContext db, IPasswordHasher hasher, ITokenService tokens)
    {
        _db = db;
        _hasher = hasher;
        _tokens = tokens;
    }

    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken ct)
    {
        var normalizedUsername = UsernameNormalizer.Normalize(request.Username);
        var user = await _db.Users
            .Include(u => u.Department)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username == normalizedUsername && u.IsActive, ct);

        if (user is null || !_hasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        var roleCodes = user.UserRoles.Select(ur => ur.Role!.Code).ToList();
        var access = _tokens.CreateAccessToken(user, roleCodes);
        var refresh = _tokens.CreateRefreshToken();

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refresh.TokenHash,
            ExpiresAt = refresh.ExpiresAt
        });
        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        var dto = new UserDto(
            user.Id,
            user.Username,
            user.FullName,
            user.Email,
            user.DepartmentId,
            user.Department?.Code,
            roleCodes);

        return new AuthResultDto(access.Token, access.ExpiresAt, refresh.RawToken, refresh.ExpiresAt, dto);
    }
}
