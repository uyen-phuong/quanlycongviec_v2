using FluentValidation;
using KHCT.Application.Common.Interfaces;
using MediatR;

namespace KHCT.Application.Attachments;

public record DeleteAttachmentCommand(Guid AttachmentId) : IRequest<bool>;

public class DeleteAttachmentCommandValidator : AbstractValidator<DeleteAttachmentCommand>
{
    public DeleteAttachmentCommandValidator()
    {
        RuleFor(x => x.AttachmentId).NotEmpty();
    }
}

public class DeleteAttachmentHandler : IRequestHandler<DeleteAttachmentCommand, bool>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IAttachmentStorage _storage;

    public DeleteAttachmentHandler(IApplicationDbContext db, ICurrentUser currentUser, IAttachmentStorage storage)
    {
        _db = db;
        _currentUser = currentUser;
        _storage = storage;
    }

    public async Task<bool> Handle(DeleteAttachmentCommand request, CancellationToken ct)
    {
        var attachment = await AttachmentSupport.LoadMutableAttachmentAsync(_db, request.AttachmentId, _currentUser, ct);
        var storedPath = attachment.StoredPath;

        _db.Attachments.Remove(attachment);
        await _db.SaveChangesAsync(ct);
        await _storage.DeleteIfExistsAsync(storedPath, ct);

        return true;
    }
}
