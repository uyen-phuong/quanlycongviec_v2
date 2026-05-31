namespace KHCT.Application.Admin;

public record AdminUserListItemDto(
    Guid Id,
    string Username,
    string FullName,
    string? Email,
    bool IsActive,
    DateTime? LastLoginAt,
    DateTime? LastLogoutAt,
    Guid? DepartmentId,
    string? DepartmentCode,
    string? DepartmentName,
    Guid? PositionId,
    string? PositionCode,
    string? PositionName,
    Guid? RoleId,
    string? RoleCode,
    string? RoleName);

public record AdminUserDetailDto(
    Guid Id,
    string Username,
    string FullName,
    string? Email,
    bool IsActive,
    DateTime? LastLoginAt,
    DateTime? LastLogoutAt,
    Guid? DepartmentId,
    string? DepartmentCode,
    string? DepartmentName,
    Guid? PositionId,
    string? PositionCode,
    string? PositionName,
    Guid? RoleId,
    string? RoleCode,
    string? RoleName,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record DepartmentDto(
    Guid Id,
    string Code,
    string Name,
    bool IsActive);

public record PositionDto(
    Guid Id,
    string Code,
    string Name,
    bool IsActive,
    int SortOrder);

public record RoleDto(
    Guid Id,
    string Code,
    string Name);

public record ApprovalConfigDto(
    Guid Id,
    string Scope,
    int Level,
    Guid? DepartmentId,
    string? DepartmentCode,
    string? DepartmentName,
    Guid RoleId,
    string RoleCode,
    string RoleName,
    bool IsActive);
