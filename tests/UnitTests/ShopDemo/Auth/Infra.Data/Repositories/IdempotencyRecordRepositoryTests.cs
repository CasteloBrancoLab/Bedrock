using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using Bedrock.BuildingBlocks.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using ShopDemo.Auth.Domain.Entities.IdempotencyRecords;
using ShopDemo.Auth.Domain.Entities.IdempotencyRecords.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Infra.Data.Repositories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.Repositories;

public class IdempotencyRecordRepositoryTests : TestBase
{
    private readonly Mock<ILogger<IdempotencyRecordRepository>> _loggerMock;
    private readonly Mock<IIdempotencyRecordPostgreSqlRepository> _postgreSqlRepositoryMock;
    private readonly IdempotencyRecordRepository _sut;

    public IdempotencyRecordRepositoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _loggerMock = new Mock<ILogger<IdempotencyRecordRepository>>();
        _loggerMock.Setup(static x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _postgreSqlRepositoryMock = new Mock<IIdempotencyRecordPostgreSqlRepository>();
        _sut = new IdempotencyRecordRepository(_loggerMock.Object, _postgreSqlRepositoryMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullPostgreSqlRepository_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Preparing null PostgreSQL repository");

        // Act & Assert
        LogAct("Creating IdempotencyRecordRepository with null postgreSqlRepository");
        LogAssert("Verifying ArgumentNullException is thrown");
        Should.Throw<ArgumentNullException>(
            () => new IdempotencyRecordRepository(_loggerMock.Object, null!));
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldNotThrow()
    {
        // Arrange
        LogArrange("Preparing valid logger and PostgreSQL repository mocks");

        // Act
        LogAct("Creating IdempotencyRecordRepository with valid parameters");
        var repository = new IdempotencyRecordRepository(_loggerMock.Object, _postgreSqlRepositoryMock.Object);

        // Assert
        LogAssert("Verifying no exception was thrown");
        repository.ShouldNotBeNull();
    }

    #endregion

    #region GetByKeyAsync Tests

    [Fact]
    public async Task GetByKeyAsync_WhenFound_ShouldReturnIdempotencyRecord()
    {
        // Arrange
        LogArrange("Setting up mock to return an idempotency record for key lookup");
        var executionContext = CreateTestExecutionContext();
        string idempotencyKey = "existing-idempotency-key";
        var expectedRecord = CreateTestIdempotencyRecord(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByKeyAsync(executionContext, idempotencyKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRecord);

        // Act
        LogAct("Calling GetByKeyAsync with existing idempotency key");
        var result = await _sut.GetByKeyAsync(executionContext, idempotencyKey, CancellationToken.None);

        // Assert
        LogAssert("Verifying the idempotency record was returned");
        result.ShouldNotBeNull();
        result.ShouldBe(expectedRecord);
        _postgreSqlRepositoryMock.Verify(
            x => x.GetByKeyAsync(executionContext, idempotencyKey, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByKeyAsync_WhenNotFound_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up mock to return null for key lookup");
        var executionContext = CreateTestExecutionContext();
        string idempotencyKey = "nonexistent-key";

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByKeyAsync(executionContext, idempotencyKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdempotencyRecord?)null);

        // Act
        LogAct("Calling GetByKeyAsync with non-existing idempotency key");
        var result = await _sut.GetByKeyAsync(executionContext, idempotencyKey, CancellationToken.None);

        // Assert
        LogAssert("Verifying null was returned");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByKeyAsync_WhenException_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up mock to throw exception on key lookup");
        var executionContext = CreateTestExecutionContext();
        string idempotencyKey = "error-key";

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByKeyAsync(executionContext, idempotencyKey, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        LogAct("Calling GetByKeyAsync when repository throws");
        var result = await _sut.GetByKeyAsync(executionContext, idempotencyKey, CancellationToken.None);

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
        var record = CreateTestIdempotencyRecord(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, record, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling UpdateAsync with an idempotency record");
        var result = await _sut.UpdateAsync(executionContext, record, CancellationToken.None);

        // Assert
        LogAssert("Verifying true was returned and repository was called");
        result.ShouldBeTrue();
        _postgreSqlRepositoryMock.Verify(
            x => x.UpdateAsync(executionContext, record, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenFailed_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up mock to return false for UpdateAsync");
        var executionContext = CreateTestExecutionContext();
        var record = CreateTestIdempotencyRecord(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, record, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Calling UpdateAsync when update fails");
        var result = await _sut.UpdateAsync(executionContext, record, CancellationToken.None);

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
        var record = CreateTestIdempotencyRecord(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, record, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        LogAct("Calling UpdateAsync when repository throws");
        var result = await _sut.UpdateAsync(executionContext, record, CancellationToken.None);

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

    #region RemoveExpiredAsync Tests

    [Fact]
    public async Task RemoveExpiredAsync_WhenSuccessful_ShouldReturnRemovedCount()
    {
        // Arrange
        LogArrange("Setting up mock to return removed count for RemoveExpiredAsync");
        var executionContext = CreateTestExecutionContext();
        var now = DateTimeOffset.UtcNow;
        int expectedCount = 5;

        _postgreSqlRepositoryMock
            .Setup(x => x.RemoveExpiredAsync(executionContext, now, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCount);

        // Act
        LogAct("Calling RemoveExpiredAsync with current timestamp");
        var result = await _sut.RemoveExpiredAsync(executionContext, now, CancellationToken.None);

        // Assert
        LogAssert("Verifying the removed count was returned and repository was called");
        result.ShouldBe(expectedCount);
        _postgreSqlRepositoryMock.Verify(
            x => x.RemoveExpiredAsync(executionContext, now, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RemoveExpiredAsync_WhenNothingToRemove_ShouldReturnZero()
    {
        // Arrange
        LogArrange("Setting up mock to return zero for RemoveExpiredAsync when no expired records");
        var executionContext = CreateTestExecutionContext();
        var now = DateTimeOffset.UtcNow;

        _postgreSqlRepositoryMock
            .Setup(x => x.RemoveExpiredAsync(executionContext, now, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        LogAct("Calling RemoveExpiredAsync when no expired records exist");
        var result = await _sut.RemoveExpiredAsync(executionContext, now, CancellationToken.None);

        // Assert
        LogAssert("Verifying zero was returned");
        result.ShouldBe(0);
    }

    [Fact]
    public async Task RemoveExpiredAsync_WhenException_ShouldReturnZero()
    {
        // Arrange
        LogArrange("Setting up mock to throw exception on RemoveExpiredAsync");
        var executionContext = CreateTestExecutionContext();
        var now = DateTimeOffset.UtcNow;

        _postgreSqlRepositoryMock
            .Setup(x => x.RemoveExpiredAsync(executionContext, now, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        LogAct("Calling RemoveExpiredAsync when repository throws");
        var result = await _sut.RemoveExpiredAsync(executionContext, now, CancellationToken.None);

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
    public async Task GetByIdAsync_WhenFound_ShouldReturnIdempotencyRecord()
    {
        // Arrange
        LogArrange("Setting up mock to return an idempotency record for GetByIdAsync via base class");
        var executionContext = CreateTestExecutionContext();
        var expectedRecord = CreateTestIdempotencyRecord(executionContext);
        var id = Id.GenerateNewId();

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByIdAsync(executionContext, id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRecord);

        // Act
        LogAct("Calling GetByIdAsync through RepositoryBase public API");
        var result = await _sut.GetByIdAsync(executionContext, id, CancellationToken.None);

        // Assert
        LogAssert("Verifying the idempotency record was returned and repository was called");
        result.ShouldNotBeNull();
        result.ShouldBe(expectedRecord);
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
            .ReturnsAsync((IdempotencyRecord?)null);

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
        var record = CreateTestIdempotencyRecord(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, record, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling RegisterNewAsync through RepositoryBase public API");
        var result = await _sut.RegisterNewAsync(executionContext, record, CancellationToken.None);

        // Assert
        LogAssert("Verifying true was returned and repository was called");
        result.ShouldBeTrue();
        _postgreSqlRepositoryMock.Verify(
            x => x.RegisterNewAsync(executionContext, record, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RegisterNewAsync_WhenFailed_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up mock to return false for RegisterNewAsync via base class");
        var executionContext = CreateTestExecutionContext();
        var record = CreateTestIdempotencyRecord(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, record, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Calling RegisterNewAsync when persistence fails");
        var result = await _sut.RegisterNewAsync(executionContext, record, CancellationToken.None);

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
        var itemsReceived = new List<IdempotencyRecord>();

        EnumerateAllItemHandler<IdempotencyRecord> handler = (ctx, item, pagination, ct) =>
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
        var itemsReceived = new List<IdempotencyRecord>();

        EnumerateModifiedSinceItemHandler<IdempotencyRecord> handler = (ctx, item, tp, s, ct) =>
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

    private static IdempotencyRecord CreateTestIdempotencyRecord(ExecutionContext executionContext)
    {
        var input = new RegisterNewIdempotencyRecordInput(
            IdempotencyKey: Guid.NewGuid().ToString(),
            RequestHash: "test-request-hash-value");
        return IdempotencyRecord.RegisterNew(executionContext, input)!;
    }

    #endregion
}
