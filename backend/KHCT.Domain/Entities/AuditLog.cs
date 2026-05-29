using KHCT.Domain.Common;

namespace KHCT.Domain.Entities;

public class AuditLog : Entity
{
    public string EntityName { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public Guid? ActorUserId { get; set; }
    public User? ActorUser { get; set; }
    public string? BeforeJson { get; set; }
    public string? AfterJson { get; set; }
}
