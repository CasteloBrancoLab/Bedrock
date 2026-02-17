using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.RefreshTokens.Enums;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.RefreshTokens.Enums;

public class RefreshTokenStatusTests : TestBase
{
    public RefreshTokenStatusTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Active_ShouldHaveValue1()
    {
        // Arrange
        LogArrange("Getting Active enum value");

        // Act
        LogAct("Casting Active to int");
        int value = (int)RefreshTokenStatus.Active;

        // Assert
        LogAssert("Verifying Active equals 1");
        value.ShouldBe(1);
    }

    [Fact]
    public void Used_ShouldHaveValue2()
    {
        // Arrange
        LogArrange("Getting Used enum value");

        // Act
        LogAct("Casting Used to int");
        int value = (int)RefreshTokenStatus.Used;

        // Assert
        LogAssert("Verifying Used equals 2");
        value.ShouldBe(2);
    }

    [Fact]
    public void Revoked_ShouldHaveValue3()
    {
        // Arrange
        LogArrange("Getting Revoked enum value");

        // Act
        LogAct("Casting Revoked to int");
        int value = (int)RefreshTokenStatus.Revoked;

        // Assert
        LogAssert("Verifying Revoked equals 3");
        value.ShouldBe(3);
    }

    [Fact]
    public void RefreshTokenStatus_ShouldHaveExactlyThreeValues()
    {
        // Arrange
        LogArrange("Getting all RefreshTokenStatus values");

        // Act
        LogAct("Getting enum values");
        var values = Enum.GetValues<RefreshTokenStatus>();

        // Assert
        LogAssert("Verifying exactly 3 values exist");
        values.Length.ShouldBe(3);
    }

    [Theory]
    [InlineData(RefreshTokenStatus.Active, "Active")]
    [InlineData(RefreshTokenStatus.Used, "Used")]
    [InlineData(RefreshTokenStatus.Revoked, "Revoked")]
    public void ToString_ShouldReturnCorrectName(RefreshTokenStatus status, string expectedName)
    {
        // Arrange
        LogArrange($"Preparing status: {status}");

        // Act
        LogAct("Converting to string");
        string name = status.ToString();

        // Assert
        LogAssert($"Verifying name is '{expectedName}'");
        name.ShouldBe(expectedName);
    }
}
