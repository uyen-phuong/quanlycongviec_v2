using FluentAssertions;
using KHCT.Application.Admin.Users;

namespace KHCT.Tests.Admin;

public class CreateUserCommandValidatorTests
{
    private readonly CreateUserCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldFail_WhenPasswordTooShort()
    {
        var command = new CreateUserCommand("user01", "short", "User 01", null, null, Guid.NewGuid(), true);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Password");
    }

    [Fact]
    public void Validate_ShouldFail_WhenEmailInvalid()
    {
        var command = new CreateUserCommand("user01", "Password123", "User 01", "bad-email", null, Guid.NewGuid(), true);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Email");
    }
}
