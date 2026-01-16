using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;
using SortDirection = Bedrock.BuildingBlocks.Core.Sortings.Enums.SortDirection;

namespace Bedrock.UnitTests.BuildingBlocks.Core.Sortings.Enums;

public class SortDirectionTests : TestBase
{
    public SortDirectionTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Ascending_ShouldHaveValue1()
    {
        // Arrange
        LogArrange("Getting Ascending value");

        // Act
        LogAct("Checking enum value");
        var value = (int)SortDirection.Ascending;

        // Assert
        LogAssert("Verifying value is 1");
        value.ShouldBe(1);
        LogInfo("Ascending = {0}", value);
    }

    [Fact]
    public void Descending_ShouldHaveValue2()
    {
        // Arrange
        LogArrange("Getting Descending value");

        // Act
        LogAct("Checking enum value");
        var value = (int)SortDirection.Descending;

        // Assert
        LogAssert("Verifying value is 2");
        value.ShouldBe(2);
        LogInfo("Descending = {0}", value);
    }

    [Fact]
    public void SortDirection_ShouldHaveExactlyTwoValues()
    {
        // Arrange
        LogArrange("Getting all enum values");

        // Act
        LogAct("Counting enum values");
        var values = Enum.GetValues<SortDirection>();

        // Assert
        LogAssert("Verifying count is 2");
        values.Length.ShouldBe(2);
        LogInfo("SortDirection has {0} values", values.Length);
    }

    [Fact]
    public void SortDirection_ToString_ShouldReturnName()
    {
        // Arrange
        LogArrange("Creating SortDirection values");

        // Act & Assert
        LogAct("Checking ToString");
        SortDirection.Ascending.ToString().ShouldBe("Ascending");
        SortDirection.Descending.ToString().ShouldBe("Descending");
        LogAssert("ToString returns correct names");
    }

    [Theory]
    [InlineData(SortDirection.Ascending, "Ascending")]
    [InlineData(SortDirection.Descending, "Descending")]
    public void SortDirection_Parse_ShouldWork(SortDirection expected, string name)
    {
        // Arrange
        LogArrange("Parsing enum");
        LogInfo("Name: {0}", name);

        // Act
        LogAct("Parsing enum");
        var parsed = Enum.Parse<SortDirection>(name);

        // Assert
        LogAssert("Verifying parsed value");
        parsed.ShouldBe(expected);
        LogInfo("Parsed {0} = {1}", name, parsed);
    }
}
