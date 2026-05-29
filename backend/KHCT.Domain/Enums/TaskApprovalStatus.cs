namespace KHCT.Domain.Enums;

public enum TaskApprovalStatus
{
    Draft = 0,
    PendingTeam = 1,
    ApprovedTeam = 2,
    ApprovedDepartment = 3,
    ApprovedFinal = 4,
    Returned = 5
}
