using Bedrock.BuildingBlocks.Security.Passwords;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Security.Passwords;

public class PasswordHashResultTests : TestBase
{
    public PasswordHashResultTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void PasswordHashResult_ShouldStoreHashAndPepperVersion()
    {
        // Arrange
        LogArrange("Creating hash result");
        byte[] hash = [1, 2, 3, 4, 5];

        // Act
        LogAct("Creating PasswordHashResult");
        var result = new PasswordHashResult(hash, 1);

        // Assert
        LogAssert("Verifying properties");
        result.Hash.ShouldBe(hash);
        result.PepperVersion.ShouldBe(1);
    }

    [Fact]
    public void PasswordVerificationResult_ShouldStoreIsValidAndNeedsRehash()
    {
        // Arrange & Act
        LogArrange("Creating verification result");
        LogAct("Creating PasswordVerificationResult");
        var result = new PasswordVerificationResult(true, false);

        // Assert
        LogAssert("Verifying properties");
        result.IsValid.ShouldBeTrue();
        result.NeedsRehash.ShouldBeFalse();
    }

    [Fact]
    public void PasswordVerificationResult_WithNeedsRehash_ShouldStoreCorrectly()
    {
        // Arrange & Act
        LogArrange("Creating verification result with rehash needed");
        LogAct("Creating PasswordVerificationResult");
        var result = new PasswordVerificationResult(true, true);

        // Assert
        LogAssert("Verifying both flags are true");
        result.IsValid.ShouldBeTrue();
        result.NeedsRehash.ShouldBeTrue();
    }

    [Fact]
    public void PasswordVerificationResult_Invalid_ShouldStoreCorrectly()
    {
        // Arrange & Act
        LogArrange("Creating invalid verification result");
        LogAct("Creating PasswordVerificationResult");
        var result = new PasswordVerificationResult(false, false);

        // Assert
        LogAssert("Verifying both flags are false");
        result.IsValid.ShouldBeFalse();
        result.NeedsRehash.ShouldBeFalse();
    }
}
