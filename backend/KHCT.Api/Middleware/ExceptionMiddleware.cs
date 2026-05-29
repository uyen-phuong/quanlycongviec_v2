using System.Text.Json;
using FluentValidation;
using KHCT.Api.Common;
using KHCT.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await WriteError(context, 400, "validation_error", "Yêu cầu không hợp lệ",
                ex.Errors.Select(e => new ApiErrorDetail(e.PropertyName, e.ErrorMessage)).ToList());
        }
        catch (UnauthorizedAccessException ex)
        {
            await WriteError(context, 401, "unauthorized", ex.Message);
        }
        catch (ForbiddenException ex)
        {
            await WriteError(context, 403, ex.Code, ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            await WriteError(context, 404, "not_found", ex.Message);
        }
        catch (DomainException ex)
        {
            await WriteError(context, 422, ex.Code, ex.Message);
        }
        catch (DbUpdateConcurrencyException)
        {
            await WriteError(context, 409, "concurrency_conflict",
                "Dữ liệu đã được thay đổi bởi người dùng khác. Vui lòng tải lại trang và thử lại.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteError(context, 500, "internal_error", "Đã có lỗi xảy ra");
        }
    }

    private static async Task WriteError(HttpContext ctx, int status, string code, string message, IReadOnlyList<ApiErrorDetail>? details = null)
    {
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/json";
        var payload = new ApiErrorEnvelope(new ApiError(code, message, details));
        await ctx.Response.WriteAsync(JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}
