using KHCT.Api.Common;
using KHCT.Application.Plans.Workflow;
using KHCT.Application.Tasks.Workflow;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KHCT.Api.Controllers;

[Authorize]
[Route("api/plans")]
[Tags("Workflow")]
public sealed class PlanWorkflowController : BaseApiController
{
    public PlanWorkflowController(ISender sender) : base(sender)
    {
    }

    public record WorkflowActionRequest(string? Comment);
    public record TaskWorkflowActionRequest(string? DepartmentCode, string? Comment);

    public record ReturnLineCommentRequest(Guid TaskId, string Content);

    public record ReturnPlanRequest(string? Comment, IReadOnlyList<ReturnLineCommentRequest>? LineComments);
    public record ReturnTaskWorkflowRequest(string? DepartmentCode, string? Comment, IReadOnlyList<ReturnLineCommentRequest>? LineComments);

    [HttpPost("{id:guid}/submit")]
    [EndpointSummary("Nộp kế hoạch để duyệt (draft → pending). Main: VAN_THU; Sub: TRUONG_PHONG/PHO_TRUONG_KTNB")]
    public async Task<IActionResult> Submit(Guid id, [FromBody] WorkflowActionRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new SubmitPlanCommand(id, request.Comment), ct);
        return Ok(new ApiEnvelope<object>(result));
    }

    [HttpPost("{id:guid}/approve")]
    [EndpointSummary("Duyệt kế hoạch lên bước tiếp theo. Bước duyệt phụ thuộc role người dùng và loại kế hoạch main/sub")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] WorkflowActionRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new ApprovePlanCommand(id, request.Comment), ct);
        return Ok(new ApiEnvelope<object>(result));
    }

    [HttpPost("{id:guid}/return")]
    [EndpointSummary("Trả kế hoạch về (→ returned). Bắt buộc kèm line comment chỉ rõ task cần sửa")]
    public async Task<IActionResult> Return(Guid id, [FromBody] ReturnPlanRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new ReturnPlanCommand(
            id,
            request.Comment,
            (request.LineComments ?? Array.Empty<ReturnLineCommentRequest>())
                .Select(x => new ReturnTaskCommentItem(x.TaskId, x.Content))
                .ToList()), ct);

        return Ok(new ApiEnvelope<object>(result));
    }

    [HttpGet("{id:guid}/approval-history")]
    public async Task<IActionResult> GetApprovalHistory(Guid id, CancellationToken ct)
    {
        var result = await Sender.Send(new GetApprovalHistoryQuery(id), ct);
        return Ok(new ApiEnvelope<object>(result));
    }

    [HttpGet("{id:guid}/line-comments")]
    public async Task<IActionResult> GetLineComments(Guid id, CancellationToken ct)
    {
        var result = await Sender.Send(new GetPlanLineCommentsQuery(id), ct);
        return Ok(new ApiEnvelope<object>(result));
    }

    [HttpPost("{id:guid}/task-workflow/submit")]
    public async Task<IActionResult> SubmitTaskWorkflow(Guid id, [FromBody] TaskWorkflowActionRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new SubmitTaskWorkflowCommand(id, request.DepartmentCode, request.Comment), ct);
        return Ok(new ApiEnvelope<object>(new { status = result }));
    }

    [HttpPost("{id:guid}/task-workflow/approve")]
    public async Task<IActionResult> ApproveTaskWorkflow(Guid id, [FromBody] TaskWorkflowActionRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new ApproveTaskWorkflowCommand(id, request.DepartmentCode, request.Comment), ct);
        return Ok(new ApiEnvelope<object>(new { status = result }));
    }

    [HttpPost("{id:guid}/task-workflow/return")]
    public async Task<IActionResult> ReturnTaskWorkflow(Guid id, [FromBody] ReturnTaskWorkflowRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new ReturnTaskWorkflowCommand(
            id,
            request.DepartmentCode,
            request.Comment,
            (request.LineComments ?? Array.Empty<ReturnLineCommentRequest>())
                .Select(x => new ReturnTaskWorkflowCommentItem(x.TaskId, x.Content))
                .ToList()), ct);

        return Ok(new ApiEnvelope<object>(new { status = result }));
    }

    [HttpGet("{id:guid}/task-workflow/history")]
    public async Task<IActionResult> GetTaskWorkflowHistory(Guid id, [FromQuery] string? departmentCode, CancellationToken ct)
    {
        var result = await Sender.Send(new GetTaskWorkflowHistoryQuery(id, departmentCode), ct);
        return Ok(new ApiEnvelope<object>(result));
    }
}
