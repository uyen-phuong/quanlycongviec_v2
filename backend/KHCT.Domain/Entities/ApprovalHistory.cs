using KHCT.Domain.Common;
using KHCT.Domain.Enums;

namespace KHCT.Domain.Entities;

public class ApprovalHistory : Entity
{
    public Guid PlanId { get; set; }
    public Plan? Plan { get; set; }
    public ApprovalAction Action { get; set; }
    public WorkflowStatus FromStatus { get; set; }
    public WorkflowStatus ToStatus { get; set; }
    public Guid ActorUserId { get; set; }
    public User? ActorUser { get; set; }
    public string? Comment { get; set; }
}
