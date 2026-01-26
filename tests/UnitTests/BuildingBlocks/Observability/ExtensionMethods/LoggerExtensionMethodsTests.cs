using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Observability.ExtensionMethods;
using Bedrock.BuildingBlocks.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;
using Xunit.Abstractions;
using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;

namespace Bedrock.UnitTests.BuildingBlocks.Observability.ExtensionMethods;

public class LoggerExtensionMethodsTests : TestBase
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly ExecutionContext _executionContext;

    public LoggerExtensionMethodsTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _loggerMock = new Mock<ILogger>();

        // Setup logger mock to capture log calls
        _loggerMock
            .Setup(x => x.IsEnabled(It.IsAny<LogLevel>()))
            .Returns(true);

        _loggerMock
            .Setup(x => x.BeginScope(It.IsAny<It.IsAnyType>()))
            .Returns(Mock.Of<IDisposable>());

        var tenantInfo = TenantInfo.Create(Guid.NewGuid(), "Test Tenant");
        _executionContext = ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: tenantInfo,
            executionUser: "test-user",
            executionOrigin: "test-origin",
            businessOperationCode: "TEST_OP",
            minimumMessageType: MessageType.Trace,
            timeProvider: TimeProvider.System
        );
    }

    #region LogTraceForDistributedTracing Tests

    [Fact]
    public void LogTraceForDistributedTracing_WhenEnabled_ShouldLogWithScope()
    {
        // Arrange
        LogArrange("Setting up logger mock for Trace level");

        // Act
        LogAct("Calling LogTraceForDistributedTracing");
        _loggerMock.Object.LogTraceForDistributedTracing(_executionContext, "Test message");

        // Assert
        LogAssert("Verifying BeginScope and Log were called");
        _loggerMock.Verify(
            x => x.BeginScope(It.IsAny<ExecutionContextScope>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogTraceForDistributedTracing logged successfully");
    }

    [Fact]
    public void LogTraceForDistributedTracing_WhenDisabled_ShouldNotLog()
    {
        // Arrange
        LogArrange("Setting up logger mock with Trace disabled");
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(false);

        // Act
        LogAct("Calling LogTraceForDistributedTracing");
        _loggerMock.Object.LogTraceForDistributedTracing(_executionContext, "Test message");

        // Assert
        LogAssert("Verifying Log was not called");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);

        LogInfo("LogTraceForDistributedTracing correctly skipped when disabled");
    }

    [Fact]
    public void LogTraceForDistributedTracing_WithOneArg_ShouldLogWithScope()
    {
        // Arrange
        LogArrange("Setting up logger mock for Trace level with one arg");

        // Act
        LogAct("Calling LogTraceForDistributedTracing with one arg");
        _loggerMock.Object.LogTraceForDistributedTracing(_executionContext, "Test {0} message", "arg1");

        // Assert
        LogAssert("Verifying Log was called");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogTraceForDistributedTracing with one arg logged successfully");
    }

    [Fact]
    public void LogTraceForDistributedTracing_WithTwoArgs_ShouldLogWithScope()
    {
        // Arrange
        LogArrange("Setting up logger mock for Trace level with two args");

        // Act
        LogAct("Calling LogTraceForDistributedTracing with two args");
        _loggerMock.Object.LogTraceForDistributedTracing(_executionContext, "Test {0} message {1}", "arg1", 42);

        // Assert
        LogAssert("Verifying Log was called");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogTraceForDistributedTracing with two args logged successfully");
    }

    [Fact]
    public void LogTraceForDistributedTracing_WithThreeArgs_ShouldLogWithScope()
    {
        // Arrange
        LogArrange("Setting up logger mock for Trace level with three args");

        // Act
        LogAct("Calling LogTraceForDistributedTracing with three args");
        _loggerMock.Object.LogTraceForDistributedTracing(_executionContext, "Test {0} {1} {2}", "arg1", 42, true);

        // Assert
        LogAssert("Verifying Log was called");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogTraceForDistributedTracing with three args logged successfully");
    }

    [Fact]
    public void LogTraceForDistributedTracing_WithParamsArray_ShouldLogWithScope()
    {
        // Arrange
        LogArrange("Setting up logger mock for Trace level with params array");

        // Act
        LogAct("Calling LogTraceForDistributedTracing with params array");
        _loggerMock.Object.LogTraceForDistributedTracing(_executionContext, "Test {0} {1} {2} {3}", ["arg1", 42, true, "extra"]);

        // Assert
        LogAssert("Verifying Log was called");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogTraceForDistributedTracing with params array logged successfully");
    }

    #endregion

    #region LogDebugForDistributedTracing Tests

    [Fact]
    public void LogDebugForDistributedTracing_WhenEnabled_ShouldLogWithScope()
    {
        // Arrange
        LogArrange("Setting up logger mock for Debug level");

        // Act
        LogAct("Calling LogDebugForDistributedTracing");
        _loggerMock.Object.LogDebugForDistributedTracing(_executionContext, "Debug message");

        // Assert
        LogAssert("Verifying BeginScope and Log were called");
        _loggerMock.Verify(
            x => x.BeginScope(It.IsAny<ExecutionContextScope>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogDebugForDistributedTracing logged successfully");
    }

    [Fact]
    public void LogDebugForDistributedTracing_WhenDisabled_ShouldNotLog()
    {
        // Arrange
        LogArrange("Setting up logger mock with Debug disabled");
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(false);

        // Act
        LogAct("Calling LogDebugForDistributedTracing");
        _loggerMock.Object.LogDebugForDistributedTracing(_executionContext, "Debug message");

        // Assert
        LogAssert("Verifying Log was not called");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);

        LogInfo("LogDebugForDistributedTracing correctly skipped when disabled");
    }

    [Fact]
    public void LogDebugForDistributedTracing_WithOneArg_ShouldLogWithScope()
    {
        // Arrange
        LogArrange("Setting up logger mock for Debug level with one arg");

        // Act
        LogAct("Calling LogDebugForDistributedTracing with one arg");
        _loggerMock.Object.LogDebugForDistributedTracing(_executionContext, "Debug {0}", "arg1");

        // Assert
        LogAssert("Verifying Log was called");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogDebugForDistributedTracing with one arg logged successfully");
    }

    [Fact]
    public void LogDebugForDistributedTracing_WithTwoArgs_ShouldLogWithScope()
    {
        // Arrange
        LogArrange("Setting up logger mock for Debug level with two args");

        // Act
        LogAct("Calling LogDebugForDistributedTracing with two args");
        _loggerMock.Object.LogDebugForDistributedTracing(_executionContext, "Debug {0} {1}", "arg1", 42);

        // Assert
        LogAssert("Verifying Log was called");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogDebugForDistributedTracing with two args logged successfully");
    }

    [Fact]
    public void LogDebugForDistributedTracing_WithThreeArgs_ShouldLogWithScope()
    {
        // Arrange
        LogArrange("Setting up logger mock for Debug level with three args");

        // Act
        LogAct("Calling LogDebugForDistributedTracing with three args");
        _loggerMock.Object.LogDebugForDistributedTracing(_executionContext, "Debug {0} {1} {2}", "arg1", 42, true);

        // Assert
        LogAssert("Verifying Log was called");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogDebugForDistributedTracing with three args logged successfully");
    }

    [Fact]
    public void LogDebugForDistributedTracing_WithParamsArray_ShouldLogWithScope()
    {
        // Arrange
        LogArrange("Setting up logger mock for Debug level with params array");

        // Act
        LogAct("Calling LogDebugForDistributedTracing with params array");
        _loggerMock.Object.LogDebugForDistributedTracing(_executionContext, "Debug {0} {1} {2} {3}", ["arg1", 42, true, "extra"]);

        // Assert
        LogAssert("Verifying Log was called");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogDebugForDistributedTracing with params array logged successfully");
    }

    #endregion

    #region LogInformationForDistributedTracing Tests

    [Fact]
    public void LogInformationForDistributedTracing_WhenEnabled_ShouldLogWithScope()
    {
        // Arrange
        LogArrange("Setting up logger mock for Information level");

        // Act
        LogAct("Calling LogInformationForDistributedTracing");
        _loggerMock.Object.LogInformationForDistributedTracing(_executionContext, "Info message");

        // Assert
        LogAssert("Verifying BeginScope and Log were called");
        _loggerMock.Verify(
            x => x.BeginScope(It.IsAny<ExecutionContextScope>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogInformationForDistributedTracing logged successfully");
    }

    [Fact]
    public void LogInformationForDistributedTracing_WhenDisabled_ShouldNotLog()
    {
        // Arrange
        LogArrange("Setting up logger mock with Information disabled");
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(false);

        // Act
        LogAct("Calling LogInformationForDistributedTracing");
        _loggerMock.Object.LogInformationForDistributedTracing(_executionContext, "Info message");

        // Assert
        LogAssert("Verifying Log was not called");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);

        LogInfo("LogInformationForDistributedTracing correctly skipped when disabled");
    }

    [Fact]
    public void LogInformationForDistributedTracing_WithOneArg_ShouldLogWithScope()
    {
        // Arrange
        LogArrange("Setting up logger mock for Information level with one arg");

        // Act
        LogAct("Calling LogInformationForDistributedTracing with one arg");
        _loggerMock.Object.LogInformationForDistributedTracing(_executionContext, "Info {0}", "arg1");

        // Assert
        LogAssert("Verifying Log was called");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogInformationForDistributedTracing with one arg logged successfully");
    }

    [Fact]
    public void LogInformationForDistributedTracing_WithTwoArgs_ShouldLogWithScope()
    {
        // Arrange
        LogArrange("Setting up logger mock for Information level with two args");

        // Act
        LogAct("Calling LogInformationForDistributedTracing with two args");
        _loggerMock.Object.LogInformationForDistributedTracing(_executionContext, "Info {0} {1}", "arg1", 42);

        // Assert
        LogAssert("Verifying Log was called");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogInformationForDistributedTracing with two args logged successfully");
    }

    [Fact]
    public void LogInformationForDistributedTracing_WithThreeArgs_ShouldLogWithScope()
    {
        // Arrange
        LogArrange("Setting up logger mock for Information level with three args");

        // Act
        LogAct("Calling LogInformationForDistributedTracing with three args");
        _loggerMock.Object.LogInformationForDistributedTracing(_executionContext, "Info {0} {1} {2}", "arg1", 42, true);

        // Assert
        LogAssert("Verifying Log was called");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogInformationForDistributedTracing with three args logged successfully");
    }

    [Fact]
    public void LogInformationForDistributedTracing_WithParamsArray_ShouldLogWithScope()
    {
        // Arrange
        LogArrange("Setting up logger mock for Information level with params array");

        // Act
        LogAct("Calling LogInformationForDistributedTracing with params array");
        _loggerMock.Object.LogInformationForDistributedTracing(_executionContext, "Info {0} {1} {2} {3}", ["arg1", 42, true, "extra"]);

        // Assert
        LogAssert("Verifying Log was called");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogInformationForDistributedTracing with params array logged successfully");
    }

    #endregion

    #region LogWarningForDistributedTracing Tests

    [Fact]
    public void LogWarningForDistributedTracing_WhenEnabled_ShouldLogWithScope()
    {
        // Arrange
        LogArrange("Setting up logger mock for Warning level");

        // Act
        LogAct("Calling LogWarningForDistributedTracing");
        _loggerMock.Object.LogWarningForDistributedTracing(_executionContext, "Warning message");

        // Assert
        LogAssert("Verifying BeginScope and Log were called");
        _loggerMock.Verify(
            x => x.BeginScope(It.IsAny<ExecutionContextScope>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogWarningForDistributedTracing logged successfully");
    }

    [Fact]
    public void LogWarningForDistributedTracing_WhenDisabled_ShouldNotLog()
    {
        // Arrange
        LogArrange("Setting up logger mock with Warning disabled");
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Warning)).Returns(false);

        // Act
        LogAct("Calling LogWarningForDistributedTracing");
        _loggerMock.Object.LogWarningForDistributedTracing(_executionContext, "Warning message");

        // Assert
        LogAssert("Verifying Log was not called");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);

        LogInfo("LogWarningForDistributedTracing correctly skipped when disabled");
    }

    [Fact]
    public void LogWarningForDistributedTracing_WithOneArg_ShouldLogWithScope()
    {
        // Arrange
        LogArrange("Setting up logger mock for Warning level with one arg");

        // Act
        LogAct("Calling LogWarningForDistributedTracing with one arg");
        _loggerMock.Object.LogWarningForDistributedTracing(_executionContext, "Warning {0}", "arg1");

        // Assert
        LogAssert("Verifying Log was called");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogWarningForDistributedTracing with one arg logged successfully");
    }

    [Fact]
    public void LogWarningForDistributedTracing_WithTwoArgs_ShouldLogWithScope()
    {
        // Arrange
        LogArrange("Setting up logger mock for Warning level with two args");

        // Act
        LogAct("Calling LogWarningForDistributedTracing with two args");
        _loggerMock.Object.LogWarningForDistributedTracing(_executionContext, "Warning {0} {1}", "arg1", 42);

        // Assert
        LogAssert("Verifying Log was called");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogWarningForDistributedTracing with two args logged successfully");
    }

    [Fact]
    public void LogWarningForDistributedTracing_WithThreeArgs_ShouldLogWithScope()
    {
        // Arrange
        LogArrange("Setting up logger mock for Warning level with three args");

        // Act
        LogAct("Calling LogWarningForDistributedTracing with three args");
        _loggerMock.Object.LogWarningForDistributedTracing(_executionContext, "Warning {0} {1} {2}", "arg1", 42, true);

        // Assert
        LogAssert("Verifying Log was called");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogWarningForDistributedTracing with three args logged successfully");
    }

    [Fact]
    public void LogWarningForDistributedTracing_WithParamsArray_ShouldLogWithScope()
    {
        // Arrange
        LogArrange("Setting up logger mock for Warning level with params array");

        // Act
        LogAct("Calling LogWarningForDistributedTracing with params array");
        _loggerMock.Object.LogWarningForDistributedTracing(_executionContext, "Warning {0} {1} {2} {3}", ["arg1", 42, true, "extra"]);

        // Assert
        LogAssert("Verifying Log was called");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogWarningForDistributedTracing with params array logged successfully");
    }

    #endregion

    #region LogErrorForDistributedTracing Tests

    [Fact]
    public void LogErrorForDistributedTracing_WhenEnabled_ShouldLogWithScope()
    {
        // Arrange
        LogArrange("Setting up logger mock for Error level");

        // Act
        LogAct("Calling LogErrorForDistributedTracing");
        _loggerMock.Object.LogErrorForDistributedTracing(_executionContext, "Error message");

        // Assert
        LogAssert("Verifying BeginScope and Log were called");
        _loggerMock.Verify(
            x => x.BeginScope(It.IsAny<ExecutionContextScope>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogErrorForDistributedTracing logged successfully");
    }

    [Fact]
    public void LogErrorForDistributedTracing_WhenDisabled_ShouldNotLog()
    {
        // Arrange
        LogArrange("Setting up logger mock with Error disabled");
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(false);

        // Act
        LogAct("Calling LogErrorForDistributedTracing");
        _loggerMock.Object.LogErrorForDistributedTracing(_executionContext, "Error message");

        // Assert
        LogAssert("Verifying Log was not called");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);

        LogInfo("LogErrorForDistributedTracing correctly skipped when disabled");
    }

    [Fact]
    public void LogErrorForDistributedTracing_WithOneArg_ShouldLogWithScope()
    {
        // Arrange
        LogArrange("Setting up logger mock for Error level with one arg");

        // Act
        LogAct("Calling LogErrorForDistributedTracing with one arg");
        _loggerMock.Object.LogErrorForDistributedTracing(_executionContext, "Error {0}", "arg1");

        // Assert
        LogAssert("Verifying Log was called");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogErrorForDistributedTracing with one arg logged successfully");
    }

    [Fact]
    public void LogErrorForDistributedTracing_WithTwoArgs_ShouldLogWithScope()
    {
        // Arrange
        LogArrange("Setting up logger mock for Error level with two args");

        // Act
        LogAct("Calling LogErrorForDistributedTracing with two args");
        _loggerMock.Object.LogErrorForDistributedTracing(_executionContext, "Error {0} {1}", "arg1", 42);

        // Assert
        LogAssert("Verifying Log was called");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogErrorForDistributedTracing with two args logged successfully");
    }

    [Fact]
    public void LogErrorForDistributedTracing_WithThreeArgs_ShouldLogWithScope()
    {
        // Arrange
        LogArrange("Setting up logger mock for Error level with three args");

        // Act
        LogAct("Calling LogErrorForDistributedTracing with three args");
        _loggerMock.Object.LogErrorForDistributedTracing(_executionContext, "Error {0} {1} {2}", "arg1", 42, true);

        // Assert
        LogAssert("Verifying Log was called");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogErrorForDistributedTracing with three args logged successfully");
    }

    [Fact]
    public void LogErrorForDistributedTracing_WithParamsArray_ShouldLogWithScope()
    {
        // Arrange
        LogArrange("Setting up logger mock for Error level with params array");

        // Act
        LogAct("Calling LogErrorForDistributedTracing with params array");
        _loggerMock.Object.LogErrorForDistributedTracing(_executionContext, "Error {0} {1} {2} {3}", ["arg1", 42, true, "extra"]);

        // Assert
        LogAssert("Verifying Log was called");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogErrorForDistributedTracing with params array logged successfully");
    }

    #endregion

    #region LogCriticalForDistributedTracing Tests

    [Fact]
    public void LogCriticalForDistributedTracing_WhenEnabled_ShouldLogWithScope()
    {
        // Arrange
        LogArrange("Setting up logger mock for Critical level");

        // Act
        LogAct("Calling LogCriticalForDistributedTracing");
        _loggerMock.Object.LogCriticalForDistributedTracing(_executionContext, "Critical message");

        // Assert
        LogAssert("Verifying BeginScope and Log were called");
        _loggerMock.Verify(
            x => x.BeginScope(It.IsAny<ExecutionContextScope>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogCriticalForDistributedTracing logged successfully");
    }

    [Fact]
    public void LogCriticalForDistributedTracing_WhenDisabled_ShouldNotLog()
    {
        // Arrange
        LogArrange("Setting up logger mock with Critical disabled");
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Critical)).Returns(false);

        // Act
        LogAct("Calling LogCriticalForDistributedTracing");
        _loggerMock.Object.LogCriticalForDistributedTracing(_executionContext, "Critical message");

        // Assert
        LogAssert("Verifying Log was not called");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);

        LogInfo("LogCriticalForDistributedTracing correctly skipped when disabled");
    }

    [Fact]
    public void LogCriticalForDistributedTracing_WithOneArg_ShouldLogWithScope()
    {
        // Arrange
        LogArrange("Setting up logger mock for Critical level with one arg");

        // Act
        LogAct("Calling LogCriticalForDistributedTracing with one arg");
        _loggerMock.Object.LogCriticalForDistributedTracing(_executionContext, "Critical {0}", "arg1");

        // Assert
        LogAssert("Verifying Log was called");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogCriticalForDistributedTracing with one arg logged successfully");
    }

    [Fact]
    public void LogCriticalForDistributedTracing_WithTwoArgs_ShouldLogWithScope()
    {
        // Arrange
        LogArrange("Setting up logger mock for Critical level with two args");

        // Act
        LogAct("Calling LogCriticalForDistributedTracing with two args");
        _loggerMock.Object.LogCriticalForDistributedTracing(_executionContext, "Critical {0} {1}", "arg1", 42);

        // Assert
        LogAssert("Verifying Log was called");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogCriticalForDistributedTracing with two args logged successfully");
    }

    [Fact]
    public void LogCriticalForDistributedTracing_WithThreeArgs_ShouldLogWithScope()
    {
        // Arrange
        LogArrange("Setting up logger mock for Critical level with three args");

        // Act
        LogAct("Calling LogCriticalForDistributedTracing with three args");
        _loggerMock.Object.LogCriticalForDistributedTracing(_executionContext, "Critical {0} {1} {2}", "arg1", 42, true);

        // Assert
        LogAssert("Verifying Log was called");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogCriticalForDistributedTracing with three args logged successfully");
    }

    [Fact]
    public void LogCriticalForDistributedTracing_WithParamsArray_ShouldLogWithScope()
    {
        // Arrange
        LogArrange("Setting up logger mock for Critical level with params array");

        // Act
        LogAct("Calling LogCriticalForDistributedTracing with params array");
        _loggerMock.Object.LogCriticalForDistributedTracing(_executionContext, "Critical {0} {1} {2} {3}", ["arg1", 42, true, "extra"]);

        // Assert
        LogAssert("Verifying Log was called");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogCriticalForDistributedTracing with params array logged successfully");
    }

    #endregion

    #region LogExceptionForDistributedTracing Tests

    [Fact]
    public void LogExceptionForDistributedTracing_WithMessage_ShouldLogWithException()
    {
        // Arrange
        LogArrange("Setting up logger mock for exception logging");
        var exception = new InvalidOperationException("Test exception");

        // Act
        LogAct("Calling LogExceptionForDistributedTracing with message");
        _loggerMock.Object.LogExceptionForDistributedTracing(_executionContext, exception, "Error occurred");

        // Assert
        LogAssert("Verifying Log was called with exception");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogExceptionForDistributedTracing with message logged successfully");
    }

    [Fact]
    public void LogExceptionForDistributedTracing_WithOneArg_ShouldLogWithException()
    {
        // Arrange
        LogArrange("Setting up logger mock for exception logging with one arg");
        var exception = new InvalidOperationException("Test exception");

        // Act
        LogAct("Calling LogExceptionForDistributedTracing with one arg");
        _loggerMock.Object.LogExceptionForDistributedTracing(_executionContext, exception, "Error {0}", "details");

        // Assert
        LogAssert("Verifying Log was called with exception");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogExceptionForDistributedTracing with one arg logged successfully");
    }

    [Fact]
    public void LogExceptionForDistributedTracing_WithTwoArgs_ShouldLogWithException()
    {
        // Arrange
        LogArrange("Setting up logger mock for exception logging with two args");
        var exception = new InvalidOperationException("Test exception");

        // Act
        LogAct("Calling LogExceptionForDistributedTracing with two args");
        _loggerMock.Object.LogExceptionForDistributedTracing(_executionContext, exception, "Error {0} {1}", "details", 42);

        // Assert
        LogAssert("Verifying Log was called with exception");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogExceptionForDistributedTracing with two args logged successfully");
    }

    [Fact]
    public void LogExceptionForDistributedTracing_WithThreeArgs_ShouldLogWithException()
    {
        // Arrange
        LogArrange("Setting up logger mock for exception logging with three args");
        var exception = new InvalidOperationException("Test exception");

        // Act
        LogAct("Calling LogExceptionForDistributedTracing with three args");
        _loggerMock.Object.LogExceptionForDistributedTracing(_executionContext, exception, "Error {0} {1} {2}", "details", 42, true);

        // Assert
        LogAssert("Verifying Log was called with exception");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogExceptionForDistributedTracing with three args logged successfully");
    }

    [Fact]
    public void LogExceptionForDistributedTracing_WithParamsArray_ShouldLogWithException()
    {
        // Arrange
        LogArrange("Setting up logger mock for exception logging with params array");
        var exception = new InvalidOperationException("Test exception");

        // Act
        LogAct("Calling LogExceptionForDistributedTracing with params array");
        _loggerMock.Object.LogExceptionForDistributedTracing(_executionContext, exception, "Error {0} {1} {2} {3}", ["details", 42, true, "extra"]);

        // Assert
        LogAssert("Verifying Log was called with exception");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogExceptionForDistributedTracing with params array logged successfully");
    }

    [Fact]
    public void LogExceptionForDistributedTracing_WithExceptionOnly_ShouldLogWithExceptionMessage()
    {
        // Arrange
        LogArrange("Setting up logger mock for exception-only logging");
        var exception = new InvalidOperationException("Test exception message");

        // Act
        LogAct("Calling LogExceptionForDistributedTracing with exception only");
        _loggerMock.Object.LogExceptionForDistributedTracing(_executionContext, exception);

        // Assert
        LogAssert("Verifying Log was called with exception");
        _loggerMock.Verify(
            x => x.BeginScope(It.IsAny<ExecutionContextScope>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogExceptionForDistributedTracing with exception only logged successfully");
    }

    [Fact]
    public void LogExceptionForDistributedTracing_WhenDisabled_ShouldNotLog()
    {
        // Arrange
        LogArrange("Setting up logger mock with Error disabled");
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(false);
        var exception = new InvalidOperationException("Test exception");

        // Act
        LogAct("Calling LogExceptionForDistributedTracing");
        _loggerMock.Object.LogExceptionForDistributedTracing(_executionContext, exception, "Error");

        // Assert
        LogAssert("Verifying Log was not called");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);

        LogInfo("LogExceptionForDistributedTracing correctly skipped when disabled");
    }

    [Fact]
    public void LogExceptionForDistributedTracing_ExceptionOnlyOverload_WhenDisabled_ShouldNotLog()
    {
        // Arrange
        LogArrange("Setting up logger mock with Error disabled");
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(false);
        var exception = new InvalidOperationException("Test exception");

        // Act
        LogAct("Calling LogExceptionForDistributedTracing (exception-only overload)");
        _loggerMock.Object.LogExceptionForDistributedTracing(_executionContext, exception);

        // Assert
        LogAssert("Verifying Log was not called");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);

        LogInfo("LogExceptionForDistributedTracing (exception-only) correctly skipped when disabled");
    }

    #endregion

    #region LogForDistributedTracing Tests

    [Fact]
    public void LogForDistributedTracing_WithNullExecutionContext_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Setting up logger mock");

        // Act & Assert
        LogAct("Calling LogForDistributedTracing with null context");
        var exception = Should.Throw<ArgumentNullException>(() =>
            _loggerMock.Object.LogForDistributedTracing(null!, LogLevel.Information, null, "Message"));

        LogAssert("Verifying exception");
        exception.ParamName.ShouldBe("executionContext");
        LogInfo("ArgumentNullException thrown for null ExecutionContext");
    }

    [Fact]
    public void LogForDistributedTracing_WithOneArg_ShouldLog()
    {
        // Arrange
        LogArrange("Setting up logger mock");

        // Act
        LogAct("Calling LogForDistributedTracing with one arg");
        _loggerMock.Object.LogForDistributedTracing(_executionContext, LogLevel.Information, null, "Message {0}", "arg1");

        // Assert
        LogAssert("Verifying Log was called");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogForDistributedTracing with one arg logged successfully");
    }

    [Fact]
    public void LogForDistributedTracing_WithTwoArgs_ShouldLog()
    {
        // Arrange
        LogArrange("Setting up logger mock");

        // Act
        LogAct("Calling LogForDistributedTracing with two args");
        _loggerMock.Object.LogForDistributedTracing(_executionContext, LogLevel.Information, null, "Message {0} {1}", "arg1", 42);

        // Assert
        LogAssert("Verifying Log was called");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogForDistributedTracing with two args logged successfully");
    }

    [Fact]
    public void LogForDistributedTracing_WithThreeArgs_ShouldLog()
    {
        // Arrange
        LogArrange("Setting up logger mock");

        // Act
        LogAct("Calling LogForDistributedTracing with three args");
        _loggerMock.Object.LogForDistributedTracing(_executionContext, LogLevel.Information, null, "Message {0} {1} {2}", "arg1", 42, true);

        // Assert
        LogAssert("Verifying Log was called");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogForDistributedTracing with three args logged successfully");
    }

    [Fact]
    public void LogForDistributedTracing_WithParamsArray_ShouldLog()
    {
        // Arrange
        LogArrange("Setting up logger mock");

        // Act
        LogAct("Calling LogForDistributedTracing with params array");
        _loggerMock.Object.LogForDistributedTracing(_executionContext, LogLevel.Information, null, "Message {0} {1} {2} {3}", ["arg1", 42, true, "extra"]);

        // Assert
        LogAssert("Verifying Log was called");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogForDistributedTracing with params array logged successfully");
    }

    [Theory]
    [InlineData(LogLevel.Trace)]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Information)]
    [InlineData(LogLevel.Warning)]
    [InlineData(LogLevel.Error)]
    [InlineData(LogLevel.Critical)]
    public void LogForDistributedTracing_WithDifferentLogLevels_ShouldLogAtCorrectLevel(LogLevel logLevel)
    {
        // Arrange
        LogArrange($"Setting up logger mock for {logLevel} level");

        // Act
        LogAct($"Calling LogForDistributedTracing with {logLevel}");
        _loggerMock.Object.LogForDistributedTracing(_executionContext, logLevel, null, "Test message");

        // Assert
        LogAssert($"Verifying Log was called with {logLevel}");
        _loggerMock.Verify(
            x => x.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo($"LogForDistributedTracing logged at {logLevel} level successfully");
    }

    [Fact]
    public void LogForDistributedTracing_WithException_ShouldPassExceptionToLog()
    {
        // Arrange
        LogArrange("Setting up logger mock");
        var exception = new InvalidOperationException("Test exception");

        // Act
        LogAct("Calling LogForDistributedTracing with exception");
        _loggerMock.Object.LogForDistributedTracing(
            _executionContext,
            LogLevel.Error,
            exception,
            "Error message");

        // Assert
        LogAssert("Verifying exception was passed to Log");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogForDistributedTracing correctly passed exception to Log");
    }

    [Fact]
    public void LogForDistributedTracing_DisposesScope()
    {
        // Arrange
        LogArrange("Setting up logger mock with disposable scope");
        var disposableMock = new Mock<IDisposable>();
        _loggerMock
            .Setup(x => x.BeginScope(It.IsAny<ExecutionContextScope>()))
            .Returns(disposableMock.Object);

        // Act
        LogAct("Calling LogForDistributedTracing");
        _loggerMock.Object.LogForDistributedTracing(_executionContext, LogLevel.Information, null, "Message");

        // Assert
        LogAssert("Verifying scope was disposed");
        disposableMock.Verify(x => x.Dispose(), Times.Once);
        LogInfo("Scope was correctly disposed after logging");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void LogForDistributedTracing_WithEmptyMessage_ShouldLog()
    {
        // Arrange
        LogArrange("Setting up logger mock");

        // Act
        LogAct("Calling LogForDistributedTracing with empty message");
        _loggerMock.Object.LogForDistributedTracing(_executionContext, LogLevel.Information, null, string.Empty);

        // Assert
        LogAssert("Verifying Log was called");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogForDistributedTracing logged empty message successfully");
    }

    [Fact]
    public void LogForDistributedTracing_WithEmptyArgs_ShouldLog()
    {
        // Arrange
        LogArrange("Setting up logger mock");

        // Act
        LogAct("Calling LogForDistributedTracing with empty args");
        _loggerMock.Object.LogForDistributedTracing(
            _executionContext,
            LogLevel.Information,
            null,
            "Message with {0}",
            Array.Empty<object>());

        // Assert
        LogAssert("Verifying Log was called");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        LogInfo("LogForDistributedTracing logged with empty args successfully");
    }

    #endregion
}
