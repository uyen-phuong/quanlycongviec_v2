namespace KHCT.Api.Common;

public record ApiEnvelope<T>(T data, object? meta = null);

public record ApiErrorEnvelope(ApiError error);

public record ApiError(string code, string message, IReadOnlyList<ApiErrorDetail>? details = null);

public record ApiErrorDetail(string field, string message);
