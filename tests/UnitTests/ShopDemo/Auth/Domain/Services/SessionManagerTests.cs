using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Testing;
using Moq;
using ShopDemo.Auth.Domain.Entities.Sessions;
using ShopDemo.Auth.Domain.Entities.Sessions.Enums;
using ShopDemo.Auth.Domain.Entities.Sessions.Inputs;
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

    [Fact]
    public async Task CreateSessionAsync_AtLimitWithRevokeOldest_WhenSuccessful_ShouldCreateSession()
    {
        // Arrange
        LogArrange("Setting up at limit with RevokeOldest and existing active session");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var refreshTokenId = Id.GenerateNewId();

        var existingSession = Session.RegisterNew(executionContext,
            new RegisterNewSessionInput(userId, Id.GenerateNewId(), "Old Device", "127.0.0.1", "OldAgent",
                executionContext.Timestamp.AddHours(24)));

        _sessionRepositoryMock
            .Setup(x => x.CountActiveByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        _sessionRepositoryMock
            .Setup(x => x.GetActiveByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Session> { existingSession! });

        _sessionRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, It.IsAny<Session>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _sessionRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, It.IsAny<Session>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Creating session with RevokeOldest strategy");
        var result = await _sut.CreateSessionAsync(
            executionContext, userId, refreshTokenId,
            "Chrome", "127.0.0.1", "Mozilla/5.0",
            executionContext.Timestamp.AddHours(24), 5,
            SessionLimitStrategy.RevokeOldest, CancellationToken.None);

        // Assert
        LogAssert("Verifying session was created after revoking oldest");
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task CreateSessionAsync_WhenRegisterNewReturnsNull_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up with DeviceInfo exceeding max length to trigger RegisterNew failure");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var refreshTokenId = Id.GenerateNewId();

        _sessionRepositoryMock
            .Setup(x => x.CountActiveByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        LogAct("Creating session with DeviceInfo exceeding max length (500)");
        var result = await _sut.CreateSessionAsync(
            executionContext, userId, refreshTokenId,
            new string('x', 501), "127.0.0.1", "Mozilla/5.0",
            executionContext.Timestamp.AddHours(24), 5,
            SessionLimitStrategy.RejectNew, CancellationToken.None);

        // Assert
        LogAssert("Verifying null returned when Session.RegisterNew fails validation");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task CreateSessionAsync_WhenRegistrationFails_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up under limit but registration fails");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var refreshTokenId = Id.GenerateNewId();

        _sessionRepositoryMock
            .Setup(x => x.CountActiveByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        _sessionRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, It.IsAny<Session>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Creating session when registration fails");
        var result = await _sut.CreateSessionAsync(
            executionContext, userId, refreshTokenId,
            "Chrome", "127.0.0.1", "Mozilla/5.0",
            executionContext.Timestamp.AddHours(24), 5,
            SessionLimitStrategy.RejectNew, CancellationToken.None);

        // Assert
        LogAssert("Verifying null returned");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task CreateSessionAsync_AtLimitWithRevokeOldest_ShouldFindAndRevokeOldestSession()
    {
        // Arrange
        LogArrange("Setting up with multiple sessions where second is older");
        var tenantId = Guid.NewGuid();
        var newerTime = DateTimeOffset.UtcNow;
        var olderTime = newerTime.AddDays(-1);

        var executionContext = CreateTestExecutionContextWithTenant(tenantId, DateTimeOffset.UtcNow);
        var newerContext = CreateTestExecutionContextWithTenant(tenantId, newerTime);
        var olderContext = CreateTestExecutionContextWithTenant(tenantId, olderTime);

        var userId = Id.GenerateNewId();
        var refreshTokenId = Id.GenerateNewId();

        var newerSession = Session.RegisterNew(newerContext,
            new RegisterNewSessionInput(userId, Id.GenerateNewId(), "NewDevice", "127.0.0.1", "Agent1",
                newerContext.Timestamp.AddHours(24)));
        var olderSession = Session.RegisterNew(olderContext,
            new RegisterNewSessionInput(userId, Id.GenerateNewId(), "OldDevice", "127.0.0.2", "Agent2",
                olderContext.Timestamp.AddHours(24)));

        _sessionRepositoryMock
            .Setup(x => x.CountActiveByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        _sessionRepositoryMock
            .Setup(x => x.GetActiveByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Session> { newerSession!, olderSession! });

        _sessionRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, It.IsAny<Session>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _sessionRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, It.IsAny<Session>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Creating session with RevokeOldest and multiple sessions");
        var result = await _sut.CreateSessionAsync(
            executionContext, userId, refreshTokenId,
            "Chrome", "127.0.0.1", "Mozilla/5.0",
            executionContext.Timestamp.AddHours(24), 5,
            SessionLimitStrategy.RevokeOldest, CancellationToken.None);

        // Assert
        LogAssert("Verifying session was created after finding and revoking oldest");
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task CreateSessionAsync_AtLimitWithRevokeOldest_WhenRevokeReturnsNull_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up at limit with RevokeOldest where oldest session has different tenant");
        var executionContext = CreateTestExecutionContext();
        var differentContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var refreshTokenId = Id.GenerateNewId();

        var session = Session.RegisterNew(differentContext,
            new RegisterNewSessionInput(userId, Id.GenerateNewId(), "Device", "127.0.0.1", "Agent",
                differentContext.Timestamp.AddHours(24)));

        _sessionRepositoryMock
            .Setup(x => x.CountActiveByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        _sessionRepositoryMock
            .Setup(x => x.GetActiveByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Session> { session! });

        // Act
        LogAct("Creating session when oldest session revoke returns null");
        var result = await _sut.CreateSessionAsync(
            executionContext, userId, refreshTokenId,
            "Chrome", "127.0.0.1", "Mozilla/5.0",
            executionContext.Timestamp.AddHours(24), 5,
            SessionLimitStrategy.RevokeOldest, CancellationToken.None);

        // Assert
        LogAssert("Verifying null returned when oldest can't be revoked");
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

    [Fact]
    public async Task RevokeSessionAsync_WhenSessionExists_ShouldReturnRevokedSession()
    {
        // Arrange
        LogArrange("Setting up with active session");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var session = Session.RegisterNew(executionContext,
            new RegisterNewSessionInput(userId, Id.GenerateNewId(), "Device", "127.0.0.1", "Agent",
                executionContext.Timestamp.AddHours(24)));
        var sessionId = session!.EntityInfo.Id;

        _sessionRepositoryMock
            .Setup(x => x.GetByIdAsync(executionContext, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _sessionRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, It.IsAny<Session>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Revoking existing session");
        var result = await _sut.RevokeSessionAsync(executionContext, sessionId, CancellationToken.None);

        // Assert
        LogAssert("Verifying revoked session returned");
        result.ShouldNotBeNull();
        result.Status.ShouldBe(SessionStatus.Revoked);
    }

    [Fact]
    public async Task RevokeSessionAsync_WhenRevokeReturnsNull_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up with session from different tenant (Revoke returns null)");
        var executionContext = CreateTestExecutionContext();
        var differentContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var session = Session.RegisterNew(differentContext,
            new RegisterNewSessionInput(userId, Id.GenerateNewId(), "Device", "127.0.0.1", "Agent",
                differentContext.Timestamp.AddHours(24)));
        var sessionId = session!.EntityInfo.Id;

        _sessionRepositoryMock
            .Setup(x => x.GetByIdAsync(executionContext, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        LogAct("Revoking session when Revoke returns null due to tenant mismatch");
        var result = await _sut.RevokeSessionAsync(executionContext, sessionId, CancellationToken.None);

        // Assert
        LogAssert("Verifying null returned");
        result.ShouldBeNull();
        _sessionRepositoryMock.Verify(
            x => x.UpdateAsync(executionContext, It.IsAny<Session>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RevokeSessionAsync_WhenUpdateFails_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up with active session where update fails");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var session = Session.RegisterNew(executionContext,
            new RegisterNewSessionInput(userId, Id.GenerateNewId(), "Device", "127.0.0.1", "Agent",
                executionContext.Timestamp.AddHours(24)));
        var sessionId = session!.EntityInfo.Id;

        _sessionRepositoryMock
            .Setup(x => x.GetByIdAsync(executionContext, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _sessionRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, It.IsAny<Session>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Revoking session when update fails");
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

    [Fact]
    public async Task RevokeAllSessionsAsync_WhenRevokeReturnsNull_ShouldSkip()
    {
        // Arrange
        LogArrange("Setting up with session from different tenant (Revoke returns null)");
        var executionContext = CreateTestExecutionContext();
        var differentContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();

        var session = Session.RegisterNew(differentContext,
            new RegisterNewSessionInput(userId, Id.GenerateNewId(), "Device", "127.0.0.1", "Agent",
                differentContext.Timestamp.AddHours(24)));

        _sessionRepositoryMock
            .Setup(x => x.GetActiveByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Session> { session! });

        // Act
        LogAct("Revoking all sessions when Revoke returns null");
        var result = await _sut.RevokeAllSessionsAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verifying zero sessions revoked");
        result.ShouldBe(0);
        _sessionRepositoryMock.Verify(
            x => x.UpdateAsync(executionContext, It.IsAny<Session>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RevokeAllSessionsAsync_WhenUpdateFails_ShouldNotCount()
    {
        // Arrange
        LogArrange("Setting up with active session where update fails");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();

        var session = Session.RegisterNew(executionContext,
            new RegisterNewSessionInput(userId, Id.GenerateNewId(), "Device", "127.0.0.1", "Agent",
                executionContext.Timestamp.AddHours(24)));

        _sessionRepositoryMock
            .Setup(x => x.GetActiveByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Session> { session! });

        _sessionRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, It.IsAny<Session>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Revoking all sessions when update fails");
        var result = await _sut.RevokeAllSessionsAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verifying zero counted when update fails");
        result.ShouldBe(0);
    }

    [Fact]
    public async Task RevokeAllSessionsAsync_WithActiveSessions_ShouldRevokeAll()
    {
        // Arrange
        LogArrange("Setting up with 2 active sessions");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();

        var session1 = Session.RegisterNew(executionContext,
            new RegisterNewSessionInput(userId, Id.GenerateNewId(), "Device1", "127.0.0.1", "Agent1",
                executionContext.Timestamp.AddHours(24)));
        var session2 = Session.RegisterNew(executionContext,
            new RegisterNewSessionInput(userId, Id.GenerateNewId(), "Device2", "127.0.0.2", "Agent2",
                executionContext.Timestamp.AddHours(24)));

        _sessionRepositoryMock
            .Setup(x => x.GetActiveByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Session> { session1!, session2! });

        _sessionRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, It.IsAny<Session>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Revoking all sessions");
        var result = await _sut.RevokeAllSessionsAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verifying 2 sessions revoked");
        result.ShouldBe(2);
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

    [Fact]
    public async Task UpdateActivityAsync_WhenSessionExists_ShouldReturnUpdatedSession()
    {
        // Arrange
        LogArrange("Setting up with active session");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var session = Session.RegisterNew(executionContext,
            new RegisterNewSessionInput(userId, Id.GenerateNewId(), "Device", "127.0.0.1", "Agent",
                executionContext.Timestamp.AddHours(24)));
        var sessionId = session!.EntityInfo.Id;

        _sessionRepositoryMock
            .Setup(x => x.GetByIdAsync(executionContext, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _sessionRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, It.IsAny<Session>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Updating activity for existing session");
        var result = await _sut.UpdateActivityAsync(executionContext, sessionId, CancellationToken.None);

        // Assert
        LogAssert("Verifying updated session returned");
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task UpdateActivityAsync_WhenUpdateActivityReturnsNull_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up with session from different tenant (UpdateActivity returns null)");
        var executionContext = CreateTestExecutionContext();
        var differentContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var session = Session.RegisterNew(differentContext,
            new RegisterNewSessionInput(userId, Id.GenerateNewId(), "Device", "127.0.0.1", "Agent",
                differentContext.Timestamp.AddHours(24)));
        var sessionId = session!.EntityInfo.Id;

        _sessionRepositoryMock
            .Setup(x => x.GetByIdAsync(executionContext, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        LogAct("Updating activity when UpdateActivity returns null");
        var result = await _sut.UpdateActivityAsync(executionContext, sessionId, CancellationToken.None);

        // Assert
        LogAssert("Verifying null returned");
        result.ShouldBeNull();
        _sessionRepositoryMock.Verify(
            x => x.UpdateAsync(executionContext, It.IsAny<Session>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateActivityAsync_WhenUpdateFails_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up with active session where update fails");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var session = Session.RegisterNew(executionContext,
            new RegisterNewSessionInput(userId, Id.GenerateNewId(), "Device", "127.0.0.1", "Agent",
                executionContext.Timestamp.AddHours(24)));
        var sessionId = session!.EntityInfo.Id;

        _sessionRepositoryMock
            .Setup(x => x.GetByIdAsync(executionContext, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _sessionRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, It.IsAny<Session>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Updating activity when update fails");
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

    private static ExecutionContext CreateTestExecutionContextWithTenant(Guid tenantId, DateTimeOffset timestamp)
    {
        var tenantInfo = TenantInfo.Create(tenantId, "Test Tenant");
        return ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: tenantInfo,
            executionUser: "test.user",
            executionOrigin: "UnitTest",
            businessOperationCode: "TEST_OP",
            minimumMessageType: MessageType.Trace,
            timeProvider: new FixedTimeProvider(timestamp));
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;
        public FixedTimeProvider(DateTimeOffset utcNow) => _utcNow = utcNow;
        public override DateTimeOffset GetUtcNow() => _utcNow;
    }

    #endregion
}
