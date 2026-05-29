using KHCT.Domain.Common;
using KHCT.Domain.Enums;

namespace KHCT.Domain.Entities;

public class ApprovalConfig : Entity
{
    public PlanScope Scope { get; set; }
    public int Level { get; set; }
    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }
    public Guid RoleId { get; set; }
    public Role? Role { get; set; }
    public bool IsActive { get; set; } = true;
}
