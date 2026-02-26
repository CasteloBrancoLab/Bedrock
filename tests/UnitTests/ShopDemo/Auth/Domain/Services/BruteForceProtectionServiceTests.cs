using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Testing;
using Moq;
using ShopDemo.Auth.Domain.Entities.LoginAttempts;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Services;
using ShopDemo.Auth.Domain.Services.Interfaces;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Services;

public class BruteForceProtectionServiceTests : TestBase
{
    private readonly Mock<ILoginAttemptRepository> _loginAttemptRepositoryMock;
    private readonly BruteForceProtectionService _sut;

    public BruteForceProtectionServiceTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _loginAttemptRepositoryMock = new Mock<ILoginAttemptRepository>();
        _sut = new BruteForceProtectionService(_loginAttemptRepositoryMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullRepository_ShouldThrow()
    {
        // Act & Assert
        LogAct("Creating BruteForceProtectionService with null repository");
        LogAssert("Verifying ArgumentNullException is thrown");
        Should.Throw<ArgumentNullException>(() => new BruteForceProtectionService(null!));
    }

    #endregion

    #region Interface Implementation

    [Fact]
    public void ShouldImplementIBruteForceProtectionService()
    {
        LogAssert("Verifying interface implementation");
        _sut.ShouldBeAssignableTo<IBruteForceProtectionService>();
    }

    #endregion

    #region RecordLoginAttemptAsync Tests

    [Fact]
    public async Task RecordLoginAttemptAsync_WithValidData_ShouldReturnLoginAttempt()
    {
        // Arrange
        LogArrange("Setting up repository for successful registration");
        var executionContext = CreateTestExecutionContext();
        _loginAttemptRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, It.IsAny<LoginAttempt>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Recording successful login attempt");
        var result = await _sut.RecordLoginAttemptAsync(executionContext, "testuser", "127.0.0.1", true, null, CancellationToken.None);

        // Assert
        LogAssert("Verifying login attempt was returned");
        result.ShouldNotBeNull();
        _loginAttemptRepositoryMock.Verify(
            x => x.RegisterNewAsync(executionContext, It.IsAny<LoginAttempt>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RecordLoginAttemptAsync_WhenRepositoryFails_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up repository to fail");
        var executionContext = CreateTestExecutionContext();
        _loginAttemptRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, It.IsAny<LoginAttempt>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Recording login attempt with repository failure");
        var result = await _sut.RecordLoginAttemptAsync(executionContext, "testuser", "127.0.0.1", false, "invalid_password", CancellationToken.None);

        // Assert
        LogAssert("Verifying null returned");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task RecordLoginAttemptAsync_WithNullUsername_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up with null username to trigger entity creation failure");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Recording login attempt with null username");
        var result = await _sut.RecordLoginAttemptAsync(executionContext, null!, null, false, null, CancellationToken.None);

        // Assert
        LogAssert("Verifying null returned because LoginAttempt.RegisterNew returns null");
        result.ShouldBeNull();
    }

    #endregion

    #region IsLockedOutAsync Tests

    [Fact]
    public async Task IsLockedOutAsync_WithNoRecentAttempts_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up repository with no recent attempts");
        var executionContext = CreateTestExecutionContext();
        _loginAttemptRepositoryMock
            .Setup(x => x.GetRecentByUsernameAsync(executionContext, "testuser", It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LoginAttempt>());

        // Act
        LogAct("Checking if user is locked out");
        var result = await _sut.IsLockedOutAsync(executionContext, "testuser", CancellationToken.None);

        // Assert
        LogAssert("Verifying user is not locked out");
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task IsLockedOutAsync_WithFiveFailedAttempts_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Setting up repository with 5 failed login attempts");
        var executionContext = CreateTestExecutionContext();
        var failedAttempts = CreateFailedLoginAttempts(executionContext, 5);

        _loginAttemptRepositoryMock
            .Setup(x => x.GetRecentByUsernameAsync(executionContext, "testuser", It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedAttempts);

        // Act
        LogAct("Checking if user is locked out");
        var result = await _sut.IsLockedOutAsync(executionContext, "testuser", CancellationToken.None);

        // Assert
        LogAssert("Verifying user is locked out");
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task IsLockedOutAsync_WithFourFailedAttempts_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up repository with 4 failed login attempts");
        var executionContext = CreateTestExecutionContext();
        var failedAttempts = CreateFailedLoginAttempts(executionContext, 4);

        _loginAttemptRepositoryMock
            .Setup(x => x.GetRecentByUsernameAsync(executionContext, "testuser", It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedAttempts);

        // Act
        LogAct("Checking if user is locked out");
        var result = await _sut.IsLockedOutAsync(executionContext, "testuser", CancellationToken.None);

        // Assert
        LogAssert("Verifying user is not locked out");
        result.ShouldBeFalse();
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

    private static List<LoginAttempt> CreateFailedLoginAttempts(ExecutionContext executionContext, int count)
    {
        var attempts = new List<LoginAttempt>();
        for (int i = 0; i < count; i++)
        {
            var attempt = LoginAttempt.RegisterNew(
                executionContext,
                new ShopDemo.Auth.Domain.Entities.LoginAttempts.Inputs.RegisterNewLoginAttemptInput(
                    "testuser", "127.0.0.1", false, "invalid_password"));
            if (attempt is not null)
                attempts.Add(attempt);
        }
        return attempts;
    }

    #endregion
}
