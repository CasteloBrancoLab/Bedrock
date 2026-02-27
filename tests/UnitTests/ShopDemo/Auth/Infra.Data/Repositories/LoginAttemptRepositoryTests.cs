using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using Bedrock.BuildingBlocks.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using ShopDemo.Auth.Domain.Entities.LoginAttempts;
using ShopDemo.Auth.Domain.Entities.LoginAttempts.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Infra.Data.Repositories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.Repositories;

public class LoginAttemptRepositoryTests : TestBase
{
    private readonly Mock<ILogger<LoginAttemptRepository>> _loggerMock;
    private readonly Mock<ILoginAttemptPostgreSqlRepository> _postgreSqlRepositoryMock;
    private readonly LoginAttemptRepository _sut;

    public LoginAttemptRepositoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _loggerMock = new Mock<ILogger<LoginAttemptRepository>>();
        _loggerMock.Setup(static x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _postgreSqlRepositoryMock = new Mock<ILoginAttemptPostgreSqlRepository>();
        _sut = new LoginAttemptRepository(_loggerMock.Object, _postgreSqlRepositoryMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullPostgreSqlRepository_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Preparing null PostgreSQL repository");

        // Act & Assert
        LogAct("Creating LoginAttemptRepository with null postgreSqlRepository");
        LogAssert("Verifying ArgumentNullException is thrown");
        Should.Throw<ArgumentNullException>(
            () => new LoginAttemptRepository(_loggerMock.Object, null!));
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldNotThrow()
    {
        // Arrange
        LogArrange("Preparing valid logger and PostgreSQL repository mocks");

        // Act
        LogAct("Creating LoginAttemptRepository with valid parameters");
        var repository = new LoginAttemptRepository(_loggerMock.Object, _postgreSqlRepositoryMock.Object);

        // Assert
        LogAssert("Verifying no exception was thrown");
        repository.ShouldNotBeNull();
    }

    #endregion

    #region GetRecentByUsernameAsync Tests

    [Fact]
    public async Task GetRecentByUsernameAsync_WhenFound_ShouldReturnLoginAttempts()
    {
        // Arrange
        LogArrange("Setting up mock to return a list of login attempts for username lookup");
        var executionContext = CreateTestExecutionContext();
        string username = "existing-user";
        var since = DateTimeOffset.UtcNow.AddHours(-1);
        var expectedAttempt = CreateTestLoginAttempt(executionContext);
        IReadOnlyList<LoginAttempt> expectedList = [expectedAttempt];

        _postgreSqlRepositoryMock
            .Setup(x => x.GetRecentByUsernameAsync(executionContext, username, since, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedList);

        // Act
        LogAct("Calling GetRecentByUsernameAsync with existing username");
        var result = await _sut.GetRecentByUsernameAsync(executionContext, username, since, CancellationToken.None);

        // Assert
        LogAssert("Verifying the login attempts list was returned");
        result.ShouldNotBeEmpty();
        result.Count.ShouldBe(1);
        _postgreSqlRepositoryMock.Verify(
            x => x.GetRecentByUsernameAsync(executionContext, username, since, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetRecentByUsernameAsync_WhenEmpty_ShouldReturnEmpty()
    {
        // Arrange
        LogArrange("Setting up mock to return empty list for username lookup");
        var executionContext = CreateTestExecutionContext();
        string username = "no-attempts-user";
        var since = DateTimeOffset.UtcNow.AddHours(-1);

        _postgreSqlRepositoryMock
            .Setup(x => x.GetRecentByUsernameAsync(executionContext, username, since, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        LogAct("Calling GetRecentByUsernameAsync when no login attempts exist for username");
        var result = await _sut.GetRecentByUsernameAsync(executionContext, username, since, CancellationToken.None);

        // Assert
        LogAssert("Verifying empty list was returned");
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetRecentByUsernameAsync_WhenException_ShouldReturnEmpty()
    {
        // Arrange
        LogArrange("Setting up mock to throw exception on username lookup");
        var executionContext = CreateTestExecutionContext();
        string username = "error-user";
        var since = DateTimeOffset.UtcNow.AddHours(-1);

        _postgreSqlRepositoryMock
            .Setup(x => x.GetRecentByUsernameAsync(executionContext, username, since, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        LogAct("Calling GetRecentByUsernameAsync when repository throws");
        var result = await _sut.GetRecentByUsernameAsync(executionContext, username, since, CancellationToken.None);

        // Assert
        LogAssert("Verifying empty list was returned after exception and error was logged");
        result.ShouldBeEmpty();
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

    #region GetRecentByIpAddressAsync Tests

    [Fact]
    public async Task GetRecentByIpAddressAsync_WhenFound_ShouldReturnLoginAttempts()
    {
        // Arrange
        LogArrange("Setting up mock to return a list of login attempts for IP address lookup");
        var executionContext = CreateTestExecutionContext();
        string ipAddress = "192.168.1.1";
        var since = DateTimeOffset.UtcNow.AddHours(-1);
        var expectedAttempt = CreateTestLoginAttempt(executionContext);
        IReadOnlyList<LoginAttempt> expectedList = [expectedAttempt];

        _postgreSqlRepositoryMock
            .Setup(x => x.GetRecentByIpAddressAsync(executionContext, ipAddress, since, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedList);

        // Act
        LogAct("Calling GetRecentByIpAddressAsync with existing IP address");
        var result = await _sut.GetRecentByIpAddressAsync(executionContext, ipAddress, since, CancellationToken.None);

        // Assert
        LogAssert("Verifying the login attempts list was returned");
        result.ShouldNotBeEmpty();
        result.Count.ShouldBe(1);
        _postgreSqlRepositoryMock.Verify(
            x => x.GetRecentByIpAddressAsync(executionContext, ipAddress, since, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetRecentByIpAddressAsync_WhenEmpty_ShouldReturnEmpty()
    {
        // Arrange
        LogArrange("Setting up mock to return empty list for IP address lookup");
        var executionContext = CreateTestExecutionContext();
        string ipAddress = "10.0.0.1";
        var since = DateTimeOffset.UtcNow.AddHours(-1);

        _postgreSqlRepositoryMock
            .Setup(x => x.GetRecentByIpAddressAsync(executionContext, ipAddress, since, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        LogAct("Calling GetRecentByIpAddressAsync when no login attempts exist for IP address");
        var result = await _sut.GetRecentByIpAddressAsync(executionContext, ipAddress, since, CancellationToken.None);

        // Assert
        LogAssert("Verifying empty list was returned");
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetRecentByIpAddressAsync_WhenException_ShouldReturnEmpty()
    {
        // Arrange
        LogArrange("Setting up mock to throw exception on IP address lookup");
        var executionContext = CreateTestExecutionContext();
        string ipAddress = "0.0.0.0";
        var since = DateTimeOffset.UtcNow.AddHours(-1);

        _postgreSqlRepositoryMock
            .Setup(x => x.GetRecentByIpAddressAsync(executionContext, ipAddress, since, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        LogAct("Calling GetRecentByIpAddressAsync when repository throws");
        var result = await _sut.GetRecentByIpAddressAsync(executionContext, ipAddress, since, CancellationToken.None);

        // Assert
        LogAssert("Verifying empty list was returned after exception and error was logged");
        result.ShouldBeEmpty();
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
    public async Task GetByIdAsync_WhenFound_ShouldReturnLoginAttempt()
    {
        // Arrange
        LogArrange("Setting up mock to return a login attempt for GetByIdAsync via base class");
        var executionContext = CreateTestExecutionContext();
        var expectedAttempt = CreateTestLoginAttempt(executionContext);
        var id = Id.GenerateNewId();

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByIdAsync(executionContext, id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAttempt);

        // Act
        LogAct("Calling GetByIdAsync through RepositoryBase public API");
        var result = await _sut.GetByIdAsync(executionContext, id, CancellationToken.None);

        // Assert
        LogAssert("Verifying the login attempt was returned and repository was called");
        result.ShouldNotBeNull();
        result.ShouldBe(expectedAttempt);
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
            .ReturnsAsync((LoginAttempt?)null);

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
        var attempt = CreateTestLoginAttempt(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, attempt, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling RegisterNewAsync through RepositoryBase public API");
        var result = await _sut.RegisterNewAsync(executionContext, attempt, CancellationToken.None);

        // Assert
        LogAssert("Verifying true was returned and repository was called");
        result.ShouldBeTrue();
        _postgreSqlRepositoryMock.Verify(
            x => x.RegisterNewAsync(executionContext, attempt, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RegisterNewAsync_WhenFailed_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up mock to return false for RegisterNewAsync via base class");
        var executionContext = CreateTestExecutionContext();
        var attempt = CreateTestLoginAttempt(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, attempt, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Calling RegisterNewAsync when persistence fails");
        var result = await _sut.RegisterNewAsync(executionContext, attempt, CancellationToken.None);

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
        var itemsReceived = new List<LoginAttempt>();

        EnumerateAllItemHandler<LoginAttempt> handler = (ctx, item, pagination, ct) =>
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
        var itemsReceived = new List<LoginAttempt>();

        EnumerateModifiedSinceItemHandler<LoginAttempt> handler = (ctx, item, tp, s, ct) =>
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

    private static LoginAttempt CreateTestLoginAttempt(ExecutionContext executionContext)
    {
        var input = new RegisterNewLoginAttemptInput(
            Username: "test.user@example.com",
            IpAddress: "127.0.0.1",
            IsSuccessful: true,
            FailureReason: null);
        return LoginAttempt.RegisterNew(executionContext, input)!;
    }

    #endregion
}
