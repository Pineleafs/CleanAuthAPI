using CleanAuthApi.Infrastructure.Security;
using Xunit;

namespace CleanAuthApi.Tests;

public class SecurityUnitTests
{
    private readonly PasswordHasher _passwordHasher = new();

    [Fact]
    public void PasswordHasher_ShouldHashAndVerifyPassword_Correctly()
    {
        var rawPassword = "StrongPassword123!@#";

        var hash = _passwordHasher.HashPassword(rawPassword);

        Assert.NotEqual(rawPassword, hash);
        Assert.Contains(".", hash);
        Assert.True(_passwordHasher.VerifyPassword(rawPassword, hash));
        Assert.False(_passwordHasher.VerifyPassword("WrongPassword123!", hash));
    }

    [Fact]
    public void PasswordHasher_ShouldProduceUniqueSalts_ForSamePassword()
    {
        var rawPassword = "SamePassword123!";

        var hash1 = _passwordHasher.HashPassword(rawPassword);
        var hash2 = _passwordHasher.HashPassword(rawPassword);

        // Aynı şifre için her seferinde farklı salt üretilmeli (Rainbow table koruması)
        Assert.NotEqual(hash1, hash2);
        Assert.True(_passwordHasher.VerifyPassword(rawPassword, hash1));
        Assert.True(_passwordHasher.VerifyPassword(rawPassword, hash2));
    }
}
