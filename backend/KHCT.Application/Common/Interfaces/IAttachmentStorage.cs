namespace KHCT.Application.Common.Interfaces;

public interface IAttachmentStorage
{
    Task<AttachmentTempFile> WriteTempAsync(byte[] content, CancellationToken ct);
    string CreateRelativePath(string extension, DateTime utcNow);
    Task PromoteAsync(string tempPath, string relativePath, CancellationToken ct);
    Task<byte[]> ReadAsync(string relativePath, CancellationToken ct);
    Task DeleteIfExistsAsync(string relativePath, CancellationToken ct);
}

public sealed record AttachmentTempFile(string TempPath);
