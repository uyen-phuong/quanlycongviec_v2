using KHCT.Domain.Common;
using KHCT.Domain.Enums;
using TaskEntity = KHCT.Domain.Entities.Task;

namespace KHCT.Domain.Entities;

public class LineComment : Entity
{
    public Guid TaskId { get; set; }
    public TaskEntity? Task { get; set; }
    public Guid? ParentCommentId { get; set; }
    public LineComment? ParentComment { get; set; }
    public Guid AuthorUserId { get; set; }
    public User? AuthorUser { get; set; }
    public CommentRole AuthorRole { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public Guid? ResolvedByUserId { get; set; }
    public User? ResolvedByUser { get; set; }
}
