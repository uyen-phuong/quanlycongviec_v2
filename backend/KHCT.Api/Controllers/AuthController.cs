using KHCT.Api.Common;
using KHCT.Application.Auth;
using KHCT.Application.Auth.Login;
using KHCT.Application.Auth.Logout;
using KHCT.Application.Auth.Me;
using KHCT.Application.Auth.Refresh;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KHCT.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Tags("Auth")]
public sealed class AuthController : BaseApiController
{
    private const string RefreshCookieName = "khct_refresh";
    private readonly IWebHostEnvironment _env;

    public AuthController(ISender sender, IWebHostEnvironment env) : base(sender)
    {
        _env = env;
    }

    public record LoginRequest(string Username, string Password);
    public record RefreshRequest(string? RefreshToken);

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest body, CancellationToken ct)
    {
        var result = await Sender.Send(new LoginCommand(body.Username, body.Password), ct);
        SetRefreshCookie(result.RefreshToken, result.RefreshExpiresAt);
        return Ok(new ApiEnvelope<object>(new
        {
            accessToken = result.AccessToken,
            accessExpiresAt = result.AccessExpiresAt,
            user = result.User
        }));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest? body, CancellationToken ct)
    {
        var token = body?.RefreshToken
            ?? (Request.Cookies.TryGetValue(RefreshCookieName, out var c) ? c : null);
        if (string.IsNullOrEmpty(token))
            throw new UnauthorizedAccessException("Thiếu refresh token");

        var result = await Sender.Send(new RefreshTokenCommand(token), ct);
        SetRefreshCookie(result.RefreshToken, result.RefreshExpiresAt);
        return Ok(new ApiEnvelope<object>(new
        {
            accessToken = result.AccessToken,
            accessExpiresAt = result.AccessExpiresAt,
            user = result.User
        }));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var token = Request.Cookies.TryGetValue(RefreshCookieName, out var c) ? c : null;
        await Sender.Send(new LogoutCommand(token), ct);
        ClearRefreshCookie();
        return Ok(new ApiEnvelope<object>(new { success = true }));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var user = await Sender.Send(new GetMeQuery(), ct);
        return Ok(new ApiEnvelope<UserDto>(user));
    }

    private void SetRefreshCookie(string token, DateTime expires)
    {
        Response.Cookies.Append(RefreshCookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = !_env.IsDevelopment(),
            SameSite = SameSiteMode.Strict,
            Path = "/api/auth",
            Expires = expires
        });
    }

    private void ClearRefreshCookie()
    {
        Response.Cookies.Delete(RefreshCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = !_env.IsDevelopment(),
            SameSite = SameSiteMode.Strict,
            Path = "/api/auth"
        });
    }
}
