using FluentAssertions;

namespace KHCT.Tests;

public class SmokeTests
{
    [Fact]
    public void True_Should_Be_True()
    {
        true.Should().BeTrue();
    }
}
