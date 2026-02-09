using Bedrock.BuildingBlocks.Core.EmailAddresses;
using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Security.Passwords;
using Bedrock.BuildingBlocks.Security.Passwords.Interfaces;
using Bedrock.BuildingBlocks.Testing;
using Moq;
using ShopDemo.Auth.Domain.Entities.Users;
using ShopDemo.Auth.Domain.Entities.Users.Inputs;
using ShopDemo.Auth.Domain.Repositories;
using ShopDemo.Auth.Domain.Services;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Services;

public class AuthenticationServiceTests : TestBase
{
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly AuthenticationService _sut;

    public AuthenticationServiceTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _sut = new AuthenticationService(_passwordHasherMock.Object, _userRepositoryMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullPasswordHasher_ShouldThrow()
    {
        // Arrange
        LogArrange("Preparing null password hasher");

        // Act & Assert
        LogAct("Creating AuthenticationService with null hasher");
        LogAssert("Verifying ArgumentNullException is thrown");
        Should.Throw<ArgumentNullException>(() => new AuthenticationService(null!, _userRepositoryMock.Object));
    }

    [Fact]
    public void Constructor_WithNullRepository_ShouldThrow()
    {
        // Arrange
        LogArrange("Preparing null repository");

        // Act & Assert
        LogAct("Creating AuthenticationService with null repository");
        LogAssert("Verifying ArgumentNullException is thrown");
        Should.Throw<ArgumentNullException>(() => new AuthenticationService(_passwordHasherMock.Object, null!));
    }

    #endregion

    #region RegisterUserAsync Tests

    [Fact]
    public async Task RegisterUserAsync_WithValidCredentials_ShouldReturnUser()
    {
        // Arrange
        LogArrange("Setting up mocks for valid registration");
        var executionContext = CreateTestExecutionContext();
        string email = "test@example.com";
        string password = "ValidPassword1!";
        byte[] hashBytes = CreateValidHashBytes();

        _passwordHasherMock
            .Setup(x => x.HashPassword(executionContext, password))
            .Returns(new PasswordHashResult(hashBytes, 1));

        _userRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Registering new user");
        var result = await _sut.RegisterUserAsync(executionContext, email, password, CancellationToken.None);

        // Assert
        LogAssert("Verifying user was created");
        result.ShouldNotBeNull();
        result.Email.Value.ShouldBe("test@example.com");
        result.Status.ShouldBe(ShopDemo.Auth.Domain.Entities.Users.Enums.UserStatus.Active);
        _passwordHasherMock.Verify(x => x.HashPassword(executionContext, password), Times.Once);
        _userRepositoryMock.Verify(x => x.RegisterNewAsync(executionContext, It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_WithInvalidPassword_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up for password policy violation");
        var executionContext = CreateTestExecutionContext();
        string email = "test@example.com";
        string password = "short"; // too short for policy

        // Act
        LogAct("Registering with short password");
        var result = await _sut.RegisterUserAsync(executionContext, email, password, CancellationToken.None);

        // Assert
        LogAssert("Verifying null returned and hasher not called");
        result.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
        _passwordHasherMock.Verify(x => x.HashPassword(It.IsAny<ExecutionContext>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RegisterUserAsync_WhenRepositoryFails_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up mocks for repository failure");
        var executionContext = CreateTestExecutionContext();
        string email = "test@example.com";
        string password = "ValidPassword1!";
        byte[] hashBytes = CreateValidHashBytes();

        _passwordHasherMock
            .Setup(x => x.HashPassword(executionContext, password))
            .Returns(new PasswordHashResult(hashBytes, 1));

        _userRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Registering user when repository fails");
        var result = await _sut.RegisterUserAsync(executionContext, email, password, CancellationToken.None);

        // Assert
        LogAssert("Verifying null returned");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task RegisterUserAsync_WithNullPassword_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up for null password");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Registering with null password");
        var result = await _sut.RegisterUserAsync(executionContext, "test@example.com", null!, CancellationToken.None);

        // Assert
        LogAssert("Verifying null returned");
        result.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region VerifyCredentialsAsync Tests

    [Fact]
    public async Task VerifyCredentialsAsync_WithCorrectCredentials_ShouldReturnUser()
    {
        // Arrange
        LogArrange("Setting up mocks for valid credential verification");
        var executionContext = CreateTestExecutionContext();
        string email = "test@example.com";
        string password = "CorrectPassword!";
        byte[] hashBytes = CreateValidHashBytes();

        var user = CreateTestUser(executionContext);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(executionContext, It.IsAny<EmailAddress>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword(executionContext, password, It.IsAny<byte[]>()))
            .Returns(new PasswordVerificationResult(true, false));

        // Act
        LogAct("Verifying correct credentials");
        var result = await _sut.VerifyCredentialsAsync(executionContext, email, password, CancellationToken.None);

        // Assert
        LogAssert("Verifying user returned");
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task VerifyCredentialsAsync_WithWrongPassword_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up mocks for wrong password");
        var executionContext = CreateTestExecutionContext();
        string email = "test@example.com";
        string password = "WrongPassword!";

        var user = CreateTestUser(executionContext);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(executionContext, It.IsAny<EmailAddress>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword(executionContext, password, It.IsAny<byte[]>()))
            .Returns(new PasswordVerificationResult(false, false));

        // Act
        LogAct("Verifying with wrong password");
        var result = await _sut.VerifyCredentialsAsync(executionContext, email, password, CancellationToken.None);

        // Assert
        LogAssert("Verifying null returned with generic error");
        result.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public async Task VerifyCredentialsAsync_WithNonExistentEmail_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up mocks for non-existent email");
        var executionContext = CreateTestExecutionContext();
        string email = "nonexistent@example.com";
        string password = "AnyPassword!";

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(executionContext, It.IsAny<EmailAddress>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        LogAct("Verifying with non-existent email");
        var result = await _sut.VerifyCredentialsAsync(executionContext, email, password, CancellationToken.None);

        // Assert
        LogAssert("Verifying null returned with generic error (anti-enumeration)");
        result.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
        _passwordHasherMock.Verify(x => x.VerifyPassword(It.IsAny<ExecutionContext>(), It.IsAny<string>(), It.IsAny<byte[]>()), Times.Never);
    }

    [Fact]
    public async Task VerifyCredentialsAsync_WhenNeedsRehash_ShouldRehash()
    {
        // Arrange
        LogArrange("Setting up mocks for pepper rotation scenario");
        var executionContext = CreateTestExecutionContext();
        string email = "test@example.com";
        string password = "CorrectPassword!";
        byte[] newHashBytes = CreateValidHashBytes();

        var user = CreateTestUser(executionContext);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(executionContext, It.IsAny<EmailAddress>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword(executionContext, password, It.IsAny<byte[]>()))
            .Returns(new PasswordVerificationResult(true, true));

        _passwordHasherMock
            .Setup(x => x.HashPassword(executionContext, password))
            .Returns(new PasswordHashResult(newHashBytes, 2));

        // Act
        LogAct("Verifying credentials with pepper needing rehash");
        var result = await _sut.VerifyCredentialsAsync(executionContext, email, password, CancellationToken.None);

        // Assert
        LogAssert("Verifying user returned with rehashed password");
        result.ShouldNotBeNull();
        _passwordHasherMock.Verify(x => x.HashPassword(executionContext, password), Times.Once);
    }

    [Fact]
    public async Task VerifyCredentialsAsync_WhenNotNeedsRehash_ShouldNotRehash()
    {
        // Arrange
        LogArrange("Setting up mocks for current pepper version");
        var executionContext = CreateTestExecutionContext();
        string email = "test@example.com";
        string password = "CorrectPassword!";

        var user = CreateTestUser(executionContext);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(executionContext, It.IsAny<EmailAddress>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword(executionContext, password, It.IsAny<byte[]>()))
            .Returns(new PasswordVerificationResult(true, false));

        // Act
        LogAct("Verifying credentials without rehash needed");
        var result = await _sut.VerifyCredentialsAsync(executionContext, email, password, CancellationToken.None);

        // Assert
        LogAssert("Verifying no rehash happened");
        result.ShouldNotBeNull();
        _passwordHasherMock.Verify(x => x.HashPassword(It.IsAny<ExecutionContext>(), It.IsAny<string>()), Times.Never);
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
        bytes[0] = 1; // pepper version
        for (int i = 1; i < bytes.Length; i++)
        {
            bytes[i] = (byte)(i % 256);
        }
        return bytes;
    }

    #endregion
}
