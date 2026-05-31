using KHCT.Api.Common;
using KHCT.Application.Plans.Workflow;
using KHCT.Application.Tasks;
using KHCT.Application.Tasks.Workflow;
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
        Guid? ControllerUserId,
        Guid? OwnerDepartmentId,
        string? BksMemberText,
        string? KtnbLeaderText,
        string? NoteText,
        string? ProgressText,
        string? ReasonNotCompleted,
        string? Priority,
        string? Complexity,
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
        Guid? ControllerUserId,
        Guid? OwnerDepartmentId,
        string? BksMemberText,
        string? KtnbLeaderText,
        string? NoteText,
        string? ProgressText,
        string? ReasonNotCompleted,
        string? Priority,
        string? Complexity,
        IReadOnlyList<Guid>? SupportingDepartmentIds);

    [HttpGet("plans/{planId:guid}/tasks")]
    public async Task<IActionResult> GetTasksByPlan(Guid planId, [FromQuery] int? workType = null, [FromQuery] string? departmentCode = null, CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetTasksByPlanQuery(planId, workType, departmentCode), ct);
        return Ok(new ApiEnvelope<object>(result));
    }

    [HttpGet("departments/tasks")]
    public async Task<IActionResult> GetDepartmentTasks([FromQuery] string? departmentCode = null, CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetDepartmentTasksQuery(departmentCode), ct);
        return Ok(new ApiEnvelope<object>(result));
    }

    public record CreateDepartmentTaskRequest(
        Guid? ParentTaskId,
        string? OutlineIndex,
        int DisplayOrder,
        bool IsHeader,
        string Title,
        DateTime? Deadline,
        Guid? AssigneeUserId,
        Guid? ControllerUserId,
        string? NoteText,
        string? Priority,
        string? Complexity,
        IReadOnlyList<Guid>? SupportingDepartmentIds);

    [HttpPost("departments/tasks")]
    public async Task<IActionResult> CreateDepartmentTask([FromBody] CreateDepartmentTaskRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new CreateDepartmentTaskCommand(
            request.ParentTaskId,
            request.OutlineIndex,
            request.DisplayOrder,
            request.IsHeader,
            request.Title,
            request.Deadline,
            request.AssigneeUserId,
            request.ControllerUserId,
            request.NoteText,
            request.Priority,
            request.Complexity,
            request.SupportingDepartmentIds ?? Array.Empty<Guid>()), ct);

        return Ok(new ApiEnvelope<object>(result));
    }

    [HttpGet("personal/tasks")]
    public async Task<IActionResult> GetPersonalTasks(CancellationToken ct)
    {
        var result = await Sender.Send(new GetPersonalTasksQuery(), ct);
        return Ok(new ApiEnvelope<object>(result));
    }

    public record CreatePersonalTaskRequest(
        string Title,
        DateTime? Deadline,
        string? NoteText,
        string? Priority,
        string? Complexity,
        int DisplayOrder);

    [HttpPost("personal/tasks")]
    public async Task<IActionResult> CreatePersonalTask([FromBody] CreatePersonalTaskRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new CreatePersonalTaskCommand(
            request.Title,
            request.Deadline,
            request.NoteText,
            request.Priority,
            request.Complexity,
            request.DisplayOrder), ct);

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
            request.Title.Trim(),
            request.WorkType,
            request.WorkStatus,
            request.Deadline,
            request.AssigneeUserId,
            request.ControllerUserId,
            request.OwnerDepartmentId,
            request.BksMemberText,
            request.KtnbLeaderText,
            request.NoteText,
            request.ProgressText,
            request.ReasonNotCompleted,
            request.Priority,
            request.Complexity,
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
            request.ControllerUserId,
            request.OwnerDepartmentId,
            request.BksMemberText,
            request.KtnbLeaderText,
            request.NoteText,
            request.ProgressText,
            request.ReasonNotCompleted,
            request.Priority,
            request.Complexity,
            request.SupportingDepartmentIds ?? Array.Empty<Guid>()), ct);

        return Ok(new ApiEnvelope<object>(result));
    }

    [HttpDelete("tasks/{id:guid}")]
    public async Task<IActionResult> DeleteTask(Guid id, CancellationToken ct)
    {
        var result = await Sender.Send(new DeleteTaskCommand(id), ct);
        return Ok(new ApiEnvelope<bool>(result));
    }

    public record SingleTaskWorkflowRequest(string? Comment);
    public record AssignTaskRequest(Guid AssigneeUserId, Guid? ControllerUserId);

    [HttpPost("tasks/{id:guid}/submit")]
    public async Task<IActionResult> SubmitTask(Guid id, [FromBody] SingleTaskWorkflowRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new SubmitSingleTaskCommand(id, request.Comment), ct);
        return Ok(new ApiEnvelope<object>(result));
    }

    [HttpPost("tasks/{id:guid}/assign")]
    public async Task<IActionResult> AssignTask(Guid id, [FromBody] AssignTaskRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new AssignSingleTaskCommand(id, request.AssigneeUserId, request.ControllerUserId), ct);
        return Ok(new ApiEnvelope<object>(result));
    }

    [HttpPost("tasks/{id:guid}/approve")]
    public async Task<IActionResult> ApproveTask(Guid id, [FromBody] SingleTaskWorkflowRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new ApproveSingleTaskCommand(id, request.Comment), ct);
        return Ok(new ApiEnvelope<object>(result));
    }

    [HttpPost("tasks/{id:guid}/return")]
    public async Task<IActionResult> ReturnTask(Guid id, [FromBody] SingleTaskWorkflowRequest request, CancellationToken ct)
    {
        var comment = string.IsNullOrWhiteSpace(request.Comment) ? "Yêu cầu chỉnh sửa công việc" : request.Comment;
        var result = await Sender.Send(new ReturnSingleTaskCommand(id, comment), ct);
        return Ok(new ApiEnvelope<object>(result));
    }

    public record AddLineCommentRequest(string Content);

    [HttpPost("tasks/{taskId:guid}/line-comments")]
    public async Task<IActionResult> AddLineComment(Guid taskId, [FromBody] AddLineCommentRequest body, CancellationToken ct)
    {
        var result = await Sender.Send(new CreateLineCommentCommand(taskId, body.Content), ct);
        return Ok(new ApiEnvelope<object>(result));
    }
}
