using KHCT.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Notifications;

public record MarkNotificationsReadCommand : IRequest<bool>;

public class MarkNotificationsReadHandler : IRequestHandler<MarkNotificationsReadCommand, bool>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public MarkNotificationsReadHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<bool> Handle(MarkNotificationsReadCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue) return false;

        var userId = _currentUser.UserId.Value;
        var unread = await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(ct);
        foreach (var n in unread) n.IsRead = true;
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
