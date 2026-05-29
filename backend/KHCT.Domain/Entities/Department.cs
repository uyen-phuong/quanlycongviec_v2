using KHCT.Domain.Common;

namespace KHCT.Domain.Entities;

public class Department : Entity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
