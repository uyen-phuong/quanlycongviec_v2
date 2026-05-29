using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace KHCT.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected BaseApiController(ISender sender)
    {
        Sender = sender;
    }

    protected ISender Sender { get; }
}
