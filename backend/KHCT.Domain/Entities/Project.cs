using KHCT.Domain.Common;
using KHCT.Domain.Enums;

namespace KHCT.Domain.Entities;

public class Project : Entity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid LeaderId { get; set; } // Tổ trưởng
    public User? Leader { get; set; }
    public Guid? SubLeaderId { get; set; } // Tổ phó
    public User? SubLeader { get; set; }
    
    public WorkflowStatus Status { get; set; } = WorkflowStatus.Draft; // Draft -> Pending -> Approved
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    public ICollection<Task> Tasks { get; set; } = new List<Task>();
}
