using KHCT.Api.Common;
using KHCT.Application.Admin.Positions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KHCT.Api.Controllers;

[Authorize(Policy = "RequireAdmin")]
[Route("api/admin/positions")]
[Tags("Admin")]
public sealed class AdminPositionsController : BaseApiController
{
    public AdminPositionsController(ISender sender) : base(sender)
    {
    }

    public record CreatePositionRequest(string Code, string Name, int SortOrder = 99);
    public record UpdatePositionRequest(string Name, bool IsActive, int SortOrder = 99);

    [HttpGet]
    public async Task<IActionResult> GetPositions(CancellationToken ct)
    {
        var result = await Sender.Send(new GetPositionsQuery(), ct);
        return Ok(new ApiEnvelope<object>(result));
    }

    [HttpPost]
    public async Task<IActionResult> CreatePosition([FromBody] CreatePositionRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new CreatePositionCommand(request.Code, request.Name, request.SortOrder), ct);
        return Ok(new ApiEnvelope<object>(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdatePosition(Guid id, [FromBody] UpdatePositionRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new UpdatePositionCommand(id, request.Name, request.IsActive, request.SortOrder), ct);
        return Ok(new ApiEnvelope<object>(result));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePosition(Guid id, CancellationToken ct)
    {
        await Sender.Send(new DeletePositionCommand(id), ct);
        return Ok(new ApiEnvelope<object>(new { success = true }));
    }
}
