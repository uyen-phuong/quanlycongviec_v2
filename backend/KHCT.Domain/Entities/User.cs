using KHCT.Domain.Common;

namespace KHCT.Domain.Entities;

public class User : Entity
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
