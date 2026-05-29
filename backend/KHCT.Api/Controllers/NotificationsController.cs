using KHCT.Api.Common;
using KHCT.Application.Notifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KHCT.Api.Controllers;

[Authorize]
[Route("api/notifications")]
[Tags("Notifications")]
public sealed class NotificationsController : BaseApiController
{
    public NotificationsController(ISender sender) : base(sender) { }

    [HttpGet]
    public async Task<IActionResult> GetNotifications(CancellationToken ct)
    {
        var result = await Sender.Send(new GetNotificationsQuery(), ct);
        return Ok(new ApiEnvelope<GetNotificationsResult>(result));
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        await Sender.Send(new MarkNotificationsReadCommand(), ct);
        return Ok(new ApiEnvelope<object>(new { success = true }));
    }
}
