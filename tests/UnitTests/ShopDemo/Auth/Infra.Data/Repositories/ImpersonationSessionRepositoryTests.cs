using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using Bedrock.BuildingBlocks.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using ShopDemo.Auth.Domain.Entities.ImpersonationSessions;
using ShopDemo.Auth.Domain.Entities.ImpersonationSessions.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Infra.Data.Repositories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.Repositories;

public class ImpersonationSessionRepositoryTests : TestBase
{
    private readonly Mock<ILogger<ImpersonationSessionRepository>> _loggerMock;
    private readonly Mock<IImpersonationSessionPostgreSqlRepository> _postgreSqlRepositoryMock;
    private readonly ImpersonationSessionRepository _sut;

    public ImpersonationSessionRepositoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _loggerMock = new Mock<ILogger<ImpersonationSessionRepository>>();
        _loggerMock.Setup(static x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _postgreSqlRepositoryMock = new Mock<IImpersonationSessionPostgreSqlRepository>();
        _sut = new ImpersonationSessionRepository(_loggerMock.Object, _postgreSqlRepositoryMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullPostgreSqlRepository_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Preparing null PostgreSQL repository");

        // Act & Assert
        LogAct("Creating ImpersonationSessionRepository with null postgreSqlRepository");
        LogAssert("Verifying ArgumentNullException is thrown");
        Should.Throw<ArgumentNullException>(
            () => new ImpersonationSessionRepository(_loggerMock.Object, null!));
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldNotThrow()
    {
        // Arrange
        LogArrange("Preparing valid logger and PostgreSQL repository mocks");

        // Act
        LogAct("Creating ImpersonationSessionRepository with valid parameters");
        var repository = new ImpersonationSessionRepository(_loggerMock.Object, _postgreSqlRepositoryMock.Object);

        // Assert
        LogAssert("Verifying no exception was thrown");
        repository.ShouldNotBeNull();
    }

    #endregion

    #region GetActiveByOperatorUserIdAsync Tests

    [Fact]
    public async Task GetActiveByOperatorUserIdAsync_WhenFound_ShouldReturnImpersonationSession()
    {
        // Arrange
        LogArrange("Setting up mock to return an impersonation session for operator user ID lookup");
        var executionContext = CreateTestExecutionContext();
        var operatorUserId = Id.GenerateNewId();
        var expectedSession = CreateTestImpersonationSession(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.GetActiveByOperatorUserIdAsync(executionContext, operatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSession);

        // Act
        LogAct("Calling GetActiveByOperatorUserIdAsync with existing operator user ID");
        var result = await _sut.GetActiveByOperatorUserIdAsync(executionContext, operatorUserId, CancellationToken.None);

        // Assert
        LogAssert("Verifying the impersonation session was returned");
        result.ShouldNotBeNull();
        result.ShouldBe(expectedSession);
        _postgreSqlRepositoryMock.Verify(
            x => x.GetActiveByOperatorUserIdAsync(executionContext, operatorUserId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetActiveByOperatorUserIdAsync_WhenNotFound_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up mock to return null for operator user ID lookup");
        var executionContext = CreateTestExecutionContext();
        var operatorUserId = Id.GenerateNewId();

        _postgreSqlRepositoryMock
            .Setup(x => x.GetActiveByOperatorUserIdAsync(executionContext, operatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ImpersonationSession?)null);

        // Act
        LogAct("Calling GetActiveByOperatorUserIdAsync with non-existing operator user ID");
        var result = await _sut.GetActiveByOperatorUserIdAsync(executionContext, operatorUserId, CancellationToken.None);

        // Assert
        LogAssert("Verifying null was returned");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetActiveByOperatorUserIdAsync_WhenException_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up mock to throw exception on operator user ID lookup");
        var executionContext = CreateTestExecutionContext();
        var operatorUserId = Id.GenerateNewId();

        _postgreSqlRepositoryMock
            .Setup(x => x.GetActiveByOperatorUserIdAsync(executionContext, operatorUserId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        LogAct("Calling GetActiveByOperatorUserIdAsync when repository throws");
        var result = await _sut.GetActiveByOperatorUserIdAsync(executionContext, operatorUserId, CancellationToken.None);

        // Assert
        LogAssert("Verifying null was returned after exception and error was logged");
        result.ShouldBeNull();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region GetActiveByTargetUserIdAsync Tests

    [Fact]
    public async Task GetActiveByTargetUserIdAsync_WhenFound_ShouldReturnImpersonationSession()
    {
        // Arrange
        LogArrange("Setting up mock to return an impersonation session for target user ID lookup");
        var executionContext = CreateTestExecutionContext();
        var targetUserId = Id.GenerateNewId();
        var expectedSession = CreateTestImpersonationSession(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.GetActiveByTargetUserIdAsync(executionContext, targetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSession);

        // Act
        LogAct("Calling GetActiveByTargetUserIdAsync with existing target user ID");
        var result = await _sut.GetActiveByTargetUserIdAsync(executionContext, targetUserId, CancellationToken.None);

        // Assert
        LogAssert("Verifying the impersonation session was returned");
        result.ShouldNotBeNull();
        result.ShouldBe(expectedSession);
        _postgreSqlRepositoryMock.Verify(
            x => x.GetActiveByTargetUserIdAsync(executionContext, targetUserId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetActiveByTargetUserIdAsync_WhenNotFound_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up mock to return null for target user ID lookup");
        var executionContext = CreateTestExecutionContext();
        var targetUserId = Id.GenerateNewId();

        _postgreSqlRepositoryMock
            .Setup(x => x.GetActiveByTargetUserIdAsync(executionContext, targetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ImpersonationSession?)null);

        // Act
        LogAct("Calling GetActiveByTargetUserIdAsync with non-existing target user ID");
        var result = await _sut.GetActiveByTargetUserIdAsync(executionContext, targetUserId, CancellationToken.None);

        // Assert
        LogAssert("Verifying null was returned");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetActiveByTargetUserIdAsync_WhenException_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up mock to throw exception on target user ID lookup");
        var executionContext = CreateTestExecutionContext();
        var targetUserId = Id.GenerateNewId();

        _postgreSqlRepositoryMock
            .Setup(x => x.GetActiveByTargetUserIdAsync(executionContext, targetUserId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        LogAct("Calling GetActiveByTargetUserIdAsync when repository throws");
        var result = await _sut.GetActiveByTargetUserIdAsync(executionContext, targetUserId, CancellationToken.None);

        // Assert
        LogAssert("Verifying null was returned after exception and error was logged");
        result.ShouldBeNull();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WhenSuccessful_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Setting up mock to return true for UpdateAsync");
        var executionContext = CreateTestExecutionContext();
        var session = CreateTestImpersonationSession(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, session, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling UpdateAsync with an impersonation session");
        var result = await _sut.UpdateAsync(executionContext, session, CancellationToken.None);

        // Assert
        LogAssert("Verifying true was returned and repository was called");
        result.ShouldBeTrue();
        _postgreSqlRepositoryMock.Verify(
            x => x.UpdateAsync(executionContext, session, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenFailed_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up mock to return false for UpdateAsync");
        var executionContext = CreateTestExecutionContext();
        var session = CreateTestImpersonationSession(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, session, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Calling UpdateAsync when update fails");
        var result = await _sut.UpdateAsync(executionContext, session, CancellationToken.None);

        // Assert
        LogAssert("Verifying false was returned");
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task UpdateAsync_WhenException_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up mock to throw exception on UpdateAsync");
        var executionContext = CreateTestExecutionContext();
        var session = CreateTestImpersonationSession(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, session, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        LogAct("Calling UpdateAsync when repository throws");
        var result = await _sut.UpdateAsync(executionContext, session, CancellationToken.None);

        // Assert
        LogAssert("Verifying false was returned after exception and error was logged");
        result.ShouldBeFalse();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region ExistsAsync (via RepositoryBase) Tests

    [Fact]
    public async Task ExistsAsync_WhenFound_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Setting up mock to return true for ExistsAsync via base class");
        var executionContext = CreateTestExecutionContext();
        var id = Id.GenerateNewId();

        _postgreSqlRepositoryMock
            .Setup(x => x.ExistsAsync(executionContext, id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling ExistsAsync through RepositoryBase public API");
        var result = await _sut.ExistsAsync(executionContext, id, CancellationToken.None);

        // Assert
        LogAssert("Verifying true was returned and repository was called");
        result.ShouldBeTrue();
        _postgreSqlRepositoryMock.Verify(
            x => x.ExistsAsync(executionContext, id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExistsAsync_WhenNotFound_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up mock to return false for ExistsAsync via base class");
        var executionContext = CreateTestExecutionContext();
        var id = Id.GenerateNewId();

        _postgreSqlRepositoryMock
            .Setup(x => x.ExistsAsync(executionContext, id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Calling ExistsAsync with non-existing ID");
        var result = await _sut.ExistsAsync(executionContext, id, CancellationToken.None);

        // Assert
        LogAssert("Verifying false was returned");
        result.ShouldBeFalse();
    }

    #endregion

    #region GetByIdAsync (via RepositoryBase) Tests

    [Fact]
    public async Task GetByIdAsync_WhenFound_ShouldReturnImpersonationSession()
    {
        // Arrange
        LogArrange("Setting up mock to return an impersonation session for GetByIdAsync via base class");
        var executionContext = CreateTestExecutionContext();
        var expectedSession = CreateTestImpersonationSession(executionContext);
        var id = Id.GenerateNewId();

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByIdAsync(executionContext, id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSession);

        // Act
        LogAct("Calling GetByIdAsync through RepositoryBase public API");
        var result = await _sut.GetByIdAsync(executionContext, id, CancellationToken.None);

        // Assert
        LogAssert("Verifying the impersonation session was returned and repository was called");
        result.ShouldNotBeNull();
        result.ShouldBe(expectedSession);
        _postgreSqlRepositoryMock.Verify(
            x => x.GetByIdAsync(executionContext, id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up mock to return null for GetByIdAsync via base class");
        var executionContext = CreateTestExecutionContext();
        var id = Id.GenerateNewId();

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByIdAsync(executionContext, id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ImpersonationSession?)null);

        // Act
        LogAct("Calling GetByIdAsync with non-existing ID");
        var result = await _sut.GetByIdAsync(executionContext, id, CancellationToken.None);

        // Assert
        LogAssert("Verifying null was returned");
        result.ShouldBeNull();
    }

    #endregion

    #region RegisterNewAsync (via RepositoryBase) Tests

    [Fact]
    public async Task RegisterNewAsync_WhenSuccessful_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Setting up mock to return true for RegisterNewAsync via base class");
        var executionContext = CreateTestExecutionContext();
        var session = CreateTestImpersonationSession(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, session, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling RegisterNewAsync through RepositoryBase public API");
        var result = await _sut.RegisterNewAsync(executionContext, session, CancellationToken.None);

        // Assert
        LogAssert("Verifying true was returned and repository was called");
        result.ShouldBeTrue();
        _postgreSqlRepositoryMock.Verify(
            x => x.RegisterNewAsync(executionContext, session, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RegisterNewAsync_WhenFailed_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up mock to return false for RegisterNewAsync via base class");
        var executionContext = CreateTestExecutionContext();
        var session = CreateTestImpersonationSession(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, session, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Calling RegisterNewAsync when persistence fails");
        var result = await _sut.RegisterNewAsync(executionContext, session, CancellationToken.None);

        // Assert
        LogAssert("Verifying false was returned");
        result.ShouldBeFalse();
    }

    #endregion

    #region EnumerateAllAsync (via RepositoryBase) Tests

    [Fact]
    public async Task EnumerateAllAsync_ShouldReturnTrueWithNoItems()
    {
        // Arrange
        LogArrange("Setting up EnumerateAllAsync test - GetAllInternalAsync does yield break");
        var executionContext = CreateTestExecutionContext();
        var paginationInfo = PaginationInfo.All;
        var itemsReceived = new List<ImpersonationSession>();

        EnumerateAllItemHandler<ImpersonationSession> handler = (ctx, item, pagination, ct) =>
        {
            itemsReceived.Add(item);
            return Task.FromResult(true);
        };

        // Act
        LogAct("Calling EnumerateAllAsync through RepositoryBase public API");
        var result = await _sut.EnumerateAllAsync(
            executionContext,
            paginationInfo,
            handler,
            CancellationToken.None);

        // Assert
        LogAssert("Verifying true was returned and no items were yielded");
        result.ShouldBeTrue();
        itemsReceived.ShouldBeEmpty();
    }

    #endregion

    #region EnumerateModifiedSinceAsync (via RepositoryBase) Tests

    [Fact]
    public async Task EnumerateModifiedSinceAsync_ShouldReturnTrueWithNoItems()
    {
        // Arrange
        LogArrange("Setting up EnumerateModifiedSinceAsync test - GetModifiedSinceInternalAsync does yield break");
        var executionContext = CreateTestExecutionContext();
        var timeProvider = TimeProvider.System;
        var since = DateTimeOffset.UtcNow.AddDays(-1);
        var itemsReceived = new List<ImpersonationSession>();

        EnumerateModifiedSinceItemHandler<ImpersonationSession> handler = (ctx, item, tp, s, ct) =>
        {
            itemsReceived.Add(item);
            return Task.FromResult(true);
        };

        // Act
        LogAct("Calling EnumerateModifiedSinceAsync through RepositoryBase public API");
        var result = await _sut.EnumerateModifiedSinceAsync(
            executionContext,
            timeProvider,
            since,
            handler,
            CancellationToken.None);

        // Assert
        LogAssert("Verifying true was returned and no items were yielded");
        result.ShouldBeTrue();
        itemsReceived.ShouldBeEmpty();
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

    private static ImpersonationSession CreateTestImpersonationSession(ExecutionContext executionContext)
    {
        var input = new RegisterNewImpersonationSessionInput(
            OperatorUserId: Id.GenerateNewId(),
            TargetUserId: Id.GenerateNewId(),
            ExpiresAt: DateTimeOffset.UtcNow.AddHours(1));
        return ImpersonationSession.RegisterNew(executionContext, input)!;
    }

    #endregion
}
