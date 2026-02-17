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
using ShopDemo.Auth.Domain.Entities.RefreshTokens;
using ShopDemo.Auth.Domain.Entities.RefreshTokens.Enums;
using ShopDemo.Auth.Domain.Entities.RefreshTokens.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Repositories;

public class RefreshTokenPostgreSqlRepositoryTests : TestBase
{
    private static readonly Faker Faker = new();

    private readonly Mock<IRefreshTokenDataModelRepository> _dataModelRepositoryMock;
    private readonly RefreshTokenPostgreSqlRepository _repository;

    public RefreshTokenPostgreSqlRepositoryTests(ITestOutputHelper output) : base(output)
    {
        _dataModelRepositoryMock = new Mock<IRefreshTokenDataModelRepository>();
        _repository = new RefreshTokenPostgreSqlRepository(_dataModelRepositoryMock.Object);
    }

    #region Constructor

    [Fact]
    public void Constructor_WithNullDataModelRepository_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        LogAssert("Verificando que construtor lanca ArgumentNullException para null");
        Should.Throw<ArgumentNullException>(() =>
            new RefreshTokenPostgreSqlRepository(null!));
    }

    [Fact]
    public void Constructor_WithValidDependency_ShouldCreateInstance()
    {
        // Arrange & Act
        LogAct("Criando instancia com dependencia valida");
        var repository = new RefreshTokenPostgreSqlRepository(_dataModelRepositoryMock.Object);

        // Assert
        LogAssert("Verificando que instancia foi criada");
        repository.ShouldNotBeNull();
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_WhenDataModelExists_ShouldReturnRefreshToken()
    {
        // Arrange
        LogArrange("Configurando mock para retornar data model");
        ExecutionContext executionContext = CreateTestExecutionContext();
        Guid entityId = Guid.NewGuid();
        Id id = Id.CreateFromExistingInfo(entityId);
        RefreshTokenDataModel dataModel = CreateTestDataModel(entityId);

        _dataModelRepositoryMock
            .Setup(static x => x.GetByIdAsync(
                It.IsAny<ExecutionContext>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dataModel);

        // Act
        LogAct("Chamando GetByIdAsync");
        RefreshToken? result = await _repository.GetByIdAsync(
            executionContext, id, CancellationToken.None);

        // Assert
        LogAssert("Verificando que RefreshToken foi retornado");
        result.ShouldNotBeNull();
        result.EntityInfo.Id.Value.ShouldBe(entityId);
    }

    [Fact]
    public async Task GetByIdAsync_WhenDataModelDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Configurando mock para retornar null");
        ExecutionContext executionContext = CreateTestExecutionContext();
        Id id = Id.CreateFromExistingInfo(Guid.NewGuid());

        _dataModelRepositoryMock
            .Setup(static x => x.GetByIdAsync(
                It.IsAny<ExecutionContext>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshTokenDataModel?)null);

        // Act
        LogAct("Chamando GetByIdAsync");
        RefreshToken? result = await _repository.GetByIdAsync(
            executionContext, id, CancellationToken.None);

        // Assert
        LogAssert("Verificando que retornou null");
        result.ShouldBeNull();
    }

    #endregion

    #region ExistsAsync

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExistsAsync_ShouldDelegateToDataModelRepository(bool expectedResult)
    {
        // Arrange
        LogArrange($"Configurando mock para retornar {expectedResult}");
        ExecutionContext executionContext = CreateTestExecutionContext();
        Id id = Id.CreateFromExistingInfo(Guid.NewGuid());

        _dataModelRepositoryMock
            .Setup(static x => x.ExistsAsync(
                It.IsAny<ExecutionContext>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        LogAct("Chamando ExistsAsync");
        bool result = await _repository.ExistsAsync(
            executionContext, id, CancellationToken.None);

        // Assert
        LogAssert($"Verificando que retornou {expectedResult}");
        result.ShouldBe(expectedResult);
    }

    #endregion

    #region RegisterNewAsync

    [Fact]
    public async Task RegisterNewAsync_ShouldCreateDataModelAndInsert()
    {
        // Arrange
        LogArrange("Configurando mock para InsertAsync");
        ExecutionContext executionContext = CreateTestExecutionContext();
        RefreshToken entity = CreateTestRefreshToken();

        _dataModelRepositoryMock
            .Setup(static x => x.InsertAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<RefreshTokenDataModel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Chamando RegisterNewAsync");
        bool result = await _repository.RegisterNewAsync(
            executionContext, entity, CancellationToken.None);

        // Assert
        LogAssert("Verificando que retornou true e InsertAsync foi chamado");
        result.ShouldBeTrue();
        _dataModelRepositoryMock.Verify(
            static x => x.InsertAsync(
                It.IsAny<ExecutionContext>(),
                It.Is<RefreshTokenDataModel>(static dm => dm.UserId != Guid.Empty),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetByUserIdAsync

    [Fact]
    public async Task GetByUserIdAsync_WhenTokensExist_ShouldReturnList()
    {
        // Arrange
        LogArrange("Configurando mock para retornar lista de data models");
        ExecutionContext executionContext = CreateTestExecutionContext();
        Guid userId = Guid.NewGuid();
        Id userIdObj = Id.CreateFromExistingInfo(userId);
        var dataModels = new List<RefreshTokenDataModel>
        {
            CreateTestDataModel(userId: userId),
            CreateTestDataModel(userId: userId)
        };

        _dataModelRepositoryMock
            .Setup(x => x.GetByUserIdAsync(
                It.IsAny<ExecutionContext>(), userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dataModels);

        // Act
        LogAct("Chamando GetByUserIdAsync");
        IReadOnlyList<RefreshToken> result = await _repository.GetByUserIdAsync(
            executionContext, userIdObj, CancellationToken.None);

        // Assert
        LogAssert("Verificando que lista com 2 itens foi retornada");
        result.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetByUserIdAsync_WhenNoTokens_ShouldReturnEmptyList()
    {
        // Arrange
        LogArrange("Configurando mock para retornar lista vazia");
        ExecutionContext executionContext = CreateTestExecutionContext();
        Id userId = Id.CreateFromExistingInfo(Guid.NewGuid());

        _dataModelRepositoryMock
            .Setup(x => x.GetByUserIdAsync(
                It.IsAny<ExecutionContext>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RefreshTokenDataModel>());

        // Act
        LogAct("Chamando GetByUserIdAsync");
        IReadOnlyList<RefreshToken> result = await _repository.GetByUserIdAsync(
            executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que lista vazia foi retornada");
        result.Count.ShouldBe(0);
    }

    #endregion

    #region GetByTokenHashAsync

    [Fact]
    public async Task GetByTokenHashAsync_WhenTokenExists_ShouldReturnRefreshToken()
    {
        // Arrange
        LogArrange("Configurando mock para retornar data model");
        ExecutionContext executionContext = CreateTestExecutionContext();
        byte[] hashBytes = Faker.Random.Bytes(32);
        TokenHash tokenHash = TokenHash.CreateNew(hashBytes);
        RefreshTokenDataModel dataModel = CreateTestDataModel(tokenHash: hashBytes);

        _dataModelRepositoryMock
            .Setup(x => x.GetByTokenHashAsync(
                It.IsAny<ExecutionContext>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dataModel);

        // Act
        LogAct("Chamando GetByTokenHashAsync");
        RefreshToken? result = await _repository.GetByTokenHashAsync(
            executionContext, tokenHash, CancellationToken.None);

        // Assert
        LogAssert("Verificando que RefreshToken foi retornado");
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetByTokenHashAsync_WhenTokenDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Configurando mock para retornar null");
        ExecutionContext executionContext = CreateTestExecutionContext();
        TokenHash tokenHash = TokenHash.CreateNew(Faker.Random.Bytes(32));

        _dataModelRepositoryMock
            .Setup(x => x.GetByTokenHashAsync(
                It.IsAny<ExecutionContext>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshTokenDataModel?)null);

        // Act
        LogAct("Chamando GetByTokenHashAsync");
        RefreshToken? result = await _repository.GetByTokenHashAsync(
            executionContext, tokenHash, CancellationToken.None);

        // Assert
        LogAssert("Verificando que retornou null");
        result.ShouldBeNull();
    }

    #endregion

    #region GetActiveByFamilyIdAsync

    [Fact]
    public async Task GetActiveByFamilyIdAsync_WhenTokensExist_ShouldReturnList()
    {
        // Arrange
        LogArrange("Configurando mock para retornar lista de data models");
        ExecutionContext executionContext = CreateTestExecutionContext();
        Guid familyIdGuid = Guid.NewGuid();
        TokenFamily familyId = TokenFamily.CreateFromExistingInfo(familyIdGuid);
        var dataModels = new List<RefreshTokenDataModel>
        {
            CreateTestDataModel(familyId: familyIdGuid),
            CreateTestDataModel(familyId: familyIdGuid)
        };

        _dataModelRepositoryMock
            .Setup(x => x.GetActiveByFamilyIdAsync(
                It.IsAny<ExecutionContext>(), familyIdGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dataModels);

        // Act
        LogAct("Chamando GetActiveByFamilyIdAsync");
        IReadOnlyList<RefreshToken> result = await _repository.GetActiveByFamilyIdAsync(
            executionContext, familyId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que lista com 2 itens foi retornada");
        result.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetActiveByFamilyIdAsync_WhenNoTokens_ShouldReturnEmptyList()
    {
        // Arrange
        LogArrange("Configurando mock para retornar lista vazia");
        ExecutionContext executionContext = CreateTestExecutionContext();
        TokenFamily familyId = TokenFamily.CreateFromExistingInfo(Guid.NewGuid());

        _dataModelRepositoryMock
            .Setup(x => x.GetActiveByFamilyIdAsync(
                It.IsAny<ExecutionContext>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RefreshTokenDataModel>());

        // Act
        LogAct("Chamando GetActiveByFamilyIdAsync");
        IReadOnlyList<RefreshToken> result = await _repository.GetActiveByFamilyIdAsync(
            executionContext, familyId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que lista vazia foi retornada");
        result.Count.ShouldBe(0);
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_WhenEntityExists_ShouldAdaptAndUpdate()
    {
        // Arrange
        LogArrange("Configurando mock para GetByIdAsync e UpdateAsync");
        ExecutionContext executionContext = CreateTestExecutionContext();
        Guid entityId = Guid.NewGuid();
        Guid entityUserId = Guid.NewGuid();
        long expectedVersion = 3;
        RefreshToken entity = CreateTestRefreshTokenWithVersion(entityId, expectedVersion, entityUserId);
        RefreshTokenDataModel existingDataModel = CreateTestDataModel(entityId);
        existingDataModel.EntityVersion = expectedVersion;

        RefreshTokenDataModel? capturedDataModel = null;

        _dataModelRepositoryMock
            .Setup(static x => x.GetByIdAsync(
                It.IsAny<ExecutionContext>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingDataModel);

        _dataModelRepositoryMock
            .Setup(static x => x.UpdateAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<RefreshTokenDataModel>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .Callback<ExecutionContext, RefreshTokenDataModel, long, CancellationToken>(
                (_, dm, _, _) => capturedDataModel = dm)
            .ReturnsAsync(true);

        // Act
        LogAct("Chamando UpdateAsync");
        bool result = await _repository.UpdateAsync(
            executionContext, entity, CancellationToken.None);

        // Assert
        LogAssert("Verificando que UpdateAsync retornou true e campos foram adaptados");
        result.ShouldBeTrue();
        capturedDataModel.ShouldNotBeNull();
        capturedDataModel.ShouldBeSameAs(existingDataModel);
        capturedDataModel.UserId.ShouldBe(entityUserId);

        _dataModelRepositoryMock.Verify(
            static x => x.UpdateAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<RefreshTokenDataModel>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenEntityDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Configurando mock para retornar null no GetByIdAsync");
        ExecutionContext executionContext = CreateTestExecutionContext();
        RefreshToken entity = CreateTestRefreshToken();

        _dataModelRepositoryMock
            .Setup(static x => x.GetByIdAsync(
                It.IsAny<ExecutionContext>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshTokenDataModel?)null);

        // Act
        LogAct("Chamando UpdateAsync");
        bool result = await _repository.UpdateAsync(
            executionContext, entity, CancellationToken.None);

        // Assert
        LogAssert("Verificando que retornou false e UpdateAsync nao foi chamado");
        result.ShouldBeFalse();
        _dataModelRepositoryMock.Verify(
            static x => x.UpdateAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<RefreshTokenDataModel>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPassExpectedVersionFromExistingDataModel()
    {
        // Arrange
        LogArrange("Configurando mock com version especifica");
        ExecutionContext executionContext = CreateTestExecutionContext();
        Guid entityId = Guid.NewGuid();
        long expectedVersion = 7;
        RefreshToken entity = CreateTestRefreshTokenWithVersion(entityId, expectedVersion);
        RefreshTokenDataModel existingDataModel = CreateTestDataModel(entityId);
        existingDataModel.EntityVersion = expectedVersion;

        long capturedVersion = 0;

        _dataModelRepositoryMock
            .Setup(static x => x.GetByIdAsync(
                It.IsAny<ExecutionContext>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingDataModel);

        _dataModelRepositoryMock
            .Setup(static x => x.UpdateAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<RefreshTokenDataModel>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .Callback<ExecutionContext, RefreshTokenDataModel, long, CancellationToken>(
                (_, _, version, _) => capturedVersion = version)
            .ReturnsAsync(true);

        // Act
        LogAct("Chamando UpdateAsync");
        await _repository.UpdateAsync(executionContext, entity, CancellationToken.None);

        // Assert
        LogAssert("Verificando que version correta foi passada");
        capturedVersion.ShouldBe(expectedVersion);
    }

    #endregion

    #region EnumerateAllAsync

    [Fact]
    public async Task EnumerateAllAsync_ShouldDelegateToDataModelRepository()
    {
        // Arrange
        LogArrange("Configurando mock para EnumerateAllAsync");
        ExecutionContext executionContext = CreateTestExecutionContext();
        PaginationInfo paginationInfo = PaginationInfo.Create(1, 10);
        EnumerateAllItemHandler<RefreshToken> handler = (_, _, _, _) => Task.FromResult(true);

        _dataModelRepositoryMock
            .Setup(static x => x.EnumerateAllAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<PaginationInfo>(),
                It.IsAny<DataModelItemHandler<RefreshTokenDataModel>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Chamando EnumerateAllAsync");
        bool result = await _repository.EnumerateAllAsync(
            executionContext, paginationInfo, handler, CancellationToken.None);

        // Assert
        LogAssert("Verificando que retornou true e delegou para data model repository");
        result.ShouldBeTrue();
        _dataModelRepositoryMock.Verify(
            static x => x.EnumerateAllAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<PaginationInfo>(),
                It.IsAny<DataModelItemHandler<RefreshTokenDataModel>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region EnumerateModifiedSinceAsync

    [Fact]
    public async Task EnumerateModifiedSinceAsync_ShouldDelegateToDataModelRepository()
    {
        // Arrange
        LogArrange("Configurando mock para EnumerateModifiedSinceAsync");
        ExecutionContext executionContext = CreateTestExecutionContext();
        DateTimeOffset since = DateTimeOffset.UtcNow.AddHours(-1);
        EnumerateModifiedSinceItemHandler<RefreshToken> handler = (_, _, _, _, _) => Task.FromResult(true);

        _dataModelRepositoryMock
            .Setup(static x => x.EnumerateModifiedSinceAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DataModelItemHandler<RefreshTokenDataModel>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Chamando EnumerateModifiedSinceAsync");
        bool result = await _repository.EnumerateModifiedSinceAsync(
            executionContext, TimeProvider.System, since, handler, CancellationToken.None);

        // Assert
        LogAssert("Verificando que retornou true e delegou para data model repository");
        result.ShouldBeTrue();
        _dataModelRepositoryMock.Verify(
            static x => x.EnumerateModifiedSinceAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DataModelItemHandler<RefreshTokenDataModel>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Helper Methods

    private static ExecutionContext CreateTestExecutionContext()
    {
        TenantInfo tenantInfo = TenantInfo.Create(Guid.NewGuid());
        return ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: tenantInfo,
            executionUser: "test-user",
            executionOrigin: "UnitTest",
            businessOperationCode: "TEST_OP",
            minimumMessageType: MessageType.Trace,
            timeProvider: TimeProvider.System);
    }

    private static RefreshTokenDataModel CreateTestDataModel(
        Guid? entityId = null,
        Guid? userId = null,
        byte[]? tokenHash = null,
        Guid? familyId = null)
    {
        return new RefreshTokenDataModel
        {
            Id = entityId ?? Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedCorrelationId = Guid.NewGuid(),
            CreatedExecutionOrigin = "UnitTest",
            CreatedBusinessOperationCode = "CREATE_TOKEN",
            LastChangedBy = null,
            LastChangedAt = null,
            LastChangedCorrelationId = null,
            LastChangedExecutionOrigin = null,
            LastChangedBusinessOperationCode = null,
            EntityVersion = 1,
            UserId = userId ?? Guid.NewGuid(),
            TokenHash = tokenHash ?? Faker.Random.Bytes(32),
            FamilyId = familyId ?? Guid.NewGuid(),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            Status = (short)RefreshTokenStatus.Active,
            RevokedAt = null,
            ReplacedByTokenId = null
        };
    }

    private static RefreshToken CreateTestRefreshToken(Guid? entityId = null)
    {
        EntityInfo entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(entityId ?? Guid.NewGuid()),
            tenantInfo: TenantInfo.Create(Guid.NewGuid()),
            createdAt: DateTimeOffset.UtcNow,
            createdBy: "test-creator",
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "UnitTest",
            createdBusinessOperationCode: "CREATE_TOKEN",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null,
            entityVersion: RegistryVersion.CreateFromExistingInfo(1));

        return RefreshToken.CreateFromExistingInfo(
            new CreateFromExistingInfoRefreshTokenInput(
                entityInfo,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                TokenHash.CreateNew(Faker.Random.Bytes(32)),
                TokenFamily.CreateFromExistingInfo(Guid.NewGuid()),
                DateTimeOffset.UtcNow.AddDays(7),
                RefreshTokenStatus.Active,
                null,
                null));
    }

    private static RefreshToken CreateTestRefreshTokenWithVersion(Guid entityId, long entityVersion, Guid? userId = null)
    {
        EntityInfo entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(entityId),
            tenantInfo: TenantInfo.Create(Guid.NewGuid()),
            createdAt: DateTimeOffset.UtcNow,
            createdBy: "test-creator",
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "UnitTest",
            createdBusinessOperationCode: "CREATE_TOKEN",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null,
            entityVersion: RegistryVersion.CreateFromExistingInfo(entityVersion));

        return RefreshToken.CreateFromExistingInfo(
            new CreateFromExistingInfoRefreshTokenInput(
                entityInfo,
                Id.CreateFromExistingInfo(userId ?? Guid.NewGuid()),
                TokenHash.CreateNew(Faker.Random.Bytes(32)),
                TokenFamily.CreateFromExistingInfo(Guid.NewGuid()),
                DateTimeOffset.UtcNow.AddDays(7),
                RefreshTokenStatus.Active,
                null,
                null));
    }

    #endregion
}
