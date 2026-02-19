using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Testing;
using Moq;
using ShopDemo.Auth.Domain.Entities.DenyListEntries;
using ShopDemo.Auth.Domain.Entities.DenyListEntries.Enums;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Services;
using ShopDemo.Auth.Domain.Services.Interfaces;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Services;

public class DenyListServiceTests : TestBase
{
    private readonly Mock<IDenyListRepository> _denyListRepositoryMock;
    private readonly DenyListService _sut;

    public DenyListServiceTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _denyListRepositoryMock = new Mock<IDenyListRepository>();
        _sut = new DenyListService(_denyListRepositoryMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullRepository_ShouldThrow()
    {
        // Act & Assert
        LogAct("Creating DenyListService with null repository");
        LogAssert("Verifying ArgumentNullException is thrown");
        Should.Throw<ArgumentNullException>(() => new DenyListService(null!));
    }

    #endregion

    #region Interface Implementation

    [Fact]
    public void ShouldImplementIDenyListService()
    {
        LogAssert("Verifying interface implementation");
        _sut.ShouldBeAssignableTo<IDenyListService>();
    }

    #endregion

    #region RevokeTokenAsync Tests

    [Fact]
    public async Task RevokeTokenAsync_WhenAlreadyRevoked_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Setting up repository to return already revoked");
        var executionContext = CreateTestExecutionContext();
        _denyListRepositoryMock
            .Setup(x => x.ExistsByTypeAndValueAsync(executionContext, DenyListEntryType.Jti, "test-jti", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Revoking already revoked token");
        var result = await _sut.RevokeTokenAsync(executionContext, "test-jti", DateTimeOffset.UtcNow.AddHours(1), "test", CancellationToken.None);

        // Assert
        LogAssert("Verifying returns true without creating new entry");
        result.ShouldBeTrue();
        _denyListRepositoryMock.Verify(
            x => x.RegisterNewAsync(It.IsAny<ExecutionContext>(), It.IsAny<DenyListEntry>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RevokeTokenAsync_WhenNewRevocation_ShouldCreateEntry()
    {
        // Arrange
        LogArrange("Setting up repository for new revocation");
        var executionContext = CreateTestExecutionContext();
        _denyListRepositoryMock
            .Setup(x => x.ExistsByTypeAndValueAsync(executionContext, DenyListEntryType.Jti, "test-jti", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _denyListRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, It.IsAny<DenyListEntry>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Revoking new token");
        var result = await _sut.RevokeTokenAsync(executionContext, "test-jti", DateTimeOffset.UtcNow.AddHours(1), "test", CancellationToken.None);

        // Assert
        LogAssert("Verifying entry was created and returns true");
        result.ShouldBeTrue();
        _denyListRepositoryMock.Verify(
            x => x.RegisterNewAsync(executionContext, It.IsAny<DenyListEntry>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region RevokeUserAsync Tests

    [Fact]
    public async Task RevokeUserAsync_WhenAlreadyRevoked_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Setting up repository to return already revoked");
        var executionContext = CreateTestExecutionContext();
        _denyListRepositoryMock
            .Setup(x => x.ExistsByTypeAndValueAsync(executionContext, DenyListEntryType.UserId, "user-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Revoking already revoked user");
        var result = await _sut.RevokeUserAsync(executionContext, "user-123", DateTimeOffset.UtcNow.AddDays(1), "test", CancellationToken.None);

        // Assert
        LogAssert("Verifying returns true without creating new entry");
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task RevokeUserAsync_WhenNewRevocation_ShouldCreateEntry()
    {
        // Arrange
        LogArrange("Setting up repository for new user revocation");
        var executionContext = CreateTestExecutionContext();
        _denyListRepositoryMock
            .Setup(x => x.ExistsByTypeAndValueAsync(executionContext, DenyListEntryType.UserId, "user-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _denyListRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, It.IsAny<DenyListEntry>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Revoking new user");
        var result = await _sut.RevokeUserAsync(executionContext, "user-123", DateTimeOffset.UtcNow.AddDays(1), "test", CancellationToken.None);

        // Assert
        LogAssert("Verifying entry was created");
        result.ShouldBeTrue();
    }

    #endregion

    #region IsTokenRevokedAsync Tests

    [Fact]
    public async Task IsTokenRevokedAsync_WhenRevoked_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Setting up repository with revoked token");
        var executionContext = CreateTestExecutionContext();
        _denyListRepositoryMock
            .Setup(x => x.ExistsByTypeAndValueAsync(executionContext, DenyListEntryType.Jti, "test-jti", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Checking if token is revoked");
        var result = await _sut.IsTokenRevokedAsync(executionContext, "test-jti", CancellationToken.None);

        // Assert
        LogAssert("Verifying returns true");
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task IsTokenRevokedAsync_WhenNotRevoked_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up repository with no revocation");
        var executionContext = CreateTestExecutionContext();
        _denyListRepositoryMock
            .Setup(x => x.ExistsByTypeAndValueAsync(executionContext, DenyListEntryType.Jti, "test-jti", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Checking if token is revoked");
        var result = await _sut.IsTokenRevokedAsync(executionContext, "test-jti", CancellationToken.None);

        // Assert
        LogAssert("Verifying returns false");
        result.ShouldBeFalse();
    }

    #endregion

    #region IsUserRevokedAsync Tests

    [Fact]
    public async Task IsUserRevokedAsync_WhenRevoked_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Setting up repository with revoked user");
        var executionContext = CreateTestExecutionContext();
        _denyListRepositoryMock
            .Setup(x => x.ExistsByTypeAndValueAsync(executionContext, DenyListEntryType.UserId, "user-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Checking if user is revoked");
        var result = await _sut.IsUserRevokedAsync(executionContext, "user-123", CancellationToken.None);

        // Assert
        LogAssert("Verifying returns true");
        result.ShouldBeTrue();
    }

    #endregion

    #region CleanupExpiredAsync Tests

    [Fact]
    public async Task CleanupExpiredAsync_ShouldDelegateToRepository()
    {
        // Arrange
        LogArrange("Setting up repository to return cleanup count");
        var executionContext = CreateTestExecutionContext();
        _denyListRepositoryMock
            .Setup(x => x.DeleteExpiredAsync(executionContext, executionContext.Timestamp, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        // Act
        LogAct("Cleaning up expired entries");
        var result = await _sut.CleanupExpiredAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying count returned from repository");
        result.ShouldBe(5);
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

    #endregion
}
