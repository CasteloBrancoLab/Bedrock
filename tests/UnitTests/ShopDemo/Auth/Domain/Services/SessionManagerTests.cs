using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Testing;
using Moq;
using ShopDemo.Auth.Domain.Entities.Sessions;
using ShopDemo.Auth.Domain.Entities.Sessions.Enums;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Services;
using ShopDemo.Auth.Domain.Services.Interfaces;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Services;

public class SessionManagerTests : TestBase
{
    private readonly Mock<ISessionRepository> _sessionRepositoryMock;
    private readonly SessionManager _sut;

    public SessionManagerTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _sessionRepositoryMock = new Mock<ISessionRepository>();
        _sut = new SessionManager(_sessionRepositoryMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullRepository_ShouldThrow()
    {
        // Act & Assert
        LogAct("Creating SessionManager with null repository");
        LogAssert("Verifying ArgumentNullException is thrown");
        Should.Throw<ArgumentNullException>(() => new SessionManager(null!));
    }

    #endregion

    #region Interface Implementation

    [Fact]
    public void ShouldImplementISessionManager()
    {
        LogAssert("Verifying interface implementation");
        _sut.ShouldBeAssignableTo<ISessionManager>();
    }

    #endregion

    #region CreateSessionAsync Tests

    [Fact]
    public async Task CreateSessionAsync_UnderLimit_ShouldCreateSession()
    {
        // Arrange
        LogArrange("Setting up repository under session limit");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var refreshTokenId = Id.GenerateNewId();

        _sessionRepositoryMock
            .Setup(x => x.CountActiveByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        _sessionRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, It.IsAny<Session>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Creating session under limit");
        var result = await _sut.CreateSessionAsync(
            executionContext, userId, refreshTokenId,
            "Chrome", "127.0.0.1", "Mozilla/5.0",
            executionContext.Timestamp.AddHours(24), 5,
            SessionLimitStrategy.RejectNew, CancellationToken.None);

        // Assert
        LogAssert("Verifying session was created");
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task CreateSessionAsync_AtLimitWithRejectNew_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up repository at session limit with RejectNew strategy");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var refreshTokenId = Id.GenerateNewId();

        _sessionRepositoryMock
            .Setup(x => x.CountActiveByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        // Act
        LogAct("Creating session at limit with RejectNew");
        var result = await _sut.CreateSessionAsync(
            executionContext, userId, refreshTokenId,
            "Chrome", "127.0.0.1", "Mozilla/5.0",
            executionContext.Timestamp.AddHours(24), 5,
            SessionLimitStrategy.RejectNew, CancellationToken.None);

        // Assert
        LogAssert("Verifying null returned with error");
        result.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public async Task CreateSessionAsync_AtLimitWithRevokeOldest_WhenNoActiveSessions_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up at limit with RevokeOldest but no active sessions to revoke");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var refreshTokenId = Id.GenerateNewId();

        _sessionRepositoryMock
            .Setup(x => x.CountActiveByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        _sessionRepositoryMock
            .Setup(x => x.GetActiveByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Session>());

        // Act
        LogAct("Creating session at limit with RevokeOldest and no sessions to revoke");
        var result = await _sut.CreateSessionAsync(
            executionContext, userId, refreshTokenId,
            "Chrome", "127.0.0.1", "Mozilla/5.0",
            executionContext.Timestamp.AddHours(24), 5,
            SessionLimitStrategy.RevokeOldest, CancellationToken.None);

        // Assert
        LogAssert("Verifying null returned");
        result.ShouldBeNull();
    }

    #endregion

    #region RevokeSessionAsync Tests

    [Fact]
    public async Task RevokeSessionAsync_WhenSessionNotFound_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up repository with no session");
        var executionContext = CreateTestExecutionContext();
        var sessionId = Id.GenerateNewId();

        _sessionRepositoryMock
            .Setup(x => x.GetByIdAsync(executionContext, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Session?)null);

        // Act
        LogAct("Revoking non-existent session");
        var result = await _sut.RevokeSessionAsync(executionContext, sessionId, CancellationToken.None);

        // Assert
        LogAssert("Verifying null returned");
        result.ShouldBeNull();
    }

    #endregion

    #region RevokeAllSessionsAsync Tests

    [Fact]
    public async Task RevokeAllSessionsAsync_WithNoActiveSessions_ShouldReturnZero()
    {
        // Arrange
        LogArrange("Setting up repository with no active sessions");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();

        _sessionRepositoryMock
            .Setup(x => x.GetActiveByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Session>());

        // Act
        LogAct("Revoking all sessions for user with none");
        var result = await _sut.RevokeAllSessionsAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verifying zero revoked");
        result.ShouldBe(0);
    }

    #endregion

    #region UpdateActivityAsync Tests

    [Fact]
    public async Task UpdateActivityAsync_WhenSessionNotFound_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up repository with no session");
        var executionContext = CreateTestExecutionContext();
        var sessionId = Id.GenerateNewId();

        _sessionRepositoryMock
            .Setup(x => x.GetByIdAsync(executionContext, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Session?)null);

        // Act
        LogAct("Updating activity for non-existent session");
        var result = await _sut.UpdateActivityAsync(executionContext, sessionId, CancellationToken.None);

        // Assert
        LogAssert("Verifying null returned");
        result.ShouldBeNull();
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
