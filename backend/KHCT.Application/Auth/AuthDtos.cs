namespace KHCT.Application.Auth;

public record UserDto(
    Guid Id,
    string Username,
    string FullName,
    string? Email,
    Guid? DepartmentId,
    string? DepartmentCode,
    IReadOnlyList<string> Roles);

public record AuthResultDto(
    string AccessToken,
    DateTime AccessExpiresAt,
    string RefreshToken,
    DateTime RefreshExpiresAt,
    UserDto User);
