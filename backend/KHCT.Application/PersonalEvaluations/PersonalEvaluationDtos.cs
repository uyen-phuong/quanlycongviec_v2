namespace KHCT.Application.PersonalEvaluations;

public record PersonalEvaluationItemDto(
    Guid Id,
    Guid PeriodId,
    int DisplayOrder,
    string? AssignmentSource,
    string? TaskName,
    string? TaskDetail,
    string? ActualResult,
    string? Note,
    DateTime? Deadline,
    DateTime? CompletedAt,
    decimal? SelfProgressScore,
    decimal? SelfQualityScore,
    decimal? TeamLeadProgressScore,
    decimal? TeamLeadQualityScore,
    decimal? ManagerProgressScore,
    decimal? ManagerQualityScore,
    decimal? DeputyProgressScore,
    decimal? DeputyQualityScore,
    decimal? HeadProgressScore,
    decimal? HeadQualityScore);

public record PersonalEvaluationPeriodDto(
    Guid Id,
    Guid UserId,
    string UserFullName,
    Guid DepartmentId,
    string DepartmentName,
    int ReportYear,
    int ReportMonth,
    string Status,
    decimal? CapacityAttitudeSelfScore,
    decimal? CapacityAttitudeTeamLeadScore,
    decimal? CapacityAttitudeManagerScore,
    decimal? CapacityAttitudeDeputyScore,
    decimal? CapacityAttitudeHeadScore,
    decimal? DisciplineSelfScore,
    decimal? DisciplineTeamLeadScore,
    decimal? DisciplineManagerScore,
    decimal? DisciplineDeputyScore,
    decimal? DisciplineHeadScore,
    decimal? InspectionSelfScore,
    decimal? InspectionTeamLeadScore,
    decimal? InspectionManagerScore,
    decimal? InspectionDeputyScore,
    decimal? InspectionHeadScore);

public record PersonalEvaluationResponse(
    PersonalEvaluationPeriodDto Period,
    IReadOnlyList<PersonalEvaluationItemDto> Items);
