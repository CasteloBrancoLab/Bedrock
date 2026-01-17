using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Models;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Persistence.PostgreSql.Mappers.Models;

public class WhereClauseTests : TestBase
{
    public WhereClauseTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        LogArrange("Creating WhereClause with test value");
        var clause = new WhereClause("column = @param");

        // Act
        LogAct("Converting to string");
        var result = clause.ToString();

        // Assert
        LogAssert("Verifying result matches value");
        result.ShouldBe("column = @param");
    }

    [Fact]
    public void AndOperator_ShouldCombineClauses()
    {
        // Arrange
        LogArrange("Creating two WhereClause instances");
        var left = new WhereClause("column1 = @param1");
        var right = new WhereClause("column2 = @param2");

        // Act
        LogAct("Combining with AND operator");
        var result = left & right;

        // Assert
        LogAssert("Verifying result is combined with AND");
        result.ToString().ShouldBe("column1 = @param1 AND column2 = @param2");
    }

    [Fact]
    public void OrOperator_ShouldCombineClausesWithParentheses()
    {
        // Arrange
        LogArrange("Creating two WhereClause instances");
        var left = new WhereClause("column1 = @param1");
        var right = new WhereClause("column2 = @param2");

        // Act
        LogAct("Combining with OR operator");
        var result = left | right;

        // Assert
        LogAssert("Verifying result is combined with OR and parentheses");
        result.ToString().ShouldBe("(column1 = @param1 OR column2 = @param2)");
    }

    [Fact]
    public void MultipleAndOperators_ShouldChainCorrectly()
    {
        // Arrange
        LogArrange("Creating three WhereClause instances");
        var clause1 = new WhereClause("a = @a");
        var clause2 = new WhereClause("b = @b");
        var clause3 = new WhereClause("c = @c");

        // Act
        LogAct("Chaining with AND operators");
        var result = clause1 & clause2 & clause3;

        // Assert
        LogAssert("Verifying result is chained correctly");
        result.ToString().ShouldBe("a = @a AND b = @b AND c = @c");
    }

    [Fact]
    public void MixedOperators_ShouldCombineCorrectly()
    {
        // Arrange
        LogArrange("Creating WhereClause instances for complex expression");
        var clause1 = new WhereClause("a = @a");
        var clause2 = new WhereClause("b = @b");
        var clause3 = new WhereClause("c = @c");

        // Act
        LogAct("Combining with mixed AND and OR operators");
        var result = clause1 & (clause2 | clause3);

        // Assert
        LogAssert("Verifying result has correct structure");
        result.ToString().ShouldBe("a = @a AND (b = @b OR c = @c)");
    }
}
