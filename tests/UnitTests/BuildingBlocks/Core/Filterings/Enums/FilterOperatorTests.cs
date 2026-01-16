using Bedrock.BuildingBlocks.Core.Filterings.Enums;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Core.Filterings.Enums;

public class FilterOperatorTests : TestBase
{
    public FilterOperatorTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Theory]
    [InlineData(FilterOperator.Equals, 1)]
    [InlineData(FilterOperator.NotEquals, 2)]
    [InlineData(FilterOperator.Contains, 3)]
    [InlineData(FilterOperator.StartsWith, 4)]
    [InlineData(FilterOperator.EndsWith, 5)]
    [InlineData(FilterOperator.GreaterThan, 6)]
    [InlineData(FilterOperator.GreaterThanOrEquals, 7)]
    [InlineData(FilterOperator.LessThan, 8)]
    [InlineData(FilterOperator.LessThanOrEquals, 9)]
    [InlineData(FilterOperator.Between, 10)]
    [InlineData(FilterOperator.In, 11)]
    [InlineData(FilterOperator.NotIn, 12)]
    public void FilterOperator_ShouldHaveCorrectValue(FilterOperator op, int expectedValue)
    {
        // Arrange
        LogArrange("Getting enum value");
        LogInfo("Operator: {0}", op);

        // Act
        LogAct("Checking enum value");
        var value = (int)op;

        // Assert
        LogAssert("Verifying value");
        value.ShouldBe(expectedValue);
        LogInfo("{0} = {1}", op, value);
    }

    [Fact]
    public void FilterOperator_ShouldHaveExactly12Values()
    {
        // Arrange
        LogArrange("Getting all enum values");

        // Act
        LogAct("Counting enum values");
        var values = Enum.GetValues<FilterOperator>();

        // Assert
        LogAssert("Verifying count is 12");
        values.Length.ShouldBe(12);
        LogInfo("FilterOperator has {0} values", values.Length);
    }

    [Theory]
    [InlineData(FilterOperator.Equals, "Equals")]
    [InlineData(FilterOperator.NotEquals, "NotEquals")]
    [InlineData(FilterOperator.Contains, "Contains")]
    [InlineData(FilterOperator.StartsWith, "StartsWith")]
    [InlineData(FilterOperator.EndsWith, "EndsWith")]
    [InlineData(FilterOperator.GreaterThan, "GreaterThan")]
    [InlineData(FilterOperator.GreaterThanOrEquals, "GreaterThanOrEquals")]
    [InlineData(FilterOperator.LessThan, "LessThan")]
    [InlineData(FilterOperator.LessThanOrEquals, "LessThanOrEquals")]
    [InlineData(FilterOperator.Between, "Between")]
    [InlineData(FilterOperator.In, "In")]
    [InlineData(FilterOperator.NotIn, "NotIn")]
    public void FilterOperator_ToString_ShouldReturnName(FilterOperator op, string expectedName)
    {
        // Arrange
        LogArrange("Getting ToString");
        LogInfo("Operator: {0}", op);

        // Act
        LogAct("Calling ToString");
        var name = op.ToString();

        // Assert
        LogAssert("Verifying name");
        name.ShouldBe(expectedName);
        LogInfo("{0}.ToString() = {1}", op, name);
    }

    [Theory]
    [InlineData("Equals", FilterOperator.Equals)]
    [InlineData("NotEquals", FilterOperator.NotEquals)]
    [InlineData("Contains", FilterOperator.Contains)]
    [InlineData("StartsWith", FilterOperator.StartsWith)]
    [InlineData("EndsWith", FilterOperator.EndsWith)]
    [InlineData("GreaterThan", FilterOperator.GreaterThan)]
    [InlineData("GreaterThanOrEquals", FilterOperator.GreaterThanOrEquals)]
    [InlineData("LessThan", FilterOperator.LessThan)]
    [InlineData("LessThanOrEquals", FilterOperator.LessThanOrEquals)]
    [InlineData("Between", FilterOperator.Between)]
    [InlineData("In", FilterOperator.In)]
    [InlineData("NotIn", FilterOperator.NotIn)]
    public void FilterOperator_Parse_ShouldWork(string name, FilterOperator expected)
    {
        // Arrange
        LogArrange("Parsing enum");
        LogInfo("Name: {0}", name);

        // Act
        LogAct("Parsing enum");
        var parsed = Enum.Parse<FilterOperator>(name);

        // Assert
        LogAssert("Verifying parsed value");
        parsed.ShouldBe(expected);
        LogInfo("Parsed {0} = {1}", name, parsed);
    }
}
