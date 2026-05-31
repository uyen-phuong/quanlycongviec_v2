namespace KHCT.Domain.Enums;

public enum TaskWorkflowStatus
{
    New = 0,
    PendingAssign = 1,
    InProgress = 2,
    PendingReview = 3,
    PendingApproval = 4,
    Completed = 5,
    Returned = 6
}
