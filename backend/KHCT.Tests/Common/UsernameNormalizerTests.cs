using FluentAssertions;
using KHCT.Application.Common.Security;

namespace KHCT.Tests.Common;

public class UsernameNormalizerTests
{
    [Fact]
    public void Normalize_ShouldTrim_AndLowercase()
    {
        var result = UsernameNormalizer.Normalize("  AdminX  ");

        result.Should().Be("adminx");
    }
}
