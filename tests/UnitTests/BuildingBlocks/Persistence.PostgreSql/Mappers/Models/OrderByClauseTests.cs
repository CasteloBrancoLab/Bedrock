using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Models;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Persistence.PostgreSql.Mappers.Models;

public class OrderByClauseTests : TestBase
{
    public OrderByClauseTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        LogArrange("Creating OrderByClause with test value");
        var clause = new OrderByClause("column ASC");

        // Act
        LogAct("Converting to string");
        var result = clause.ToString();

        // Assert
        LogAssert("Verifying result matches value");
        result.ShouldBe("column ASC");
    }

    [Fact]
    public void PlusOperator_ShouldCombineClauses()
    {
        // Arrange
        LogArrange("Creating two OrderByClause instances");
        var left = new OrderByClause("column1 ASC");
        var right = new OrderByClause("column2 DESC");

        // Act
        LogAct("Combining with + operator");
        var result = left + right;

        // Assert
        LogAssert("Verifying result is combined with comma");
        result.ToString().ShouldBe("column1 ASC, column2 DESC");
    }

    [Fact]
    public void MultiplePlusOperators_ShouldChainCorrectly()
    {
        // Arrange
        LogArrange("Creating three OrderByClause instances");
        var clause1 = new OrderByClause("a ASC");
        var clause2 = new OrderByClause("b DESC");
        var clause3 = new OrderByClause("c ASC");

        // Act
        LogAct("Chaining with + operators");
        var result = clause1 + clause2 + clause3;

        // Assert
        LogAssert("Verifying result is chained correctly");
        result.ToString().ShouldBe("a ASC, b DESC, c ASC");
    }
}
