using KHCT.Domain.Common;
using KHCT.Domain.Enums;

namespace KHCT.Domain.Entities;

public class ApprovalHistory : Entity
{
    public Guid PlanId { get; set; }
    public Plan? Plan { get; set; }
    public ApprovalAction Action { get; set; }
    public ApprovalStatus FromStatus { get; set; }
    public ApprovalStatus ToStatus { get; set; }
    public Guid ActorUserId { get; set; }
    public User? ActorUser { get; set; }
    public string? Comment { get; set; }
}
