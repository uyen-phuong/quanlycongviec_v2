using KHCT.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Attachments;

public record ListTaskAttachmentsQuery(Guid TaskId) : IRequest<IReadOnlyList<AttachmentListItemDto>>;

public class ListTaskAttachmentsHandler : IRequestHandler<ListTaskAttachmentsQuery, IReadOnlyList<AttachmentListItemDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public ListTaskAttachmentsHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<AttachmentListItemDto>> Handle(ListTaskAttachmentsQuery request, CancellationToken ct)
    {
        await AttachmentSupport.LoadReadableTaskAsync(_db, request.TaskId, _currentUser, ct);

        var items = await _db.Attachments
            .AsNoTracking()
            .Include(x => x.UploadedByUser)
            .Where(x => x.OwnerType == AttachmentSupport.OwnerTypeTask && x.OwnerId == request.TaskId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

        return items.Select(AttachmentSupport.ToListItem).ToList();
    }
}
