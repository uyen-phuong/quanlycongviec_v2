using KHCT.Api.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KHCT.Api.Controllers;

[Route("api/plans/sub")]
[Tags("Plans")]
public sealed class PlansSubController : BaseApiController
{
    public PlansSubController(ISender sender) : base(sender)
    {
    }

    [Authorize]
    [Route("")]
    [Route("{*path}")]
    public IActionResult Disabled()
    {
        return StatusCode(StatusCodes.Status410Gone, new ApiEnvelope<object>(new
        {
            code = "sub_plan_retired",
            message = "Sub plan routes have been retired. Use main plans and task workflow endpoints instead."
        }));
    }
}
