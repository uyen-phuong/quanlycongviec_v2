using FluentValidation;
using KHCT.Application.Plans;
using KHCT.Application.Common.Interfaces;
using KHCT.Domain.Entities;
using MediatR;

namespace KHCT.Application.Attachments;

public record UploadTaskAttachmentCommand(
    Guid TaskId,
    string FileName,
    string? ContentType,
    byte[] Content) : IRequest<AttachmentListItemDto>;

public class UploadTaskAttachmentCommandValidator : AbstractValidator<UploadTaskAttachmentCommand>
{
    public UploadTaskAttachmentCommandValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(512);
        RuleFor(x => x.Content).NotNull().Must(x => x.Length > 0).WithMessage("File is required.");
    }
}

public class UploadTaskAttachmentHandler : IRequestHandler<UploadTaskAttachmentCommand, AttachmentListItemDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IAttachmentStorage _storage;

    public UploadTaskAttachmentHandler(IApplicationDbContext db, ICurrentUser currentUser, IAttachmentStorage storage)
    {
        _db = db;
        _currentUser = currentUser;
        _storage = storage;
    }

    public async Task<AttachmentListItemDto> Handle(UploadTaskAttachmentCommand request, CancellationToken ct)
    {
        await AttachmentSupport.LoadMutableTaskAsync(_db, request.TaskId, _currentUser, ct);
        return await SaveAttachmentAsync(AttachmentSupport.OwnerTypeTask, request.TaskId, request.FileName, request.ContentType, request.Content, ct);
    }

    private async Task<AttachmentListItemDto> SaveAttachmentAsync(
        string ownerType,
        Guid ownerId,
        string fileName,
        string? contentType,
        byte[] content,
        CancellationToken ct)
    {
        var normalizedFileName = AttachmentSupport.NormalizeFileName(fileName);
        AttachmentSupport.ValidateFileSize(content.LongLength);
        var extension = AttachmentSupport.ValidateAndGetExtension(normalizedFileName);
        AttachmentSupport.ValidateSignature(extension, content);

        var temp = await _storage.WriteTempAsync(content, ct);
        var relativePath = _storage.CreateRelativePath(extension, DateTime.UtcNow);
        var attachment = new Attachment
        {
            Id = Guid.NewGuid(),
            OwnerType = ownerType,
            OwnerId = ownerId,
            FileName = normalizedFileName,
            StoredPath = relativePath,
            SizeBytes = content.LongLength,
            ContentType = AttachmentSupport.ResolveContentType(extension, contentType),
            UploadedByUserId = PlanSupport.RequireActorId(_currentUser)
        };

        try
        {
            _db.Attachments.Add(attachment);
            await _storage.PromoteAsync(temp.TempPath, relativePath, ct);
            await _db.SaveChangesAsync(ct);
        }
        catch
        {
            await _storage.DeleteIfExistsAsync(relativePath, ct);
            throw;
        }

        return new AttachmentListItemDto(
            attachment.Id,
            attachment.OwnerType,
            attachment.OwnerId,
            attachment.FileName,
            attachment.SizeBytes,
            attachment.ContentType,
            attachment.UploadedByUserId,
            _currentUser.Username,
            attachment.CreatedAt);
    }
}
