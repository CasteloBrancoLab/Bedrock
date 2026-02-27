using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using Bedrock.BuildingBlocks.Persistence.Abstractions.Repositories.Interfaces;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using Moq;
using ShopDemo.Auth.Domain.Entities.IdempotencyRecords;
using ShopDemo.Auth.Domain.Entities.IdempotencyRecords.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Repositories;

public class IdempotencyRecordPostgreSqlRepositoryTests : TestBase
{
    private static readonly Faker Faker = new();

    private readonly Mock<IIdempotencyRecordDataModelRepository> _dataModelRepositoryMock;
    private readonly IdempotencyRecordPostgreSqlRepository _repository;

    public IdempotencyRecordPostgreSqlRepositoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _dataModelRepositoryMock = new Mock<IIdempotencyRecordDataModelRepository>();
        _repository = new IdempotencyRecordPostgreSqlRepository(_dataModelRepositoryMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullDataModelRepository_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Preparing null argument for constructor");

        // Act
        LogAct("Attempting to create IdempotencyRecordPostgreSqlRepository with null");
        var action = () => new IdempotencyRecordPostgreSqlRepository(null!);

        // Assert
        LogAssert("Verifying ArgumentNullException is thrown");
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithValidDataModelRepository_ShouldCreateInstance()
    {
        // Arrange
        LogArrange("Creating mock IIdempotencyRecordDataModelRepository");
        var dataModelRepositoryMock = new Mock<IIdempotencyRecordDataModelRepository>();

        // Act
        LogAct("Creating IdempotencyRecordPostgreSqlRepository");
        var repository = new IdempotencyRecordPostgreSqlRepository(dataModelRepositoryMock.Object);

        // Assert
        LogAssert("Verifying instance was created");
        repository.ShouldNotBeNull();
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenDataModelFound_ShouldReturnIdempotencyRecord()
    {
        // Arrange
        LogArrange("Setting up mock to return an IdempotencyRecordDataModel");
        var executionContext = CreateTestExecutionContext();
        var entityId = Guid.NewGuid();
        var id = Id.CreateFromExistingInfo(entityId);
        var dataModel = CreateTestDataModel(entityId);

        _dataModelRepositoryMock
            .Setup(static x => x.GetByIdAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dataModel);

        // Act
        LogAct("Calling GetByIdAsync");
        var result = await _repository.GetByIdAsync(executionContext, id, CancellationToken.None);

        // Assert
        LogAssert("Verifying IdempotencyRecord was created from DataModel");
        result.ShouldNotBeNull();
        result.IdempotencyKey.ShouldBe(dataModel.IdempotencyKey);
        result.RequestHash.ShouldBe(dataModel.RequestHash);
        result.StatusCode.ShouldBe(dataModel.StatusCode);
    }

    [Fact]
    public async Task GetByIdAsync_WhenDataModelNotFound_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up mock to return null");
        var executionContext = CreateTestExecutionContext();
        var id = Id.CreateFromExistingInfo(Guid.NewGuid());

        _dataModelRepositoryMock
            .Setup(static x => x.GetByIdAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdempotencyRecordDataModel?)null);

        // Act
        LogAct("Calling GetByIdAsync");
        var result = await _repository.GetByIdAsync(executionContext, id, CancellationToken.None);

        // Assert
        LogAssert("Verifying null was returned");
        result.ShouldBeNull();
    }

    #endregion

    #region ExistsAsync Tests

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExistsAsync_ShouldDelegateToDataModelRepository(bool expectedResult)
    {
        // Arrange
        LogArrange($"Setting up mock to return {expectedResult}");
        var executionContext = CreateTestExecutionContext();
        var id = Id.CreateFromExistingInfo(Guid.NewGuid());

        _dataModelRepositoryMock
            .Setup(static x => x.ExistsAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        LogAct("Calling ExistsAsync");
        var result = await _repository.ExistsAsync(executionContext, id, CancellationToken.None);

        // Assert
        LogAssert($"Verifying result is {expectedResult}");
        result.ShouldBe(expectedResult);
        _dataModelRepositoryMock.Verify(
            static x => x.ExistsAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region RegisterNewAsync Tests

    [Fact]
    public async Task RegisterNewAsync_ShouldCreateDataModelAndDelegateInsert()
    {
        // Arrange
        LogArrange("Creating IdempotencyRecord entity and setting up mock");
        var executionContext = CreateTestExecutionContext();
        var record = CreateTestIdempotencyRecord("idem-key-001", "req-hash-abc");

        _dataModelRepositoryMock
            .Setup(static x => x.InsertAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<IdempotencyRecordDataModel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling RegisterNewAsync");
        var result = await _repository.RegisterNewAsync(executionContext, record, CancellationToken.None);

        // Assert
        LogAssert("Verifying InsertAsync was called with an IdempotencyRecordDataModel");
        result.ShouldBeTrue();
        _dataModelRepositoryMock.Verify(
            static x => x.InsertAsync(
                It.IsAny<ExecutionContext>(),
                It.Is<IdempotencyRecordDataModel>(static dm =>
                    dm.IdempotencyKey == "idem-key-001" &&
                    dm.RequestHash == "req-hash-abc"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetByKeyAsync Tests

    [Fact]
    public async Task GetByKeyAsync_WhenFound_ShouldReturnIdempotencyRecord()
    {
        // Arrange
        LogArrange("Setting up mock to return an IdempotencyRecordDataModel by key");
        var executionContext = CreateTestExecutionContext();
        string idempotencyKey = "my-idem-key";
        var dataModel = CreateTestDataModel(Guid.NewGuid());
        dataModel.IdempotencyKey = idempotencyKey;

        _dataModelRepositoryMock
            .Setup(static x => x.GetByKeyAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dataModel);

        // Act
        LogAct("Calling GetByKeyAsync");
        var result = await _repository.GetByKeyAsync(executionContext, idempotencyKey, CancellationToken.None);

        // Assert
        LogAssert("Verifying IdempotencyRecord was returned from DataModel");
        result.ShouldNotBeNull();
        result.IdempotencyKey.ShouldBe(idempotencyKey);
    }

    [Fact]
    public async Task GetByKeyAsync_WhenNotFound_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up mock to return null for key search");
        var executionContext = CreateTestExecutionContext();

        _dataModelRepositoryMock
            .Setup(static x => x.GetByKeyAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdempotencyRecordDataModel?)null);

        // Act
        LogAct("Calling GetByKeyAsync");
        var result = await _repository.GetByKeyAsync(executionContext, "nonexistent-key", CancellationToken.None);

        // Assert
        LogAssert("Verifying null was returned");
        result.ShouldBeNull();
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WhenExistingDataModelFound_ShouldAdaptAndUpdate()
    {
        // Arrange
        LogArrange("Setting up mock to return existing DataModel and successful update");
        var executionContext = CreateTestExecutionContext();
        var entityId = Guid.NewGuid();
        var existingDataModel = CreateTestDataModel(entityId);
        var record = CreateTestIdempotencyRecord("idem-key", "req-hash", entityId);

        _dataModelRepositoryMock
            .Setup(static x => x.GetByIdAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingDataModel);

        _dataModelRepositoryMock
            .Setup(static x => x.UpdateAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<IdempotencyRecordDataModel>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling UpdateAsync");
        var result = await _repository.UpdateAsync(executionContext, record, CancellationToken.None);

        // Assert
        LogAssert("Verifying update was successful");
        result.ShouldBeTrue();
        _dataModelRepositoryMock.Verify(
            static x => x.GetByIdAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _dataModelRepositoryMock.Verify(
            static x => x.UpdateAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<IdempotencyRecordDataModel>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenExistingDataModelNotFound_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up mock to return null for GetByIdAsync");
        var executionContext = CreateTestExecutionContext();
        var record = CreateTestIdempotencyRecord("idem-key", "req-hash");

        _dataModelRepositoryMock
            .Setup(static x => x.GetByIdAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdempotencyRecordDataModel?)null);

        // Act
        LogAct("Calling UpdateAsync");
        var result = await _repository.UpdateAsync(executionContext, record, CancellationToken.None);

        // Assert
        LogAssert("Verifying false was returned and UpdateAsync was not called");
        result.ShouldBeFalse();
        _dataModelRepositoryMock.Verify(
            static x => x.UpdateAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<IdempotencyRecordDataModel>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPassExistingDataModelVersionToDataModelRepository()
    {
        // Arrange
        LogArrange("Setting up mock to capture existing data model version (not entity version)");
        var executionContext = CreateTestExecutionContext();
        var entityId = Guid.NewGuid();
        long dbVersion = Faker.Random.Long(1);
        var existingDataModel = CreateTestDataModel(entityId);
        existingDataModel.EntityVersion = dbVersion;
        var record = CreateTestIdempotencyRecord("idem-key", "req-hash", entityId);

        _dataModelRepositoryMock
            .Setup(static x => x.GetByIdAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingDataModel);

        _dataModelRepositoryMock
            .Setup(static x => x.UpdateAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<IdempotencyRecordDataModel>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling UpdateAsync");
        await _repository.UpdateAsync(executionContext, record, CancellationToken.None);

        // Assert
        LogAssert("Verifying existing data model version was passed (not entity version)");
        _dataModelRepositoryMock.Verify(
            x => x.UpdateAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<IdempotencyRecordDataModel>(),
                dbVersion,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region RemoveExpiredAsync Tests

    [Fact]
    public async Task RemoveExpiredAsync_ShouldDelegateToDataModelRepository()
    {
        // Arrange
        LogArrange("Setting up mock to return count of removed records");
        var executionContext = CreateTestExecutionContext();
        var now = DateTimeOffset.UtcNow;

        _dataModelRepositoryMock
            .Setup(static x => x.RemoveExpiredAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        // Act
        LogAct("Calling RemoveExpiredAsync");
        var result = await _repository.RemoveExpiredAsync(executionContext, now, CancellationToken.None);

        // Assert
        LogAssert("Verifying result was 3 removed records");
        result.ShouldBe(3);
        _dataModelRepositoryMock.Verify(
            static x => x.RemoveExpiredAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region EnumerateAllAsync Tests

    [Fact]
    public async Task EnumerateAllAsync_ShouldDelegateToDataModelRepository()
    {
        // Arrange
        LogArrange("Setting up mock to return true for EnumerateAllAsync");
        var executionContext = CreateTestExecutionContext();
        var paginationInfo = PaginationInfo.Create(page: 1, pageSize: 10);
        EnumerateAllItemHandler<IdempotencyRecord> handler = (_, _, _, _) => Task.FromResult(true);

        _dataModelRepositoryMock
            .Setup(static x => x.EnumerateAllAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<PaginationInfo>(),
                It.IsAny<DataModelItemHandler<IdempotencyRecordDataModel>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling EnumerateAllAsync");
        var result = await _repository.EnumerateAllAsync(
            executionContext, paginationInfo, handler, CancellationToken.None);

        // Assert
        LogAssert("Verifying EnumerateAllAsync was delegated and returned true");
        result.ShouldBeTrue();
        _dataModelRepositoryMock.Verify(
            static x => x.EnumerateAllAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<PaginationInfo>(),
                It.IsAny<DataModelItemHandler<IdempotencyRecordDataModel>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EnumerateAllAsync_WhenFails_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up mock to return false for EnumerateAllAsync");
        var executionContext = CreateTestExecutionContext();
        var paginationInfo = PaginationInfo.Create(page: 1, pageSize: 10);
        EnumerateAllItemHandler<IdempotencyRecord> handler = (_, _, _, _) => Task.FromResult(true);

        _dataModelRepositoryMock
            .Setup(static x => x.EnumerateAllAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<PaginationInfo>(),
                It.IsAny<DataModelItemHandler<IdempotencyRecordDataModel>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Calling EnumerateAllAsync");
        var result = await _repository.EnumerateAllAsync(
            executionContext, paginationInfo, handler, CancellationToken.None);

        // Assert
        LogAssert("Verifying EnumerateAllAsync returned false");
        result.ShouldBeFalse();
    }

    #endregion

    #region EnumerateModifiedSinceAsync Tests

    [Fact]
    public async Task EnumerateModifiedSinceAsync_ShouldDelegateToDataModelRepository()
    {
        // Arrange
        LogArrange("Setting up mock to return true for EnumerateModifiedSinceAsync");
        var executionContext = CreateTestExecutionContext();
        var timeProvider = TimeProvider.System;
        var since = DateTimeOffset.UtcNow.AddDays(-1);
        EnumerateModifiedSinceItemHandler<IdempotencyRecord> handler = (_, _, _, _, _) => Task.FromResult(true);

        _dataModelRepositoryMock
            .Setup(static x => x.EnumerateModifiedSinceAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DataModelItemHandler<IdempotencyRecordDataModel>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling EnumerateModifiedSinceAsync");
        var result = await _repository.EnumerateModifiedSinceAsync(
            executionContext, timeProvider, since, handler, CancellationToken.None);

        // Assert
        LogAssert("Verifying EnumerateModifiedSinceAsync was delegated and returned true");
        result.ShouldBeTrue();
        _dataModelRepositoryMock.Verify(
            static x => x.EnumerateModifiedSinceAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DataModelItemHandler<IdempotencyRecordDataModel>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EnumerateModifiedSinceAsync_WhenFails_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up mock to return false for EnumerateModifiedSinceAsync");
        var executionContext = CreateTestExecutionContext();
        var timeProvider = TimeProvider.System;
        var since = DateTimeOffset.UtcNow.AddDays(-1);
        EnumerateModifiedSinceItemHandler<IdempotencyRecord> handler = (_, _, _, _, _) => Task.FromResult(true);

        _dataModelRepositoryMock
            .Setup(static x => x.EnumerateModifiedSinceAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DataModelItemHandler<IdempotencyRecordDataModel>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Calling EnumerateModifiedSinceAsync");
        var result = await _repository.EnumerateModifiedSinceAsync(
            executionContext, timeProvider, since, handler, CancellationToken.None);

        // Assert
        LogAssert("Verifying EnumerateModifiedSinceAsync returned false");
        result.ShouldBeFalse();
    }

    #endregion

    #region Helper Methods

    private static ExecutionContext CreateTestExecutionContext()
    {
        var tenantInfo = TenantInfo.Create(Guid.NewGuid(), "Test Tenant");
        var timeProvider = TimeProvider.System;

        return ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: tenantInfo,
            executionUser: "test.user",
            executionOrigin: "UnitTest",
            businessOperationCode: "TEST_OP",
            minimumMessageType: MessageType.Trace,
            timeProvider: timeProvider);
    }

    private static IdempotencyRecordDataModel CreateTestDataModel(Guid id)
    {
        return new IdempotencyRecordDataModel
        {
            Id = id,
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            LastChangedBy = null,
            LastChangedAt = null,
            LastChangedExecutionOrigin = null,
            LastChangedCorrelationId = null,
            LastChangedBusinessOperationCode = null,
            EntityVersion = 1,
            IdempotencyKey = Guid.NewGuid().ToString(),
            RequestHash = Faker.Random.AlphaNumeric(64),
            ResponseBody = null,
            StatusCode = 0,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(24)
        };
    }

    private static IdempotencyRecord CreateTestIdempotencyRecord(
        string idempotencyKey,
        string requestHash,
        Guid? entityId = null)
    {
        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(entityId ?? Guid.NewGuid()),
            tenantInfo: TenantInfo.Create(Guid.NewGuid()),
            createdAt: DateTimeOffset.UtcNow,
            createdBy: "test-creator",
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "UnitTest",
            createdBusinessOperationCode: "TEST_OP",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null,
            entityVersion: RegistryVersion.CreateFromExistingInfo(DateTimeOffset.UtcNow));

        return IdempotencyRecord.CreateFromExistingInfo(
            new CreateFromExistingInfoIdempotencyRecordInput(
                entityInfo,
                idempotencyKey,
                requestHash,
                null,
                0,
                DateTimeOffset.UtcNow.AddHours(24)));
    }

    #endregion
}
