using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models;
using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Core.ExecutionContexts.Models;

public class MessageTests : TestBase
{
    private readonly TimeProvider _timeProvider;

    public MessageTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _timeProvider = TimeProvider.System;
    }

    #region Create Tests

    [Fact]
    public void Create_WithValidParameters_ShouldCreateMessage()
    {
        // Arrange
        LogArrange("Preparing valid parameters for Message creation");
        const string code = "TEST_CODE";
        const string text = "Test message text";

        // Act
        LogAct("Creating Message with valid parameters");
        var message = Message.Create(_timeProvider, MessageType.Information, code, text);

        // Assert
        LogAssert("Verifying message properties");
        message.Code.ShouldBe(code);
        message.Text.ShouldBe(text);
        message.MessageType.ShouldBe(MessageType.Information);
        message.Id.Value.ShouldNotBe(Guid.Empty);
        message.TimeStamp.ShouldNotBe(default);
        LogInfo("Message created: Id={0}, Code={1}, Type={2}", message.Id.Value, message.Code, message.MessageType);
    }

    [Fact]
    public void Create_WithNullText_ShouldCreateMessageWithNullText()
    {
        // Arrange
        LogArrange("Preparing parameters with null text");
        const string code = "TEST_CODE";

        // Act
        LogAct("Creating Message with null text");
        var message = Message.Create(_timeProvider, MessageType.Warning, code, null);

        // Assert
        LogAssert("Verifying text is null");
        message.Text.ShouldBeNull();
        message.Code.ShouldBe(code);
        LogInfo("Message created with null text");
    }

    [Fact]
    public void Create_WithNullTimeProvider_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Preparing null TimeProvider");

        // Act & Assert
        LogAct("Creating Message with null TimeProvider");
        var exception = Should.Throw<ArgumentNullException>(() =>
            Message.Create(null!, MessageType.Information, "CODE"));

        LogAssert("Verifying exception");
        exception.ParamName.ShouldBe("timeProvider");
        LogInfo("ArgumentNullException thrown for null TimeProvider");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidCode_ShouldThrowArgumentException(string? invalidCode)
    {
        // Arrange
        LogArrange($"Preparing invalid code: '{invalidCode ?? "(null)"}'");

        // Act & Assert
        LogAct("Creating Message with invalid code");
        var exception = Should.Throw<ArgumentException>(() =>
            Message.Create(_timeProvider, MessageType.Information, invalidCode!));

        LogAssert("Verifying exception");
        exception.ParamName.ShouldBe("code");
        LogInfo("ArgumentException thrown for invalid code");
    }

    [Theory]
    [InlineData((MessageType)(-1))]
    [InlineData((MessageType)100)]
    public void Create_WithInvalidMessageType_ShouldThrowArgumentOutOfRangeException(MessageType invalidType)
    {
        // Arrange
        LogArrange($"Preparing invalid MessageType: {(int)invalidType}");

        // Act & Assert
        LogAct("Creating Message with invalid MessageType");
        var exception = Should.Throw<ArgumentOutOfRangeException>(() =>
            Message.Create(_timeProvider, invalidType, "CODE"));

        LogAssert("Verifying exception");
        exception.ParamName.ShouldBe("messageType");
        LogInfo("ArgumentOutOfRangeException thrown for invalid MessageType");
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        // Arrange
        LogArrange("Creating multiple messages");

        // Act
        LogAct("Creating two messages");
        var message1 = Message.Create(_timeProvider, MessageType.Information, "CODE1");
        var message2 = Message.Create(_timeProvider, MessageType.Information, "CODE2");

        // Assert
        LogAssert("Verifying IDs are unique");
        message1.Id.ShouldNotBe(message2.Id);
        LogInfo("Messages have unique IDs: {0} != {1}", message1.Id.Value, message2.Id.Value);
    }

    #endregion

    #region Factory Method Tests

    [Fact]
    public void CreateTrace_ShouldCreateTraceMessage()
    {
        // Arrange
        LogArrange("Preparing Trace message creation");
        const string code = "TRACE_CODE";
        const string text = "Trace text";

        // Act
        LogAct("Creating Trace message");
        var message = Message.CreateTrace(_timeProvider, code, text);

        // Assert
        LogAssert("Verifying MessageType is Trace");
        message.MessageType.ShouldBe(MessageType.Trace);
        message.Code.ShouldBe(code);
        message.Text.ShouldBe(text);
        LogInfo("Trace message created");
    }

    [Fact]
    public void CreateDebug_ShouldCreateDebugMessage()
    {
        // Arrange
        LogArrange("Preparing Debug message creation");
        const string code = "DEBUG_CODE";
        const string text = "Debug text";

        // Act
        LogAct("Creating Debug message");
        var message = Message.CreateDebug(_timeProvider, code, text);

        // Assert
        LogAssert("Verifying MessageType is Debug");
        message.MessageType.ShouldBe(MessageType.Debug);
        message.Code.ShouldBe(code);
        message.Text.ShouldBe(text);
        LogInfo("Debug message created");
    }

    [Fact]
    public void CreateInformation_ShouldCreateInformationMessage()
    {
        // Arrange
        LogArrange("Preparing Information message creation");
        const string code = "INFO_CODE";
        const string text = "Info text";

        // Act
        LogAct("Creating Information message");
        var message = Message.CreateInformation(_timeProvider, code, text);

        // Assert
        LogAssert("Verifying MessageType is Information");
        message.MessageType.ShouldBe(MessageType.Information);
        message.Code.ShouldBe(code);
        message.Text.ShouldBe(text);
        LogInfo("Information message created");
    }

    [Fact]
    public void CreateWarning_ShouldCreateWarningMessage()
    {
        // Arrange
        LogArrange("Preparing Warning message creation");
        const string code = "WARN_CODE";
        const string text = "Warning text";

        // Act
        LogAct("Creating Warning message");
        var message = Message.CreateWarning(_timeProvider, code, text);

        // Assert
        LogAssert("Verifying MessageType is Warning");
        message.MessageType.ShouldBe(MessageType.Warning);
        message.Code.ShouldBe(code);
        message.Text.ShouldBe(text);
        LogInfo("Warning message created");
    }

    [Fact]
    public void CreateError_ShouldCreateErrorMessage()
    {
        // Arrange
        LogArrange("Preparing Error message creation");
        const string code = "ERROR_CODE";
        const string text = "Error text";

        // Act
        LogAct("Creating Error message");
        var message = Message.CreateError(_timeProvider, code, text);

        // Assert
        LogAssert("Verifying MessageType is Error");
        message.MessageType.ShouldBe(MessageType.Error);
        message.Code.ShouldBe(code);
        message.Text.ShouldBe(text);
        LogInfo("Error message created");
    }

    [Fact]
    public void CreateCritical_ShouldCreateCriticalMessage()
    {
        // Arrange
        LogArrange("Preparing Critical message creation");
        const string code = "CRIT_CODE";
        const string text = "Critical text";

        // Act
        LogAct("Creating Critical message");
        var message = Message.CreateCritical(_timeProvider, code, text);

        // Assert
        LogAssert("Verifying MessageType is Critical");
        message.MessageType.ShouldBe(MessageType.Critical);
        message.Code.ShouldBe(code);
        message.Text.ShouldBe(text);
        LogInfo("Critical message created");
    }

    [Fact]
    public void CreateNone_ShouldCreateNoneMessage()
    {
        // Arrange
        LogArrange("Preparing None message creation");
        const string code = "NONE_CODE";
        const string text = "None text";

        // Act
        LogAct("Creating None message");
        var message = Message.CreateNone(_timeProvider, code, text);

        // Assert
        LogAssert("Verifying MessageType is None");
        message.MessageType.ShouldBe(MessageType.None);
        message.Code.ShouldBe(code);
        message.Text.ShouldBe(text);
        LogInfo("None message created");
    }

    [Fact]
    public void CreateSuccess_ShouldCreateSuccessMessage()
    {
        // Arrange
        LogArrange("Preparing Success message creation");
        const string code = "SUCCESS_CODE";
        const string text = "Success text";

        // Act
        LogAct("Creating Success message");
        var message = Message.CreateSuccess(_timeProvider, code, text);

        // Assert
        LogAssert("Verifying MessageType is Success");
        message.MessageType.ShouldBe(MessageType.Success);
        message.Code.ShouldBe(code);
        message.Text.ShouldBe(text);
        LogInfo("Success message created");
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
    public void FactoryMethods_ShouldAllowNullText(MessageType messageType)
    {
        // Arrange
        LogArrange($"Creating {messageType} message without text");

        // Act
        LogAct("Creating message using factory method");
        var message = messageType switch
        {
            MessageType.Trace => Message.CreateTrace(_timeProvider, "CODE"),
            MessageType.Debug => Message.CreateDebug(_timeProvider, "CODE"),
            MessageType.Information => Message.CreateInformation(_timeProvider, "CODE"),
            MessageType.Warning => Message.CreateWarning(_timeProvider, "CODE"),
            MessageType.Error => Message.CreateError(_timeProvider, "CODE"),
            MessageType.Critical => Message.CreateCritical(_timeProvider, "CODE"),
            MessageType.None => Message.CreateNone(_timeProvider, "CODE"),
            MessageType.Success => Message.CreateSuccess(_timeProvider, "CODE"),
            _ => throw new ArgumentOutOfRangeException(nameof(messageType))
        };

        // Assert
        LogAssert("Verifying text is null");
        message.Text.ShouldBeNull();
        message.MessageType.ShouldBe(messageType);
        LogInfo("{0} message created with null text", messageType);
    }

    #endregion

    #region WithMessageType Tests

    [Theory]
    [InlineData(MessageType.Trace, MessageType.Error)]
    [InlineData(MessageType.Warning, MessageType.Information)]
    [InlineData(MessageType.Error, MessageType.Warning)]
    [InlineData(MessageType.Information, MessageType.Success)]
    public void WithMessageType_ShouldCreateNewMessageWithNewType(
        MessageType originalType,
        MessageType newType)
    {
        // Arrange
        LogArrange($"Creating message with type {originalType}");
        var original = Message.Create(_timeProvider, originalType, "CODE", "Text");

        // Act
        LogAct($"Changing message type to {newType}");
        var updated = original.WithMessageType(newType);

        // Assert
        LogAssert("Verifying new message has new type and preserved other properties");
        updated.MessageType.ShouldBe(newType);
        updated.Id.ShouldBe(original.Id);
        updated.TimeStamp.ShouldBe(original.TimeStamp);
        updated.Code.ShouldBe(original.Code);
        updated.Text.ShouldBe(original.Text);
        LogInfo("Message type changed from {0} to {1}", originalType, newType);
    }

    [Theory]
    [InlineData((MessageType)(-1))]
    [InlineData((MessageType)100)]
    public void WithMessageType_WithInvalidType_ShouldThrowArgumentOutOfRangeException(MessageType invalidType)
    {
        // Arrange
        LogArrange("Creating message");
        var message = Message.Create(_timeProvider, MessageType.Information, "CODE");

        // Act & Assert
        LogAct($"Changing to invalid type {(int)invalidType}");
        var exception = Should.Throw<ArgumentOutOfRangeException>(() =>
            message.WithMessageType(invalidType));

        LogAssert("Verifying exception");
        exception.ParamName.ShouldBe("messageType");
        LogInfo("ArgumentOutOfRangeException thrown for invalid type");
    }

    [Fact]
    public void WithMessageType_ShouldNotModifyOriginal()
    {
        // Arrange
        LogArrange("Creating original message");
        var original = Message.Create(_timeProvider, MessageType.Information, "CODE");
        var originalType = original.MessageType;

        // Act
        LogAct("Creating new message with different type");
        _ = original.WithMessageType(MessageType.Error);

        // Assert
        LogAssert("Verifying original is unchanged");
        original.MessageType.ShouldBe(originalType);
        LogInfo("Original message type unchanged: {0}", original.MessageType);
    }

    #endregion

    #region WithText Tests

    [Fact]
    public void WithText_ShouldCreateNewMessageWithNewText()
    {
        // Arrange
        LogArrange("Creating message with original text");
        var original = Message.Create(_timeProvider, MessageType.Information, "CODE", "Original text");

        // Act
        LogAct("Changing text");
        var updated = original.WithText("New text");

        // Assert
        LogAssert("Verifying new message has new text and preserved other properties");
        updated.Text.ShouldBe("New text");
        updated.Id.ShouldBe(original.Id);
        updated.TimeStamp.ShouldBe(original.TimeStamp);
        updated.Code.ShouldBe(original.Code);
        updated.MessageType.ShouldBe(original.MessageType);
        LogInfo("Text changed from '{0}' to '{1}'", original.Text, updated.Text);
    }

    [Fact]
    public void WithText_WithNull_ShouldClearText()
    {
        // Arrange
        LogArrange("Creating message with text");
        var original = Message.Create(_timeProvider, MessageType.Information, "CODE", "Original text");

        // Act
        LogAct("Changing text to null");
        var updated = original.WithText(null);

        // Assert
        LogAssert("Verifying text is null");
        updated.Text.ShouldBeNull();
        LogInfo("Text cleared successfully");
    }

    [Fact]
    public void WithText_ShouldNotModifyOriginal()
    {
        // Arrange
        LogArrange("Creating original message");
        const string originalText = "Original text";
        var original = Message.Create(_timeProvider, MessageType.Information, "CODE", originalText);

        // Act
        LogAct("Creating new message with different text");
        _ = original.WithText("New text");

        // Assert
        LogAssert("Verifying original is unchanged");
        original.Text.ShouldBe(originalText);
        LogInfo("Original message text unchanged: {0}", original.Text);
    }

    #endregion

    #region ValidateMessageType Tests

    [Theory]
    [InlineData(MessageType.Trace, true)]
    [InlineData(MessageType.Debug, true)]
    [InlineData(MessageType.Information, true)]
    [InlineData(MessageType.Warning, true)]
    [InlineData(MessageType.Error, true)]
    [InlineData(MessageType.Critical, true)]
    [InlineData(MessageType.None, true)]
    [InlineData(MessageType.Success, true)]
    [InlineData((MessageType)(-1), false)]
    [InlineData((MessageType)8, false)]
    [InlineData((MessageType)100, false)]
    public void ValidateMessageType_ShouldReturnCorrectResult(MessageType type, bool expected)
    {
        // Arrange
        LogArrange($"Validating MessageType {(int)type}");

        // Act
        LogAct("Calling ValidateMessageType");
        var result = Message.ValidateMessageType(type);

        // Assert
        LogAssert("Verifying result");
        result.ShouldBe(expected);
        LogInfo("ValidateMessageType({0}) = {1}", (int)type, result);
    }

    #endregion

    #region ThrowIfInvalidMessageType Tests

    [Theory]
    [InlineData(MessageType.Trace)]
    [InlineData(MessageType.Debug)]
    [InlineData(MessageType.Information)]
    [InlineData(MessageType.Warning)]
    [InlineData(MessageType.Error)]
    [InlineData(MessageType.Critical)]
    [InlineData(MessageType.None)]
    [InlineData(MessageType.Success)]
    public void ThrowIfInvalidMessageType_WithValidType_ShouldNotThrow(MessageType type)
    {
        // Arrange
        LogArrange($"Testing valid MessageType {type}");

        // Act & Assert
        LogAct("Calling ThrowIfInvalidMessageType");
        Should.NotThrow(() => Message.ThrowIfInvalidMessageType(type));
        LogAssert("No exception thrown for valid type");
    }

    [Theory]
    [InlineData((MessageType)(-1))]
    [InlineData((MessageType)8)]
    [InlineData((MessageType)100)]
    public void ThrowIfInvalidMessageType_WithInvalidType_ShouldThrow(MessageType type)
    {
        // Arrange
        LogArrange($"Testing invalid MessageType {(int)type}");

        // Act & Assert
        LogAct("Calling ThrowIfInvalidMessageType");
        var exception = Should.Throw<ArgumentOutOfRangeException>(() =>
            Message.ThrowIfInvalidMessageType(type));

        LogAssert("Verifying exception details");
        exception.ParamName.ShouldBe("messageType");
        exception.Message.ShouldContain("MessageType must be a valid MessageType value");
        LogInfo("Exception thrown for invalid type {0}", (int)type);
    }

    #endregion

    #region Record Struct Behavior Tests

    [Fact]
    public void Message_ShouldSupportRecordEquality()
    {
        // Arrange
        LogArrange("Creating two messages with same properties");

        // Note: Since Message uses internal constructor and generates new IDs,
        // we test equality through the same instance
        var message1 = Message.Create(_timeProvider, MessageType.Information, "CODE", "Text");
        var message2 = message1; // Same instance

        // Act
        LogAct("Comparing messages");
        var areEqual = message1.Equals(message2);

        // Assert
        LogAssert("Verifying equality");
        areEqual.ShouldBeTrue();
        LogInfo("Same message instances are equal");
    }

    [Fact]
    public void Message_DifferentMessages_ShouldNotBeEqual()
    {
        // Arrange
        LogArrange("Creating two different messages");
        var message1 = Message.Create(_timeProvider, MessageType.Information, "CODE1");
        var message2 = Message.Create(_timeProvider, MessageType.Information, "CODE2");

        // Act
        LogAct("Comparing different messages");
        var areEqual = message1.Equals(message2);

        // Assert
        LogAssert("Verifying inequality");
        areEqual.ShouldBeFalse();
        LogInfo("Different messages are not equal");
    }

    [Fact]
    public void Message_GetHashCode_ShouldBeConsistent()
    {
        // Arrange
        LogArrange("Creating message");
        var message = Message.Create(_timeProvider, MessageType.Information, "CODE");

        // Act
        LogAct("Getting hash code multiple times");
        var hash1 = message.GetHashCode();
        var hash2 = message.GetHashCode();

        // Assert
        LogAssert("Verifying hash code consistency");
        hash1.ShouldBe(hash2);
        LogInfo("Hash code is consistent: {0}", hash1);
    }

    [Fact]
    public void Message_ToString_ShouldReturnMeaningfulString()
    {
        // Arrange
        LogArrange("Creating message");
        var message = Message.Create(_timeProvider, MessageType.Error, "ERROR_CODE", "Error occurred");

        // Act
        LogAct("Calling ToString");
        var result = message.ToString();

        // Assert
        LogAssert("Verifying ToString contains relevant info");
        result.ShouldNotBeNullOrEmpty();
        result.ShouldContain("ERROR_CODE");
        result.ShouldContain("Error");
        LogInfo("ToString result: {0}", result);
    }

    #endregion

    #region TimeStamp Tests

    [Fact]
    public void Create_ShouldSetTimeStampFromTimeProvider()
    {
        // Arrange
        LogArrange("Getting current time");
        var before = _timeProvider.GetUtcNow();

        // Act
        LogAct("Creating message");
        var message = Message.Create(_timeProvider, MessageType.Information, "CODE");
        var after = _timeProvider.GetUtcNow();

        // Assert
        LogAssert("Verifying timestamp is within range");
        message.TimeStamp.ShouldBeGreaterThanOrEqualTo(before);
        message.TimeStamp.ShouldBeLessThanOrEqualTo(after);
        LogInfo("Message timestamp: {0}", message.TimeStamp);
    }

    #endregion
}
