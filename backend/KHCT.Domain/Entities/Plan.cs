using KHCT.Domain.Common;
using KHCT.Domain.Enums;
using TaskEntity = KHCT.Domain.Entities.Task;

namespace KHCT.Domain.Entities;

public class Plan : Entity
{
    public string Name { get; set; } = string.Empty;
    public PlanScope Scope { get; set; }
    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public ReportingPeriodType ReportingPeriodType { get; set; } = ReportingPeriodType.Monthly;
    public int CurrentPeriodIndex { get; set; } = 0;
    public WorkflowStatus Status { get; set; } = WorkflowStatus.Draft;
    public Guid CreatedById { get; set; }
    public User? CreatedBy { get; set; }
    public Guid? KtnbLeaderId { get; set; }
    public User? KtnbLeader { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public ICollection<TaskEntity> Tasks { get; set; } = new List<TaskEntity>();

    public byte[] RowVersion { get; set; } = [];
}
