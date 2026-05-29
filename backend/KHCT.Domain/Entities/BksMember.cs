using KHCT.Domain.Common;

namespace KHCT.Domain.Entities;

public class BksMember : Entity
{
    public string FullName { get; set; } = string.Empty;
    public string? Title { get; set; }
    public bool IsActive { get; set; } = true;
}
