using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using Bedrock.BuildingBlocks.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using ShopDemo.Auth.Domain.Entities.ExternalLogins;
using ShopDemo.Auth.Domain.Entities.ExternalLogins.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Infra.Data.Repositories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.Repositories;

public class ExternalLoginRepositoryTests : TestBase
{
    private readonly Mock<ILogger<ExternalLoginRepository>> _loggerMock;
    private readonly Mock<IExternalLoginPostgreSqlRepository> _postgreSqlRepositoryMock;
    private readonly ExternalLoginRepository _sut;

    public ExternalLoginRepositoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _loggerMock = new Mock<ILogger<ExternalLoginRepository>>();
        _loggerMock.Setup(static x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _postgreSqlRepositoryMock = new Mock<IExternalLoginPostgreSqlRepository>();
        _sut = new ExternalLoginRepository(_loggerMock.Object, _postgreSqlRepositoryMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullPostgreSqlRepository_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Preparing null PostgreSQL repository");

        // Act & Assert
        LogAct("Creating ExternalLoginRepository with null postgreSqlRepository");
        LogAssert("Verifying ArgumentNullException is thrown");
        Should.Throw<ArgumentNullException>(
            () => new ExternalLoginRepository(_loggerMock.Object, null!));
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldNotThrow()
    {
        // Arrange
        LogArrange("Preparing valid logger and PostgreSQL repository mocks");

        // Act
        LogAct("Creating ExternalLoginRepository with valid parameters");
        var repository = new ExternalLoginRepository(_loggerMock.Object, _postgreSqlRepositoryMock.Object);

        // Assert
        LogAssert("Verifying no exception was thrown");
        repository.ShouldNotBeNull();
    }

    #endregion

    #region GetByProviderAndProviderUserIdAsync Tests

    [Fact]
    public async Task GetByProviderAndProviderUserIdAsync_WhenFound_ShouldReturnExternalLogin()
    {
        // Arrange
        LogArrange("Setting up mock to return an external login for provider and provider user ID lookup");
        var executionContext = CreateTestExecutionContext();
        var provider = LoginProvider.Google;
        string providerUserId = "google-user-123";
        var expectedLogin = CreateTestExternalLogin(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByProviderAndProviderUserIdAsync(executionContext, provider, providerUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedLogin);

        // Act
        LogAct("Calling GetByProviderAndProviderUserIdAsync with existing provider and provider user ID");
        var result = await _sut.GetByProviderAndProviderUserIdAsync(executionContext, provider, providerUserId, CancellationToken.None);

        // Assert
        LogAssert("Verifying the external login was returned");
        result.ShouldNotBeNull();
        result.ShouldBe(expectedLogin);
        _postgreSqlRepositoryMock.Verify(
            x => x.GetByProviderAndProviderUserIdAsync(executionContext, provider, providerUserId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByProviderAndProviderUserIdAsync_WhenNotFound_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up mock to return null for provider and provider user ID lookup");
        var executionContext = CreateTestExecutionContext();
        var provider = LoginProvider.GitHub;
        string providerUserId = "nonexistent-user";

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByProviderAndProviderUserIdAsync(executionContext, provider, providerUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ExternalLogin?)null);

        // Act
        LogAct("Calling GetByProviderAndProviderUserIdAsync with non-existing provider user ID");
        var result = await _sut.GetByProviderAndProviderUserIdAsync(executionContext, provider, providerUserId, CancellationToken.None);

        // Assert
        LogAssert("Verifying null was returned");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByProviderAndProviderUserIdAsync_WhenException_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up mock to throw exception on provider and provider user ID lookup");
        var executionContext = CreateTestExecutionContext();
        var provider = LoginProvider.Microsoft;
        string providerUserId = "error-user";

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByProviderAndProviderUserIdAsync(executionContext, provider, providerUserId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        LogAct("Calling GetByProviderAndProviderUserIdAsync when repository throws");
        var result = await _sut.GetByProviderAndProviderUserIdAsync(executionContext, provider, providerUserId, CancellationToken.None);

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

    #region GetByUserIdAsync Tests

    [Fact]
    public async Task GetByUserIdAsync_WhenFound_ShouldReturnExternalLogins()
    {
        // Arrange
        LogArrange("Setting up mock to return a list of external logins for user ID lookup");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var expectedLogin = CreateTestExternalLogin(executionContext);
        IReadOnlyList<ExternalLogin> expectedList = [expectedLogin];

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedList);

        // Act
        LogAct("Calling GetByUserIdAsync with existing user ID");
        var result = await _sut.GetByUserIdAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verifying the external logins list was returned");
        result.ShouldNotBeEmpty();
        result.Count.ShouldBe(1);
        _postgreSqlRepositoryMock.Verify(
            x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByUserIdAsync_WhenEmpty_ShouldReturnEmpty()
    {
        // Arrange
        LogArrange("Setting up mock to return empty list for user ID lookup");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        LogAct("Calling GetByUserIdAsync when no external logins exist for user");
        var result = await _sut.GetByUserIdAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verifying empty list was returned");
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetByUserIdAsync_WhenException_ShouldReturnEmpty()
    {
        // Arrange
        LogArrange("Setting up mock to throw exception on user ID lookup");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        LogAct("Calling GetByUserIdAsync when repository throws");
        var result = await _sut.GetByUserIdAsync(executionContext, userId, CancellationToken.None);

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

    #region DeleteByUserIdAndProviderAsync Tests

    [Fact]
    public async Task DeleteByUserIdAndProviderAsync_WhenSuccessful_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Setting up mock to return true for DeleteByUserIdAndProviderAsync");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var provider = LoginProvider.Google;

        _postgreSqlRepositoryMock
            .Setup(x => x.DeleteByUserIdAndProviderAsync(executionContext, userId, provider, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling DeleteByUserIdAndProviderAsync with valid user ID and provider");
        var result = await _sut.DeleteByUserIdAndProviderAsync(executionContext, userId, provider, CancellationToken.None);

        // Assert
        LogAssert("Verifying true was returned and repository was called");
        result.ShouldBeTrue();
        _postgreSqlRepositoryMock.Verify(
            x => x.DeleteByUserIdAndProviderAsync(executionContext, userId, provider, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteByUserIdAndProviderAsync_WhenNotFound_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up mock to return false for DeleteByUserIdAndProviderAsync");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var provider = LoginProvider.GitHub;

        _postgreSqlRepositoryMock
            .Setup(x => x.DeleteByUserIdAndProviderAsync(executionContext, userId, provider, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Calling DeleteByUserIdAndProviderAsync when login does not exist");
        var result = await _sut.DeleteByUserIdAndProviderAsync(executionContext, userId, provider, CancellationToken.None);

        // Assert
        LogAssert("Verifying false was returned");
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task DeleteByUserIdAndProviderAsync_WhenException_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up mock to throw exception on DeleteByUserIdAndProviderAsync");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var provider = LoginProvider.Apple;

        _postgreSqlRepositoryMock
            .Setup(x => x.DeleteByUserIdAndProviderAsync(executionContext, userId, provider, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        LogAct("Calling DeleteByUserIdAndProviderAsync when repository throws");
        var result = await _sut.DeleteByUserIdAndProviderAsync(executionContext, userId, provider, CancellationToken.None);

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
    public async Task GetByIdAsync_WhenFound_ShouldReturnExternalLogin()
    {
        // Arrange
        LogArrange("Setting up mock to return an external login for GetByIdAsync via base class");
        var executionContext = CreateTestExecutionContext();
        var expectedLogin = CreateTestExternalLogin(executionContext);
        var id = Id.GenerateNewId();

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByIdAsync(executionContext, id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedLogin);

        // Act
        LogAct("Calling GetByIdAsync through RepositoryBase public API");
        var result = await _sut.GetByIdAsync(executionContext, id, CancellationToken.None);

        // Assert
        LogAssert("Verifying the external login was returned and repository was called");
        result.ShouldNotBeNull();
        result.ShouldBe(expectedLogin);
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
            .ReturnsAsync((ExternalLogin?)null);

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
        var login = CreateTestExternalLogin(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, login, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling RegisterNewAsync through RepositoryBase public API");
        var result = await _sut.RegisterNewAsync(executionContext, login, CancellationToken.None);

        // Assert
        LogAssert("Verifying true was returned and repository was called");
        result.ShouldBeTrue();
        _postgreSqlRepositoryMock.Verify(
            x => x.RegisterNewAsync(executionContext, login, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RegisterNewAsync_WhenFailed_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up mock to return false for RegisterNewAsync via base class");
        var executionContext = CreateTestExecutionContext();
        var login = CreateTestExternalLogin(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, login, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Calling RegisterNewAsync when persistence fails");
        var result = await _sut.RegisterNewAsync(executionContext, login, CancellationToken.None);

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
        var itemsReceived = new List<ExternalLogin>();

        EnumerateAllItemHandler<ExternalLogin> handler = (ctx, item, pagination, ct) =>
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
        var itemsReceived = new List<ExternalLogin>();

        EnumerateModifiedSinceItemHandler<ExternalLogin> handler = (ctx, item, tp, s, ct) =>
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

    private static ExternalLogin CreateTestExternalLogin(ExecutionContext executionContext)
    {
        var input = new RegisterNewExternalLoginInput(
            UserId: Id.GenerateNewId(),
            Provider: LoginProvider.Google,
            ProviderUserId: "test-provider-user-id",
            Email: "test@example.com");
        return ExternalLogin.RegisterNew(executionContext, input)!;
    }

    #endregion
}
