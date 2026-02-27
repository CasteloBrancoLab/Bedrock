using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using Bedrock.BuildingBlocks.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using ShopDemo.Auth.Domain.Entities.DenyListEntries;
using ShopDemo.Auth.Domain.Entities.DenyListEntries.Enums;
using ShopDemo.Auth.Domain.Entities.DenyListEntries.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Infra.Data.Repositories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.Repositories;

public class DenyListEntryRepositoryTests : TestBase
{
    private readonly Mock<ILogger<DenyListEntryRepository>> _loggerMock;
    private readonly Mock<IDenyListEntryPostgreSqlRepository> _postgreSqlRepositoryMock;
    private readonly DenyListEntryRepository _sut;

    public DenyListEntryRepositoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _loggerMock = new Mock<ILogger<DenyListEntryRepository>>();
        _loggerMock.Setup(static x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _postgreSqlRepositoryMock = new Mock<IDenyListEntryPostgreSqlRepository>();
        _sut = new DenyListEntryRepository(_loggerMock.Object, _postgreSqlRepositoryMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullPostgreSqlRepository_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Preparing null PostgreSQL repository");

        // Act & Assert
        LogAct("Creating DenyListEntryRepository with null postgreSqlRepository");
        LogAssert("Verifying ArgumentNullException is thrown");
        Should.Throw<ArgumentNullException>(
            () => new DenyListEntryRepository(_loggerMock.Object, null!));
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldNotThrow()
    {
        // Arrange
        LogArrange("Preparing valid logger and PostgreSQL repository mocks");

        // Act
        LogAct("Creating DenyListEntryRepository with valid parameters");
        var repository = new DenyListEntryRepository(_loggerMock.Object, _postgreSqlRepositoryMock.Object);

        // Assert
        LogAssert("Verifying no exception was thrown");
        repository.ShouldNotBeNull();
    }

    #endregion

    #region ExistsByTypeAndValueAsync Tests

    [Fact]
    public async Task ExistsByTypeAndValueAsync_WhenExists_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Setting up mock to return true for existence check by type and value");
        var executionContext = CreateTestExecutionContext();
        var type = DenyListEntryType.Jti;
        string value = "some-jti-value";

        _postgreSqlRepositoryMock
            .Setup(x => x.ExistsByTypeAndValueAsync(executionContext, type, value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling ExistsByTypeAndValueAsync with existing entry");
        var result = await _sut.ExistsByTypeAndValueAsync(executionContext, type, value, CancellationToken.None);

        // Assert
        LogAssert("Verifying true was returned");
        result.ShouldBeTrue();
        _postgreSqlRepositoryMock.Verify(
            x => x.ExistsByTypeAndValueAsync(executionContext, type, value, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExistsByTypeAndValueAsync_WhenNotExists_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up mock to return false for existence check by type and value");
        var executionContext = CreateTestExecutionContext();
        var type = DenyListEntryType.Jti;
        string value = "nonexistent-jti-value";

        _postgreSqlRepositoryMock
            .Setup(x => x.ExistsByTypeAndValueAsync(executionContext, type, value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Calling ExistsByTypeAndValueAsync with non-existing entry");
        var result = await _sut.ExistsByTypeAndValueAsync(executionContext, type, value, CancellationToken.None);

        // Assert
        LogAssert("Verifying false was returned");
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task ExistsByTypeAndValueAsync_WhenException_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up mock to throw exception on existence check");
        var executionContext = CreateTestExecutionContext();
        var type = DenyListEntryType.Jti;
        string value = "error-jti-value";

        _postgreSqlRepositoryMock
            .Setup(x => x.ExistsByTypeAndValueAsync(executionContext, type, value, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        LogAct("Calling ExistsByTypeAndValueAsync when repository throws");
        var result = await _sut.ExistsByTypeAndValueAsync(executionContext, type, value, CancellationToken.None);

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

    #region GetByTypeAndValueAsync Tests

    [Fact]
    public async Task GetByTypeAndValueAsync_WhenFound_ShouldReturnDenyListEntry()
    {
        // Arrange
        LogArrange("Setting up mock to return a deny list entry for type and value lookup");
        var executionContext = CreateTestExecutionContext();
        var type = DenyListEntryType.UserId;
        string value = "user-id-value";
        var expectedEntry = CreateTestDenyListEntry(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByTypeAndValueAsync(executionContext, type, value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedEntry);

        // Act
        LogAct("Calling GetByTypeAndValueAsync with existing entry");
        var result = await _sut.GetByTypeAndValueAsync(executionContext, type, value, CancellationToken.None);

        // Assert
        LogAssert("Verifying the deny list entry was returned");
        result.ShouldNotBeNull();
        result.ShouldBe(expectedEntry);
        _postgreSqlRepositoryMock.Verify(
            x => x.GetByTypeAndValueAsync(executionContext, type, value, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByTypeAndValueAsync_WhenNotFound_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up mock to return null for type and value lookup");
        var executionContext = CreateTestExecutionContext();
        var type = DenyListEntryType.UserId;
        string value = "nonexistent-user-id";

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByTypeAndValueAsync(executionContext, type, value, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DenyListEntry?)null);

        // Act
        LogAct("Calling GetByTypeAndValueAsync with non-existing entry");
        var result = await _sut.GetByTypeAndValueAsync(executionContext, type, value, CancellationToken.None);

        // Assert
        LogAssert("Verifying null was returned");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByTypeAndValueAsync_WhenException_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up mock to throw exception on type and value lookup");
        var executionContext = CreateTestExecutionContext();
        var type = DenyListEntryType.UserId;
        string value = "error-user-id";

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByTypeAndValueAsync(executionContext, type, value, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        LogAct("Calling GetByTypeAndValueAsync when repository throws");
        var result = await _sut.GetByTypeAndValueAsync(executionContext, type, value, CancellationToken.None);

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

    #region DeleteExpiredAsync Tests

    [Fact]
    public async Task DeleteExpiredAsync_WhenSuccessful_ShouldReturnCount()
    {
        // Arrange
        LogArrange("Setting up mock to return count for DeleteExpiredAsync");
        var executionContext = CreateTestExecutionContext();
        var referenceDate = DateTimeOffset.UtcNow;

        _postgreSqlRepositoryMock
            .Setup(x => x.DeleteExpiredAsync(executionContext, referenceDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        // Act
        LogAct("Calling DeleteExpiredAsync with a reference date");
        var result = await _sut.DeleteExpiredAsync(executionContext, referenceDate, CancellationToken.None);

        // Assert
        LogAssert("Verifying count was returned and repository was called");
        result.ShouldBe(5);
        _postgreSqlRepositoryMock.Verify(
            x => x.DeleteExpiredAsync(executionContext, referenceDate, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteExpiredAsync_WhenNoneDeleted_ShouldReturnZero()
    {
        // Arrange
        LogArrange("Setting up mock to return zero for DeleteExpiredAsync");
        var executionContext = CreateTestExecutionContext();
        var referenceDate = DateTimeOffset.UtcNow;

        _postgreSqlRepositoryMock
            .Setup(x => x.DeleteExpiredAsync(executionContext, referenceDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        LogAct("Calling DeleteExpiredAsync when no entries are expired");
        var result = await _sut.DeleteExpiredAsync(executionContext, referenceDate, CancellationToken.None);

        // Assert
        LogAssert("Verifying zero was returned");
        result.ShouldBe(0);
    }

    [Fact]
    public async Task DeleteExpiredAsync_WhenException_ShouldReturnZero()
    {
        // Arrange
        LogArrange("Setting up mock to throw exception on DeleteExpiredAsync");
        var executionContext = CreateTestExecutionContext();
        var referenceDate = DateTimeOffset.UtcNow;

        _postgreSqlRepositoryMock
            .Setup(x => x.DeleteExpiredAsync(executionContext, referenceDate, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        LogAct("Calling DeleteExpiredAsync when repository throws");
        var result = await _sut.DeleteExpiredAsync(executionContext, referenceDate, CancellationToken.None);

        // Assert
        LogAssert("Verifying zero was returned after exception and error was logged");
        result.ShouldBe(0);
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

    #region DeleteByTypeAndValueAsync Tests

    [Fact]
    public async Task DeleteByTypeAndValueAsync_WhenSuccessful_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Setting up mock to return true for DeleteByTypeAndValueAsync");
        var executionContext = CreateTestExecutionContext();
        var type = DenyListEntryType.Jti;
        string value = "jti-to-delete";

        _postgreSqlRepositoryMock
            .Setup(x => x.DeleteByTypeAndValueAsync(executionContext, type, value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling DeleteByTypeAndValueAsync with valid parameters");
        var result = await _sut.DeleteByTypeAndValueAsync(executionContext, type, value, CancellationToken.None);

        // Assert
        LogAssert("Verifying true was returned and repository was called");
        result.ShouldBeTrue();
        _postgreSqlRepositoryMock.Verify(
            x => x.DeleteByTypeAndValueAsync(executionContext, type, value, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteByTypeAndValueAsync_WhenFailed_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up mock to return false for DeleteByTypeAndValueAsync");
        var executionContext = CreateTestExecutionContext();
        var type = DenyListEntryType.Jti;
        string value = "nonexistent-jti";

        _postgreSqlRepositoryMock
            .Setup(x => x.DeleteByTypeAndValueAsync(executionContext, type, value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Calling DeleteByTypeAndValueAsync when entry not found");
        var result = await _sut.DeleteByTypeAndValueAsync(executionContext, type, value, CancellationToken.None);

        // Assert
        LogAssert("Verifying false was returned");
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task DeleteByTypeAndValueAsync_WhenException_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up mock to throw exception on DeleteByTypeAndValueAsync");
        var executionContext = CreateTestExecutionContext();
        var type = DenyListEntryType.Jti;
        string value = "error-jti";

        _postgreSqlRepositoryMock
            .Setup(x => x.DeleteByTypeAndValueAsync(executionContext, type, value, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        LogAct("Calling DeleteByTypeAndValueAsync when repository throws");
        var result = await _sut.DeleteByTypeAndValueAsync(executionContext, type, value, CancellationToken.None);

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
    public async Task GetByIdAsync_WhenFound_ShouldReturnDenyListEntry()
    {
        // Arrange
        LogArrange("Setting up mock to return a deny list entry for GetByIdAsync via base class");
        var executionContext = CreateTestExecutionContext();
        var expectedEntry = CreateTestDenyListEntry(executionContext);
        var id = Id.GenerateNewId();

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByIdAsync(executionContext, id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedEntry);

        // Act
        LogAct("Calling GetByIdAsync through RepositoryBase public API");
        var result = await _sut.GetByIdAsync(executionContext, id, CancellationToken.None);

        // Assert
        LogAssert("Verifying the deny list entry was returned and repository was called");
        result.ShouldNotBeNull();
        result.ShouldBe(expectedEntry);
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
            .ReturnsAsync((DenyListEntry?)null);

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
        var entry = CreateTestDenyListEntry(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, entry, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling RegisterNewAsync through RepositoryBase public API");
        var result = await _sut.RegisterNewAsync(executionContext, entry, CancellationToken.None);

        // Assert
        LogAssert("Verifying true was returned and repository was called");
        result.ShouldBeTrue();
        _postgreSqlRepositoryMock.Verify(
            x => x.RegisterNewAsync(executionContext, entry, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RegisterNewAsync_WhenFailed_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up mock to return false for RegisterNewAsync via base class");
        var executionContext = CreateTestExecutionContext();
        var entry = CreateTestDenyListEntry(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, entry, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Calling RegisterNewAsync when persistence fails");
        var result = await _sut.RegisterNewAsync(executionContext, entry, CancellationToken.None);

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
        var itemsReceived = new List<DenyListEntry>();

        EnumerateAllItemHandler<DenyListEntry> handler = (ctx, item, pagination, ct) =>
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
        var itemsReceived = new List<DenyListEntry>();

        EnumerateModifiedSinceItemHandler<DenyListEntry> handler = (ctx, item, tp, s, ct) =>
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

    private static DenyListEntry CreateTestDenyListEntry(ExecutionContext executionContext)
    {
        var input = new RegisterNewDenyListEntryInput(
            Type: DenyListEntryType.Jti,
            Value: "test-jti-value",
            ExpiresAt: DateTimeOffset.UtcNow.AddHours(1),
            Reason: null);
        return DenyListEntry.RegisterNew(executionContext, input)!;
    }

    #endregion
}
