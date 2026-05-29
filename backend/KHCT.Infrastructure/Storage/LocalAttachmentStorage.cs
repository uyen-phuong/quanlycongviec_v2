using KHCT.Application.Common.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace KHCT.Infrastructure.Storage;

public sealed class LocalAttachmentStorage : IAttachmentStorage
{
    private readonly string _rootPath;

    public LocalAttachmentStorage(IHostEnvironment environment, IOptions<AttachmentStorageOptions> options)
    {
        _rootPath = Path.Combine(environment.ContentRootPath, options.Value.RootPath);
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<AttachmentTempFile> WriteTempAsync(byte[] content, CancellationToken ct)
    {
        var tempDirectory = Path.Combine(_rootPath, "attachments", "_temp");
        Directory.CreateDirectory(tempDirectory);

        var tempPath = Path.Combine(tempDirectory, $"{Guid.NewGuid():N}.tmp");
        await File.WriteAllBytesAsync(tempPath, content, ct);
        return new AttachmentTempFile(tempPath);
    }

    public string CreateRelativePath(string extension, DateTime utcNow) =>
        $"attachments/{utcNow:yyyy}/{utcNow:MM}/{Guid.NewGuid():N}{extension.ToLowerInvariant()}";

    public Task PromoteAsync(string tempPath, string relativePath, CancellationToken ct)
    {
        var finalPath = ResolveAbsolutePath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(finalPath)!);
        if (File.Exists(finalPath))
        {
            File.Delete(finalPath);
        }

        File.Move(tempPath, finalPath);
        return Task.CompletedTask;
    }

    public async Task<byte[]> ReadAsync(string relativePath, CancellationToken ct)
    {
        var fullPath = ResolveAbsolutePath(relativePath);
        if (!File.Exists(fullPath))
        {
            throw new KeyNotFoundException("Attachment file not found.");
        }

        return await File.ReadAllBytesAsync(fullPath, ct);
    }

    public Task DeleteIfExistsAsync(string relativePath, CancellationToken ct)
    {
        var fullPath = ResolveAbsolutePath(relativePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private string ResolveAbsolutePath(string relativePath)
    {
        var normalized = relativePath.Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(_rootPath, normalized));
        if (!fullPath.StartsWith(_rootPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Attachment path escapes storage root.");
        }

        return fullPath;
    }
}
