using KHCT.Domain.Common;
using KHCT.Domain.Enums;

namespace KHCT.Domain.Entities;

public class TaskApprovalHistory : Entity
{
    public Guid TaskId { get; set; }
    public Task? Task { get; set; }
    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }
    public ApprovalAction Action { get; set; }
    public TaskWorkflowStatus FromStatus { get; set; }
    public TaskWorkflowStatus ToStatus { get; set; }
    public Guid ActorUserId { get; set; }
    public User? ActorUser { get; set; }
    public string? Comment { get; set; }
}
