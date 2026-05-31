using FluentAssertions;
using KHCT.Application.Attachments;
using KHCT.Application.Common.Interfaces;
using KHCT.Application.Plans;
using KHCT.Domain.Common;
using KHCT.Domain.Entities;
using KHCT.Domain.Enums;

namespace KHCT.Tests.Attachments;

public class AttachmentSupportTests
{
    [Fact]
    public void ValidateAndGetExtension_ShouldAllowWhitelistedExtension()
    {
        var extension = AttachmentSupport.ValidateAndGetExtension("report.xlsx");

        extension.Should().Be(".xlsx");
    }

    [Fact]
    public void ValidateAndGetExtension_ShouldRejectUnsupportedExtension()
    {
        var act = () => AttachmentSupport.ValidateAndGetExtension("payload.exe");

        act.Should().Throw<DomainException>()
            .Where(x => x.Code == "invalid_file_format");
    }

    [Fact]
    public void ValidateSignature_ShouldRejectInvalidPdfSignature()
    {
        var act = () => AttachmentSupport.ValidateSignature(".pdf", "NOTPDF"u8.ToArray());

        act.Should().Throw<DomainException>()
            .Where(x => x.Code == "invalid_file_signature");
    }

    [Fact]
    public void EnsureCanMutateTaskAttachment_ShouldAllowNhanVienSameDepartment_OnLockedSubTask()
    {
        var departmentId = Guid.NewGuid();
        var plan = new Plan
        {
            Id = Guid.NewGuid(),
            Scope = PlanScope.Sub,
            DepartmentId = departmentId,
            Status = WorkflowStatus.Draft
        };
        var user = new TestCurrentUser(departmentId, PlanSupport.RoleNhanVien);

        var act = () => AttachmentSupport.EnsureCanMutateTaskAttachment(plan, user);

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureCanMutateTaskAttachment_ShouldThrowForbidden_ForNhanVienOtherDepartment()
    {
        var plan = new Plan
        {
            Id = Guid.NewGuid(),
            Scope = PlanScope.Sub,
            DepartmentId = Guid.NewGuid(),
            Status = WorkflowStatus.Draft
        };
        var user = new TestCurrentUser(Guid.NewGuid(), PlanSupport.RoleNhanVien);

        var act = () => AttachmentSupport.EnsureCanMutateTaskAttachment(plan, user);

        act.Should().Throw<ForbiddenException>()
            .Where(x => x.Code == "forbidden_role");
    }

    [Fact]
    public void EnsureCanMutatePlanAttachment_ShouldRejectNhanVien_ForSubPlan()
    {
        var departmentId = Guid.NewGuid();
        var plan = new Plan
        {
            Id = Guid.NewGuid(),
            Scope = PlanScope.Sub,
            DepartmentId = departmentId,
            Status = WorkflowStatus.Draft
        };
        var user = new TestCurrentUser(departmentId, PlanSupport.RoleNhanVien);

        var act = () => AttachmentSupport.EnsureCanMutatePlanAttachment(plan, user);

        act.Should().Throw<ForbiddenException>()
            .Where(x => x.Code == "forbidden_role");
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
