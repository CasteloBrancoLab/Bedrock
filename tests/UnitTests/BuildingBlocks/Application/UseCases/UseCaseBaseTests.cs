using Bedrock.BuildingBlocks.Application.UseCases;
using Bedrock.BuildingBlocks.Application.UseCases.Models;
using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Persistence.Abstractions.UnitOfWork.Interfaces;
using Bedrock.BuildingBlocks.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;
using Xunit.Abstractions;
using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;

namespace Bedrock.UnitTests.BuildingBlocks.Application.UseCases;

public class UseCaseBaseTests : TestBase
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly ExecutionContext _executionContext;

    public UseCaseBaseTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _loggerMock = new Mock<ILogger>();
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

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Preparing to create use case with null logger");

        // Act & Assert
        LogAct("Creating use case with null logger");
        var exception = Should.Throw<ArgumentNullException>(() =>
            new TestUseCase(null!));

        LogAssert("Verifying exception");
        exception.ParamName.ShouldBe("logger");
        LogInfo("ArgumentNullException thrown for null logger");
    }

    [Fact]
    public void Constructor_WithValidLogger_ShouldCreateInstance()
    {
        // Arrange
        LogArrange("Preparing to create use case with valid logger");

        // Act
        LogAct("Creating use case with valid logger");
        var useCase = new TestUseCase(_loggerMock.Object);

        // Assert
        LogAssert("Verifying instance creation and ExecutionOptions");
        useCase.ShouldNotBeNull();
        useCase.ExposedLogger.ShouldBe(_loggerMock.Object);
        useCase.ExposedExecutionOptions.ShouldNotBeNull();
        LogInfo("Use case created successfully with valid logger and ExecutionOptions");
    }

    [Fact]
    public void Constructor_ShouldInitializeExecutionOptionsWithNullUnitOfWork()
    {
        // Arrange
        LogArrange("Preparing to create use case");

        // Act
        LogAct("Creating use case");
        var useCase = new TestUseCase(_loggerMock.Object);

        // Assert
        LogAssert("Verifying ExecutionOptions.UnitOfWork is null");
        useCase.ExposedExecutionOptions.UnitOfWork.ShouldBeNull();
        LogInfo("ExecutionOptions initialized with null UnitOfWork");
    }

    #endregion

    #region ExecuteAsync Tests

    [Fact]
    public async Task ExecuteAsync_WhenInternalReturnsOutput_ShouldReturnOutput()
    {
        // Arrange
        LogArrange("Setting up use case that returns output");
        var expectedOutput = new TestOutput("test-result");
        var useCase = new TestUseCase(_loggerMock.Object)
        {
            ResultToReturn = expectedOutput
        };
        var input = new TestInput("test-value");

        // Act
        LogAct("Calling ExecuteAsync");
        var result = await useCase.ExecuteAsync(_executionContext, input, CancellationToken.None);

        // Assert
        LogAssert("Verifying output returned");
        result.ShouldNotBeNull();
        result.ShouldBe(expectedOutput);
        LogInfo("ExecuteAsync returned expected output");
    }

    [Fact]
    public async Task ExecuteAsync_WhenInternalReturnsNull_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up use case that returns null");
        var useCase = new TestUseCase(_loggerMock.Object)
        {
            ResultToReturn = null
        };
        var input = new TestInput("test-value");

        // Act
        LogAct("Calling ExecuteAsync");
        var result = await useCase.ExecuteAsync(_executionContext, input, CancellationToken.None);

        // Assert
        LogAssert("Verifying null returned");
        result.ShouldBeNull();
        LogInfo("ExecuteAsync returned null as expected");
    }

    [Fact]
    public async Task ExecuteAsync_WhenInternalThrowsException_ShouldLogAndReturnNull()
    {
        // Arrange
        LogArrange("Setting up use case that throws exception");
        var useCase = new TestUseCase(_loggerMock.Object)
        {
            ShouldThrow = true
        };
        var input = new TestInput("test-value");

        // Act
        LogAct("Calling ExecuteAsync");
        var result = await useCase.ExecuteAsync(_executionContext, input, CancellationToken.None);

        // Assert
        LogAssert("Verifying null returned and exception logged");
        result.ShouldBeNull();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
        LogInfo("Exception was caught, logged, and null returned");
    }

    [Fact]
    public async Task ExecuteAsync_CalledMultipleTimes_ShouldOnlyConfigureOnce()
    {
        // Arrange
        LogArrange("Setting up use case to track configure calls");
        var useCase = new TestUseCase(_loggerMock.Object)
        {
            ResultToReturn = new TestOutput("result")
        };
        var input = new TestInput("test-value");

        // Act
        LogAct("Calling ExecuteAsync three times");
        await useCase.ExecuteAsync(_executionContext, input, CancellationToken.None);
        await useCase.ExecuteAsync(_executionContext, input, CancellationToken.None);
        await useCase.ExecuteAsync(_executionContext, input, CancellationToken.None);

        // Assert
        LogAssert("Verifying ConfigureExecutionInternal called exactly once");
        useCase.ConfigureCallCount.ShouldBe(1);
        LogInfo("ConfigureExecutionInternal was called only once across 3 executions");
    }

    #endregion

    #region UnitOfWork Tests

    [Fact]
    public async Task ExecuteAsync_WithUnitOfWork_WhenHandlerReturnsOutput_ShouldReturnOutput()
    {
        // Arrange
        LogArrange("Setting up use case with UnitOfWork that succeeds");
        var expectedOutput = new TestOutput("uow-result");
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock
            .Setup(x => x.ExecuteAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<TestInput>(),
                It.IsAny<Func<ExecutionContext, TestInput, CancellationToken, Task<bool>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<ExecutionContext, TestInput, Func<ExecutionContext, TestInput, CancellationToken, Task<bool>>, CancellationToken>(
                async (ctx, inp, handler, ct) => await handler(ctx, inp, ct));

        var useCase = new TestUseCase(_loggerMock.Object)
        {
            ResultToReturn = expectedOutput
        };
        useCase.ExposedExecutionOptions.UnitOfWork = unitOfWorkMock.Object;
        var input = new TestInput("test-value");

        // Act
        LogAct("Calling ExecuteAsync with UnitOfWork");
        var result = await useCase.ExecuteAsync(_executionContext, input, CancellationToken.None);

        // Assert
        LogAssert("Verifying output returned through UnitOfWork");
        result.ShouldNotBeNull();
        result.ShouldBe(expectedOutput);
        unitOfWorkMock.Verify(
            x => x.ExecuteAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<TestInput>(),
                It.IsAny<Func<ExecutionContext, TestInput, CancellationToken, Task<bool>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        LogInfo("UnitOfWork delegated to handler and returned output");
    }

    [Fact]
    public async Task ExecuteAsync_WithUnitOfWork_WhenHandlerReturnsNull_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up use case with UnitOfWork where handler returns null");
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock
            .Setup(x => x.ExecuteAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<TestInput>(),
                It.IsAny<Func<ExecutionContext, TestInput, CancellationToken, Task<bool>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<ExecutionContext, TestInput, Func<ExecutionContext, TestInput, CancellationToken, Task<bool>>, CancellationToken>(
                async (ctx, inp, handler, ct) => await handler(ctx, inp, ct));

        var useCase = new TestUseCase(_loggerMock.Object)
        {
            ResultToReturn = null
        };
        useCase.ExposedExecutionOptions.UnitOfWork = unitOfWorkMock.Object;
        var input = new TestInput("test-value");

        // Act
        LogAct("Calling ExecuteAsync with UnitOfWork (null result)");
        var result = await useCase.ExecuteAsync(_executionContext, input, CancellationToken.None);

        // Assert
        LogAssert("Verifying null returned when handler returns null");
        result.ShouldBeNull();
        LogInfo("UnitOfWork handler returned null → false → result is null");
    }

    [Fact]
    public async Task ExecuteAsync_WithUnitOfWork_WhenUoWReturnsFalse_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up use case with UnitOfWork that returns false");
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock
            .Setup(x => x.ExecuteAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<TestInput>(),
                It.IsAny<Func<ExecutionContext, TestInput, CancellationToken, Task<bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var useCase = new TestUseCase(_loggerMock.Object)
        {
            ResultToReturn = new TestOutput("should-be-discarded")
        };
        useCase.ExposedExecutionOptions.UnitOfWork = unitOfWorkMock.Object;
        var input = new TestInput("test-value");

        // Act
        LogAct("Calling ExecuteAsync with UnitOfWork returning false");
        var result = await useCase.ExecuteAsync(_executionContext, input, CancellationToken.None);

        // Assert
        LogAssert("Verifying null returned when UoW returns false");
        result.ShouldBeNull();
        LogInfo("UnitOfWork returned false → result discarded");
    }

    [Fact]
    public async Task ExecuteAsync_WithUnitOfWork_WhenUoWThrowsException_ShouldLogAndReturnNull()
    {
        // Arrange
        LogArrange("Setting up use case with UnitOfWork that throws");
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock
            .Setup(x => x.ExecuteAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<TestInput>(),
                It.IsAny<Func<ExecutionContext, TestInput, CancellationToken, Task<bool>>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("UoW transaction failed"));

        var useCase = new TestUseCase(_loggerMock.Object)
        {
            ResultToReturn = new TestOutput("should-not-reach")
        };
        useCase.ExposedExecutionOptions.UnitOfWork = unitOfWorkMock.Object;
        var input = new TestInput("test-value");

        // Act
        LogAct("Calling ExecuteAsync with UnitOfWork that throws");
        var result = await useCase.ExecuteAsync(_executionContext, input, CancellationToken.None);

        // Assert
        LogAssert("Verifying null returned and exception logged");
        result.ShouldBeNull();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
        LogInfo("UoW exception was caught, logged, and null returned");
    }

    #endregion

    #region Test Helpers

    private sealed record TestInput(string Value);

    private sealed record TestOutput(string Result);

    private sealed class TestUseCase : UseCaseBase<TestInput, TestOutput>
    {
        public TestOutput? ResultToReturn { get; set; }
        public bool ShouldThrow { get; set; }
        public int ConfigureCallCount { get; private set; }
        public ILogger ExposedLogger => Logger;
        public UseCaseExecutionOptions ExposedExecutionOptions => ExecutionOptions;

        public TestUseCase(ILogger logger) : base(logger) { }

        protected override void ConfigureExecutionInternal(UseCaseExecutionOptions options)
        {
            ConfigureCallCount++;
        }

        protected override Task<TestOutput?> ExecuteInternalAsync(
            ExecutionContext executionContext,
            TestInput input,
            CancellationToken cancellationToken)
        {
            if (ShouldThrow)
                throw new InvalidOperationException("Test exception in ExecuteInternal");

            return Task.FromResult(ResultToReturn);
        }
    }

    #endregion
}
