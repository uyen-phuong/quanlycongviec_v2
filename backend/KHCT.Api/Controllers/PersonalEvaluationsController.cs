using KHCT.Api.Common;
using KHCT.Application.PersonalEvaluations;
using KHCT.Application.PersonalEvaluations.Commands;
using KHCT.Application.PersonalEvaluations.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KHCT.Api.Controllers;

[Authorize]
[Route("api/personal-evaluations")]
[Tags("PersonalEvaluations")]
public sealed class PersonalEvaluationsController : BaseApiController
{
    private readonly IPersonalEvaluationExportService _exportService;

    public PersonalEvaluationsController(ISender sender, IPersonalEvaluationExportService exportService) : base(sender)
    {
        _exportService = exportService;
    }

    public record SaveItemRequest(
        int DisplayOrder,
        string? AssignmentSource,
        string? TaskName,
        string? TaskDetail,
        string? ActualResult,
        string? Note,
        DateTime? Deadline,
        DateTime? CompletedAt,
        decimal? SelfProgressScore,
        decimal? SelfQualityScore,
        decimal? TeamLeadProgressScore,
        decimal? TeamLeadQualityScore,
        decimal? ManagerProgressScore,
        decimal? ManagerQualityScore,
        decimal? DeputyProgressScore,
        decimal? DeputyQualityScore,
        decimal? HeadProgressScore,
        decimal? HeadQualityScore);

    public record SavePeriodRequest(
        decimal? CapacityAttitudeSelfScore,
        decimal? CapacityAttitudeTeamLeadScore,
        decimal? CapacityAttitudeManagerScore,
        decimal? CapacityAttitudeDeputyScore,
        decimal? CapacityAttitudeHeadScore,
        decimal? DisciplineSelfScore,
        decimal? DisciplineTeamLeadScore,
        decimal? DisciplineManagerScore,
        decimal? DisciplineDeputyScore,
        decimal? DisciplineHeadScore,
        decimal? InspectionSelfScore,
        decimal? InspectionTeamLeadScore,
        decimal? InspectionManagerScore,
        decimal? InspectionDeputyScore,
        decimal? InspectionHeadScore);

    public record CreateItemRequest(Guid PeriodId);

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int year, [FromQuery] int month, [FromQuery] Guid? userId = null, CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetPersonalEvaluationQuery(year, month, userId), ct);
        return Ok(new ApiEnvelope<object>(result));
    }

    [HttpGet("scorable-users")]
    public async Task<IActionResult> GetScorableUsers(CancellationToken ct)
    {
        var result = await Sender.Send(new GetScorableUsersQuery(), ct);
        return Ok(new ApiEnvelope<object>(result));
    }

    [HttpPost("items")]
    public async Task<IActionResult> CreateItem([FromBody] CreateItemRequest body, CancellationToken ct)
    {
        var result = await Sender.Send(new CreatePersonalEvaluationItemCommand(body.PeriodId), ct);
        return Ok(new ApiEnvelope<object>(result));
    }

    [HttpPut("items/{id:guid}")]
    public async Task<IActionResult> UpdateItem(Guid id, [FromBody] SaveItemRequest body, CancellationToken ct)
    {
        var result = await Sender.Send(new UpdatePersonalEvaluationItemCommand(
            id,
            body.DisplayOrder,
            body.AssignmentSource, body.TaskName, body.TaskDetail, body.ActualResult, body.Note,
            body.Deadline, body.CompletedAt,
            body.SelfProgressScore, body.SelfQualityScore,
            body.TeamLeadProgressScore, body.TeamLeadQualityScore,
            body.ManagerProgressScore, body.ManagerQualityScore,
            body.DeputyProgressScore, body.DeputyQualityScore,
            body.HeadProgressScore, body.HeadQualityScore), ct);
        return Ok(new ApiEnvelope<object>(result));
    }

    [HttpDelete("items/{id:guid}")]
    public async Task<IActionResult> DeleteItem(Guid id, CancellationToken ct)
    {
        var result = await Sender.Send(new DeletePersonalEvaluationItemCommand(id), ct);
        return Ok(new ApiEnvelope<bool>(result));
    }

    [HttpGet("{periodId:guid}/export/phu-luc-01")]
    public async Task<IActionResult> ExportPhuLuc01(Guid periodId, CancellationToken ct)
    {
        var result = await _exportService.ExportPhuLuc01Async(periodId, ct);
        return File(result.Content, result.ContentType, result.FileName);
    }

    [HttpGet("{periodId:guid}/export/phu-luc-01a")]
    public async Task<IActionResult> ExportPhuLuc01A(Guid periodId, CancellationToken ct)
    {
        var result = await _exportService.ExportPhuLuc01AAsync(periodId, ct);
        return File(result.Content, result.ContentType, result.FileName);
    }

    [HttpPut("period/{id:guid}")]
    public async Task<IActionResult> UpdatePeriod(Guid id, [FromBody] SavePeriodRequest body, CancellationToken ct)
    {
        var result = await Sender.Send(new UpdatePersonalEvaluationPeriodCommand(
            id,
            body.CapacityAttitudeSelfScore, body.CapacityAttitudeTeamLeadScore, body.CapacityAttitudeManagerScore, body.CapacityAttitudeDeputyScore, body.CapacityAttitudeHeadScore,
            body.DisciplineSelfScore, body.DisciplineTeamLeadScore, body.DisciplineManagerScore, body.DisciplineDeputyScore, body.DisciplineHeadScore,
            body.InspectionSelfScore, body.InspectionTeamLeadScore, body.InspectionManagerScore, body.InspectionDeputyScore, body.InspectionHeadScore), ct);
        return Ok(new ApiEnvelope<object>(result));
    }
}
