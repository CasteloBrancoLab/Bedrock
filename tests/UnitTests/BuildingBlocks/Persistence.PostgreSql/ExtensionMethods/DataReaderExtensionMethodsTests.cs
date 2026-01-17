using Bedrock.BuildingBlocks.Persistence.PostgreSql.ExtensionMethods;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Persistence.PostgreSql.ExtensionMethods;

public class DataReaderExtensionMethodsTests : TestBase
{
    public DataReaderExtensionMethodsTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void GetValueOrDbNull_WithNonNullValue_ShouldReturnValue()
    {
        // Arrange
        LogArrange("Creating non-null value");
        object? value = "test";

        // Act
        LogAct("Calling GetValueOrDbNull");
        var result = value.GetValueOrDbNull();

        // Assert
        LogAssert("Verifying result is the original value");
        result.ShouldBe("test");
    }

    [Fact]
    public void GetValueOrDbNull_WithNullValue_ShouldReturnDbNull()
    {
        // Arrange
        LogArrange("Creating null value");
        object? value = null;

        // Act
        LogAct("Calling GetValueOrDbNull");
        var result = value.GetValueOrDbNull();

        // Assert
        LogAssert("Verifying result is DBNull.Value");
        result.ShouldBe(DBNull.Value);
    }

    [Fact]
    public void GetValueOrDbNull_WithIntValue_ShouldReturnValue()
    {
        // Arrange
        LogArrange("Creating int value");
        object? value = 42;

        // Act
        LogAct("Calling GetValueOrDbNull");
        var result = value.GetValueOrDbNull();

        // Assert
        LogAssert("Verifying result is the original value");
        result.ShouldBe(42);
    }

    [Fact]
    public void GetValueOrDbNull_WithGuidValue_ShouldReturnValue()
    {
        // Arrange
        LogArrange("Creating Guid value");
        var guid = Guid.NewGuid();
        object? value = guid;

        // Act
        LogAct("Calling GetValueOrDbNull");
        var result = value.GetValueOrDbNull();

        // Assert
        LogAssert("Verifying result is the original value");
        result.ShouldBe(guid);
    }

    [Fact]
    public void GetValueOrDbNull_WithEmptyString_ShouldReturnEmptyString()
    {
        // Arrange
        LogArrange("Creating empty string value");
        object? value = string.Empty;

        // Act
        LogAct("Calling GetValueOrDbNull");
        var result = value.GetValueOrDbNull();

        // Assert
        LogAssert("Verifying result is empty string (not DBNull)");
        result.ShouldBe(string.Empty);
    }

    [Fact]
    public void GetValueOrDbNull_WithDateTimeOffset_ShouldReturnValue()
    {
        // Arrange
        LogArrange("Creating DateTimeOffset value");
        var dateTime = DateTimeOffset.UtcNow;
        object? value = dateTime;

        // Act
        LogAct("Calling GetValueOrDbNull");
        var result = value.GetValueOrDbNull();

        // Assert
        LogAssert("Verifying result is the original value");
        result.ShouldBe(dateTime);
    }
}
