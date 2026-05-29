using KHCT.Domain.Common;

namespace KHCT.Domain.Entities;

public class Notification : Entity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Body { get; set; }
    public string EventType { get; set; } = string.Empty;
    public Guid? PlanId { get; set; }
    public Guid? TaskId { get; set; }
    public bool IsRead { get; set; }
}
