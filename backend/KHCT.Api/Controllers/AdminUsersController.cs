using KHCT.Api.Common;
using KHCT.Application.Admin.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KHCT.Api.Controllers;

[Authorize(Roles = "ADMIN")]
[Route("api/admin/users")]
[Tags("Admin")]
public sealed class AdminUsersController : BaseApiController
{
    public AdminUsersController(ISender sender) : base(sender)
    {
    }

    public record CreateUserRequest(
        string Username,
        string Password,
        string FullName,
        string? Email,
        Guid? DepartmentId,
        Guid RoleId,
        bool IsActive);

    public record UpdateUserRequest(
        string FullName,
        string? Email,
        Guid? DepartmentId,
        bool IsActive);

    public record ChangeUserRoleRequest(Guid RoleId);

    public record ResetUserPasswordRequest(string Password);

    [HttpGet]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] string? roleCode = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? keyword = null,
        CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetUsersQuery(page, pageSize, departmentId, roleCode, isActive, keyword), ct);
        return Ok(new ApiEnvelope<object>(result.Items, new
        {
            page = result.Page,
            pageSize = result.PageSize,
            total = result.Total
        }));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken ct)
    {
        var result = await Sender.Send(new GetUserByIdQuery(id), ct);
        return Ok(new ApiEnvelope<object>(result));
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new CreateUserCommand(
            request.Username,
            request.Password,
            request.FullName,
            request.Email,
            request.DepartmentId,
            request.RoleId,
            request.IsActive), ct);

        return Ok(new ApiEnvelope<object>(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new UpdateUserCommand(
            id,
            request.FullName,
            request.Email,
            request.DepartmentId,
            request.IsActive), ct);

        return Ok(new ApiEnvelope<object>(result));
    }

    [HttpPut("{id:guid}/role")]
    public async Task<IActionResult> ChangeRole(Guid id, [FromBody] ChangeUserRoleRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new ChangeUserRoleCommand(id, request.RoleId), ct);
        return Ok(new ApiEnvelope<object>(result));
    }

    [HttpPut("{id:guid}/password")]
    public async Task<IActionResult> ResetPassword(Guid id, [FromBody] ResetUserPasswordRequest request, CancellationToken ct)
    {
        await Sender.Send(new ResetUserPasswordCommand(id, request.Password), ct);
        return Ok(new ApiEnvelope<object>(new { success = true }));
    }
}
