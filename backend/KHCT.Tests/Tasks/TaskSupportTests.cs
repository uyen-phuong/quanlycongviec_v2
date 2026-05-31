using FluentAssertions;
using KHCT.Application.Common.Interfaces;
using KHCT.Application.Plans;
using KHCT.Application.Tasks;
using KHCT.Domain.Common;
using KHCT.Domain.Entities;
using KHCT.Domain.Enums;
using TaskEntity = KHCT.Domain.Entities.Task;

namespace KHCT.Tests.Tasks;

public class TaskSupportTests
{
    [Fact]
    public void EnsureCanCreateOrDeleteTask_ShouldAllowVanThu_ForMainDraft()
    {
        var plan = MainPlan();
        var user = new TestCurrentUser(null, PlanSupport.RoleVanThu);

        var act = () => TaskSupport.EnsureCanCreateOrDeleteTask(plan, user);

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureCanCreateOrDeleteTask_ShouldThrowForbidden_ForNhanVien()
    {
        var plan = SubPlan(Guid.NewGuid());
        var user = new TestCurrentUser(plan.DepartmentId, PlanSupport.RoleNhanVien);

        var act = () => TaskSupport.EnsureCanCreateOrDeleteTask(plan, user);

        act.Should().Throw<ForbiddenException>();
    }

    [Fact]
    public void EnsureCanUpdateTaskProgress_ShouldAllowNhanVienSameDepartment()
    {
        var departmentId = Guid.NewGuid();
        var plan = SubPlan(departmentId);
        var task = new TaskEntity { PlanId = plan.Id };
        var user = new TestCurrentUser(departmentId, PlanSupport.RoleNhanVien);

        var act = () => TaskSupport.EnsureCanUpdateTaskProgress(plan, task, user);

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureCanUpdateTaskProgress_ShouldThrowForbidden_ForNhanVienOtherDepartment()
    {
        var plan = SubPlan(Guid.NewGuid());
        var task = new TaskEntity { PlanId = plan.Id };
        var user = new TestCurrentUser(Guid.NewGuid(), PlanSupport.RoleNhanVien);

        var act = () => TaskSupport.EnsureCanUpdateTaskProgress(plan, task, user);

        act.Should().Throw<ForbiddenException>();
    }

    [Fact]
    public void EnsureCanUpdateTaskFull_ShouldThrowForbidden_WhenTaskLocked()
    {
        var plan = MainPlan();
        var task = new TaskEntity { PlanId = plan.Id, IsLocked = true };
        var user = new TestCurrentUser(null, PlanSupport.RoleAdmin);

        var act = () => TaskSupport.EnsureCanUpdateTaskFull(plan, task, user);

        act.Should().Throw<ForbiddenException>()
            .Where(x => x.Code == "task_locked");
    }

    [Fact]
    public void EnsureCanCreateOrDeleteTask_ShouldThrowDomain_WhenPlanNotDraft()
    {
        var plan = MainPlan();
        plan.Status = WorkflowStatus.Pending;
        var user = new TestCurrentUser(null, PlanSupport.RoleAdmin);

        var act = () => TaskSupport.EnsureCanCreateOrDeleteTask(plan, user);

        act.Should().Throw<DomainException>()
            .Where(x => x.Code == "plan_not_editable");
    }

    [Fact]
    public void EnsureCanCreateOrDeleteTask_ShouldAllowReturnedPlan()
    {
        var plan = MainPlan();
        plan.Status = WorkflowStatus.Returned;
        var user = new TestCurrentUser(null, PlanSupport.RoleAdmin);

        var act = () => TaskSupport.EnsureCanCreateOrDeleteTask(plan, user);

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureHeaderNormalized_ShouldClearBusinessFields()
    {
        var task = new TaskEntity
        {
            IsHeader = true,
            Deadline = DateTime.UtcNow,
            AssigneeUserId = Guid.NewGuid(),
            BksMemberText = "BKS",
            KtnbLeaderText = "Leader",
            ProgressText = "progress",
            ReasonNotCompleted = "reason",
            WorkStatus = WorkStatus.Overdue
        };
        task.SupportingDepts.Add(new TaskSupportingDept { TaskId = task.Id, DepartmentId = Guid.NewGuid() });

        TaskSupport.EnsureHeaderNormalized(task);

        task.Deadline.Should().BeNull();
        task.AssigneeUserId.Should().BeNull();
        task.OwnerDepartmentId.Should().BeNull();
        task.BksMemberText.Should().BeNull();
        task.KtnbLeaderText.Should().BeNull();
        task.ProgressText.Should().BeNull();
        task.ReasonNotCompleted.Should().BeNull();
        task.WorkStatus.Should().Be(WorkStatus.NotStarted);
        task.SupportingDepts.Should().BeEmpty();
    }

    [Fact]
    public void EnsureProgressOnlyPayload_ShouldThrow_WhenTitleChanged()
    {
        var task = new TaskEntity
        {
            Id = Guid.NewGuid(),
            Title = "Old",
            WorkType = WorkType.General,
            WorkStatus = WorkStatus.NotStarted,
            DisplayOrder = 10
        };
        var values = new UpdateTaskValues(null, null, 10, false, "New", WorkType.General, WorkStatus.InProgress, null, null, null, null, null, null, null, "p", null, "normal", "medium", []);

        var act = () => TaskSupport.EnsureProgressOnlyPayload(task, values);

        act.Should().Throw<ForbiddenException>()
            .Where(x => x.Code == "forbidden_field_change");
    }

    [Fact]
    public void EnsureProgressOnlyPayload_ShouldThrow_WhenOwnerDepartmentChanged()
    {
        var ownerDepartmentId = Guid.NewGuid();
        var task = new TaskEntity
        {
            Id = Guid.NewGuid(),
            Title = "Task",
            WorkType = WorkType.General,
            WorkStatus = WorkStatus.NotStarted,
            DisplayOrder = 10,
            OwnerDepartmentId = ownerDepartmentId
        };
        var values = new UpdateTaskValues(null, null, 10, false, "Task", WorkType.General, WorkStatus.InProgress, null, null, null, Guid.NewGuid(), null, null, null, "p", null, "normal", "medium", []);

        var act = () => TaskSupport.EnsureProgressOnlyPayload(task, values);

        act.Should().Throw<ForbiddenException>()
            .Where(x => x.Code == "forbidden_field_change");
    }

    [Fact]
    public void ValidateDeadline_ShouldThrow_WhenBeforePlanCreatedDate()
    {
        var plan = MainPlan();
        plan.CreatedAt = new DateTime(2026, 5, 13, 0, 0, 0, DateTimeKind.Utc);

        var act = () => TaskSupport.ValidateDeadline(plan, new DateTime(2026, 5, 12));

        act.Should().Throw<DomainException>()
            .Where(x => x.Code == "deadline_before_plan");
    }

    private static Plan MainPlan() =>
        new()
        {
            Id = Guid.NewGuid(),
            Scope = PlanScope.Main,
            Status = WorkflowStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

    private static Plan SubPlan(Guid departmentId) =>
        new()
        {
            Id = Guid.NewGuid(),
            Scope = PlanScope.Sub,
            DepartmentId = departmentId,
            Status = WorkflowStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

    private sealed class TestCurrentUser : ICurrentUser
    {
        public TestCurrentUser(Guid? departmentId, params string[] roles)
        {
            DepartmentId = departmentId;
            Roles = roles;
        }

        public Guid? UserId { get; } = Guid.NewGuid();
        public string? Username => "tester";
        public Guid? DepartmentId { get; }
        public IReadOnlyList<string> Roles { get; }
        public bool IsAuthenticated => true;
    }
}
