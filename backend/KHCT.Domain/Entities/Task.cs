using KHCT.Domain.Common;
using KHCT.Domain.Enums;

namespace KHCT.Domain.Entities;

public class Task : Entity
{
    public Guid? PlanId { get; set; }
    public Plan? Plan { get; set; }
    public Guid? ProjectId { get; set; }
    public TaskCategory Category { get; set; } = TaskCategory.PlanTask;
    public Guid? ParentTaskId { get; set; }
    public Task? ParentTask { get; set; }
    public string? OutlineIndex { get; set; }
    public int DisplayOrder { get; set; }
    private bool _isHeader;
    public bool IsHeader
    {
        get => _isHeader;
        set
        {
            _isHeader = value;
            SyncWorkStatus();
        }
    }

    public string Title { get; set; } = string.Empty;
    public WorkType WorkType { get; set; } = WorkType.General;
    
    public WorkStatus WorkStatus { get; set; } = WorkStatus.NotStarted;

    private DateTime? _deadline;
    public DateTime? Deadline
    {
        get => _deadline;
        set
        {
            _deadline = value;
            SyncWorkStatus();
        }
    }

    public Guid? AssigneeUserId { get; set; }
    public User? AssigneeUser { get; set; }
    public Guid? OwnerDepartmentId { get; set; }
    public Department? OwnerDepartment { get; set; }
    public Guid? ControllerUserId { get; set; }
    public User? ControllerUser { get; set; }
    public string? BksMemberText { get; set; }
    public string? KtnbLeaderText { get; set; }
    public string? NoteText { get; set; }
    public bool IsLocked { get; set; }
    public Guid? InheritedFromTaskId { get; set; }
    public Task? InheritedFromTask { get; set; }
    public bool HasOpenComment { get; set; }
    public string? ReasonNotCompleted { get; set; }
    public string? ProgressText { get; set; }

    private TaskWorkflowStatus _workflowStatus = TaskWorkflowStatus.New;
    public TaskWorkflowStatus WorkflowStatus
    {
        get => _workflowStatus;
        set
        {
            _workflowStatus = value;
            SyncWorkStatus();
        }
    }

    public TaskPriority Priority { get; set; } = TaskPriority.Normal;
    public TaskComplexity Complexity { get; set; } = TaskComplexity.Medium;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public ICollection<Task> Children { get; set; } = new List<Task>();
    public ICollection<TaskSupportingDept> SupportingDepts { get; set; } = new List<TaskSupportingDept>();
    public ICollection<TaskApprovalHistory> ApprovalHistories { get; set; } = new List<TaskApprovalHistory>();
    public ICollection<TaskCollaborator> Collaborators { get; set; } = new List<TaskCollaborator>();

    private void SyncWorkStatus()
    {
        if (_isHeader)
        {
            WorkStatus = WorkStatus.NotStarted;
            return;
        }

        if (_deadline.HasValue && _deadline.Value.Date < DateTime.UtcNow.Date && _workflowStatus != TaskWorkflowStatus.Completed)
        {
            WorkStatus = WorkStatus.Overdue;
            return;
        }

        WorkStatus = _workflowStatus switch
        {
            TaskWorkflowStatus.New => WorkStatus.NotStarted,
            TaskWorkflowStatus.PendingAssign => WorkStatus.NotStarted,
            TaskWorkflowStatus.InProgress => WorkStatus.InProgress,
            TaskWorkflowStatus.PendingReview => WorkStatus.InProgress,
            TaskWorkflowStatus.PendingApproval => WorkStatus.InProgress,
            TaskWorkflowStatus.Completed => WorkStatus.Done,
            TaskWorkflowStatus.Returned => WorkStatus.InProgress,
            _ => WorkStatus.NotStarted
        };
    }
}
