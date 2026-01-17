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
    private readonly List<(LogLevel Level, string Message, object[] Args)> _capturedLogs;

    public LoggerExtensionMethodsTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _loggerMock = new Mock<ILogger>();
        _capturedLogs = [];

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
            x => x.BeginScope(It.Is<Dictionary<string, object?>>(d =>
                d.ContainsKey("CorrelationId") &&
                d.ContainsKey("ExecutionUser"))),
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
    public void LogTraceForDistributedTracing_WithArgs_ShouldPassArgsToLog()
    {
        // Arrange
        LogArrange("Setting up logger mock for Trace level with args");

        // Act
        LogAct("Calling LogTraceForDistributedTracing with args");
        _loggerMock.Object.LogTraceForDistributedTracing(_executionContext, "Test {0} message {1}", ["arg1", 42]);

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

        LogInfo("LogTraceForDistributedTracing with args logged successfully");
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
            x => x.BeginScope(It.IsAny<Dictionary<string, object?>>()),
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
            x => x.BeginScope(It.IsAny<Dictionary<string, object?>>()),
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
            x => x.BeginScope(It.IsAny<Dictionary<string, object?>>()),
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
            x => x.BeginScope(It.IsAny<Dictionary<string, object?>>()),
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
            x => x.BeginScope(It.IsAny<Dictionary<string, object?>>()),
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

    #endregion

    #region LogExceptionForDistributedTracing Tests

    [Fact]
    public void LogExceptionForDistributedTracing_WithMessageAndArgs_ShouldLogWithException()
    {
        // Arrange
        LogArrange("Setting up logger mock for exception logging");
        var exception = new InvalidOperationException("Test exception");

        // Act
        LogAct("Calling LogExceptionForDistributedTracing with message");
        _loggerMock.Object.LogExceptionForDistributedTracing(_executionContext, exception, "Error occurred: {0}", ["details"]);

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
            x => x.BeginScope(It.IsAny<Dictionary<string, object?>>()),
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
    public void LogForDistributedTracing_ScopeContainsExecutionContextData()
    {
        // Arrange
        LogArrange("Setting up logger mock to capture scope");
        Dictionary<string, object?>? capturedScope = null;

        _loggerMock
            .Setup(x => x.BeginScope(It.IsAny<Dictionary<string, object?>>()))
            .Callback<Dictionary<string, object?>>(scope => capturedScope = scope)
            .Returns(Mock.Of<IDisposable>());

        // Act
        LogAct("Calling LogForDistributedTracing");
        _loggerMock.Object.LogForDistributedTracing(
            _executionContext,
            LogLevel.Information,
            null,
            "Test message");

        // Assert
        LogAssert("Verifying scope contains execution context data");
        capturedScope.ShouldNotBeNull();
        capturedScope.ShouldContainKey("CorrelationId");
        capturedScope.ShouldContainKey("TenantCode");
        capturedScope.ShouldContainKey("TenantName");
        capturedScope.ShouldContainKey("ExecutionUser");
        capturedScope.ShouldContainKey("ExecutionOrigin");
        capturedScope.ShouldContainKey("BusinessOperationCode");
        capturedScope.ShouldContainKey("Timestamp");

        capturedScope["ExecutionUser"].ShouldBe("test-user");
        capturedScope["ExecutionOrigin"].ShouldBe("test-origin");
        capturedScope["BusinessOperationCode"].ShouldBe("TEST_OP");

        LogInfo("Scope correctly contains all execution context data");
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
            .Setup(x => x.BeginScope(It.IsAny<Dictionary<string, object?>>()))
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
