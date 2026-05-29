namespace KHCT.Application.Plans;

public record PlanListItemDto(
    Guid Id,
    string Scope,
    int Year,
    int Month,
    string Status,
    Guid? DepartmentId,
    string? DepartmentCode,
    string? DepartmentName,
    Guid CreatedById,
    string? CreatedByName,
    DateTime? SubmittedAt,
    DateTime? ApprovedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record PlanDetailDto(
    Guid Id,
    string Scope,
    int Year,
    int Month,
    string Status,
    Guid? DepartmentId,
    string? DepartmentCode,
    string? DepartmentName,
    Guid CreatedById,
    string? CreatedByName,
    DateTime? SubmittedAt,
    DateTime? ApprovedAt,
    int TaskCount,
    DateTime CreatedAt,
    DateTime UpdatedAt);
