using KHCT.Application.Common.Interfaces;
using MediatR;

namespace KHCT.Application.Attachments;

public record DownloadAttachmentQuery(Guid AttachmentId) : IRequest<AttachmentDownloadDto>;

public class DownloadAttachmentHandler : IRequestHandler<DownloadAttachmentQuery, AttachmentDownloadDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IAttachmentStorage _storage;

    public DownloadAttachmentHandler(IApplicationDbContext db, ICurrentUser currentUser, IAttachmentStorage storage)
    {
        _db = db;
        _currentUser = currentUser;
        _storage = storage;
    }

    public async Task<AttachmentDownloadDto> Handle(DownloadAttachmentQuery request, CancellationToken ct)
    {
        var attachment = await AttachmentSupport.LoadReadableAttachmentAsync(_db, request.AttachmentId, _currentUser, ct);
        var content = await _storage.ReadAsync(attachment.StoredPath, ct);

        return new AttachmentDownloadDto(
            attachment.Id,
            attachment.FileName,
            string.IsNullOrWhiteSpace(attachment.ContentType) ? "application/octet-stream" : attachment.ContentType,
            content);
    }
}
