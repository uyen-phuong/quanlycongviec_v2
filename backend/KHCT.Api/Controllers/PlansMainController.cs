using KHCT.Api.Common;
using KHCT.Application.Plans.Export;
using KHCT.Application.Plans.Import;
using KHCT.Application.Plans.Main;
using KHCT.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KHCT.Api.Controllers;

[Route("api/plans/main")]
[Tags("Plans")]
public sealed class PlansMainController : BaseApiController
{
    public PlansMainController(ISender sender) : base(sender)
    {
    }

    public record CreateMainPlanRequest(int Year, int Month);

    public record UpdateMainPlanRequest(int Year, int Month);

    public record ImportMainPlanExcelRequest(IFormFile? File);

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetPlans(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? year = null,
        [FromQuery] int? month = null,
        [FromQuery] string? status = null,
        CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetMainPlansQuery(
            page,
            pageSize,
            year,
            month,
            ParseStatus(status)), ct);

        return Ok(new ApiEnvelope<object>(result.Items, new
        {
            page = result.Page,
            pageSize = result.PageSize,
            total = result.Total
        }));
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPlan(Guid id, CancellationToken ct)
    {
        var result = await Sender.Send(new GetMainPlanByIdQuery(id), ct);
        return Ok(new ApiEnvelope<object>(result));
    }

    [Authorize]
    [HttpGet("{id:guid}/export-excel")]
    [EndpointSummary("Xuất kế hoạch main ra file Excel (workbook 2 sheet: Báo cáo + Chi tiết)")]
    public async Task<IActionResult> ExportExcel(Guid id, CancellationToken ct)
    {
        var result = await Sender.Send(new ExportMainPlanExcelQuery(id), ct);
        return File(result.Content, result.ContentType, result.FileName);
    }

    [Authorize(Roles = "VAN_THU,ADMIN")]
    [HttpPost]
    public async Task<IActionResult> CreatePlan([FromBody] CreateMainPlanRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new CreateMainPlanCommand(request.Year, request.Month), ct);
        return Ok(new ApiEnvelope<object>(result));
    }

    [Authorize(Roles = "VAN_THU,ADMIN")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdatePlan(Guid id, [FromBody] UpdateMainPlanRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new UpdateMainPlanCommand(id, request.Year, request.Month), ct);
        return Ok(new ApiEnvelope<object>(result));
    }

    [Authorize(Roles = "VAN_THU,ADMIN")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePlan(Guid id, CancellationToken ct)
    {
        var result = await Sender.Send(new DeleteMainPlanCommand(id), ct);
        return Ok(new ApiEnvelope<bool>(result));
    }

    [Authorize(Roles = "VAN_THU,ADMIN")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    [HttpPost("{id:guid}/import-excel")]
    [EndpointSummary("Import task từ file Excel Phụ lục 03 vào kế hoạch main (thay thế toàn bộ task hiện có, tối đa 10MB)")]
    public async Task<IActionResult> ImportExcel(Guid id, [FromForm] ImportMainPlanExcelRequest request, CancellationToken ct)
    {
        if (request.File is null || request.File.Length == 0)
        {
            throw new FluentValidation.ValidationException("File is required.");
        }

        if (request.File.Length > 10 * 1024 * 1024)
        {
            throw new FluentValidation.ValidationException("File must be 10MB or smaller.");
        }

        if (!request.File.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            throw new FluentValidation.ValidationException("Only .xlsx files are supported.");
        }

        await using var stream = new MemoryStream();
        await request.File.CopyToAsync(stream, ct);
        var result = await Sender.Send(new ImportMainPlanExcelCommand(id, request.File.FileName, stream.ToArray()), ct);
        return Ok(new ApiEnvelope<object>(result));
    }

    private static ApprovalStatus? ParseStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        return status.Trim().ToLowerInvariant() switch
        {
            "draft" => ApprovalStatus.Draft,
            "pending" => ApprovalStatus.Pending,
            "approved_1" => ApprovalStatus.Approved1,
            "approved_2" => ApprovalStatus.Approved2,
            "approved_3" => ApprovalStatus.Approved3,
            "returned" => ApprovalStatus.Returned,
            _ => throw new FluentValidation.ValidationException("Invalid status.")
        };
    }
}
