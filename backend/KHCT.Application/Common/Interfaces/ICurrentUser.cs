namespace KHCT.Application.Common.Interfaces;

public interface ICurrentUser
{
    Guid? UserId { get; }
    string? Username { get; }
    Guid? DepartmentId { get; }
    IReadOnlyList<string> Roles { get; }
    bool IsAuthenticated { get; }
}
