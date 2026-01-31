using Bedrock.BuildingBlocks.Serialization.Json.Schema.Models;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Serialization.Json.Schema;

public class SchemaValidationResultTests : TestBase
{
    public SchemaValidationResultTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Valid_ShouldReturnValidResult()
    {
        // Arrange & Act
        LogAct("Creating valid result");
        var result = SchemaValidationResult.Valid();

        // Assert
        LogAssert("Verifying valid result");
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
        LogInfo("Valid result created successfully");
    }

    [Fact]
    public void Invalid_WithErrors_ShouldReturnInvalidResult()
    {
        // Arrange
        LogArrange("Creating errors list");
        var errors = new List<SchemaValidationError>
        {
            new("/name", "Required property missing"),
            new("/age", "Value must be a number"),
        };

        // Act
        LogAct("Creating invalid result");
        var result = SchemaValidationResult.Invalid(errors);

        // Assert
        LogAssert("Verifying invalid result");
        result.IsValid.ShouldBeFalse();
        result.Errors.Count.ShouldBe(2);
        result.Errors[0].Path.ShouldBe("/name");
        result.Errors[1].Message.ShouldBe("Value must be a number");
        LogInfo("Invalid result with {0} errors", result.Errors.Count);
    }

    [Fact]
    public void Invalid_WithEmptyErrors_ShouldReturnInvalidResult()
    {
        // Arrange
        LogArrange("Creating empty errors list");
        var errors = new List<SchemaValidationError>();

        // Act
        LogAct("Creating invalid result with empty errors");
        var result = SchemaValidationResult.Invalid(errors);

        // Assert
        LogAssert("Verifying invalid result with empty errors");
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldBeEmpty();
        LogInfo("Invalid result with empty errors created");
    }

    [Fact]
    public void Invalid_WithNullErrors_ShouldThrow()
    {
        // Arrange
        LogArrange("Preparing null errors");

        // Act & Assert
        LogAct("Creating invalid result with null errors");
        Should.Throw<ArgumentNullException>(() => SchemaValidationResult.Invalid(null!));
        LogAssert("ArgumentNullException thrown as expected");
    }
}
