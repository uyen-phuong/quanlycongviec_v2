namespace KHCT.Application.Plans.Workflow;

public record ApprovalHistoryDto(
    Guid Id,
    Guid PlanId,
    string Action,
    string FromStatus,
    string ToStatus,
    Guid ActorUserId,
    string? ActorUserName,
    string? Comment,
    DateTime CreatedAt);

public record LineCommentDto(
    Guid Id,
    Guid TaskId,
    string TaskTitle,
    string? TaskOutlineIndex,
    Guid AuthorUserId,
    string? AuthorUserName,
    string AuthorRole,
    string Content,
    bool IsResolved,
    DateTime? ResolvedAt,
    Guid? ResolvedByUserId,
    string? ResolvedByUserName,
    DateTime CreatedAt);
