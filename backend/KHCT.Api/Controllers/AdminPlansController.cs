using KHCT.Api.Common;
using KHCT.Application.Plans.Workflow;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KHCT.Api.Controllers;

[Authorize(Roles = "ADMIN")]
[Route("api/admin/plans")]
[Tags("Admin")]
public sealed class AdminPlansController : BaseApiController
{
    public AdminPlansController(ISender sender) : base(sender) { }

    /// <summary>Re-run inheritance for a main plan (approved_2). Creates sub plans for supporting departments that were missed.</summary>
    [HttpPost("{id:guid}/re-inherit")]
    public async Task<IActionResult> ReInherit(Guid id, CancellationToken ct)
    {
        var result = await Sender.Send(new ReInheritCommand(id), ct);
        return Ok(new ApiEnvelope<ReInheritResult>(result));
    }
}
