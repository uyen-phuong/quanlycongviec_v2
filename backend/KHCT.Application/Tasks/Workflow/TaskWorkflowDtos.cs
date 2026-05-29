namespace KHCT.Application.Tasks.Workflow;

public record TaskApprovalHistoryDto(
    Guid Id,
    Guid TaskId,
    Guid? DepartmentId,
    string Action,
    string FromStatus,
    string ToStatus,
    Guid ActorUserId,
    string? ActorUserName,
    string? Comment,
    DateTime CreatedAt);
