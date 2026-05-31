using KHCT.Domain.Enums;

namespace KHCT.Application.Projects;

public record ProjectListItemDto(
    Guid Id,
    string Name,
    string Description,
    Guid LeaderId,
    string LeaderName,
    Guid? SubLeaderId,
    string? SubLeaderName,
    string Status,
    DateTime? SubmittedAt,
    DateTime? ApprovedAt,
    int TotalTasksCount,
    int CompletedTasksCount,
    int CompletionPercent,
    IReadOnlyList<Guid> MemberUserIds,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record ProjectDetailDto(
    Guid Id,
    string Name,
    string Description,
    Guid LeaderId,
    string LeaderName,
    Guid? SubLeaderId,
    string? SubLeaderName,
    string Status,
    DateTime? SubmittedAt,
    DateTime? ApprovedAt,
    IReadOnlyList<Guid> MemberUserIds,
    IReadOnlyList<ProjectMemberDto> Members,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record ProjectMemberDto(
    Guid UserId,
    string FullName,
    string DepartmentName);
