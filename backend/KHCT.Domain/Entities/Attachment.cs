using KHCT.Domain.Common;

namespace KHCT.Domain.Entities;

public class Attachment : Entity
{
    public string OwnerType { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoredPath { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string? ContentType { get; set; }
    public Guid UploadedByUserId { get; set; }
    public User? UploadedByUser { get; set; }
}
