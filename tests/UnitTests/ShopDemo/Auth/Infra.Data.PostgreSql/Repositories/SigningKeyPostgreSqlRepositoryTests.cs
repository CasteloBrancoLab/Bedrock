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
using ShopDemo.Auth.Domain.Entities.SigningKeys;
using ShopDemo.Auth.Domain.Entities.SigningKeys.Enums;
using ShopDemo.Auth.Domain.Entities.SigningKeys.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Repositories;

public class SigningKeyPostgreSqlRepositoryTests : TestBase
{
    private static readonly Faker Faker = new();

    private readonly Mock<ISigningKeyDataModelRepository> _dataModelRepositoryMock;
    private readonly SigningKeyPostgreSqlRepository _repository;

    public SigningKeyPostgreSqlRepositoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _dataModelRepositoryMock = new Mock<ISigningKeyDataModelRepository>();
        _repository = new SigningKeyPostgreSqlRepository(_dataModelRepositoryMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullDataModelRepository_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Preparing null argument for constructor");

        // Act
        LogAct("Attempting to create SigningKeyPostgreSqlRepository with null");
        var action = () => new SigningKeyPostgreSqlRepository(null!);

        // Assert
        LogAssert("Verifying ArgumentNullException is thrown");
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithValidDataModelRepository_ShouldCreateInstance()
    {
        // Arrange
        LogArrange("Creating mock ISigningKeyDataModelRepository");
        var dataModelRepositoryMock = new Mock<ISigningKeyDataModelRepository>();

        // Act
        LogAct("Creating SigningKeyPostgreSqlRepository");
        var repository = new SigningKeyPostgreSqlRepository(dataModelRepositoryMock.Object);

        // Assert
        LogAssert("Verifying instance was created");
        repository.ShouldNotBeNull();
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenDataModelFound_ShouldReturnSigningKey()
    {
        // Arrange
        LogArrange("Setting up mock to return a SigningKeyDataModel");
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
        LogAssert("Verifying SigningKey was created from DataModel");
        result.ShouldNotBeNull();
        result.EntityInfo.Id.Value.ShouldBe(entityId);
        result.Kid.Value.ShouldBe(dataModel.Kid);
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
            .ReturnsAsync((SigningKeyDataModel?)null);

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
        LogArrange("Creating SigningKey entity and setting up mock");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestSigningKey();

        _dataModelRepositoryMock
            .Setup(static x => x.InsertAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<SigningKeyDataModel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling RegisterNewAsync");
        var result = await _repository.RegisterNewAsync(executionContext, entity, CancellationToken.None);

        // Assert
        LogAssert("Verifying InsertAsync was called with a SigningKeyDataModel");
        result.ShouldBeTrue();
        _dataModelRepositoryMock.Verify(
            static x => x.InsertAsync(
                It.IsAny<ExecutionContext>(),
                It.Is<SigningKeyDataModel>(static dm => dm.Kid != null),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetActiveAsync Tests

    [Fact]
    public async Task GetActiveAsync_WhenActiveKeyExists_ShouldReturnSigningKey()
    {
        // Arrange
        LogArrange("Setting up mock to return an active SigningKeyDataModel");
        var executionContext = CreateTestExecutionContext();
        var dataModel = CreateTestDataModel();

        _dataModelRepositoryMock
            .Setup(static x => x.GetActiveAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dataModel);

        // Act
        LogAct("Calling GetActiveAsync");
        var result = await _repository.GetActiveAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying SigningKey was returned");
        result.ShouldNotBeNull();
        result.Kid.Value.ShouldBe(dataModel.Kid);
    }

    [Fact]
    public async Task GetActiveAsync_WhenNoActiveKeyExists_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up mock to return null for active signing key");
        var executionContext = CreateTestExecutionContext();

        _dataModelRepositoryMock
            .Setup(static x => x.GetActiveAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((SigningKeyDataModel?)null);

        // Act
        LogAct("Calling GetActiveAsync");
        var result = await _repository.GetActiveAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying null was returned");
        result.ShouldBeNull();
    }

    #endregion

    #region GetByKidAsync Tests

    [Fact]
    public async Task GetByKidAsync_WhenKeyExists_ShouldReturnSigningKey()
    {
        // Arrange
        LogArrange("Setting up mock to return a SigningKeyDataModel by kid");
        var executionContext = CreateTestExecutionContext();
        string kidValue = Faker.Random.AlphaNumeric(16);
        var kid = Kid.CreateFromExistingInfo(kidValue);
        var dataModel = CreateTestDataModel(kid: kidValue);

        _dataModelRepositoryMock
            .Setup(static x => x.GetByKidAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dataModel);

        // Act
        LogAct("Calling GetByKidAsync");
        var result = await _repository.GetByKidAsync(executionContext, kid, CancellationToken.None);

        // Assert
        LogAssert("Verifying SigningKey was returned");
        result.ShouldNotBeNull();
        result.Kid.Value.ShouldBe(kidValue);
    }

    [Fact]
    public async Task GetByKidAsync_WhenKeyDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up mock to return null for kid search");
        var executionContext = CreateTestExecutionContext();
        var kid = Kid.CreateFromExistingInfo(Faker.Random.AlphaNumeric(16));

        _dataModelRepositoryMock
            .Setup(static x => x.GetByKidAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((SigningKeyDataModel?)null);

        // Act
        LogAct("Calling GetByKidAsync");
        var result = await _repository.GetByKidAsync(executionContext, kid, CancellationToken.None);

        // Assert
        LogAssert("Verifying null was returned");
        result.ShouldBeNull();
    }

    #endregion

    #region GetAllValidAsync Tests

    [Fact]
    public async Task GetAllValidAsync_WhenValidKeysExist_ShouldReturnList()
    {
        // Arrange
        LogArrange("Setting up mock to return a list of valid SigningKeyDataModels");
        var executionContext = CreateTestExecutionContext();
        var dataModels = new List<SigningKeyDataModel>
        {
            CreateTestDataModel(),
            CreateTestDataModel()
        };

        _dataModelRepositoryMock
            .Setup(static x => x.GetAllValidAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dataModels);

        // Act
        LogAct("Calling GetAllValidAsync");
        var result = await _repository.GetAllValidAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying list with 2 items was returned");
        result.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetAllValidAsync_WhenNoValidKeysExist_ShouldReturnEmptyList()
    {
        // Arrange
        LogArrange("Setting up mock to return empty list of valid keys");
        var executionContext = CreateTestExecutionContext();

        _dataModelRepositoryMock
            .Setup(static x => x.GetAllValidAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SigningKeyDataModel>());

        // Act
        LogAct("Calling GetAllValidAsync");
        var result = await _repository.GetAllValidAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying empty list was returned");
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
        var entity = CreateTestSigningKey(entityId);

        _dataModelRepositoryMock
            .Setup(static x => x.GetByIdAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingDataModel);

        _dataModelRepositoryMock
            .Setup(static x => x.UpdateAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<SigningKeyDataModel>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling UpdateAsync");
        var result = await _repository.UpdateAsync(executionContext, entity, CancellationToken.None);

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
                It.IsAny<SigningKeyDataModel>(),
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
        var entity = CreateTestSigningKey();

        _dataModelRepositoryMock
            .Setup(static x => x.GetByIdAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((SigningKeyDataModel?)null);

        // Act
        LogAct("Calling UpdateAsync");
        var result = await _repository.UpdateAsync(executionContext, entity, CancellationToken.None);

        // Assert
        LogAssert("Verifying false was returned and UpdateAsync was not called");
        result.ShouldBeFalse();
        _dataModelRepositoryMock.Verify(
            static x => x.UpdateAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<SigningKeyDataModel>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPassExistingDataModelVersionToDataModelRepository()
    {
        // Arrange
        LogArrange("Setting up mock to capture existing data model version");
        var executionContext = CreateTestExecutionContext();
        var entityId = Guid.NewGuid();
        long dbVersion = Faker.Random.Long(1, 100);
        var existingDataModel = CreateTestDataModel(entityId);
        existingDataModel.EntityVersion = dbVersion;
        var entity = CreateTestSigningKeyWithVersion(entityId, dbVersion + 50);

        long capturedVersion = 0;

        _dataModelRepositoryMock
            .Setup(static x => x.GetByIdAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingDataModel);

        _dataModelRepositoryMock
            .Setup(static x => x.UpdateAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<SigningKeyDataModel>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .Callback<ExecutionContext, SigningKeyDataModel, long, CancellationToken>(
                (_, _, version, _) => capturedVersion = version)
            .ReturnsAsync(true);

        // Act
        LogAct("Calling UpdateAsync");
        await _repository.UpdateAsync(executionContext, entity, CancellationToken.None);

        // Assert
        LogAssert("Verifying existing data model version was passed (not entity version)");
        capturedVersion.ShouldBe(dbVersion);
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
        EnumerateAllItemHandler<SigningKey> handler = (_, _, _, _) => Task.FromResult(true);

        _dataModelRepositoryMock
            .Setup(static x => x.EnumerateAllAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<PaginationInfo>(),
                It.IsAny<DataModelItemHandler<SigningKeyDataModel>>(),
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
                It.IsAny<DataModelItemHandler<SigningKeyDataModel>>(),
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
        EnumerateAllItemHandler<SigningKey> handler = (_, _, _, _) => Task.FromResult(true);

        _dataModelRepositoryMock
            .Setup(static x => x.EnumerateAllAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<PaginationInfo>(),
                It.IsAny<DataModelItemHandler<SigningKeyDataModel>>(),
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
        EnumerateModifiedSinceItemHandler<SigningKey> handler = (_, _, _, _, _) => Task.FromResult(true);

        _dataModelRepositoryMock
            .Setup(static x => x.EnumerateModifiedSinceAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DataModelItemHandler<SigningKeyDataModel>>(),
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
                It.IsAny<DataModelItemHandler<SigningKeyDataModel>>(),
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
        EnumerateModifiedSinceItemHandler<SigningKey> handler = (_, _, _, _, _) => Task.FromResult(true);

        _dataModelRepositoryMock
            .Setup(static x => x.EnumerateModifiedSinceAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DataModelItemHandler<SigningKeyDataModel>>(),
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
        var tenantInfo = TenantInfo.Create(Guid.NewGuid());
        return ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: tenantInfo,
            executionUser: "test.user",
            executionOrigin: "UnitTest",
            businessOperationCode: "TEST_OP",
            minimumMessageType: MessageType.Trace,
            timeProvider: TimeProvider.System);
    }

    private static SigningKeyDataModel CreateTestDataModel(Guid? entityId = null, string? kid = null)
    {
        return new SigningKeyDataModel
        {
            Id = entityId ?? Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedCorrelationId = Guid.NewGuid(),
            CreatedExecutionOrigin = "UnitTest",
            CreatedBusinessOperationCode = "TEST_OP",
            LastChangedBy = null,
            LastChangedAt = null,
            LastChangedCorrelationId = null,
            LastChangedExecutionOrigin = null,
            LastChangedBusinessOperationCode = null,
            EntityVersion = 1,
            Kid = kid ?? Faker.Random.AlphaNumeric(16),
            Algorithm = "RS256",
            PublicKey = Faker.Random.AlphaNumeric(256),
            EncryptedPrivateKey = Faker.Random.AlphaNumeric(512),
            Status = (short)SigningKeyStatus.Active,
            RotatedAt = null,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(90)
        };
    }

    private static SigningKey CreateTestSigningKey(Guid? entityId = null)
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
            entityVersion: RegistryVersion.CreateFromExistingInfo(1));

        return SigningKey.CreateFromExistingInfo(
            new CreateFromExistingInfoSigningKeyInput(
                entityInfo,
                Kid.CreateFromExistingInfo(Faker.Random.AlphaNumeric(16)),
                "RS256",
                Faker.Random.AlphaNumeric(256),
                Faker.Random.AlphaNumeric(512),
                SigningKeyStatus.Active,
                null,
                DateTimeOffset.UtcNow.AddDays(90)));
    }

    private static SigningKey CreateTestSigningKeyWithVersion(Guid entityId, long entityVersion)
    {
        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(entityId),
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
            entityVersion: RegistryVersion.CreateFromExistingInfo(entityVersion));

        return SigningKey.CreateFromExistingInfo(
            new CreateFromExistingInfoSigningKeyInput(
                entityInfo,
                Kid.CreateFromExistingInfo(Faker.Random.AlphaNumeric(16)),
                "RS256",
                Faker.Random.AlphaNumeric(256),
                Faker.Random.AlphaNumeric(512),
                SigningKeyStatus.Active,
                null,
                DateTimeOffset.UtcNow.AddDays(90)));
    }

    #endregion
}
