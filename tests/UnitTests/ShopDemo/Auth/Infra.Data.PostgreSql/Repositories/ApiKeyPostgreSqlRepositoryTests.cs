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
using ShopDemo.Auth.Domain.Entities.ApiKeys;
using ShopDemo.Auth.Domain.Entities.ApiKeys.Enums;
using ShopDemo.Auth.Domain.Entities.ApiKeys.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Repositories;

public class ApiKeyPostgreSqlRepositoryTests : TestBase
{
    private static readonly Faker Faker = new();

    private readonly Mock<IApiKeyDataModelRepository> _dataModelRepositoryMock;
    private readonly ApiKeyPostgreSqlRepository _repository;

    public ApiKeyPostgreSqlRepositoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _dataModelRepositoryMock = new Mock<IApiKeyDataModelRepository>();
        _repository = new ApiKeyPostgreSqlRepository(_dataModelRepositoryMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullDataModelRepository_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Preparing null argument for constructor");

        // Act
        LogAct("Attempting to create ApiKeyPostgreSqlRepository with null");
        var action = () => new ApiKeyPostgreSqlRepository(null!);

        // Assert
        LogAssert("Verifying ArgumentNullException is thrown");
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithValidDataModelRepository_ShouldCreateInstance()
    {
        // Arrange
        LogArrange("Creating mock IApiKeyDataModelRepository");
        var dataModelRepositoryMock = new Mock<IApiKeyDataModelRepository>();

        // Act
        LogAct("Creating ApiKeyPostgreSqlRepository");
        var repository = new ApiKeyPostgreSqlRepository(dataModelRepositoryMock.Object);

        // Assert
        LogAssert("Verifying instance was created");
        repository.ShouldNotBeNull();
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenDataModelFound_ShouldReturnApiKey()
    {
        // Arrange
        LogArrange("Setting up mock to return an ApiKeyDataModel");
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
        LogAssert("Verifying ApiKey was created from DataModel");
        result.ShouldNotBeNull();
        result.KeyHash.ShouldBe(dataModel.KeyHash);
        result.KeyPrefix.ShouldBe(dataModel.KeyPrefix);
        result.Status.ShouldBe((ApiKeyStatus)dataModel.Status);
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
            .ReturnsAsync((ApiKeyDataModel?)null);

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
        LogArrange("Creating ApiKey entity and setting up mock");
        var executionContext = CreateTestExecutionContext();
        var apiKey = CreateTestApiKey("sk_test_prefix", "abc123hash", Guid.NewGuid());

        _dataModelRepositoryMock
            .Setup(static x => x.InsertAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<ApiKeyDataModel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling RegisterNewAsync");
        var result = await _repository.RegisterNewAsync(executionContext, apiKey, CancellationToken.None);

        // Assert
        LogAssert("Verifying InsertAsync was called with an ApiKeyDataModel");
        result.ShouldBeTrue();
        _dataModelRepositoryMock.Verify(
            static x => x.InsertAsync(
                It.IsAny<ExecutionContext>(),
                It.Is<ApiKeyDataModel>(static dm =>
                    dm.KeyPrefix == "sk_test_prefix" &&
                    dm.KeyHash == "abc123hash"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetByKeyHashAsync Tests

    [Fact]
    public async Task GetByKeyHashAsync_WhenFound_ShouldReturnApiKey()
    {
        // Arrange
        LogArrange("Setting up mock to return an ApiKeyDataModel by key hash");
        var executionContext = CreateTestExecutionContext();
        string keyHash = "the-key-hash";
        var dataModel = CreateTestDataModel(Guid.NewGuid());
        dataModel.KeyHash = keyHash;

        _dataModelRepositoryMock
            .Setup(static x => x.GetByKeyHashAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dataModel);

        // Act
        LogAct("Calling GetByKeyHashAsync");
        var result = await _repository.GetByKeyHashAsync(executionContext, keyHash, CancellationToken.None);

        // Assert
        LogAssert("Verifying ApiKey was returned from DataModel");
        result.ShouldNotBeNull();
        result.KeyHash.ShouldBe(keyHash);
    }

    [Fact]
    public async Task GetByKeyHashAsync_WhenNotFound_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up mock to return null for key hash search");
        var executionContext = CreateTestExecutionContext();

        _dataModelRepositoryMock
            .Setup(static x => x.GetByKeyHashAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApiKeyDataModel?)null);

        // Act
        LogAct("Calling GetByKeyHashAsync");
        var result = await _repository.GetByKeyHashAsync(executionContext, "nonexistent-hash", CancellationToken.None);

        // Assert
        LogAssert("Verifying null was returned");
        result.ShouldBeNull();
    }

    #endregion

    #region GetByServiceClientIdAsync Tests

    [Fact]
    public async Task GetByServiceClientIdAsync_WhenItemsExist_ShouldReturnApiKeys()
    {
        // Arrange
        LogArrange("Setting up mock to return a list of ApiKeyDataModels");
        var executionContext = CreateTestExecutionContext();
        var serviceClientId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var dataModels = new List<ApiKeyDataModel>
        {
            CreateTestDataModel(Guid.NewGuid()),
            CreateTestDataModel(Guid.NewGuid())
        };

        _dataModelRepositoryMock
            .Setup(static x => x.GetByServiceClientIdAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dataModels);

        // Act
        LogAct("Calling GetByServiceClientIdAsync");
        var result = await _repository.GetByServiceClientIdAsync(executionContext, serviceClientId, CancellationToken.None);

        // Assert
        LogAssert("Verifying list of ApiKeys was returned");
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetByServiceClientIdAsync_WhenNoItems_ShouldReturnEmptyList()
    {
        // Arrange
        LogArrange("Setting up mock to return an empty list");
        var executionContext = CreateTestExecutionContext();
        var serviceClientId = Id.CreateFromExistingInfo(Guid.NewGuid());

        _dataModelRepositoryMock
            .Setup(static x => x.GetByServiceClientIdAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ApiKeyDataModel>());

        // Act
        LogAct("Calling GetByServiceClientIdAsync");
        var result = await _repository.GetByServiceClientIdAsync(executionContext, serviceClientId, CancellationToken.None);

        // Assert
        LogAssert("Verifying empty list was returned");
        result.ShouldNotBeNull();
        result.Count.ShouldBe(0);
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
        var apiKey = CreateTestApiKey("prefix", "hash", Guid.NewGuid(), entityId);

        _dataModelRepositoryMock
            .Setup(static x => x.GetByIdAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingDataModel);

        _dataModelRepositoryMock
            .Setup(static x => x.UpdateAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<ApiKeyDataModel>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling UpdateAsync");
        var result = await _repository.UpdateAsync(executionContext, apiKey, CancellationToken.None);

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
                It.IsAny<ApiKeyDataModel>(),
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
        var apiKey = CreateTestApiKey("prefix", "hash", Guid.NewGuid());

        _dataModelRepositoryMock
            .Setup(static x => x.GetByIdAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApiKeyDataModel?)null);

        // Act
        LogAct("Calling UpdateAsync");
        var result = await _repository.UpdateAsync(executionContext, apiKey, CancellationToken.None);

        // Assert
        LogAssert("Verifying false was returned and UpdateAsync was not called");
        result.ShouldBeFalse();
        _dataModelRepositoryMock.Verify(
            static x => x.UpdateAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<ApiKeyDataModel>(),
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
        var apiKey = CreateTestApiKey("prefix", "hash", Guid.NewGuid(), entityId);

        _dataModelRepositoryMock
            .Setup(static x => x.GetByIdAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingDataModel);

        _dataModelRepositoryMock
            .Setup(static x => x.UpdateAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<ApiKeyDataModel>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling UpdateAsync");
        await _repository.UpdateAsync(executionContext, apiKey, CancellationToken.None);

        // Assert
        LogAssert("Verifying existing data model version was passed (not entity version)");
        _dataModelRepositoryMock.Verify(
            x => x.UpdateAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<ApiKeyDataModel>(),
                dbVersion,
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
        EnumerateAllItemHandler<ApiKey> handler = (_, _, _, _) => Task.FromResult(true);

        _dataModelRepositoryMock
            .Setup(static x => x.EnumerateAllAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<PaginationInfo>(),
                It.IsAny<DataModelItemHandler<ApiKeyDataModel>>(),
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
                It.IsAny<DataModelItemHandler<ApiKeyDataModel>>(),
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
        EnumerateAllItemHandler<ApiKey> handler = (_, _, _, _) => Task.FromResult(true);

        _dataModelRepositoryMock
            .Setup(static x => x.EnumerateAllAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<PaginationInfo>(),
                It.IsAny<DataModelItemHandler<ApiKeyDataModel>>(),
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
        EnumerateModifiedSinceItemHandler<ApiKey> handler = (_, _, _, _, _) => Task.FromResult(true);

        _dataModelRepositoryMock
            .Setup(static x => x.EnumerateModifiedSinceAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DataModelItemHandler<ApiKeyDataModel>>(),
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
                It.IsAny<DataModelItemHandler<ApiKeyDataModel>>(),
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
        EnumerateModifiedSinceItemHandler<ApiKey> handler = (_, _, _, _, _) => Task.FromResult(true);

        _dataModelRepositoryMock
            .Setup(static x => x.EnumerateModifiedSinceAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DataModelItemHandler<ApiKeyDataModel>>(),
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

    private static ApiKeyDataModel CreateTestDataModel(Guid id)
    {
        return new ApiKeyDataModel
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
            ServiceClientId = Guid.NewGuid(),
            KeyPrefix = Faker.Random.AlphaNumeric(8),
            KeyHash = Faker.Random.AlphaNumeric(64),
            Status = (short)ApiKeyStatus.Active,
            ExpiresAt = DateTimeOffset.UtcNow.AddYears(1),
            LastUsedAt = null,
            RevokedAt = null
        };
    }

    private static ApiKey CreateTestApiKey(
        string keyPrefix,
        string keyHash,
        Guid serviceClientId,
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

        return ApiKey.CreateFromExistingInfo(
            new CreateFromExistingInfoApiKeyInput(
                entityInfo,
                Id.CreateFromExistingInfo(serviceClientId),
                keyPrefix,
                keyHash,
                ApiKeyStatus.Active,
                DateTimeOffset.UtcNow.AddYears(1),
                null,
                null));
    }

    #endregion
}
