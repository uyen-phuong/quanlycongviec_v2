using KHCT.Api.Common;
using KHCT.Application.Admin.Departments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KHCT.Api.Controllers;

[Authorize(Roles = "ADMIN")]
[Route("api/admin/departments")]
[Tags("Admin")]
public sealed class AdminDepartmentsController : BaseApiController
{
    public AdminDepartmentsController(ISender sender) : base(sender)
    {
    }

    public record UpdateDepartmentRequest(string Name, bool IsActive);

    [HttpGet]
    public async Task<IActionResult> GetDepartments(CancellationToken ct)
    {
        var result = await Sender.Send(new GetDepartmentsQuery(), ct);
        return Ok(new ApiEnvelope<object>(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateDepartment(Guid id, [FromBody] UpdateDepartmentRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new UpdateDepartmentCommand(id, request.Name, request.IsActive), ct);
        return Ok(new ApiEnvelope<object>(result));
    }
}
