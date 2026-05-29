using KHCT.Api.Common;
using Microsoft.AspNetCore.Mvc;

namespace KHCT.Api.Controllers;

[ApiController]
[Route("api/health")]
[Tags("Health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() =>
        Ok(new ApiEnvelope<object>(new { service = "KHCT API", status = "Healthy" }));
}
