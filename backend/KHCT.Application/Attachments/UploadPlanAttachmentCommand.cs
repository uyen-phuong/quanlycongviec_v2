using FluentValidation;
using KHCT.Application.Plans;
using KHCT.Application.Common.Interfaces;
using KHCT.Domain.Entities;
using MediatR;

namespace KHCT.Application.Attachments;

public record UploadPlanAttachmentCommand(
    Guid PlanId,
    string FileName,
    string? ContentType,
    byte[] Content) : IRequest<AttachmentListItemDto>;

public class UploadPlanAttachmentCommandValidator : AbstractValidator<UploadPlanAttachmentCommand>
{
    public UploadPlanAttachmentCommandValidator()
    {
        RuleFor(x => x.PlanId).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(512);
        RuleFor(x => x.Content).NotNull().Must(x => x.Length > 0).WithMessage("File is required.");
    }
}

public class UploadPlanAttachmentHandler : IRequestHandler<UploadPlanAttachmentCommand, AttachmentListItemDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IAttachmentStorage _storage;

    public UploadPlanAttachmentHandler(IApplicationDbContext db, ICurrentUser currentUser, IAttachmentStorage storage)
    {
        _db = db;
        _currentUser = currentUser;
        _storage = storage;
    }

    public async Task<AttachmentListItemDto> Handle(UploadPlanAttachmentCommand request, CancellationToken ct)
    {
        await AttachmentSupport.LoadMutablePlanAsync(_db, request.PlanId, _currentUser, ct);
        return await SaveAttachmentAsync(AttachmentSupport.OwnerTypePlan, request.PlanId, request.FileName, request.ContentType, request.Content, ct);
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
