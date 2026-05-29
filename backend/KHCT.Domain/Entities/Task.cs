using KHCT.Domain.Common;
using KHCT.Domain.Enums;

namespace KHCT.Domain.Entities;

public class Task : Entity
{
    public Guid PlanId { get; set; }
    public Plan? Plan { get; set; }
    public Guid? ParentTaskId { get; set; }
    public Task? ParentTask { get; set; }
    public string? OutlineIndex { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsHeader { get; set; }
    public string Title { get; set; } = string.Empty;
    public WorkType WorkType { get; set; } = WorkType.General;
    public WorkStatus WorkStatus { get; set; } = WorkStatus.NotStarted;
    public DateTime? Deadline { get; set; }
    public Guid? AssigneeUserId { get; set; }
    public User? AssigneeUser { get; set; }
    public Guid? OwnerDepartmentId { get; set; }
    public Department? OwnerDepartment { get; set; }
    public string? BksMemberText { get; set; }
    public string? KtnbLeaderText { get; set; }
    public string? NoteText { get; set; }
    public bool IsLocked { get; set; }
    public Guid? InheritedFromTaskId { get; set; }
    public Task? InheritedFromTask { get; set; }
    public bool HasOpenComment { get; set; }
    public string? ReasonNotCompleted { get; set; }
    public string? ProgressText { get; set; }
    public TaskApprovalStatus ApprovalStatus { get; set; } = TaskApprovalStatus.Draft;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public ICollection<Task> Children { get; set; } = new List<Task>();
    public ICollection<TaskSupportingDept> SupportingDepts { get; set; } = new List<TaskSupportingDept>();
    public ICollection<TaskApprovalHistory> ApprovalHistories { get; set; } = new List<TaskApprovalHistory>();
}
