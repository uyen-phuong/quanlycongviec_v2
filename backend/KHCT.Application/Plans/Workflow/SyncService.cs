using KHCT.Application.Common.Interfaces;
using KHCT.Application.Common.Support;
using KHCT.Application.Tasks;
using KHCT.Domain.Common;
using KHCT.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using TaskEntity = KHCT.Domain.Entities.Task;

namespace KHCT.Application.Plans.Workflow;

internal static class SyncService
{
    public static async System.Threading.Tasks.Task EnsureSyncReadyAsync(
        IApplicationDbContext db,
        Domain.Entities.Plan subPlan,
        CancellationToken ct)
    {
        var inheritedSources = subPlan.Tasks
            .Where(t => t.InheritedFromTaskId.HasValue)
            .Select(t => t.InheritedFromTaskId!.Value)
            .ToList();

        if (inheritedSources.Count == 0) return;

        var duplicate = inheritedSources
            .GroupBy(x => x)
            .FirstOrDefault(g => g.Count() > 1);
        if (duplicate != null)
        {
            throw new DomainException(
                "sync_duplicate_source",
                $"Multiple sub tasks reference main task {duplicate.Key}.");
        }

        var existingIds = await db.Tasks
            .Where(t => inheritedSources.Contains(t.Id))
            .Select(t => t.Id)
            .ToListAsync(ct);

        var missing = inheritedSources.Except(existingIds).ToList();
        if (missing.Count > 0)
        {
            throw new DomainException(
                "sync_missing_target",
                $"Main task(s) not found: {string.Join(", ", missing)}.");
        }
    }

    public static async System.Threading.Tasks.Task RunAsync(
        IApplicationDbContext db,
        Domain.Entities.Plan subPlan,
        Guid? actorUserId,
        CancellationToken ct)
    {
        var subTasks = await db.Tasks
            .Include(t => t.SupportingDepts)
            .Where(t => t.PlanId == subPlan.Id)
            .ToListAsync(ct);

        var pairs = subTasks
            .Where(t => t.InheritedFromTaskId.HasValue)
            .ToList();

        var mainIds = pairs.Select(t => t.InheritedFromTaskId!.Value).ToList();
        var mainTasks = await db.Tasks
            .Include(t => t.SupportingDepts)
            .Where(t => mainIds.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, ct);

        foreach (var subTask in pairs)
        {
            var mainId = subTask.InheritedFromTaskId!.Value;
            if (!mainTasks.TryGetValue(mainId, out var mainTask))
            {
                throw new DomainException(
                    "sync_missing_target",
                    $"Main task {mainId} not found.");
            }

            // Only sync progress from the owner department's sub plan.
            // Supporting departments have read/track access but do not overwrite main plan data.
            if (mainTask.OwnerDepartmentId != subPlan.DepartmentId)
                continue;

            mainTask.OutlineIndex = subTask.OutlineIndex;
            mainTask.DisplayOrder = subTask.DisplayOrder;
            mainTask.Title = subTask.Title;
            mainTask.Deadline = subTask.Deadline;
            mainTask.BksMemberText = subTask.BksMemberText;
            mainTask.KtnbLeaderText = subTask.KtnbLeaderText;
            mainTask.NoteText = subTask.NoteText;
            mainTask.ProgressText = subTask.ProgressText;
            mainTask.WorkStatus = subTask.WorkStatus;
            mainTask.ReasonNotCompleted = subTask.ReasonNotCompleted;
            TaskSupport.ApplySupportingDepartments(
                mainTask,
                subTask.SupportingDepts.Select(x => x.DepartmentId));

            db.AuditLogs.Add(ApplicationSupport.CreateAudit(
                "task",
                mainTask.Id,
                "sync_from_sub",
                actorUserId,
                null,
                new
                {
                    SubPlanId = subPlan.Id,
                    SubTaskId = subTask.Id,
                    InheritedFromTaskId = mainId
                }));
        }

    }
}
