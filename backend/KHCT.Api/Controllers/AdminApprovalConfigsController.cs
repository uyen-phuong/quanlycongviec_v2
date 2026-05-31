using KHCT.Api.Common;
using KHCT.Application.Admin.ApprovalConfigs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KHCT.Api.Controllers;

[Authorize(Policy = "RequireAdmin")]
[Route("api/admin/approval-configs")]
[Tags("Admin")]
public sealed class AdminApprovalConfigsController : BaseApiController
{
    public AdminApprovalConfigsController(ISender sender) : base(sender)
    {
    }

    public record UpdateApprovalConfigRequest(Guid? DepartmentId, Guid RoleId, bool IsActive);

    [HttpGet]
    public async Task<IActionResult> GetApprovalConfigs(CancellationToken ct)
    {
        var result = await Sender.Send(new GetApprovalConfigsQuery(), ct);
        return Ok(new ApiEnvelope<object>(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateApprovalConfig(Guid id, [FromBody] UpdateApprovalConfigRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new UpdateApprovalConfigCommand(id, request.DepartmentId, request.RoleId, request.IsActive), ct);
        return Ok(new ApiEnvelope<object>(result));
    }
}
