using KHCT.Api.Common;
using KHCT.Application.Projects;
using KHCT.Application.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KHCT.Api.Controllers;

[Authorize]
[Route("api/projects")]
[Tags("Projects")]
public sealed class ProjectsController : BaseApiController
{
    public ProjectsController(ISender sender) : base(sender)
    {
    }

    public record CreateProjectRequest(
        string Name,
        string Description,
        Guid LeaderId,
        Guid? SubLeaderId,
        IReadOnlyList<Guid> MemberUserIds);

    public record ReturnProjectRequest(string Comment);

    [HttpGet]
    public async Task<IActionResult> GetProjects(CancellationToken ct)
    {
        var result = await Sender.Send(new GetProjectsQuery(), ct);
        return Ok(new ApiEnvelope<object>(result));
    }

    [HttpPost]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new CreateProjectCommand(
            request.Name,
            request.Description,
            request.LeaderId,
            request.SubLeaderId,
            request.MemberUserIds), ct);

        return Ok(new ApiEnvelope<object>(result));
    }

    [HttpPost("{id:guid}/submit")]
    public async Task<IActionResult> SubmitProject(Guid id, CancellationToken ct)
    {
        var result = await Sender.Send(new SubmitProjectCommand(id), ct);
        return Ok(new ApiEnvelope<bool>(result));
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> ApproveProject(Guid id, CancellationToken ct)
    {
        var result = await Sender.Send(new ApproveProjectCommand(id), ct);
        return Ok(new ApiEnvelope<bool>(result));
    }

    [HttpPost("{id:guid}/return")]
    public async Task<IActionResult> ReturnProject(Guid id, [FromBody] ReturnProjectRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new ReturnProjectCommand(id, request.Comment), ct);
        return Ok(new ApiEnvelope<bool>(result));
    }

    public record CreateProjectTaskRequest(
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

    [HttpGet("{id:guid}/tasks")]
    public async Task<IActionResult> GetProjectTasks(Guid id, CancellationToken ct)
    {
        var result = await Sender.Send(new GetTasksByProjectQuery(id), ct);
        return Ok(new ApiEnvelope<object>(result));
    }

    [HttpPost("{id:guid}/tasks")]
    public async Task<IActionResult> CreateProjectTask(Guid id, [FromBody] CreateProjectTaskRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new CreateProjectTaskCommand(
            id,
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
}
