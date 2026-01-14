using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Core.ExecutionContexts.Models.Enums;

public class MessageTypeTests : TestBase
{
    public MessageTypeTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void MessageType_ShouldHaveCorrectValues()
    {
        // Arrange
        LogArrange("Listing all MessageType enum values");

        // Act & Assert
        LogAct("Verifying each enum value");
        ((int)MessageType.Trace).ShouldBe(0);
        ((int)MessageType.Debug).ShouldBe(1);
        ((int)MessageType.Information).ShouldBe(2);
        ((int)MessageType.Warning).ShouldBe(3);
        ((int)MessageType.Error).ShouldBe(4);
        ((int)MessageType.Critical).ShouldBe(5);
        ((int)MessageType.None).ShouldBe(6);
        ((int)MessageType.Success).ShouldBe(7);

        LogAssert("All MessageType values are correct");
    }

    [Theory]
    [InlineData(MessageType.Trace, MessageType.Debug, -1)]
    [InlineData(MessageType.Debug, MessageType.Trace, 1)]
    [InlineData(MessageType.Information, MessageType.Information, 0)]
    [InlineData(MessageType.Warning, MessageType.Error, -1)]
    [InlineData(MessageType.Error, MessageType.Warning, 1)]
    [InlineData(MessageType.Critical, MessageType.None, -1)]
    [InlineData(MessageType.None, MessageType.Success, -1)]
    [InlineData(MessageType.Trace, MessageType.Success, -1)]
    public void MessageType_CompareValues_ShouldReturnExpectedResult(
        MessageType left,
        MessageType right,
        int expectedSign)
    {
        // Arrange
        LogArrange($"Comparing {left} with {right}");

        // Act
        LogAct("Getting comparison result");
        var result = left.CompareTo(right);

        // Assert
        LogAssert("Verifying comparison result sign");
        Math.Sign(result).ShouldBe(expectedSign);
        LogInfo("Comparison {0}.CompareTo({1}) = {2}", left, right, result);
    }

    [Theory]
    [InlineData(MessageType.Trace)]
    [InlineData(MessageType.Debug)]
    [InlineData(MessageType.Information)]
    [InlineData(MessageType.Warning)]
    [InlineData(MessageType.Error)]
    [InlineData(MessageType.Critical)]
    [InlineData(MessageType.None)]
    [InlineData(MessageType.Success)]
    public void MessageType_AllValues_ShouldBeDefined(MessageType value)
    {
        // Arrange
        LogArrange($"Checking if {value} is defined");

        // Act
        LogAct("Calling Enum.IsDefined");
        var isDefined = Enum.IsDefined(value);

        // Assert
        LogAssert("Verifying value is defined");
        isDefined.ShouldBeTrue();
        LogInfo("{0} is a defined MessageType value", value);
    }

    [Fact]
    public void MessageType_InvalidValue_ShouldNotBeDefined()
    {
        // Arrange
        LogArrange("Creating invalid MessageType value");
        var invalidValue = (MessageType)(-1);

        // Act
        LogAct("Checking if invalid value is defined");
        var isDefined = Enum.IsDefined(invalidValue);

        // Assert
        LogAssert("Verifying invalid value is not defined");
        isDefined.ShouldBeFalse();
        LogInfo("Value {0} is correctly identified as not defined", (int)invalidValue);
    }

    [Fact]
    public void MessageType_OutOfRangeHigh_ShouldNotBeDefined()
    {
        // Arrange
        LogArrange("Creating out of range MessageType value (high)");
        var invalidValue = (MessageType)100;

        // Act
        LogAct("Checking if out of range value is defined");
        var isDefined = Enum.IsDefined(invalidValue);

        // Assert
        LogAssert("Verifying out of range value is not defined");
        isDefined.ShouldBeFalse();
        LogInfo("Value {0} is correctly identified as not defined", (int)invalidValue);
    }

    [Theory]
    [InlineData(MessageType.Trace, "Trace")]
    [InlineData(MessageType.Debug, "Debug")]
    [InlineData(MessageType.Information, "Information")]
    [InlineData(MessageType.Warning, "Warning")]
    [InlineData(MessageType.Error, "Error")]
    [InlineData(MessageType.Critical, "Critical")]
    [InlineData(MessageType.None, "None")]
    [InlineData(MessageType.Success, "Success")]
    public void MessageType_ToString_ShouldReturnCorrectName(MessageType value, string expectedName)
    {
        // Arrange
        LogArrange($"Getting string representation of {value}");

        // Act
        LogAct("Calling ToString");
        var result = value.ToString();

        // Assert
        LogAssert("Verifying string representation");
        result.ShouldBe(expectedName);
        LogInfo("{0}.ToString() = \"{1}\"", value, result);
    }

    [Fact]
    public void MessageType_SeverityOrder_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Verifying severity ordering");

        // Act & Assert
        LogAct("Checking that Trace < Debug < Information < Warning < Error < Critical");
        (MessageType.Trace < MessageType.Debug).ShouldBeTrue("Trace should be less than Debug");
        (MessageType.Debug < MessageType.Information).ShouldBeTrue("Debug should be less than Information");
        (MessageType.Information < MessageType.Warning).ShouldBeTrue("Information should be less than Warning");
        (MessageType.Warning < MessageType.Error).ShouldBeTrue("Warning should be less than Error");
        (MessageType.Error < MessageType.Critical).ShouldBeTrue("Error should be less than Critical");

        LogAssert("Severity ordering is correct");
    }

    [Theory]
    [InlineData(MessageType.Trace, MessageType.Critical, true)]
    [InlineData(MessageType.Warning, MessageType.Warning, true)]
    [InlineData(MessageType.Error, MessageType.Debug, false)]
    [InlineData(MessageType.Information, MessageType.Warning, true)]
    public void MessageType_GreaterThanOrEqual_ShouldWorkCorrectly(
        MessageType minimumLevel,
        MessageType actualLevel,
        bool shouldPass)
    {
        // Arrange
        LogArrange($"Testing if {actualLevel} passes minimum level {minimumLevel}");

        // Act
        LogAct("Checking >= comparison");
        var passes = actualLevel >= minimumLevel;

        // Assert
        LogAssert("Verifying result");
        passes.ShouldBe(shouldPass);
        LogInfo("{0} >= {1} = {2}", actualLevel, minimumLevel, passes);
    }

    [Fact]
    public void MessageType_GetValues_ShouldReturnAllValues()
    {
        // Arrange
        LogArrange("Getting all MessageType values");

        // Act
        LogAct("Calling Enum.GetValues");
        var values = Enum.GetValues<MessageType>();

        // Assert
        LogAssert("Verifying count and content");
        values.Length.ShouldBe(8);
        values.ShouldContain(MessageType.Trace);
        values.ShouldContain(MessageType.Debug);
        values.ShouldContain(MessageType.Information);
        values.ShouldContain(MessageType.Warning);
        values.ShouldContain(MessageType.Error);
        values.ShouldContain(MessageType.Critical);
        values.ShouldContain(MessageType.None);
        values.ShouldContain(MessageType.Success);
        LogInfo("Found {0} MessageType values", values.Length);
    }

    [Fact]
    public void MessageType_Parse_ShouldWorkForValidNames()
    {
        // Arrange
        LogArrange("Parsing MessageType from string");

        // Act
        LogAct("Calling Enum.Parse for each value");
        var trace = Enum.Parse<MessageType>("Trace");
        var debug = Enum.Parse<MessageType>("Debug");
        var information = Enum.Parse<MessageType>("Information");
        var warning = Enum.Parse<MessageType>("Warning");
        var error = Enum.Parse<MessageType>("Error");
        var critical = Enum.Parse<MessageType>("Critical");
        var none = Enum.Parse<MessageType>("None");
        var success = Enum.Parse<MessageType>("Success");

        // Assert
        LogAssert("Verifying parsed values");
        trace.ShouldBe(MessageType.Trace);
        debug.ShouldBe(MessageType.Debug);
        information.ShouldBe(MessageType.Information);
        warning.ShouldBe(MessageType.Warning);
        error.ShouldBe(MessageType.Error);
        critical.ShouldBe(MessageType.Critical);
        none.ShouldBe(MessageType.None);
        success.ShouldBe(MessageType.Success);
        LogInfo("All values parsed successfully");
    }

    [Fact]
    public void MessageType_TryParse_InvalidValue_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Trying to parse invalid MessageType name");

        // Act
        LogAct("Calling Enum.TryParse");
        var result = Enum.TryParse<MessageType>("InvalidType", out var value);

        // Assert
        LogAssert("Verifying parse failed");
        result.ShouldBeFalse();
        LogInfo("TryParse correctly returned false for invalid name");
    }

    [Fact]
    public void MessageType_SpecialTypes_ShouldBeHigherThanCritical()
    {
        // Arrange
        LogArrange("Checking that None and Success are positioned after Critical");

        // Act & Assert
        LogAct("Comparing None and Success with Critical");
        (MessageType.None > MessageType.Critical).ShouldBeTrue("None should be greater than Critical");
        (MessageType.Success > MessageType.Critical).ShouldBeTrue("Success should be greater than Critical");
        (MessageType.Success > MessageType.None).ShouldBeTrue("Success should be greater than None");

        LogAssert("Special types are correctly positioned");
    }
}
