using Bedrock.BuildingBlocks.Outbox.Models;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Outbox.Models;

public class OutboxEntryStatusTests : TestBase
{
    public OutboxEntryStatusTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Theory]
    [InlineData(OutboxEntryStatus.Pending, 1)]
    [InlineData(OutboxEntryStatus.Processing, 2)]
    [InlineData(OutboxEntryStatus.Sent, 3)]
    [InlineData(OutboxEntryStatus.Failed, 4)]
    [InlineData(OutboxEntryStatus.Dead, 5)]
    public void EnumValues_ShouldHaveExpectedUnderlyingValues(OutboxEntryStatus status, byte expected)
    {
        // Arrange
        LogArrange($"Checking enum value for {status}");

        // Act
        LogAct("Casting to underlying byte");
        var value = (byte)status;

        // Assert
        LogAssert($"Verifying {status} == {expected}");
        value.ShouldBe(expected);
    }

    [Fact]
    public void EnumCount_ShouldHaveExactlyFiveValues()
    {
        // Arrange
        LogArrange("Getting all OutboxEntryStatus values");

        // Act
        LogAct("Enumerating values");
        var values = Enum.GetValues<OutboxEntryStatus>();

        // Assert
        LogAssert("Verifying exactly 5 values exist");
        values.Length.ShouldBe(5);
    }

    [Fact]
    public void UnderlyingType_ShouldBeByte()
    {
        // Arrange
        LogArrange("Checking underlying type of OutboxEntryStatus");

        // Act
        LogAct("Getting underlying type");
        var underlyingType = Enum.GetUnderlyingType(typeof(OutboxEntryStatus));

        // Assert
        LogAssert("Verifying underlying type is byte");
        underlyingType.ShouldBe(typeof(byte));
    }
}
