using KHCT.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Notifications;

public record GetNotificationsQuery : IRequest<GetNotificationsResult>;

public record NotificationDto(
    Guid Id,
    string Title,
    string? Body,
    string EventType,
    Guid? PlanId,
    Guid? TaskId,
    bool IsRead,
    DateTime CreatedAt);

public record GetNotificationsResult(
    IReadOnlyList<NotificationDto> Items,
    int UnreadCount);

public class GetNotificationsHandler : IRequestHandler<GetNotificationsQuery, GetNotificationsResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetNotificationsHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<GetNotificationsResult> Handle(GetNotificationsQuery request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return new GetNotificationsResult([], 0);

        var userId = _currentUser.UserId.Value;

        var items = await _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(30)
            .Select(n => new NotificationDto(
                n.Id, n.Title, n.Body, n.EventType,
                n.PlanId, n.TaskId, n.IsRead, n.CreatedAt))
            .ToListAsync(ct);

        var unreadCount = await _db.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead, ct);

        return new GetNotificationsResult(items, unreadCount);
    }
}
