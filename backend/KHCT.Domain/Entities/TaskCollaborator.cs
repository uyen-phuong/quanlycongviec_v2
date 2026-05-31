using KHCT.Domain.Enums;
using TaskEntity = KHCT.Domain.Entities.Task;

namespace KHCT.Domain.Entities;

public class TaskCollaborator
{
    public Guid TaskId { get; set; }
    public TaskEntity? Task { get; set; }
    
    public Guid UserId { get; set; }
    public User? User { get; set; }
    
    public TaskCollaboratorRole Role { get; set; } = TaskCollaboratorRole.Collaborator;
    public string? CollaborationContent { get; set; }
    public DateTime CreatedAt { get; set; }
}
