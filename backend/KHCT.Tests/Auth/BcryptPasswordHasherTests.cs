using FluentAssertions;
using KHCT.Infrastructure.Auth;

namespace KHCT.Tests.Auth;

public class BcryptPasswordHasherTests
{
    [Fact]
    public void Verify_returns_true_for_correct_password()
    {
        var hasher = new BcryptPasswordHasher();
        var hash = hasher.Hash("Admin@123");
        hasher.Verify("Admin@123", hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_returns_false_for_wrong_password()
    {
        var hasher = new BcryptPasswordHasher();
        var hash = hasher.Hash("Admin@123");
        hasher.Verify("wrong", hash).Should().BeFalse();
    }

    [Fact]
    public void Verify_returns_false_for_malformed_hash()
    {
        var hasher = new BcryptPasswordHasher();
        hasher.Verify("anything", "not-a-bcrypt-hash").Should().BeFalse();
    }
}
