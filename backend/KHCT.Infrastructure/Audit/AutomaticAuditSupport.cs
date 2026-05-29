using System.Text.Json;
using KHCT.Domain.Common;
using KHCT.Domain.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TaskEntity = KHCT.Domain.Entities.Task;

namespace KHCT.Infrastructure.Audit;

internal static class AutomaticAuditSupport
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static bool ShouldAutoAudit(EntityEntry entry) =>
        entry.Entity is not AuditLog
        && entry.State is Microsoft.EntityFrameworkCore.EntityState.Added or Microsoft.EntityFrameworkCore.EntityState.Modified or Microsoft.EntityFrameworkCore.EntityState.Deleted
        && entry.Entity switch
        {
            Plan => true,
            TaskEntity => true,
            User => true,
            Department => true,
            ApprovalConfig => true,
            Attachment => true,
            PersonalEvaluationPeriod => true,
            PersonalEvaluationItem => true,
            _ => false
        };

    public static string EntityName(Entity entity) =>
        entity switch
        {
            Plan => "plan",
            TaskEntity => "task",
            User => "user",
            Department => "department",
            ApprovalConfig => "approval_config",
            Attachment => "attachment",
            PersonalEvaluationPeriod => "personal_evaluation_period",
            PersonalEvaluationItem => "personal_evaluation_item",
            _ => throw new InvalidOperationException($"Unsupported audit entity: {entity.GetType().Name}")
        };

    public static string ActionName(Microsoft.EntityFrameworkCore.EntityState state) =>
        state switch
        {
            Microsoft.EntityFrameworkCore.EntityState.Added => "create",
            Microsoft.EntityFrameworkCore.EntityState.Modified => "update",
            Microsoft.EntityFrameworkCore.EntityState.Deleted => "delete",
            _ => throw new InvalidOperationException($"Unsupported audit state: {state}")
        };

    public static object? BuildBeforeSnapshot(EntityEntry entry) =>
        entry.State switch
        {
            Microsoft.EntityFrameworkCore.EntityState.Added => null,
            Microsoft.EntityFrameworkCore.EntityState.Modified or Microsoft.EntityFrameworkCore.EntityState.Deleted => BuildSnapshot(entry, entry.OriginalValues),
            _ => null
        };

    public static object? BuildAfterSnapshot(EntityEntry entry) =>
        entry.State switch
        {
            Microsoft.EntityFrameworkCore.EntityState.Deleted => null,
            Microsoft.EntityFrameworkCore.EntityState.Added or Microsoft.EntityFrameworkCore.EntityState.Modified => BuildSnapshot(entry, entry.CurrentValues),
            _ => null
        };

    public static bool HasMeaningfulDifference(object? before, object? after)
    {
        if (before is null || after is null)
        {
            return true;
        }

        return JsonSerializer.Serialize(before, JsonOptions) != JsonSerializer.Serialize(after, JsonOptions);
    }

    private static object BuildSnapshot(EntityEntry entry, PropertyValues values)
    {
        if (entry.Entity is TaskEntity task)
        {
            return new
            {
                task.Id,
                task.PlanId,
                task.ParentTaskId,
                task.OutlineIndex,
                task.DisplayOrder,
                task.IsHeader,
                task.Title,
                WorkType = (int)task.WorkType,
                WorkStatus = task.WorkStatus.ToString(),
                task.Deadline,
                task.AssigneeUserId,
                task.OwnerDepartmentId,
                task.BksMemberText,
                task.KtnbLeaderText,
                task.NoteText,
                task.IsLocked,
                task.InheritedFromTaskId,
                task.HasOpenComment,
                task.ProgressText,
                task.ReasonNotCompleted,
                SupportingDepartmentIds = task.SupportingDepts.Select(x => x.DepartmentId).OrderBy(x => x).ToList()
            };
        }

        if (entry.Entity is User user)
        {
            return new
            {
                user.Id,
                user.Username,
                user.FullName,
                user.Email,
                user.DepartmentId,
                user.IsActive,
                user.LastLoginAt,
                RoleIds = user.UserRoles.Select(x => x.RoleId).OrderBy(x => x).ToList()
            };
        }

        var dictionary = new SortedDictionary<string, object?>(StringComparer.Ordinal);
        foreach (var property in values.Properties)
        {
            var name = property.Name;
            if (name is nameof(Entity.CreatedAt) or nameof(Entity.UpdatedAt) or "PasswordHash")
            {
                continue;
            }

            dictionary[name] = values[property];
        }

        return dictionary;
    }
}
