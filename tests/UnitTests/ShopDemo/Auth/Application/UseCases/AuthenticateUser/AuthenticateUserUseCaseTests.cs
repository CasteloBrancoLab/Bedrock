using Bedrock.BuildingBlocks.Core.EmailAddresses;
using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using ShopDemo.Auth.Application.UseCases.AuthenticateUser;
using ShopDemo.Auth.Application.UseCases.AuthenticateUser.Interfaces;
using ShopDemo.Auth.Application.UseCases.AuthenticateUser.Models;
using ShopDemo.Auth.Domain.Entities.Users;
using ShopDemo.Auth.Domain.Entities.Users.Inputs;
using ShopDemo.Auth.Domain.Services.Interfaces;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Application.UseCases.AuthenticateUser;

public class AuthenticateUserUseCaseTests : TestBase
{
    private readonly Mock<ILogger<AuthenticateUserUseCase>> _loggerMock;
    private readonly Mock<IAuthenticationService> _authServiceMock;
    private readonly AuthenticateUserUseCase _sut;

    public AuthenticateUserUseCaseTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _loggerMock = new Mock<ILogger<AuthenticateUserUseCase>>();
        _authServiceMock = new Mock<IAuthenticationService>();
        _sut = new AuthenticateUserUseCase(_loggerMock.Object, _authServiceMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrow()
    {
        LogAct("Creating use case with null logger");
        LogAssert("Verifying ArgumentNullException is thrown");
        Should.Throw<ArgumentNullException>(() =>
            new AuthenticateUserUseCase(null!, _authServiceMock.Object));
    }

    [Fact]
    public void Constructor_WithNullAuthService_ShouldThrow()
    {
        LogAct("Creating use case with null auth service");
        LogAssert("Verifying ArgumentNullException is thrown");
        Should.Throw<ArgumentNullException>(() =>
            new AuthenticateUserUseCase(_loggerMock.Object, null!));
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
