namespace KHCT.Application.Attachments;

public record AttachmentListItemDto(
    Guid Id,
    string OwnerType,
    Guid OwnerId,
    string FileName,
    long SizeBytes,
    string? ContentType,
    Guid UploadedByUserId,
    string? UploadedByName,
    DateTime CreatedAt);

public record AttachmentDownloadDto(
    Guid Id,
    string FileName,
    string ContentType,
    byte[] Content);
