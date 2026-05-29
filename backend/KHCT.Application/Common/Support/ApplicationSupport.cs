using System.Text.Json;
using KHCT.Application.Common.Interfaces;
using KHCT.Domain.Common;
using KHCT.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Common.Support;

public static class ApplicationSupport
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static AuditLog CreateAudit(
        string entityName,
        Guid entityId,
        string action,
        Guid? actorUserId,
        object? before,
        object? after) =>
        new()
        {
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            ActorUserId = actorUserId,
            BeforeJson = before is null ? null : JsonSerializer.Serialize(before, JsonOptions),
            AfterJson = after is null ? null : JsonSerializer.Serialize(after, JsonOptions)
        };

    public static async Task<Department?> RequireActiveDepartmentAsync(
        IApplicationDbContext db,
        Guid? departmentId,
        CancellationToken ct)
    {
        if (!departmentId.HasValue)
        {
            return null;
        }

        return await db.Departments.FirstOrDefaultAsync(x => x.Id == departmentId.Value && x.IsActive, ct)
            ?? throw new DomainException("department_invalid", "Department is missing or inactive.");
    }
}
