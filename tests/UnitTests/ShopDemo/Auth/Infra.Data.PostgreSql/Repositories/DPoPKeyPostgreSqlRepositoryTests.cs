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
using ShopDemo.Auth.Domain.Entities.DPoPKeys;
using ShopDemo.Auth.Domain.Entities.DPoPKeys.Enums;
using ShopDemo.Auth.Domain.Entities.DPoPKeys.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Repositories;

public class DPoPKeyPostgreSqlRepositoryTests : TestBase
{
    private static readonly Faker Faker = new();

    private readonly Mock<IDPoPKeyDataModelRepository> _dataModelRepositoryMock;
    private readonly DPoPKeyPostgreSqlRepository _repository;

    public DPoPKeyPostgreSqlRepositoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _dataModelRepositoryMock = new Mock<IDPoPKeyDataModelRepository>();
        _repository = new DPoPKeyPostgreSqlRepository(_dataModelRepositoryMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullDataModelRepository_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Preparing null argument for constructor");

        // Act
        LogAct("Attempting to create DPoPKeyPostgreSqlRepository with null");
        var action = () => new DPoPKeyPostgreSqlRepository(null!);

        // Assert
        LogAssert("Verifying ArgumentNullException is thrown");
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithValidDataModelRepository_ShouldCreateInstance()
    {
        // Arrange
        LogArrange("Creating mock IDPoPKeyDataModelRepository");
        var dataModelRepositoryMock = new Mock<IDPoPKeyDataModelRepository>();

        // Act
        LogAct("Creating DPoPKeyPostgreSqlRepository");
        var repository = new DPoPKeyPostgreSqlRepository(dataModelRepositoryMock.Object);

        // Assert
        LogAssert("Verifying instance was created");
        repository.ShouldNotBeNull();
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenDataModelFound_ShouldReturnDPoPKey()
    {
        // Arrange
        LogArrange("Setting up mock to return a DPoPKeyDataModel");
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
        LogAssert("Verifying DPoPKey was created from DataModel");
        result.ShouldNotBeNull();
        result.JwkThumbprint.Value.ShouldBe(dataModel.JwkThumbprint);
        result.PublicKeyJwk.ShouldBe(dataModel.PublicKeyJwk);
        result.Status.ShouldBe((DPoPKeyStatus)dataModel.Status);
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
            .ReturnsAsync((DPoPKeyDataModel?)null);

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
        LogArrange("Creating DPoPKey entity and setting up mock");
        var executionContext = CreateTestExecutionContext();
        var dPoPKey = CreateTestDPoPKey("thumbprint-abc", "{\"kty\":\"EC\"}", Guid.NewGuid());

        _dataModelRepositoryMock
            .Setup(static x => x.InsertAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<DPoPKeyDataModel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling RegisterNewAsync");
        var result = await _repository.RegisterNewAsync(executionContext, dPoPKey, CancellationToken.None);

        // Assert
        LogAssert("Verifying InsertAsync was called with a DPoPKeyDataModel");
        result.ShouldBeTrue();
        _dataModelRepositoryMock.Verify(
            static x => x.InsertAsync(
                It.IsAny<ExecutionContext>(),
                It.Is<DPoPKeyDataModel>(static dm =>
                    dm.JwkThumbprint == "thumbprint-abc" &&
                    dm.PublicKeyJwk == "{\"kty\":\"EC\"}"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetActiveByUserIdAndThumbprintAsync Tests

    [Fact]
    public async Task GetActiveByUserIdAndThumbprintAsync_WhenFound_ShouldReturnDPoPKey()
    {
        // Arrange
        LogArrange("Setting up mock to return a DPoPKeyDataModel by userId and thumbprint");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var thumbprint = JwkThumbprint.CreateNew("my-thumbprint");
        var dataModel = CreateTestDataModel(Guid.NewGuid());
        dataModel.JwkThumbprint = thumbprint.Value;

        _dataModelRepositoryMock
            .Setup(static x => x.GetActiveByUserIdAndThumbprintAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dataModel);

        // Act
        LogAct("Calling GetActiveByUserIdAndThumbprintAsync");
        var result = await _repository.GetActiveByUserIdAndThumbprintAsync(
            executionContext, userId, thumbprint, CancellationToken.None);

        // Assert
        LogAssert("Verifying DPoPKey was returned from DataModel");
        result.ShouldNotBeNull();
        result.JwkThumbprint.Value.ShouldBe(thumbprint.Value);
    }

    [Fact]
    public async Task GetActiveByUserIdAndThumbprintAsync_WhenNotFound_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up mock to return null for userId and thumbprint search");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var thumbprint = JwkThumbprint.CreateNew("nonexistent-thumbprint");

        _dataModelRepositoryMock
            .Setup(static x => x.GetActiveByUserIdAndThumbprintAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((DPoPKeyDataModel?)null);

        // Act
        LogAct("Calling GetActiveByUserIdAndThumbprintAsync");
        var result = await _repository.GetActiveByUserIdAndThumbprintAsync(
            executionContext, userId, thumbprint, CancellationToken.None);

        // Assert
        LogAssert("Verifying null was returned");
        result.ShouldBeNull();
    }

    #endregion

    #region GetByUserIdAsync Tests

    [Fact]
    public async Task GetByUserIdAsync_WhenItemsExist_ShouldReturnDPoPKeys()
    {
        // Arrange
        LogArrange("Setting up mock to return a list of DPoPKeyDataModels");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var dataModels = new List<DPoPKeyDataModel>
        {
            CreateTestDataModel(Guid.NewGuid()),
            CreateTestDataModel(Guid.NewGuid())
        };

        _dataModelRepositoryMock
            .Setup(static x => x.GetByUserIdAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dataModels);

        // Act
        LogAct("Calling GetByUserIdAsync");
        var result = await _repository.GetByUserIdAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verifying list of DPoPKeys was returned");
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetByUserIdAsync_WhenNoItems_ShouldReturnEmptyList()
    {
        // Arrange
        LogArrange("Setting up mock to return an empty list");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());

        _dataModelRepositoryMock
            .Setup(static x => x.GetByUserIdAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DPoPKeyDataModel>());

        // Act
        LogAct("Calling GetByUserIdAsync");
        var result = await _repository.GetByUserIdAsync(executionContext, userId, CancellationToken.None);

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
        var dPoPKey = CreateTestDPoPKey("thumbprint", "{\"kty\":\"EC\"}", Guid.NewGuid(), entityId);

        _dataModelRepositoryMock
            .Setup(static x => x.GetByIdAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingDataModel);

        _dataModelRepositoryMock
            .Setup(static x => x.UpdateAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<DPoPKeyDataModel>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling UpdateAsync");
        var result = await _repository.UpdateAsync(executionContext, dPoPKey, CancellationToken.None);

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
                It.IsAny<DPoPKeyDataModel>(),
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
        var dPoPKey = CreateTestDPoPKey("thumbprint", "{\"kty\":\"EC\"}", Guid.NewGuid());

        _dataModelRepositoryMock
            .Setup(static x => x.GetByIdAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((DPoPKeyDataModel?)null);

        // Act
        LogAct("Calling UpdateAsync");
        var result = await _repository.UpdateAsync(executionContext, dPoPKey, CancellationToken.None);

        // Assert
        LogAssert("Verifying false was returned and UpdateAsync was not called");
        result.ShouldBeFalse();
        _dataModelRepositoryMock.Verify(
            static x => x.UpdateAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<DPoPKeyDataModel>(),
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
        var dPoPKey = CreateTestDPoPKey("thumbprint", "{\"kty\":\"EC\"}", Guid.NewGuid(), entityId);

        _dataModelRepositoryMock
            .Setup(static x => x.GetByIdAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingDataModel);

        _dataModelRepositoryMock
            .Setup(static x => x.UpdateAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<DPoPKeyDataModel>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling UpdateAsync");
        await _repository.UpdateAsync(executionContext, dPoPKey, CancellationToken.None);

        // Assert
        LogAssert("Verifying existing data model version was passed (not entity version)");
        _dataModelRepositoryMock.Verify(
            x => x.UpdateAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<DPoPKeyDataModel>(),
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
        EnumerateAllItemHandler<DPoPKey> handler = (_, _, _, _) => Task.FromResult(true);

        _dataModelRepositoryMock
            .Setup(static x => x.EnumerateAllAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<PaginationInfo>(),
                It.IsAny<DataModelItemHandler<DPoPKeyDataModel>>(),
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
                It.IsAny<DataModelItemHandler<DPoPKeyDataModel>>(),
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
        EnumerateAllItemHandler<DPoPKey> handler = (_, _, _, _) => Task.FromResult(true);

        _dataModelRepositoryMock
            .Setup(static x => x.EnumerateAllAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<PaginationInfo>(),
                It.IsAny<DataModelItemHandler<DPoPKeyDataModel>>(),
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
        EnumerateModifiedSinceItemHandler<DPoPKey> handler = (_, _, _, _, _) => Task.FromResult(true);

        _dataModelRepositoryMock
            .Setup(static x => x.EnumerateModifiedSinceAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DataModelItemHandler<DPoPKeyDataModel>>(),
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
                It.IsAny<DataModelItemHandler<DPoPKeyDataModel>>(),
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
        EnumerateModifiedSinceItemHandler<DPoPKey> handler = (_, _, _, _, _) => Task.FromResult(true);

        _dataModelRepositoryMock
            .Setup(static x => x.EnumerateModifiedSinceAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DataModelItemHandler<DPoPKeyDataModel>>(),
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

    private static DPoPKeyDataModel CreateTestDataModel(Guid id)
    {
        return new DPoPKeyDataModel
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
            UserId = Guid.NewGuid(),
            JwkThumbprint = Faker.Random.AlphaNumeric(32),
            PublicKeyJwk = "{\"kty\":\"EC\",\"crv\":\"P-256\"}",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(1),
            Status = (short)DPoPKeyStatus.Active,
            RevokedAt = null
        };
    }

    private static DPoPKey CreateTestDPoPKey(
        string jwkThumbprint,
        string publicKeyJwk,
        Guid userId,
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

        return DPoPKey.CreateFromExistingInfo(
            new CreateFromExistingInfoDPoPKeyInput(
                entityInfo,
                Id.CreateFromExistingInfo(userId),
                JwkThumbprint.CreateNew(jwkThumbprint),
                publicKeyJwk,
                DateTimeOffset.UtcNow.AddDays(1),
                DPoPKeyStatus.Active,
                null));
    }

    #endregion
}
