using KHCT.Api.Common;
using KHCT.Application.Admin.Roles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KHCT.Api.Controllers;

[Authorize(Policy = "RequireAdmin")]
[Route("api/admin/roles")]
[Tags("Admin")]
public sealed class AdminRolesController : BaseApiController
{
    public AdminRolesController(ISender sender) : base(sender)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetRoles(CancellationToken ct)
    {
        var result = await Sender.Send(new GetRolesQuery(), ct);
        return Ok(new ApiEnvelope<object>(result));
    }
}
