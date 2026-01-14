using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models;
using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;
using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;

namespace Bedrock.UnitTests.BuildingBlocks.Core.ExecutionContexts;

public class ExecutionContextTests : TestBase
{
    private readonly TimeProvider _timeProvider;
    private readonly TenantInfo _tenantInfo;
    private readonly Guid _correlationId;

    public ExecutionContextTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _timeProvider = TimeProvider.System;
        _tenantInfo = TenantInfo.Create(Guid.NewGuid(), "Test Tenant");
        _correlationId = Guid.NewGuid();
    }

    private ExecutionContext CreateDefaultContext(MessageType minimumMessageType = MessageType.Trace)
    {
        return ExecutionContext.Create(
            correlationId: _correlationId,
            tenantInfo: _tenantInfo,
            executionUser: "test-user",
            executionOrigin: "test-origin",
            businessOperationCode: "TEST_OP",
            minimumMessageType: minimumMessageType,
            timeProvider: _timeProvider
        );
    }

    #region Create Tests

    [Fact]
    public void Create_WithValidParameters_ShouldCreateContext()
    {
        // Arrange
        LogArrange("Preparing valid parameters");
        var correlationId = Guid.NewGuid();
        var tenantInfo = TenantInfo.Create(Guid.NewGuid(), "Tenant");
        const string executionUser = "user@test.com";
        const string executionOrigin = "WebAPI";
        const string businessOperationCode = "CREATE_ORDER";

        // Act
        LogAct("Creating ExecutionContext");
        var context = ExecutionContext.Create(
            correlationId,
            tenantInfo,
            executionUser,
            executionOrigin,
            businessOperationCode,
            MessageType.Information,
            _timeProvider
        );

        // Assert
        LogAssert("Verifying all properties");
        context.CorrelationId.ShouldBe(correlationId);
        context.TenantInfo.ShouldBe(tenantInfo);
        context.ExecutionUser.ShouldBe(executionUser);
        context.ExecutionOrigin.ShouldBe(executionOrigin);
        context.BusinessOperationCode.ShouldBe(businessOperationCode);
        context.MinimumMessageType.ShouldBe(MessageType.Information);
        context.TimeProvider.ShouldBe(_timeProvider);
        context.Timestamp.ShouldNotBe(default);
        LogInfo("ExecutionContext created successfully");
    }

    [Fact]
    public void Create_WithNullTimeProvider_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Preparing null TimeProvider");

        // Act & Assert
        LogAct("Creating context with null TimeProvider");
        var exception = Should.Throw<ArgumentNullException>(() =>
            ExecutionContext.Create(
                Guid.NewGuid(),
                _tenantInfo,
                "user",
                "origin",
                "code",
                MessageType.Trace,
                null!
            ));

        LogAssert("Verifying exception");
        exception.ParamName.ShouldBe("timeProvider");
        LogInfo("ArgumentNullException thrown for null TimeProvider");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidExecutionUser_ShouldThrowArgumentException(string? invalidUser)
    {
        // Arrange
        LogArrange($"Preparing invalid executionUser: '{invalidUser ?? "(null)"}'");

        // Act & Assert
        LogAct("Creating context with invalid executionUser");
        var exception = Should.Throw<ArgumentException>(() =>
            ExecutionContext.Create(
                Guid.NewGuid(),
                _tenantInfo,
                invalidUser!,
                "origin",
                "code",
                MessageType.Trace,
                _timeProvider
            ));

        LogAssert("Verifying exception");
        exception.ParamName.ShouldBe("executionUser");
        LogInfo("ArgumentException thrown for invalid executionUser");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidExecutionOrigin_ShouldThrowArgumentException(string? invalidOrigin)
    {
        // Arrange
        LogArrange($"Preparing invalid executionOrigin: '{invalidOrigin ?? "(null)"}'");

        // Act & Assert
        LogAct("Creating context with invalid executionOrigin");
        var exception = Should.Throw<ArgumentException>(() =>
            ExecutionContext.Create(
                Guid.NewGuid(),
                _tenantInfo,
                "user",
                invalidOrigin!,
                "code",
                MessageType.Trace,
                _timeProvider
            ));

        LogAssert("Verifying exception");
        exception.ParamName.ShouldBe("executionOrigin");
        LogInfo("ArgumentException thrown for invalid executionOrigin");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidBusinessOperationCode_ShouldThrowArgumentException(string? invalidCode)
    {
        // Arrange
        LogArrange($"Preparing invalid businessOperationCode: '{invalidCode ?? "(null)"}'");

        // Act & Assert
        LogAct("Creating context with invalid businessOperationCode");
        var exception = Should.Throw<ArgumentException>(() =>
            ExecutionContext.Create(
                Guid.NewGuid(),
                _tenantInfo,
                "user",
                "origin",
                invalidCode!,
                MessageType.Trace,
                _timeProvider
            ));

        LogAssert("Verifying exception");
        exception.ParamName.ShouldBe("businessOperationCode");
        LogInfo("ArgumentException thrown for invalid businessOperationCode");
    }

    [Theory]
    [InlineData((MessageType)(-1))]
    [InlineData((MessageType)100)]
    public void Create_WithInvalidMinimumMessageType_ShouldThrowArgumentOutOfRangeException(MessageType invalidType)
    {
        // Arrange
        LogArrange($"Preparing invalid MessageType: {(int)invalidType}");

        // Act & Assert
        LogAct("Creating context with invalid minimumMessageType");
        var exception = Should.Throw<ArgumentOutOfRangeException>(() =>
            ExecutionContext.Create(
                Guid.NewGuid(),
                _tenantInfo,
                "user",
                "origin",
                "code",
                invalidType,
                _timeProvider
            ));

        LogAssert("Verifying exception");
        exception.ParamName.ShouldBe("messageType");
        LogInfo("ArgumentOutOfRangeException thrown for invalid MessageType");
    }

    #endregion

    #region Initial State Tests

    [Fact]
    public void Create_InitialState_ShouldHaveNoMessages()
    {
        // Arrange & Act
        LogArrange("Creating new context");
        var context = CreateDefaultContext();

        // Assert
        LogAssert("Verifying no messages");
        context.HasMessages.ShouldBeFalse();
        context.Messages.ShouldBeEmpty();
        LogInfo("Context created with no messages");
    }

    [Fact]
    public void Create_InitialState_ShouldHaveNoExceptions()
    {
        // Arrange & Act
        LogArrange("Creating new context");
        var context = CreateDefaultContext();

        // Assert
        LogAssert("Verifying no exceptions");
        context.HasExceptions.ShouldBeFalse();
        context.Exceptions.ShouldBeEmpty();
        LogInfo("Context created with no exceptions");
    }

    [Fact]
    public void Create_InitialState_ShouldBeSuccessful()
    {
        // Arrange & Act
        LogArrange("Creating new context");
        var context = CreateDefaultContext();

        // Assert
        LogAssert("Verifying IsSuccessful is true");
        context.IsSuccessful.ShouldBeTrue();
        context.IsFaulted.ShouldBeFalse();
        context.HasErrorMessages.ShouldBeFalse();
        LogInfo("New context is successful");
    }

    #endregion

    #region AddMessage Tests

    [Fact]
    public void AddTraceMessage_BelowMinimum_ShouldNotAddMessage()
    {
        // Arrange
        LogArrange("Creating context with minimum level Information");
        var context = CreateDefaultContext(MessageType.Information);

        // Act
        LogAct("Adding Trace message");
        context.AddTraceMessage("TRACE_CODE", "Trace text");

        // Assert
        LogAssert("Verifying message was not added");
        context.HasMessages.ShouldBeFalse();
        LogInfo("Trace message correctly filtered out");
    }

    [Fact]
    public void AddTraceMessage_AtMinimum_ShouldAddMessage()
    {
        // Arrange
        LogArrange("Creating context with minimum level Trace");
        var context = CreateDefaultContext(MessageType.Trace);

        // Act
        LogAct("Adding Trace message");
        context.AddTraceMessage("TRACE_CODE", "Trace text");

        // Assert
        LogAssert("Verifying message was added");
        context.HasMessages.ShouldBeTrue();
        context.Messages.Count().ShouldBe(1);
        context.Messages.First().MessageType.ShouldBe(MessageType.Trace);
        LogInfo("Trace message added successfully");
    }

    [Fact]
    public void AddDebugMessage_BelowMinimum_ShouldNotAddMessage()
    {
        // Arrange
        LogArrange("Creating context with minimum level Warning");
        var context = CreateDefaultContext(MessageType.Warning);

        // Act
        LogAct("Adding Debug message");
        context.AddDebugMessage("DEBUG_CODE");

        // Assert
        LogAssert("Verifying message was not added");
        context.HasMessages.ShouldBeFalse();
        LogInfo("Debug message correctly filtered out");
    }

    [Fact]
    public void AddDebugMessage_AtMinimum_ShouldAddMessage()
    {
        // Arrange
        LogArrange("Creating context with minimum level Debug");
        var context = CreateDefaultContext(MessageType.Debug);

        // Act
        LogAct("Adding Debug message");
        context.AddDebugMessage("DEBUG_CODE", "Debug text");

        // Assert
        LogAssert("Verifying message was added");
        context.HasMessages.ShouldBeTrue();
        context.Messages.First().MessageType.ShouldBe(MessageType.Debug);
        LogInfo("Debug message added successfully");
    }

    [Fact]
    public void AddInformationMessage_BelowMinimum_ShouldNotAddMessage()
    {
        // Arrange
        LogArrange("Creating context with minimum level Error");
        var context = CreateDefaultContext(MessageType.Error);

        // Act
        LogAct("Adding Information message");
        context.AddInformationMessage("INFO_CODE");

        // Assert
        LogAssert("Verifying message was not added");
        context.HasMessages.ShouldBeFalse();
        LogInfo("Information message correctly filtered out");
    }

    [Fact]
    public void AddInformationMessage_AtMinimum_ShouldAddMessage()
    {
        // Arrange
        LogArrange("Creating context with minimum level Information");
        var context = CreateDefaultContext(MessageType.Information);

        // Act
        LogAct("Adding Information message");
        context.AddInformationMessage("INFO_CODE", "Info text");

        // Assert
        LogAssert("Verifying message was added");
        context.HasMessages.ShouldBeTrue();
        context.Messages.First().MessageType.ShouldBe(MessageType.Information);
        LogInfo("Information message added successfully");
    }

    [Fact]
    public void AddWarningMessage_BelowMinimum_ShouldNotAddMessage()
    {
        // Arrange
        LogArrange("Creating context with minimum level Critical");
        var context = CreateDefaultContext(MessageType.Critical);

        // Act
        LogAct("Adding Warning message");
        context.AddWarningMessage("WARN_CODE");

        // Assert
        LogAssert("Verifying message was not added");
        context.HasMessages.ShouldBeFalse();
        LogInfo("Warning message correctly filtered out");
    }

    [Fact]
    public void AddWarningMessage_AtMinimum_ShouldAddMessage()
    {
        // Arrange
        LogArrange("Creating context with minimum level Warning");
        var context = CreateDefaultContext(MessageType.Warning);

        // Act
        LogAct("Adding Warning message");
        context.AddWarningMessage("WARN_CODE", "Warning text");

        // Assert
        LogAssert("Verifying message was added");
        context.HasMessages.ShouldBeTrue();
        context.Messages.First().MessageType.ShouldBe(MessageType.Warning);
        LogInfo("Warning message added successfully");
    }

    [Fact]
    public void AddErrorMessage_ShouldAlwaysAddMessage()
    {
        // Arrange
        LogArrange("Creating context with high minimum level");
        var context = CreateDefaultContext(MessageType.Success);

        // Act
        LogAct("Adding Error message");
        context.AddErrorMessage("ERROR_CODE", "Error text");

        // Assert
        LogAssert("Verifying message was added");
        context.HasMessages.ShouldBeTrue();
        context.HasErrorMessages.ShouldBeTrue();
        context.IsSuccessful.ShouldBeFalse();
        context.IsFaulted.ShouldBeTrue();
        LogInfo("Error message added regardless of minimum level");
    }

    [Fact]
    public void AddCriticalMessage_ShouldAlwaysAddMessage()
    {
        // Arrange
        LogArrange("Creating context with high minimum level");
        var context = CreateDefaultContext(MessageType.Success);

        // Act
        LogAct("Adding Critical message");
        context.AddCriticalMessage("CRITICAL_CODE", "Critical text");

        // Assert
        LogAssert("Verifying message was added");
        context.HasMessages.ShouldBeTrue();
        context.HasErrorMessages.ShouldBeTrue();
        context.IsSuccessful.ShouldBeFalse();
        context.IsFaulted.ShouldBeTrue();
        LogInfo("Critical message added regardless of minimum level");
    }

    [Fact]
    public void AddSuccessMessage_ShouldAlwaysAddMessage()
    {
        // Arrange
        LogArrange("Creating context with any minimum level");
        var context = CreateDefaultContext(MessageType.None);

        // Act
        LogAct("Adding Success message");
        context.AddSuccessMessage("SUCCESS_CODE", "Success text");

        // Assert
        LogAssert("Verifying message was added");
        context.HasMessages.ShouldBeTrue();
        context.Messages.First().MessageType.ShouldBe(MessageType.Success);
        LogInfo("Success message added regardless of minimum level");
    }

    #endregion

    #region AddException Tests

    [Fact]
    public void AddException_ShouldAddException()
    {
        // Arrange
        LogArrange("Creating context and exception");
        var context = CreateDefaultContext();
        var exception = new InvalidOperationException("Test exception");

        // Act
        LogAct("Adding exception");
        context.AddException(exception);

        // Assert
        LogAssert("Verifying exception was added");
        context.HasExceptions.ShouldBeTrue();
        context.Exceptions.ShouldContain(exception);
        context.IsSuccessful.ShouldBeFalse();
        context.IsFaulted.ShouldBeTrue();
        LogInfo("Exception added successfully");
    }

    [Fact]
    public void AddException_WithNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Creating context");
        var context = CreateDefaultContext();

        // Act & Assert
        LogAct("Adding null exception");
        var exception = Should.Throw<ArgumentNullException>(() =>
            context.AddException(null!));

        LogAssert("Verifying exception");
        exception.ParamName.ShouldBe("exception");
        LogInfo("ArgumentNullException thrown for null exception");
    }

    [Fact]
    public void AddMultipleExceptions_ShouldContainAll()
    {
        // Arrange
        LogArrange("Creating context and exceptions");
        var context = CreateDefaultContext();
        var ex1 = new InvalidOperationException("Exception 1");
        var ex2 = new ArgumentException("Exception 2");
        var ex3 = new NullReferenceException("Exception 3");

        // Act
        LogAct("Adding multiple exceptions");
        context.AddException(ex1);
        context.AddException(ex2);
        context.AddException(ex3);

        // Assert
        LogAssert("Verifying all exceptions added");
        context.Exceptions.Count().ShouldBe(3);
        context.Exceptions.ShouldContain(ex1);
        context.Exceptions.ShouldContain(ex2);
        context.Exceptions.ShouldContain(ex3);
        LogInfo("All exceptions added successfully");
    }

    #endregion

    #region State Property Tests

    [Fact]
    public void HasErrorMessages_WithErrorMessage_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating context");
        var context = CreateDefaultContext();

        // Act
        LogAct("Adding Error message");
        context.AddErrorMessage("ERROR");

        // Assert
        LogAssert("Verifying HasErrorMessages");
        context.HasErrorMessages.ShouldBeTrue();
        LogInfo("HasErrorMessages returns true with Error message");
    }

    [Fact]
    public void HasErrorMessages_WithCriticalMessage_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating context");
        var context = CreateDefaultContext();

        // Act
        LogAct("Adding Critical message");
        context.AddCriticalMessage("CRITICAL");

        // Assert
        LogAssert("Verifying HasErrorMessages");
        context.HasErrorMessages.ShouldBeTrue();
        LogInfo("HasErrorMessages returns true with Critical message");
    }

    [Fact]
    public void HasErrorMessages_WithOnlyWarnings_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating context");
        var context = CreateDefaultContext();

        // Act
        LogAct("Adding Warning message");
        context.AddWarningMessage("WARNING");

        // Assert
        LogAssert("Verifying HasErrorMessages");
        context.HasErrorMessages.ShouldBeFalse();
        LogInfo("HasErrorMessages returns false with only Warning messages");
    }

    [Fact]
    public void IsPartiallySuccessful_WithSuccessAndError_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating context");
        var context = CreateDefaultContext();

        // Act
        LogAct("Adding Success and Error messages");
        context.AddSuccessMessage("SUCCESS");
        context.AddErrorMessage("ERROR");

        // Assert
        LogAssert("Verifying IsPartiallySuccessful");
        context.IsPartiallySuccessful.ShouldBeTrue();
        LogInfo("IsPartiallySuccessful returns true with both Success and Error");
    }

    [Fact]
    public void IsPartiallySuccessful_WithOnlySuccess_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating context");
        var context = CreateDefaultContext();

        // Act
        LogAct("Adding only Success message");
        context.AddSuccessMessage("SUCCESS");

        // Assert
        LogAssert("Verifying IsPartiallySuccessful");
        context.IsPartiallySuccessful.ShouldBeFalse();
        LogInfo("IsPartiallySuccessful returns false with only Success");
    }

    [Fact]
    public void IsPartiallySuccessful_WithOnlyError_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating context");
        var context = CreateDefaultContext();

        // Act
        LogAct("Adding only Error message");
        context.AddErrorMessage("ERROR");

        // Assert
        LogAssert("Verifying IsPartiallySuccessful");
        context.IsPartiallySuccessful.ShouldBeFalse();
        LogInfo("IsPartiallySuccessful returns false with only Error");
    }

    [Fact]
    public void IsPartiallySuccessful_WithSuccessAndException_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating context");
        var context = CreateDefaultContext();

        // Act
        LogAct("Adding Success message and exception");
        context.AddSuccessMessage("SUCCESS");
        context.AddException(new Exception("Test"));

        // Assert
        LogAssert("Verifying IsPartiallySuccessful");
        context.IsPartiallySuccessful.ShouldBeTrue();
        LogInfo("IsPartiallySuccessful returns true with Success and exception");
    }

    [Fact]
    public void IsFaulted_WithException_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating context");
        var context = CreateDefaultContext();

        // Act
        LogAct("Adding exception");
        context.AddException(new Exception("Test"));

        // Assert
        LogAssert("Verifying IsFaulted");
        context.IsFaulted.ShouldBeTrue();
        LogInfo("IsFaulted returns true with exception");
    }

    [Fact]
    public void IsFaulted_WithErrorMessage_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating context");
        var context = CreateDefaultContext();

        // Act
        LogAct("Adding error message");
        context.AddErrorMessage("ERROR");

        // Assert
        LogAssert("Verifying IsFaulted");
        context.IsFaulted.ShouldBeTrue();
        LogInfo("IsFaulted returns true with error message");
    }

    #endregion

    #region ChangeMessageText Tests

    [Fact]
    public void ChangeMessageText_WithExistingId_ShouldReturnTrueAndChangeText()
    {
        // Arrange
        LogArrange("Creating context with message");
        var context = CreateDefaultContext();
        context.AddInformationMessage("CODE", "Original text");
        var messageId = context.Messages.First().Id;

        // Act
        LogAct("Changing message text");
        var result = context.ChangeMessageText(messageId, "New text");

        // Assert
        LogAssert("Verifying text changed");
        result.ShouldBeTrue();
        context.Messages.First().Text.ShouldBe("New text");
        LogInfo("Message text changed successfully");
    }

    [Fact]
    public void ChangeMessageText_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating context with message");
        var context = CreateDefaultContext();
        context.AddInformationMessage("CODE", "Original text");
        var nonExistingId = Id.GenerateNewId();

        // Act
        LogAct("Changing message text with non-existing ID");
        var result = context.ChangeMessageText(nonExistingId, "New text");

        // Assert
        LogAssert("Verifying result is false");
        result.ShouldBeFalse();
        LogInfo("ChangeMessageText returned false for non-existing ID");
    }

    [Fact]
    public void ChangeMessageText_ToNull_ShouldClearText()
    {
        // Arrange
        LogArrange("Creating context with message");
        var context = CreateDefaultContext();
        context.AddInformationMessage("CODE", "Original text");
        var messageId = context.Messages.First().Id;

        // Act
        LogAct("Changing message text to null");
        var result = context.ChangeMessageText(messageId, null);

        // Assert
        LogAssert("Verifying text is null");
        result.ShouldBeTrue();
        context.Messages.First().Text.ShouldBeNull();
        LogInfo("Message text cleared successfully");
    }

    #endregion

    #region ChangeMessageType Tests

    [Fact]
    public void ChangeMessageType_ById_WithExistingId_ShouldReturnTrueAndChangeType()
    {
        // Arrange
        LogArrange("Creating context with Warning message");
        var context = CreateDefaultContext();
        context.AddWarningMessage("CODE");
        var messageId = context.Messages.First().Id;

        // Act
        LogAct("Changing message type to Error");
        var result = context.ChangeMessageType(messageId, MessageType.Error);

        // Assert
        LogAssert("Verifying type changed");
        result.ShouldBeTrue();
        context.Messages.First().MessageType.ShouldBe(MessageType.Error);
        LogInfo("Message type changed successfully");
    }

    [Fact]
    public void ChangeMessageType_ById_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating context with message");
        var context = CreateDefaultContext();
        context.AddInformationMessage("CODE");
        var nonExistingId = Id.GenerateNewId();

        // Act
        LogAct("Changing message type with non-existing ID");
        var result = context.ChangeMessageType(nonExistingId, MessageType.Error);

        // Assert
        LogAssert("Verifying result is false");
        result.ShouldBeFalse();
        LogInfo("ChangeMessageType returned false for non-existing ID");
    }

    [Fact]
    public void ChangeMessageType_ByOldType_ShouldChangeAllMatchingMessages()
    {
        // Arrange
        LogArrange("Creating context with multiple Warning messages");
        var context = CreateDefaultContext();
        context.AddWarningMessage("WARN1");
        context.AddWarningMessage("WARN2");
        context.AddInformationMessage("INFO");

        // Act
        LogAct("Changing all Warning messages to Error");
        var result = context.ChangeMessageType(MessageType.Warning, MessageType.Error);

        // Assert
        LogAssert("Verifying types changed");
        result.ShouldBeTrue();
        context.Messages.Count(m => m.MessageType == MessageType.Error).ShouldBe(2);
        context.Messages.Count(m => m.MessageType == MessageType.Information).ShouldBe(1);
        LogInfo("All Warning messages changed to Error");
    }

    [Fact]
    public void ChangeMessageType_ByOldType_WithNoMatches_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating context with Information messages");
        var context = CreateDefaultContext();
        context.AddInformationMessage("INFO");

        // Act
        LogAct("Trying to change non-existing Warning messages");
        var result = context.ChangeMessageType(MessageType.Warning, MessageType.Error);

        // Assert
        LogAssert("Verifying result is false");
        result.ShouldBeFalse();
        LogInfo("ChangeMessageType returned false with no matches");
    }

    [Fact]
    public void ChangeMessageType_AllowsDowngrade()
    {
        // Arrange
        LogArrange("Creating context with Error message");
        var context = CreateDefaultContext();
        context.AddErrorMessage("ERROR");
        var messageId = context.Messages.First().Id;

        // Act
        LogAct("Downgrading Error to Warning");
        var result = context.ChangeMessageType(messageId, MessageType.Warning);

        // Assert
        LogAssert("Verifying downgrade worked");
        result.ShouldBeTrue();
        context.Messages.First().MessageType.ShouldBe(MessageType.Warning);
        context.HasErrorMessages.ShouldBeFalse();
        LogInfo("Error message successfully downgraded to Warning");
    }

    #endregion

    #region ChangeBusinessOperationCode Tests

    [Fact]
    public void ChangeBusinessOperationCode_WithValidCode_ShouldChangeCode()
    {
        // Arrange
        LogArrange("Creating context");
        var context = CreateDefaultContext();
        var originalCode = context.BusinessOperationCode;

        // Act
        LogAct("Changing business operation code");
        context.ChangeBusinessOperationCode("NEW_OP_CODE");

        // Assert
        LogAssert("Verifying code changed");
        context.BusinessOperationCode.ShouldBe("NEW_OP_CODE");
        context.BusinessOperationCode.ShouldNotBe(originalCode);
        LogInfo("Business operation code changed from {0} to {1}", originalCode, context.BusinessOperationCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ChangeBusinessOperationCode_WithInvalidCode_ShouldThrowArgumentException(string? invalidCode)
    {
        // Arrange
        LogArrange("Creating context");
        var context = CreateDefaultContext();

        // Act & Assert
        LogAct($"Changing to invalid code: '{invalidCode ?? "(null)"}'");
        var exception = Should.Throw<ArgumentException>(() =>
            context.ChangeBusinessOperationCode(invalidCode!));

        LogAssert("Verifying exception");
        exception.ParamName.ShouldBe("newBusinessOperationCode");
        LogInfo("ArgumentException thrown for invalid code");
    }

    #endregion

    #region Clone Tests

    [Fact]
    public void Clone_ShouldCreateDeepCopy()
    {
        // Arrange
        LogArrange("Creating context with messages and exception");
        var context = CreateDefaultContext();
        context.AddInformationMessage("INFO");
        context.AddErrorMessage("ERROR");
        context.AddException(new Exception("Test"));

        // Act
        LogAct("Cloning context");
        var clone = context.Clone();

        // Assert
        LogAssert("Verifying clone properties");
        clone.ShouldNotBeSameAs(context);
        clone.CorrelationId.ShouldBe(context.CorrelationId);
        clone.TenantInfo.ShouldBe(context.TenantInfo);
        clone.ExecutionUser.ShouldBe(context.ExecutionUser);
        clone.ExecutionOrigin.ShouldBe(context.ExecutionOrigin);
        clone.BusinessOperationCode.ShouldBe(context.BusinessOperationCode);
        clone.MinimumMessageType.ShouldBe(context.MinimumMessageType);
        clone.TimeProvider.ShouldBe(context.TimeProvider);
        clone.Timestamp.ShouldBe(context.Timestamp);
        clone.Messages.Count().ShouldBe(2);
        clone.Exceptions.Count().ShouldBe(1);
        LogInfo("Clone created with all properties preserved");
    }

    [Fact]
    public void Clone_ModifyingClone_ShouldNotAffectOriginal()
    {
        // Arrange
        LogArrange("Creating context with message");
        var context = CreateDefaultContext();
        context.AddInformationMessage("INFO");

        // Act
        LogAct("Cloning and modifying clone");
        var clone = context.Clone();
        clone.AddErrorMessage("ERROR_IN_CLONE");

        // Assert
        LogAssert("Verifying original not affected");
        context.Messages.Count().ShouldBe(1);
        clone.Messages.Count().ShouldBe(2);
        context.HasErrorMessages.ShouldBeFalse();
        clone.HasErrorMessages.ShouldBeTrue();
        LogInfo("Original context not affected by clone modifications");
    }

    #endregion

    #region Import Tests

    [Fact]
    public void Import_ShouldMergeMessagesAndExceptions()
    {
        // Arrange
        LogArrange("Creating two contexts");
        var context1 = CreateDefaultContext();
        context1.AddInformationMessage("INFO1");

        var context2 = CreateDefaultContext();
        context2.AddWarningMessage("WARN2");
        context2.AddException(new Exception("From context2"));

        // Act
        LogAct("Importing context2 into context1");
        context1.Import(context2);

        // Assert
        LogAssert("Verifying merged content");
        context1.Messages.Count().ShouldBe(2);
        context1.Exceptions.Count().ShouldBe(1);
        LogInfo("Messages and exceptions merged successfully");
    }

    [Fact]
    public void Import_WithNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Creating context");
        var context = CreateDefaultContext();

        // Act & Assert
        LogAct("Importing null context");
        var exception = Should.Throw<ArgumentNullException>(() =>
            context.Import(null!));

        LogAssert("Verifying exception");
        exception.ParamName.ShouldBe("other");
        LogInfo("ArgumentNullException thrown for null import");
    }

    [Fact]
    public void Import_SameMessageIds_ShouldNotDuplicate()
    {
        // Arrange
        LogArrange("Creating context with message");
        var context1 = CreateDefaultContext();
        context1.AddInformationMessage("INFO");
        var messageCount = context1.Messages.Count();

        // Act
        LogAct("Importing same context (same message IDs)");
        context1.Import(context1.Clone());

        // Assert
        LogAssert("Verifying no duplication");
        context1.Messages.Count().ShouldBe(messageCount);
        LogInfo("No duplicate messages after import");
    }

    #endregion

    #region ToDictionary Tests

    [Fact]
    public void ToDictionary_ShouldContainAllProperties()
    {
        // Arrange
        LogArrange("Creating context");
        var context = CreateDefaultContext();

        // Act
        LogAct("Converting to dictionary");
        var dict = context.ToDictionary();

        // Assert
        LogAssert("Verifying dictionary contents");
        dict.ShouldContainKey("Timestamp");
        dict.ShouldContainKey("CorrelationId");
        dict.ShouldContainKey("TenantCode");
        dict.ShouldContainKey("TenantName");
        dict.ShouldContainKey("ExecutionUser");
        dict.ShouldContainKey("ExecutionOrigin");
        dict.ShouldContainKey("BusinessOperationCode");

        dict["CorrelationId"].ShouldBe(_correlationId);
        dict["TenantCode"].ShouldBe(_tenantInfo.Code);
        dict["TenantName"].ShouldBe(_tenantInfo.Name);
        dict["ExecutionUser"].ShouldBe("test-user");
        dict["ExecutionOrigin"].ShouldBe("test-origin");
        dict["BusinessOperationCode"].ShouldBe("TEST_OP");
        LogInfo("Dictionary contains all expected properties");
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public void AddMessages_ConcurrentAccess_ShouldBeThreadSafe()
    {
        // Arrange
        LogArrange("Creating context for concurrent access");
        var context = CreateDefaultContext();
        const int messageCount = 100;
        var tasks = new List<Task>();

        // Act
        LogAct($"Adding {messageCount} messages concurrently");
        for (int i = 0; i < messageCount; i++)
        {
            var index = i;
            tasks.Add(Task.Run(() => context.AddInformationMessage($"MSG_{index}")));
        }
        Task.WaitAll(tasks.ToArray());

        // Assert
        LogAssert("Verifying all messages added");
        context.Messages.Count().ShouldBe(messageCount);
        LogInfo("All {0} messages added successfully in concurrent access", messageCount);
    }

    [Fact]
    public void AddExceptions_ConcurrentAccess_ShouldBeThreadSafe()
    {
        // Arrange
        LogArrange("Creating context for concurrent access");
        var context = CreateDefaultContext();
        const int exceptionCount = 100;
        var tasks = new List<Task>();

        // Act
        LogAct($"Adding {exceptionCount} exceptions concurrently");
        for (int i = 0; i < exceptionCount; i++)
        {
            var index = i;
            tasks.Add(Task.Run(() => context.AddException(new Exception($"Exception {index}"))));
        }
        Task.WaitAll(tasks.ToArray());

        // Assert
        LogAssert("Verifying all exceptions added");
        context.Exceptions.Count().ShouldBe(exceptionCount);
        LogInfo("All {0} exceptions added successfully in concurrent access", exceptionCount);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Create_WithEmptyGuid_ShouldWork()
    {
        // Arrange
        LogArrange("Creating context with empty correlation ID");

        // Act
        LogAct("Creating context");
        var context = ExecutionContext.Create(
            Guid.Empty,
            _tenantInfo,
            "user",
            "origin",
            "code",
            MessageType.Trace,
            _timeProvider
        );

        // Assert
        LogAssert("Verifying context created");
        context.CorrelationId.ShouldBe(Guid.Empty);
        LogInfo("Context created with empty Guid (no validation on business rules)");
    }

    [Fact]
    public void Create_WithDefaultTenantInfo_ShouldWork()
    {
        // Arrange
        LogArrange("Creating context with default TenantInfo");

        // Act
        LogAct("Creating context");
        var context = ExecutionContext.Create(
            Guid.NewGuid(),
            default,
            "user",
            "origin",
            "code",
            MessageType.Trace,
            _timeProvider
        );

        // Assert
        LogAssert("Verifying context created");
        context.TenantInfo.Code.ShouldBe(Guid.Empty);
        LogInfo("Context created with default TenantInfo (no validation on business rules)");
    }

    #endregion
}
