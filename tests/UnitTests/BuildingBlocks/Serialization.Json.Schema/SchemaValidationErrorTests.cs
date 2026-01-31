using Bedrock.BuildingBlocks.Serialization.Json.Schema.Models;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Serialization.Json.Schema;

public class SchemaValidationErrorTests : TestBase
{
    public SchemaValidationErrorTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Constructor_WithValidArguments_ShouldSetProperties()
    {
        // Arrange
        LogArrange("Creating error with valid arguments");
        var path = "/name";
        var message = "Required property missing";

        // Act
        LogAct("Creating SchemaValidationError");
        var error = new SchemaValidationError(path, message);

        // Assert
        LogAssert("Verifying properties");
        error.Path.ShouldBe(path);
        error.Message.ShouldBe(message);
        LogInfo("Error created: Path={0}, Message={1}", error.Path, error.Message);
    }

    [Fact]
    public void Constructor_WithNullPath_ShouldThrow()
    {
        // Arrange
        LogArrange("Preparing null path");

        // Act & Assert
        LogAct("Creating SchemaValidationError with null path");
        Should.Throw<ArgumentNullException>(() => new SchemaValidationError(null!, "message"));
        LogAssert("ArgumentNullException thrown as expected");
    }

    [Fact]
    public void Constructor_WithNullMessage_ShouldThrow()
    {
        // Arrange
        LogArrange("Preparing null message");

        // Act & Assert
        LogAct("Creating SchemaValidationError with null message");
        Should.Throw<ArgumentNullException>(() => new SchemaValidationError("/path", null!));
        LogAssert("ArgumentNullException thrown as expected");
    }

    [Fact]
    public void Constructor_WithEmptyPath_ShouldSetProperties()
    {
        // Arrange
        LogArrange("Creating error with empty path");

        // Act
        LogAct("Creating SchemaValidationError with empty path");
        var error = new SchemaValidationError(string.Empty, "message");

        // Assert
        LogAssert("Verifying empty path");
        error.Path.ShouldBe(string.Empty);
        error.Message.ShouldBe("message");
        LogInfo("Empty path accepted");
    }
}
