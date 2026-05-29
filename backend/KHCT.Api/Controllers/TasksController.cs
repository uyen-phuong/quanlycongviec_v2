using KHCT.Api.Common;
using KHCT.Application.Plans.Workflow;
using KHCT.Application.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KHCT.Api.Controllers;

[Authorize]
[Route("api")]
[Tags("Tasks")]
public sealed class TasksController : BaseApiController
{
    public TasksController(ISender sender) : base(sender)
    {
    }

    public record SaveTaskRequest(
        Guid? ParentTaskId,
        string? OutlineIndex,
        int DisplayOrder,
        bool IsHeader,
        string Title,
        int WorkType,
        string WorkStatus,
        DateTime? Deadline,
        Guid? AssigneeUserId,
        Guid? OwnerDepartmentId,
        string? BksMemberText,
        string? KtnbLeaderText,
        string? NoteText,
        string? ProgressText,
        string? ReasonNotCompleted,
        IReadOnlyList<Guid>? SupportingDepartmentIds);

    public record CreateTaskRequest(
        Guid PlanId,
        Guid? ParentTaskId,
        string? OutlineIndex,
        int DisplayOrder,
        bool IsHeader,
        string Title,
        int WorkType,
        string WorkStatus,
        DateTime? Deadline,
        Guid? AssigneeUserId,
        Guid? OwnerDepartmentId,
        string? BksMemberText,
        string? KtnbLeaderText,
        string? NoteText,
        string? ProgressText,
        string? ReasonNotCompleted,
        IReadOnlyList<Guid>? SupportingDepartmentIds);

    [HttpGet("plans/{planId:guid}/tasks")]
    public async Task<IActionResult> GetTasksByPlan(Guid planId, [FromQuery] int? workType = null, [FromQuery] string? departmentCode = null, CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetTasksByPlanQuery(planId, workType, departmentCode), ct);
        return Ok(new ApiEnvelope<object>(result));
    }

    [HttpGet("tasks/{id:guid}")]
    public async Task<IActionResult> GetTask(Guid id, CancellationToken ct)
    {
        var result = await Sender.Send(new GetTaskByIdQuery(id), ct);
        return Ok(new ApiEnvelope<object>(result));
    }

    [HttpPost("tasks")]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new CreateTaskCommand(
            request.PlanId,
            request.ParentTaskId,
            request.OutlineIndex,
            request.DisplayOrder,
            request.IsHeader,
            request.Title,
            request.WorkType,
            request.WorkStatus,
            request.Deadline,
            request.AssigneeUserId,
            request.OwnerDepartmentId,
            request.BksMemberText,
            request.KtnbLeaderText,
            request.NoteText,
            request.ProgressText,
            request.ReasonNotCompleted,
            request.SupportingDepartmentIds ?? Array.Empty<Guid>()), ct);

        return Ok(new ApiEnvelope<object>(result));
    }

    [HttpPut("tasks/{id:guid}")]
    public async Task<IActionResult> UpdateTask(Guid id, [FromBody] SaveTaskRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new UpdateTaskCommand(
            id,
            request.ParentTaskId,
            request.OutlineIndex,
            request.DisplayOrder,
            request.IsHeader,
            request.Title,
            request.WorkType,
            request.WorkStatus,
            request.Deadline,
            request.AssigneeUserId,
            request.OwnerDepartmentId,
            request.BksMemberText,
            request.KtnbLeaderText,
            request.NoteText,
            request.ProgressText,
            request.ReasonNotCompleted,
            request.SupportingDepartmentIds ?? Array.Empty<Guid>()), ct);

        return Ok(new ApiEnvelope<object>(result));
    }

    [HttpDelete("tasks/{id:guid}")]
    public async Task<IActionResult> DeleteTask(Guid id, CancellationToken ct)
    {
        var result = await Sender.Send(new DeleteTaskCommand(id), ct);
        return Ok(new ApiEnvelope<bool>(result));
    }

    public record AddLineCommentRequest(string Content);

    [HttpPost("tasks/{taskId:guid}/line-comments")]
    public async Task<IActionResult> AddLineComment(Guid taskId, [FromBody] AddLineCommentRequest body, CancellationToken ct)
    {
        var result = await Sender.Send(new CreateLineCommentCommand(taskId, body.Content), ct);
        return Ok(new ApiEnvelope<object>(result));
    }
}
