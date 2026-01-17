using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Models;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Persistence.PostgreSql.Mappers.Models;

public class RelationalOperatorTests : TestBase
{
    public RelationalOperatorTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Equal_ShouldReturnCorrectSqlOperator()
    {
        // Arrange
        LogArrange("Getting Equal operator");

        // Act
        LogAct("Converting to string");
        var result = RelationalOperator.Equal.ToString();

        // Assert
        LogAssert("Verifying result is '='");
        result.ShouldBe("=");
    }

    [Fact]
    public void NotEqual_ShouldReturnCorrectSqlOperator()
    {
        // Arrange
        LogArrange("Getting NotEqual operator");

        // Act
        LogAct("Converting to string");
        var result = RelationalOperator.NotEqual.ToString();

        // Assert
        LogAssert("Verifying result is '<>'");
        result.ShouldBe("<>");
    }

    [Fact]
    public void GreaterThan_ShouldReturnCorrectSqlOperator()
    {
        // Arrange
        LogArrange("Getting GreaterThan operator");

        // Act
        LogAct("Converting to string");
        var result = RelationalOperator.GreaterThan.ToString();

        // Assert
        LogAssert("Verifying result is '>'");
        result.ShouldBe(">");
    }

    [Fact]
    public void GreaterThanOrEqual_ShouldReturnCorrectSqlOperator()
    {
        // Arrange
        LogArrange("Getting GreaterThanOrEqual operator");

        // Act
        LogAct("Converting to string");
        var result = RelationalOperator.GreaterThanOrEqual.ToString();

        // Assert
        LogAssert("Verifying result is '>='");
        result.ShouldBe(">=");
    }

    [Fact]
    public void LessThan_ShouldReturnCorrectSqlOperator()
    {
        // Arrange
        LogArrange("Getting LessThan operator");

        // Act
        LogAct("Converting to string");
        var result = RelationalOperator.LessThan.ToString();

        // Assert
        LogAssert("Verifying result is '<'");
        result.ShouldBe("<");
    }

    [Fact]
    public void LessThanOrEqual_ShouldReturnCorrectSqlOperator()
    {
        // Arrange
        LogArrange("Getting LessThanOrEqual operator");

        // Act
        LogAct("Converting to string");
        var result = RelationalOperator.LessThanOrEqual.ToString();

        // Assert
        LogAssert("Verifying result is '<='");
        result.ShouldBe("<=");
    }

    [Fact]
    public void Like_ShouldReturnCorrectSqlOperator()
    {
        // Arrange
        LogArrange("Getting Like operator");

        // Act
        LogAct("Converting to string");
        var result = RelationalOperator.Like.ToString();

        // Assert
        LogAssert("Verifying result is 'LIKE'");
        result.ShouldBe("LIKE");
    }

    [Fact]
    public void ILike_ShouldReturnCorrectSqlOperator()
    {
        // Arrange
        LogArrange("Getting ILike operator");

        // Act
        LogAct("Converting to string");
        var result = RelationalOperator.ILike.ToString();

        // Assert
        LogAssert("Verifying result is 'ILIKE'");
        result.ShouldBe("ILIKE");
    }

    [Fact]
    public void IsNull_ShouldReturnCorrectSqlOperator()
    {
        // Arrange
        LogArrange("Getting IsNull operator");

        // Act
        LogAct("Converting to string");
        var result = RelationalOperator.IsNull.ToString();

        // Assert
        LogAssert("Verifying result is 'IS NULL'");
        result.ShouldBe("IS NULL");
    }

    [Fact]
    public void IsNotNull_ShouldReturnCorrectSqlOperator()
    {
        // Arrange
        LogArrange("Getting IsNotNull operator");

        // Act
        LogAct("Converting to string");
        var result = RelationalOperator.IsNotNull.ToString();

        // Assert
        LogAssert("Verifying result is 'IS NOT NULL'");
        result.ShouldBe("IS NOT NULL");
    }
}
