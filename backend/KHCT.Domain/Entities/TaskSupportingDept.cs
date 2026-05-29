using TaskEntity = KHCT.Domain.Entities.Task;

namespace KHCT.Domain.Entities;

public class TaskSupportingDept
{
    public Guid TaskId { get; set; }
    public TaskEntity? Task { get; set; }
    public Guid DepartmentId { get; set; }
    public Department? Department { get; set; }
    public DateTime CreatedAt { get; set; }
}
