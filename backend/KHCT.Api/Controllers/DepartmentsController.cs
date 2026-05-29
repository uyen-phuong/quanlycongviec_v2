using KHCT.Api.Common;
using KHCT.Application.Departments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KHCT.Api.Controllers;

[Authorize]
[Route("api/departments")]
[Tags("Departments")]
public sealed class DepartmentsController : BaseApiController
{
    public DepartmentsController(ISender sender) : base(sender)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetDepartments(CancellationToken ct)
    {
        var result = await Sender.Send(new GetDepartmentsQuery(), ct);
        return Ok(new ApiEnvelope<object>(result));
    }
}
