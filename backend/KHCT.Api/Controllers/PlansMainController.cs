using KHCT.Api.Common;
using KHCT.Application.Plans.Export;
using KHCT.Application.Plans.Import;
using KHCT.Application.Plans.Main;
using KHCT.Application.Plans.ReportingPeriods;
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

    public record CreateMainPlanRequest(string Name, int Year, int Month, string ReportingPeriodType, Guid? KtnbLeaderId);

    public record UpdateMainPlanRequest(string Name, int Year, int Month, string ReportingPeriodType, Guid? KtnbLeaderId);

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
        var result = await Sender.Send(new CreateMainPlanCommand(request.Name, request.Year, request.Month, request.ReportingPeriodType, request.KtnbLeaderId), ct);
        return Ok(new ApiEnvelope<object>(result));
    }

    [Authorize(Roles = "VAN_THU,ADMIN")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdatePlan(Guid id, [FromBody] UpdateMainPlanRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new UpdateMainPlanCommand(id, request.Name, request.Year, request.Month, request.ReportingPeriodType, request.KtnbLeaderId), ct);
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

    // ── Reporting Periods ──────────────────────────────────────────────────────

    [Authorize]
    [HttpGet("{id:guid}/reporting-periods")]
    [EndpointSummary("Lấy danh sách kỳ báo cáo của kế hoạch")]
    public async Task<IActionResult> GetReportingPeriods(Guid id, CancellationToken ct)
    {
        var result = await Sender.Send(new GetReportingPeriodsQuery(id), ct);
        return Ok(new ApiEnvelope<object>(result));
    }

    [Authorize]
    [HttpPut("{id:guid}/reporting-periods/current")]
    [EndpointSummary("Cập nhật tiến độ kỳ báo cáo hiện tại (Cán bộ đầu mối nhập)")]
    public async Task<IActionResult> UpdateReportingPeriodProgress(
        Guid id,
        [FromBody] UpdateReportingPeriodProgressRequest request,
        CancellationToken ct)
    {
        var result = await Sender.Send(new UpdateReportingPeriodProgressCommand(id, request.ProgressText, request.CompletionPercent), ct);
        return Ok(new ApiEnvelope<object>(result));
    }

    [Authorize]
    [HttpPost("{id:guid}/reporting-periods/approve")]
    [EndpointSummary("Lãnh đạo KTNB duyệt và đóng kỳ báo cáo hiện tại")]
    public async Task<IActionResult> ApproveReportingPeriod(Guid id, CancellationToken ct)
    {
        var result = await Sender.Send(new ApproveReportingPeriodCommand(id), ct);
        return Ok(new ApiEnvelope<object>(result));
    }

    public record UpdateReportingPeriodProgressRequest(string? ProgressText, int CompletionPercent);

    private static WorkflowStatus? ParseStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        return status.Trim().ToLowerInvariant() switch
        {
            "draft" => WorkflowStatus.Draft,
            "pending" => WorkflowStatus.Pending,
            "approved_1" => WorkflowStatus.Approved1,
            "approved_2" => WorkflowStatus.Approved2,
            "approved_3" => WorkflowStatus.Approved3,
            "returned" => WorkflowStatus.Returned,
            _ => throw new FluentValidation.ValidationException("Invalid status.")
        };
    }
}
