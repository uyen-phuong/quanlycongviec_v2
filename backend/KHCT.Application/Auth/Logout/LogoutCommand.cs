using KHCT.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Auth.Logout;

public record LogoutCommand(string? RefreshToken) : IRequest<Unit>;

public class LogoutHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ITokenService _tokens;

    public LogoutHandler(IApplicationDbContext db, ITokenService tokens)
    {
        _db = db;
        _tokens = tokens;
    }

    public async System.Threading.Tasks.Task<Unit> Handle(LogoutCommand request, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(request.RefreshToken)) return Unit.Value;

        var hash = _tokens.HashRefreshToken(request.RefreshToken);
        var token = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);
        if (token is not null && token.RevokedAt is null)
        {
            token.RevokedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
        return Unit.Value;
    }
}
