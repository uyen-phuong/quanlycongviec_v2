using KHCT.Application.Common.Interfaces;
using KHCT.Application.Common.Support;
using KHCT.Application.Tasks;
using KHCT.Domain.Common;
using KHCT.Domain.Entities;
using KHCT.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using TaskEntity = KHCT.Domain.Entities.Task;

namespace KHCT.Application.Plans.Workflow;

internal static class InheritService
{
    public static void EnsureInheritReady(Plan mainPlan)
    {
        var childCounts = mainPlan.Tasks
            .Where(task => task.ParentTaskId.HasValue)
            .GroupBy(task => task.ParentTaskId!.Value)
            .ToDictionary(group => group.Key, group => group.Count());

        foreach (var task in mainPlan.Tasks)
        {
            if (task.IsHeader) continue;
            if (task.WorkType != WorkType.General) continue;
            if (childCounts.ContainsKey(task.Id)) continue;
            if (task.OwnerDepartmentId == null)
            {
                throw new DomainException(
                    "inherit_general_missing_owner",
                    $"Main task '{task.Title}' is general but missing owner_department_id.");
            }
        }
    }

    public static async System.Threading.Tasks.Task RunAsync(
        IApplicationDbContext db,
        Plan mainPlan,
        Guid? actorUserId,
        CancellationToken ct)
    {
        var allTasks = await db.Tasks
            .Include(t => t.SupportingDepts)
            .Where(t => t.PlanId == mainPlan.Id)
            .OrderBy(t => t.DisplayOrder)
            .ThenBy(t => t.CreatedAt)
            .ToListAsync(ct);
        var taskById = allTasks.ToDictionary(t => t.Id);
        var childrenByParent = allTasks
            .GroupBy(task => task.ParentTaskId)
            .ToLookup(
                group => group.Key,
                group => group
                    .OrderBy(task => task.DisplayOrder)
                    .ThenBy(task => task.CreatedAt)
                    .ToList());

        // Owner dept -> leaf tasks
        var ownerLeavesByDept = allTasks
            .Where(t => !t.IsHeader && t.WorkType == WorkType.General && t.OwnerDepartmentId.HasValue)
            .GroupBy(t => t.OwnerDepartmentId!.Value)
            .ToDictionary(g => g.Key, g => (IEnumerable<TaskEntity>)g);

        // Supporting dept -> leaf tasks
        var supportingLeavesByDept = allTasks
            .Where(t => !t.IsHeader && t.WorkType == WorkType.General && t.SupportingDepts.Count > 0)
            .SelectMany(t => t.SupportingDepts.Select(sd => new { DeptId = sd.DepartmentId, Task = t }))
            .GroupBy(x => x.DeptId)
            .ToDictionary(g => g.Key, g => (IEnumerable<TaskEntity>)g.Select(x => x.Task).Distinct());

        var allDeptIds = new HashSet<Guid>(ownerLeavesByDept.Keys);
        allDeptIds.UnionWith(supportingLeavesByDept.Keys);

        foreach (var deptId in allDeptIds)
        {
            var ownerList = ownerLeavesByDept.TryGetValue(deptId, out var o) ? o : [];
            var supportList = supportingLeavesByDept.TryGetValue(deptId, out var s) ? s : [];
            var leaves = ownerList.Union(supportList).Distinct().ToList();

            var subPlan = await db.Plans
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.SupportingDepts)
                .FirstOrDefaultAsync(
                    p => p.Scope == PlanScope.Sub
                        && p.DepartmentId == deptId
                        && p.Year == mainPlan.Year
                        && p.Month == mainPlan.Month,
                    ct);

            if (subPlan == null)
            {
                subPlan = new Plan
                {
                    Id = Guid.NewGuid(),
                    Scope = PlanScope.Sub,
                    DepartmentId = deptId,
                    Year = mainPlan.Year,
                    Month = mainPlan.Month,
                    Status = WorkflowStatus.Draft,
                    CreatedById = actorUserId ?? mainPlan.CreatedById
                };
                db.Plans.Add(subPlan);
            }

            var existingInherited = subPlan.Tasks
                .Where(t => t.InheritedFromTaskId.HasValue)
                .ToDictionary(t => t.InheritedFromTaskId!.Value);

            var ancestorIds = new HashSet<Guid>();
            foreach (var leaf in leaves)
            {
                var cur = leaf.ParentTaskId;
                while (cur.HasValue && taskById.TryGetValue(cur.Value, out var parent))
                {
                    ancestorIds.Add(parent.Id);
                    cur = parent.ParentTaskId;
                }
            }

            var selectedTaskIds = leaves
                .Select(task => task.Id)
                .ToHashSet();
            var includedTaskIds = new HashSet<Guid>(selectedTaskIds);
            includedTaskIds.UnionWith(ancestorIds);

            var orderedIncludedTasks = new List<TaskEntity>();
            void Walk(Guid? parentTaskId)
            {
                foreach (var children in childrenByParent[parentTaskId])
                {
                    foreach (var child in children)
                    {
                        if (includedTaskIds.Contains(child.Id))
                        {
                            orderedIncludedTasks.Add(child);
                        }

                        Walk(child.Id);
                    }
                }
            }

            Walk(null);

            var idMap = new Dictionary<Guid, Guid>();

            foreach (var sourceTask in orderedIncludedTasks)
            {
                var isStructuralOnly = !selectedTaskIds.Contains(sourceTask.Id);
                var isHeader = sourceTask.IsHeader || isStructuralOnly;
                var supportingDepartmentIds = isStructuralOnly
                    ? Array.Empty<Guid>()
                    : sourceTask.SupportingDepts.Select(x => x.DepartmentId).ToArray();
                var targetTask = existingInherited.GetValueOrDefault(sourceTask.Id);
                if (targetTask == null)
                {
                    targetTask = new TaskEntity
                    {
                        Id = Guid.NewGuid(),
                        PlanId = subPlan.Id,
                        WorkStatus = WorkStatus.NotStarted,
                        IsLocked = true,
                        InheritedFromTaskId = sourceTask.Id
                    };
                    db.Tasks.Add(targetTask);
                }

                idMap[sourceTask.Id] = targetTask.Id;
                targetTask.ParentTaskId = sourceTask.ParentTaskId.HasValue
                    && idMap.TryGetValue(sourceTask.ParentTaskId.Value, out var mappedParent)
                    ? mappedParent
                    : null;
                targetTask.OutlineIndex = sourceTask.OutlineIndex;
                targetTask.DisplayOrder = sourceTask.DisplayOrder;
                targetTask.IsHeader = isHeader;
                targetTask.Title = sourceTask.Title;
                targetTask.WorkType = WorkType.General;
                targetTask.Deadline = isStructuralOnly ? null : sourceTask.Deadline;
                targetTask.OwnerDepartmentId = isStructuralOnly ? null : sourceTask.OwnerDepartmentId;
                targetTask.BksMemberText = isStructuralOnly ? null : sourceTask.BksMemberText;
                targetTask.KtnbLeaderText = isStructuralOnly ? null : sourceTask.KtnbLeaderText;
                targetTask.NoteText = isStructuralOnly ? null : sourceTask.NoteText;
                targetTask.ProgressText = isStructuralOnly ? null : sourceTask.ProgressText;
                targetTask.WorkStatus = isStructuralOnly ? WorkStatus.NotStarted : sourceTask.WorkStatus;
                targetTask.ReasonNotCompleted = isStructuralOnly ? null : sourceTask.ReasonNotCompleted;
                targetTask.IsLocked = true;
                targetTask.InheritedFromTaskId = sourceTask.Id;
                TaskSupport.ApplySupportingDepartments(targetTask, supportingDepartmentIds);
            }

            // Cleanup orphan inherited sub tasks: source main task no longer maps to a current
            // included leaf/ancestor for this department (e.g. deleted, owner changed, or moved
            // out of this dept). Without this, re-running inherit leaves duplicate ghost rows.
            var orphanInherited = subPlan.Tasks
                .Where(t => t.InheritedFromTaskId.HasValue
                    && !idMap.ContainsKey(t.InheritedFromTaskId!.Value))
                .ToList();

            if (orphanInherited.Count > 0)
            {
                var orphanIds = orphanInherited.Select(t => t.Id).ToHashSet();
                // Re-parent any user-added children of orphans (or other orphans) to root so
                // ParentTask FK (Restrict) does not block deletion.
                var dependents = subPlan.Tasks
                    .Where(t => t.ParentTaskId.HasValue && orphanIds.Contains(t.ParentTaskId!.Value))
                    .ToList();
                foreach (var child in dependents)
                {
                    child.ParentTaskId = null;
                }

                foreach (var orphan in orphanInherited)
                {
                    db.Tasks.Remove(orphan);
                }
            }

            if (idMap.Count > 0)
            {
                db.AuditLogs.Add(ApplicationSupport.CreateAudit(
                    "plan",
                    subPlan.Id,
                    "inherit_create",
                    actorUserId,
                    null,
                    new
                    {
                        SubPlanId = subPlan.Id,
                        MainPlanId = mainPlan.Id,
                        DepartmentId = deptId,
                        InheritedTaskCount = idMap.Count
                    }));
            }
        }
    }
}
