namespace KHCT.Application.Plans;

public record ReportingPeriodDto(
    Guid Id,
    Guid PlanId,
    string PeriodLabel,
    string? ProgressText,
    int CompletionPercent,
    string Status,
    Guid? ApprovedByUserId,
    string? ApprovedByUserName,
    DateTime? ApprovedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt);


public record PlanListItemDto(
    Guid Id,
    string Name,
    string Scope,
    int Year,
    int Month,
    string ReportingPeriodType,
    int CurrentPeriodIndex,
    string Status,
    Guid? DepartmentId,
    string? DepartmentCode,
    string? DepartmentName,
    Guid CreatedById,
    string? CreatedByName,
    Guid? KtnbLeaderId,
    string? KtnbLeaderName,
    DateTime? SubmittedAt,
    DateTime? ApprovedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record PlanDetailDto(
    Guid Id,
    string Name,
    string Scope,
    int Year,
    int Month,
    string ReportingPeriodType,
    int CurrentPeriodIndex,
    string Status,
    Guid? DepartmentId,
    string? DepartmentCode,
    string? DepartmentName,
    Guid CreatedById,
    string? CreatedByName,
    Guid? KtnbLeaderId,
    string? KtnbLeaderName,
    DateTime? SubmittedAt,
    DateTime? ApprovedAt,
    int TaskCount,
    DateTime CreatedAt,
    DateTime UpdatedAt);
