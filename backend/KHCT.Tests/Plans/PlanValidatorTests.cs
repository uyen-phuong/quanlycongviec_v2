using FluentAssertions;
using KHCT.Application.Plans.Main;

namespace KHCT.Tests.Plans;

public class PlanValidatorTests
{
    [Fact]
    public void CreateMainPlanValidator_ShouldFail_WhenMonthInvalid()
    {
        var validator = new CreateMainPlanCommandValidator();

        var result = validator.Validate(new CreateMainPlanCommand("Plan 2026", 2026, 13, "monthly", null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Month");
    }
}
