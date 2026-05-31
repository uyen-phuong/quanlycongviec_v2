using FluentAssertions;
using KHCT.Application.Common.Interfaces;
using KHCT.Application.Plans;
using KHCT.Application.Plans.Workflow;
using KHCT.Domain.Common;
using KHCT.Domain.Entities;
using KHCT.Domain.Enums;

namespace KHCT.Tests.Plans;

public class WorkflowSupportTests
{
    [Fact]
    public void EnsureEditable_ShouldAllowReturned()
    {
        var plan = new Plan { Status = WorkflowStatus.Returned };

        var act = () => PlanSupport.EnsureEditable(plan);

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureCanSubmit_ShouldAllowVanThu_ForReturnedMainPlan()
    {
        var plan = new Plan { Scope = PlanScope.Main, Status = WorkflowStatus.Returned };
        var currentUser = new TestCurrentUser(null, PlanSupport.RoleVanThu);

        var act = () => WorkflowSupport.EnsureCanSubmit(plan, currentUser);

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureCanApprove_ShouldMoveMainPending_ToApproved1_ForTruongKh()
    {
        var plan = new Plan { Scope = PlanScope.Main, Status = WorkflowStatus.Pending };
        var currentUser = new TestCurrentUser(null, PlanSupport.RoleTruongKh);

        var result = WorkflowSupport.EnsureCanApprove(plan, currentUser);

        result.Should().Be(WorkflowStatus.Approved1);
    }

    [Fact]
    public void EnsureCanApprove_ShouldMoveSubApproved2_ToApproved3_ForPhoTruongKtnb()
    {
        var plan = new Plan { Scope = PlanScope.Sub, Status = WorkflowStatus.Approved2, DepartmentId = Guid.NewGuid() };
        var currentUser = new TestCurrentUser(Guid.NewGuid(), PlanSupport.RolePhoTruongKtnb);

        var result = WorkflowSupport.EnsureCanApprove(plan, currentUser);

        result.Should().Be(WorkflowStatus.Approved3);
    }

    [Fact]
    public void EnsureCanApprove_ShouldThrowForbidden_WhenRoleStageMismatch()
    {
        var plan = new Plan { Scope = PlanScope.Main, Status = WorkflowStatus.Pending };
        var currentUser = new TestCurrentUser(null, PlanSupport.RoleTruongKtnb);

        var act = () => WorkflowSupport.EnsureCanApprove(plan, currentUser);

        act.Should().Throw<ForbiddenException>()
            .Where(x => x.Code == "forbidden_role");
    }

    [Fact]
    public void ResolveCommentRole_ShouldReturnController_ForTruongKh()
    {
        var currentUser = new TestCurrentUser(null, PlanSupport.RoleTruongKh);

        var role = WorkflowSupport.ResolveCommentRole(currentUser);

        role.Should().Be(CommentRole.Controller);
    }

    [Fact]
    public void EnsureCanResolveComment_ShouldAllowNhanVienSameDepartment_ForSubPlan()
    {
        var departmentId = Guid.NewGuid();
        var plan = new Plan { Scope = PlanScope.Sub, DepartmentId = departmentId };
        var currentUser = new TestCurrentUser(departmentId, PlanSupport.RoleNhanVien);

        var act = () => WorkflowSupport.EnsureCanResolveComment(plan, currentUser);

        act.Should().NotThrow();
    }

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
