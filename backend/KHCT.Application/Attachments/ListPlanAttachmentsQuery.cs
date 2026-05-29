using KHCT.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Attachments;

public record ListPlanAttachmentsQuery(Guid PlanId) : IRequest<IReadOnlyList<AttachmentListItemDto>>;

public class ListPlanAttachmentsHandler : IRequestHandler<ListPlanAttachmentsQuery, IReadOnlyList<AttachmentListItemDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public ListPlanAttachmentsHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<AttachmentListItemDto>> Handle(ListPlanAttachmentsQuery request, CancellationToken ct)
    {
        await AttachmentSupport.LoadReadablePlanAsync(_db, request.PlanId, _currentUser, ct);

        var items = await _db.Attachments
            .AsNoTracking()
            .Include(x => x.UploadedByUser)
            .Where(x => x.OwnerType == AttachmentSupport.OwnerTypePlan && x.OwnerId == request.PlanId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

        return items.Select(AttachmentSupport.ToListItem).ToList();
    }
}
