using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.Users.Enums;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.Users.Enums;

public class UserStatusTests : TestBase
{
    public UserStatusTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Active_ShouldHaveValue1()
    {
        // Arrange
        LogArrange("Getting Active enum value");

        // Act
        LogAct("Casting Active to int");
        int value = (int)UserStatus.Active;

        // Assert
        LogAssert("Verifying Active equals 1");
        value.ShouldBe(1);
    }

    [Fact]
    public void Suspended_ShouldHaveValue2()
    {
        // Arrange
        LogArrange("Getting Suspended enum value");

        // Act
        LogAct("Casting Suspended to int");
        int value = (int)UserStatus.Suspended;

        // Assert
        LogAssert("Verifying Suspended equals 2");
        value.ShouldBe(2);
    }

    [Fact]
    public void Blocked_ShouldHaveValue3()
    {
        // Arrange
        LogArrange("Getting Blocked enum value");

        // Act
        LogAct("Casting Blocked to int");
        int value = (int)UserStatus.Blocked;

        // Assert
        LogAssert("Verifying Blocked equals 3");
        value.ShouldBe(3);
    }

    [Fact]
    public void UserStatus_ShouldHaveExactlyThreeValues()
    {
        // Arrange
        LogArrange("Getting all UserStatus values");

        // Act
        LogAct("Getting enum values");
        var values = Enum.GetValues<UserStatus>();

        // Assert
        LogAssert("Verifying exactly 3 values exist");
        values.Length.ShouldBe(3);
    }

    [Theory]
    [InlineData(UserStatus.Active, "Active")]
    [InlineData(UserStatus.Suspended, "Suspended")]
    [InlineData(UserStatus.Blocked, "Blocked")]
    public void ToString_ShouldReturnCorrectName(UserStatus status, string expectedName)
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
