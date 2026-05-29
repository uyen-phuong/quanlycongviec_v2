using KHCT.Api.Common;
using KHCT.Application.Plans.Workflow;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KHCT.Api.Controllers;

[Authorize]
[Route("api/line-comments")]
[Tags("Comments")]
public sealed class LineCommentsController : BaseApiController
{
    public LineCommentsController(ISender sender) : base(sender)
    {
    }

    [HttpPost("{id:guid}/resolve")]
    public async Task<IActionResult> Resolve(Guid id, CancellationToken ct)
    {
        var result = await Sender.Send(new ResolveLineCommentCommand(id), ct);
        return Ok(new ApiEnvelope<object>(result));
    }
}
