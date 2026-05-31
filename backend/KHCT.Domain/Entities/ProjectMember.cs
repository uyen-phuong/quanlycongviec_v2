using KHCT.Domain.Common;

namespace KHCT.Domain.Entities;

public class ProjectMember
{
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
}
