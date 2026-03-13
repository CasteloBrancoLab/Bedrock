using Bedrock.BuildingBlocks.Core.EmailAddresses;
using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Persistence.Abstractions.UnitOfWork.Interfaces;
using Bedrock.BuildingBlocks.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using ShopDemo.Auth.Application.UseCases.AuthenticateUser;
using ShopDemo.Auth.Application.UseCases.AuthenticateUser.Interfaces;
using ShopDemo.Auth.Application.UseCases.AuthenticateUser.Models;
using ShopDemo.Auth.Domain.Entities.Users;
using ShopDemo.Auth.Domain.Entities.Users.Inputs;
using ShopDemo.Auth.Domain.Services.Interfaces;
using ShopDemo.Auth.Infra.CrossCutting.Messages.Outbox.Interfaces;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Application.UseCases.AuthenticateUser;

public class AuthenticateUserUseCaseTests : TestBase
{
    private readonly Mock<ILogger<AuthenticateUserUseCase>> _loggerMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IAuthenticationService> _authServiceMock;
    private readonly Mock<IAuthOutboxWriter> _outboxWriterMock;
    private readonly AuthenticateUserUseCase _sut;

    public AuthenticateUserUseCaseTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _loggerMock = new Mock<ILogger<AuthenticateUserUseCase>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _authServiceMock = new Mock<IAuthenticationService>();
        _outboxWriterMock = new Mock<IAuthOutboxWriter>();

        // UnitOfWork pass-through: invoke the handler delegate
        _unitOfWorkMock
            .Setup(static x => x.ExecuteAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<AuthenticateUserInput>(),
                It.IsAny<Func<ExecutionContext, AuthenticateUserInput, CancellationToken, Task<bool>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<ExecutionContext, AuthenticateUserInput,
                Func<ExecutionContext, AuthenticateUserInput, CancellationToken, Task<bool>>,
                CancellationToken>(
                static async (ctx, input, handler, ct) => await handler(ctx, input, ct));

        _sut = new AuthenticateUserUseCase(
            _loggerMock.Object,
            _unitOfWorkMock.Object,
            _authServiceMock.Object,
            _outboxWriterMock.Object,
            TimeProvider.System);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrow()
    {
        LogAct("Creating use case with null logger");
        LogAssert("Verifying ArgumentNullException is thrown");
        Should.Throw<ArgumentNullException>(() =>
            new AuthenticateUserUseCase(null!, _unitOfWorkMock.Object, _authServiceMock.Object,
                _outboxWriterMock.Object, TimeProvider.System));
    }

    [Fact]
    public void Constructor_WithNullUnitOfWork_ShouldThrow()
    {
        LogAct("Creating use case with null unit of work");
        LogAssert("Verifying ArgumentNullException is thrown");
        Should.Throw<ArgumentNullException>(() =>
            new AuthenticateUserUseCase(_loggerMock.Object, null!, _authServiceMock.Object,
                _outboxWriterMock.Object, TimeProvider.System));
    }

    [Fact]
    public void Constructor_WithNullAuthService_ShouldThrow()
    {
        LogAct("Creating use case with null auth service");
        LogAssert("Verifying ArgumentNullException is thrown");
        Should.Throw<ArgumentNullException>(() =>
            new AuthenticateUserUseCase(_loggerMock.Object, _unitOfWorkMock.Object, null!,
                _outboxWriterMock.Object, TimeProvider.System));
    }

    [Fact]
    public void Constructor_WithNullOutboxWriter_ShouldThrow()
    {
        LogAct("Creating use case with null outbox writer");
        LogAssert("Verifying ArgumentNullException is thrown");
        Should.Throw<ArgumentNullException>(() =>
            new AuthenticateUserUseCase(_loggerMock.Object, _unitOfWorkMock.Object,
                _authServiceMock.Object, null!, TimeProvider.System));
    }

    [Fact]
    public void Constructor_WithNullTimeProvider_ShouldThrow()
    {
        LogAct("Creating use case with null time provider");
        LogAssert("Verifying ArgumentNullException is thrown");
        Should.Throw<ArgumentNullException>(() =>
            new AuthenticateUserUseCase(_loggerMock.Object, _unitOfWorkMock.Object,
                _authServiceMock.Object, _outboxWriterMock.Object, null!));
    }

    #endregion

    #region Interface Implementation

    [Fact]
    public void ShouldImplementIAuthenticateUserUseCase()
    {
        LogAssert("Verifying interface implementation");
        _sut.ShouldBeAssignableTo<IAuthenticateUserUseCase>();
    }

    #endregion

    #region ExecuteAsync Tests

    [Fact]
    public async Task ExecuteAsync_WhenAuthenticationSucceeds_ShouldReturnOutput()
    {
        // Arrange
        LogArrange("Setting up successful authentication");
        var executionContext = CreateTestExecutionContext();
        var input = new AuthenticateUserInput("test@example.com", "SecurePassword123!");
        var user = CreateTestUser(executionContext);

        _authServiceMock
            .Setup(x => x.VerifyCredentialsAsync(executionContext, input.Email, input.Password,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        LogAct("Executing use case");
        var result = await _sut.ExecuteAsync(executionContext, input, CancellationToken.None);

        // Assert
        LogAssert("Verifying output with correct UserId and Email");
        result.ShouldNotBeNull();
        result.UserId.ShouldBe(user.EntityInfo.Id.Value);
        result.Email.ShouldBe(user.Email.Value ?? string.Empty);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAuthenticationSucceeds_ShouldEnqueueEvent()
    {
        // Arrange
        LogArrange("Setting up successful authentication");
        var executionContext = CreateTestExecutionContext();
        var input = new AuthenticateUserInput("test@example.com", "SecurePassword123!");
        var user = CreateTestUser(executionContext);

        _authServiceMock
            .Setup(x => x.VerifyCredentialsAsync(executionContext, input.Email, input.Password,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        LogAct("Executing use case");
        await _sut.ExecuteAsync(executionContext, input, CancellationToken.None);

        // Assert
        LogAssert("Verifying outbox EnqueueAsync was called once");
        _outboxWriterMock.Verify(
            static x => x.EnqueueAsync(It.IsAny<Bedrock.BuildingBlocks.Messages.MessageBase>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAuthenticationFails_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up failed authentication (auth service returns null)");
        var executionContext = CreateTestExecutionContext();
        var input = new AuthenticateUserInput("test@example.com", "WrongPassword!");

        _authServiceMock
            .Setup(x => x.VerifyCredentialsAsync(executionContext, input.Email, input.Password,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        LogAct("Executing use case");
        var result = await _sut.ExecuteAsync(executionContext, input, CancellationToken.None);

        // Assert
        LogAssert("Verifying null returned");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenAuthenticationFails_ShouldAddErrorMessage()
    {
        // Arrange
        LogArrange("Setting up failed authentication with no pre-existing errors");
        var executionContext = CreateTestExecutionContext();
        var input = new AuthenticateUserInput("test@example.com", "WrongPassword!");

        _authServiceMock
            .Setup(x => x.VerifyCredentialsAsync(executionContext, input.Email, input.Password,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        LogAct("Executing use case");
        await _sut.ExecuteAsync(executionContext, input, CancellationToken.None);

        // Assert
        LogAssert("Verifying error message was added to context");
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WhenAuthenticationFails_ShouldNotEnqueueEvent()
    {
        // Arrange
        LogArrange("Setting up failed authentication");
        var executionContext = CreateTestExecutionContext();
        var input = new AuthenticateUserInput("test@example.com", "WrongPassword!");

        _authServiceMock
            .Setup(x => x.VerifyCredentialsAsync(executionContext, input.Email, input.Password,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        LogAct("Executing use case");
        await _sut.ExecuteAsync(executionContext, input, CancellationToken.None);

        // Assert
        LogAssert("Verifying outbox EnqueueAsync was never called");
        _outboxWriterMock.Verify(
            static x => x.EnqueueAsync(It.IsAny<Bedrock.BuildingBlocks.Messages.MessageBase>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Helper Methods

    private static ExecutionContext CreateTestExecutionContext()
    {
        var tenantInfo = TenantInfo.Create(Guid.NewGuid(), "Test Tenant");
        return ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: tenantInfo,
            executionUser: "test.user",
            executionOrigin: "UnitTest",
            businessOperationCode: "TEST_OP",
            minimumMessageType: MessageType.Trace,
            timeProvider: TimeProvider.System);
    }

    private static User CreateTestUser(ExecutionContext executionContext)
    {
        var email = EmailAddress.CreateNew("test@example.com");
        var passwordHash = PasswordHash.CreateNew(CreateValidHashBytes());
        var input = new RegisterNewInput(email, passwordHash);
        return User.RegisterNew(executionContext, input)!;
    }

    private static byte[] CreateValidHashBytes()
    {
        byte[] bytes = new byte[49];
        bytes[0] = 1;
        for (int i = 1; i < bytes.Length; i++) bytes[i] = (byte)(i % 256);
        return bytes;
    }

    #endregion
}
