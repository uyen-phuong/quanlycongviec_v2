using FluentAssertions;
using KHCT.Application.Tasks;

namespace KHCT.Tests.Tasks;

public class TaskValidatorTests
{
    [Fact]
    public void CreateValidator_ShouldFail_WhenTitleMissing()
    {
        var validator = new CreateTaskCommandValidator();

        var result = validator.Validate(ValidCreate() with { Title = "" });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Title");
    }

    [Fact]
    public void CreateValidator_ShouldFail_WhenWorkTypeInvalid()
    {
        var validator = new CreateTaskCommandValidator();

        var result = validator.Validate(ValidCreate() with { WorkType = 99 });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "WorkType");
    }

    [Fact]
    public void UpdateValidator_ShouldFail_WhenDisplayOrderNegative()
    {
        var validator = new UpdateTaskCommandValidator();

        var result = validator.Validate(ValidUpdate() with { DisplayOrder = -1 });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "DisplayOrder");
    }

    [Fact]
    public void UpdateValidator_ShouldFail_WhenSupportingDepartmentsDuplicated()
    {
        var id = Guid.NewGuid();
        var validator = new UpdateTaskCommandValidator();

        var result = validator.Validate(ValidUpdate() with { SupportingDepartmentIds = [id, id] });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "SupportingDepartmentIds");
    }

    [Fact]
    public void UpdateValidator_ShouldFail_WhenOverduePastDeadlineMissingReason()
    {
        var validator = new UpdateTaskCommandValidator();

        var result = validator.Validate(ValidUpdate() with
        {
            WorkStatus = "overdue",
            Deadline = DateTime.UtcNow.AddDays(-1),
            ReasonNotCompleted = null
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "ReasonNotCompleted");
    }

    private static CreateTaskCommand ValidCreate() =>
        new(Guid.NewGuid(), null, "1", 10, false, "Task", 0, "not_started", null, null, null, null, null, null, null, null, null, "normal", "medium", []);

    private static UpdateTaskCommand ValidUpdate() =>
        new(Guid.NewGuid(), null, "1", 10, false, "Task", 0, "not_started", null, null, null, null, null, null, null, null, null, "normal", "medium", []);
}
