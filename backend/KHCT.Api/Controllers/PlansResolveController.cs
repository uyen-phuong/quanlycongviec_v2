using KHCT.Api.Common;
using KHCT.Application.Plans;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KHCT.Api.Controllers;

[Authorize]
[Route("api/plans")]
[Tags("Plans")]
public sealed class PlansResolveController : BaseApiController
{
    public PlansResolveController(ISender sender) : base(sender)
    {
    }

    [HttpGet("resolve")]
    public async Task<IActionResult> Resolve(
        [FromQuery] string scope = "sub",
        [FromQuery] string? departmentCode = null,
        [FromQuery] int? year = null,
        [FromQuery] int? month = null,
        CancellationToken ct = default)
    {
        var result = await Sender.Send(new ResolvePlanQuery(scope, departmentCode, year, month), ct);
        return Ok(new ApiEnvelope<object>(result));
    }
}
