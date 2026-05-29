using FluentAssertions;
using KHCT.Application.Common.Interfaces;
using KHCT.Application.Plans;
using KHCT.Domain.Common;
using KHCT.Domain.Entities;
using KHCT.Domain.Enums;

namespace KHCT.Tests.Plans;

public class PlanSupportTests
{
    [Fact]
    public void ApplySubReadScope_ShouldFilterByDepartment_ForNhanVien()
    {
        var ownDepartmentId = Guid.NewGuid();
        var otherDepartmentId = Guid.NewGuid();
        var plans = new[]
        {
            new Plan { Id = Guid.NewGuid(), Scope = PlanScope.Sub, DepartmentId = ownDepartmentId },
            new Plan { Id = Guid.NewGuid(), Scope = PlanScope.Sub, DepartmentId = otherDepartmentId }
        }.AsQueryable();

        var result = PlanSupport.ApplySubReadScope(
            plans,
            new TestCurrentUser(ownDepartmentId, PlanSupport.RoleNhanVien)).ToList();

        result.Should().ContainSingle();
        result[0].DepartmentId.Should().Be(ownDepartmentId);
    }

    [Fact]
    public void ApplySubReadScope_ShouldThrowForbidden_ForUnknownRole()
    {
        var plans = Array.Empty<Plan>().AsQueryable();
        var currentUser = new TestCurrentUser(Guid.NewGuid(), "TRUONG_NHOM");

        var act = () => PlanSupport.ApplySubReadScope(plans, currentUser).ToList();

        act.Should().Throw<ForbiddenException>()
            .Where(x => x.Code == "forbidden_role");
    }

    [Fact]
    public void EnsureCanMutateSubDepartment_ShouldThrow_WhenTruongPhongTargetsOtherDepartment()
    {
        var currentUser = new TestCurrentUser(Guid.NewGuid(), PlanSupport.RoleTruongPhong);

        var act = () => PlanSupport.EnsureCanMutateSubDepartment(currentUser, Guid.NewGuid());

        act.Should().Throw<DomainException>()
            .Where(x => x.Code == "forbidden_department");
    }

    [Fact]
    public void EnsureCanMutateSubDepartment_ShouldAllowPhoTruongKtnbCrossDepartment()
    {
        var currentUser = new TestCurrentUser(Guid.NewGuid(), PlanSupport.RolePhoTruongKtnb);

        var act = () => PlanSupport.EnsureCanMutateSubDepartment(currentUser, Guid.NewGuid());

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
