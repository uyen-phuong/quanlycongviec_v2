using System.Net.Mime;
using KHCT.Api.Common;
using KHCT.Application.Attachments;
using KHCT.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace KHCT.Api.Controllers;

[Authorize]
[Route("api")]
[Tags("Attachments")]
public sealed class AttachmentsController : BaseApiController
{
    private const long MaxUploadSizeBytes = 50L * 1024 * 1024;

    public AttachmentsController(ISender sender) : base(sender)
    {
    }

    [HttpGet("plans/{id:guid}/attachments")]
    public async Task<IActionResult> GetPlanAttachments(Guid id, CancellationToken ct)
    {
        var result = await Sender.Send(new ListPlanAttachmentsQuery(id), ct);
        return Ok(new ApiEnvelope<object>(result));
    }

    [HttpPost("plans/{id:guid}/attachments")]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxUploadSizeBytes)]
    [RequestSizeLimit(MaxUploadSizeBytes)]
    public async Task<IActionResult> UploadPlanAttachment(Guid id, [FromForm] IFormFile? file, CancellationToken ct)
    {
        var payload = await ReadUploadAsync(file, ct);
        var result = await Sender.Send(new UploadPlanAttachmentCommand(id, payload.FileName, payload.ContentType, payload.Content), ct);
        return Ok(new ApiEnvelope<object>(result));
    }

    [HttpGet("tasks/{id:guid}/attachments")]
    public async Task<IActionResult> GetTaskAttachments(Guid id, CancellationToken ct)
    {
        var result = await Sender.Send(new ListTaskAttachmentsQuery(id), ct);
        return Ok(new ApiEnvelope<object>(result));
    }

    [HttpPost("tasks/{id:guid}/attachments")]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxUploadSizeBytes)]
    [RequestSizeLimit(MaxUploadSizeBytes)]
    public async Task<IActionResult> UploadTaskAttachment(Guid id, [FromForm] IFormFile? file, CancellationToken ct)
    {
        var payload = await ReadUploadAsync(file, ct);
        var result = await Sender.Send(new UploadTaskAttachmentCommand(id, payload.FileName, payload.ContentType, payload.Content), ct);
        return Ok(new ApiEnvelope<object>(result));
    }

    [HttpGet("attachments/{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken ct)
    {
        var result = await Sender.Send(new DownloadAttachmentQuery(id), ct);
        var disposition = new ContentDisposition
        {
            DispositionType = "attachment",
            FileName = SanitizeAsciiFileName(result.FileName)
        };

        Response.Headers[HeaderNames.ContentDisposition] =
            $"{disposition}; filename*=UTF-8''{Uri.EscapeDataString(result.FileName)}";

        return File(result.Content, result.ContentType);
    }

    [HttpDelete("attachments/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await Sender.Send(new DeleteAttachmentCommand(id), ct);
        return Ok(new ApiEnvelope<bool>(result));
    }

    private static async Task<(string FileName, string? ContentType, byte[] Content)> ReadUploadAsync(IFormFile? file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
        {
            throw new DomainException("file_required", "File is required.");
        }

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream, ct);
        return (file.FileName, file.ContentType, stream.ToArray());
    }

    private static string SanitizeAsciiFileName(string fileName)
    {
        var baseName = Path.GetFileName(fileName);
        var buffer = new char[baseName.Length];
        for (var i = 0; i < baseName.Length; i++)
        {
            var ch = baseName[i];
            buffer[i] = ch is >= (char)32 and <= (char)126 ? ch : '_';
        }

        return new string(buffer);
    }
}
