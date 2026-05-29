using FluentValidation;
using KHCT.Application.Common.Interfaces;
using KHCT.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Auth.Refresh;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResultDto>;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

public class RefreshHandler : IRequestHandler<RefreshTokenCommand, AuthResultDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ITokenService _tokens;

    public RefreshHandler(IApplicationDbContext db, ITokenService tokens)
    {
        _db = db;
        _tokens = tokens;
    }

    public async System.Threading.Tasks.Task<AuthResultDto> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var hash = _tokens.HashRefreshToken(request.RefreshToken);
        var existing = await _db.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == hash, ct)
            ?? throw new UnauthorizedAccessException("Refresh token không hợp lệ");

        if (existing.RevokedAt is not null || DateTime.UtcNow >= existing.ExpiresAt)
            throw new UnauthorizedAccessException("Refresh token đã hết hạn hoặc bị thu hồi");

        var user = await _db.Users
            .Include(u => u.Department)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == existing.UserId && u.IsActive, ct)
            ?? throw new UnauthorizedAccessException("Người dùng không tồn tại hoặc đã bị khoá");

        var roleCodes = user.UserRoles.Select(ur => ur.Role!.Code).ToList();
        var access = _tokens.CreateAccessToken(user, roleCodes);
        var newRefresh = _tokens.CreateRefreshToken();

        existing.RevokedAt = DateTime.UtcNow;
        existing.ReplacedByTokenHash = newRefresh.TokenHash;
        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = newRefresh.TokenHash,
            ExpiresAt = newRefresh.ExpiresAt
        });
        await _db.SaveChangesAsync(ct);

        var dto = new UserDto(user.Id, user.Username, user.FullName, user.Email,
            user.DepartmentId, user.Department?.Code, roleCodes);
        return new AuthResultDto(access.Token, access.ExpiresAt, newRefresh.RawToken, newRefresh.ExpiresAt, dto);
    }
}
