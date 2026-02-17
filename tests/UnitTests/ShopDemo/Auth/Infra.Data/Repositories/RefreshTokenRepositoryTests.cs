using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Data.Repositories;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using Microsoft.Extensions.Logging;
using Moq;
using ShopDemo.Auth.Domain.Entities.RefreshTokens;
using ShopDemo.Auth.Domain.Entities.RefreshTokens.Enums;
using ShopDemo.Auth.Domain.Entities.RefreshTokens.Inputs;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Infra.Data.Repositories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.Repositories;

public class RefreshTokenRepositoryTests : TestBase
{
    private static readonly Faker Faker = new();

    private readonly Mock<ILogger<RefreshTokenRepository>> _loggerMock;
    private readonly Mock<IRefreshTokenPostgreSqlRepository> _postgreSqlRepositoryMock;
    private readonly RefreshTokenRepository _sut;

    public RefreshTokenRepositoryTests(ITestOutputHelper output) : base(output)
    {
        _loggerMock = new Mock<ILogger<RefreshTokenRepository>>();
        _loggerMock.Setup(static x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _postgreSqlRepositoryMock = new Mock<IRefreshTokenPostgreSqlRepository>();
        _sut = new RefreshTokenRepository(_loggerMock.Object, _postgreSqlRepositoryMock.Object);
    }

    #region Constructor

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        LogAssert("Verificando que construtor lanca ArgumentNullException para logger null");
        Should.Throw<ArgumentNullException>(() =>
            new RefreshTokenRepository(null!, _postgreSqlRepositoryMock.Object));
    }

    [Fact]
    public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        LogAssert("Verificando que construtor lanca ArgumentNullException para repository null");
        Should.Throw<ArgumentNullException>(() =>
            new RefreshTokenRepository(_loggerMock.Object, null!));
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        LogAct("Criando instancia com parametros validos");
        var repository = new RefreshTokenRepository(
            _loggerMock.Object, _postgreSqlRepositoryMock.Object);

        // Assert
        LogAssert("Verificando que instancia foi criada");
        repository.ShouldNotBeNull();
        repository.ShouldBeAssignableTo<RepositoryBase<RefreshToken>>();
        repository.ShouldBeAssignableTo<IRefreshTokenRepository>();
    }

    #endregion

    #region GetByUserIdAsync

    [Fact]
    public async Task GetByUserIdAsync_WhenTokensExist_ShouldReturnList()
    {
        // Arrange
        LogArrange("Configurando mock para retornar lista de tokens");
        ExecutionContext executionContext = CreateTestExecutionContext();
        Id userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var tokens = new List<RefreshToken> { CreateTestRefreshToken(executionContext) };

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tokens);

        // Act
        LogAct("Chamando GetByUserIdAsync");
        IReadOnlyList<RefreshToken> result = await _sut.GetByUserIdAsync(
            executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que lista foi retornada");
        result.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetByUserIdAsync_WhenNoTokens_ShouldReturnEmptyList()
    {
        // Arrange
        LogArrange("Configurando mock para retornar lista vazia");
        ExecutionContext executionContext = CreateTestExecutionContext();
        Id userId = Id.CreateFromExistingInfo(Guid.NewGuid());

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RefreshToken>());

        // Act
        LogAct("Chamando GetByUserIdAsync");
        IReadOnlyList<RefreshToken> result = await _sut.GetByUserIdAsync(
            executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que lista vazia foi retornada");
        result.Count.ShouldBe(0);
    }

    [Fact]
    public async Task GetByUserIdAsync_WhenExceptionThrown_ShouldReturnEmptyListAndLog()
    {
        // Arrange
        LogArrange("Configurando mock para lancar excecao");
        ExecutionContext executionContext = CreateTestExecutionContext();
        Id userId = Id.CreateFromExistingInfo(Guid.NewGuid());

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        LogAct("Chamando GetByUserIdAsync");
        IReadOnlyList<RefreshToken> result = await _sut.GetByUserIdAsync(
            executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que retornou lista vazia e logou erro");
        result.Count.ShouldBe(0);
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

    #region GetByTokenHashAsync

    [Fact]
    public async Task GetByTokenHashAsync_WhenTokenExists_ShouldReturnRefreshToken()
    {
        // Arrange
        LogArrange("Configurando mock para retornar token");
        ExecutionContext executionContext = CreateTestExecutionContext();
        TokenHash tokenHash = TokenHash.CreateNew(Faker.Random.Bytes(32));
        RefreshToken token = CreateTestRefreshToken(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByTokenHashAsync(executionContext, tokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        // Act
        LogAct("Chamando GetByTokenHashAsync");
        RefreshToken? result = await _sut.GetByTokenHashAsync(
            executionContext, tokenHash, CancellationToken.None);

        // Assert
        LogAssert("Verificando que token foi retornado");
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetByTokenHashAsync_WhenTokenDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Configurando mock para retornar null");
        ExecutionContext executionContext = CreateTestExecutionContext();
        TokenHash tokenHash = TokenHash.CreateNew(Faker.Random.Bytes(32));

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByTokenHashAsync(executionContext, tokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        LogAct("Chamando GetByTokenHashAsync");
        RefreshToken? result = await _sut.GetByTokenHashAsync(
            executionContext, tokenHash, CancellationToken.None);

        // Assert
        LogAssert("Verificando que retornou null");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByTokenHashAsync_WhenExceptionThrown_ShouldReturnNullAndLog()
    {
        // Arrange
        LogArrange("Configurando mock para lancar excecao");
        ExecutionContext executionContext = CreateTestExecutionContext();
        TokenHash tokenHash = TokenHash.CreateNew(Faker.Random.Bytes(32));

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByTokenHashAsync(executionContext, tokenHash, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        LogAct("Chamando GetByTokenHashAsync");
        RefreshToken? result = await _sut.GetByTokenHashAsync(
            executionContext, tokenHash, CancellationToken.None);

        // Assert
        LogAssert("Verificando que retornou null e logou erro");
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

    #region GetActiveByFamilyIdAsync

    [Fact]
    public async Task GetActiveByFamilyIdAsync_WhenTokensExist_ShouldReturnList()
    {
        // Arrange
        LogArrange("Configurando mock para retornar lista de tokens");
        ExecutionContext executionContext = CreateTestExecutionContext();
        TokenFamily familyId = TokenFamily.CreateFromExistingInfo(Guid.NewGuid());
        var tokens = new List<RefreshToken>
        {
            CreateTestRefreshToken(executionContext),
            CreateTestRefreshToken(executionContext)
        };

        _postgreSqlRepositoryMock
            .Setup(x => x.GetActiveByFamilyIdAsync(executionContext, familyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tokens);

        // Act
        LogAct("Chamando GetActiveByFamilyIdAsync");
        IReadOnlyList<RefreshToken> result = await _sut.GetActiveByFamilyIdAsync(
            executionContext, familyId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que lista com 2 itens foi retornada");
        result.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetActiveByFamilyIdAsync_WhenExceptionThrown_ShouldReturnEmptyListAndLog()
    {
        // Arrange
        LogArrange("Configurando mock para lancar excecao");
        ExecutionContext executionContext = CreateTestExecutionContext();
        TokenFamily familyId = TokenFamily.CreateFromExistingInfo(Guid.NewGuid());

        _postgreSqlRepositoryMock
            .Setup(x => x.GetActiveByFamilyIdAsync(executionContext, familyId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        LogAct("Chamando GetActiveByFamilyIdAsync");
        IReadOnlyList<RefreshToken> result = await _sut.GetActiveByFamilyIdAsync(
            executionContext, familyId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que retornou lista vazia e logou erro");
        result.Count.ShouldBe(0);
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

    #region UpdateAsync

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UpdateAsync_ShouldDelegateToPostgreSqlRepository(bool expectedResult)
    {
        // Arrange
        LogArrange($"Configurando mock para retornar {expectedResult}");
        ExecutionContext executionContext = CreateTestExecutionContext();
        RefreshToken entity = CreateTestRefreshToken(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, entity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        LogAct("Chamando UpdateAsync");
        bool result = await _sut.UpdateAsync(
            executionContext, entity, CancellationToken.None);

        // Assert
        LogAssert($"Verificando que retornou {expectedResult}");
        result.ShouldBe(expectedResult);
    }

    [Fact]
    public async Task UpdateAsync_WhenExceptionThrown_ShouldReturnFalseAndLog()
    {
        // Arrange
        LogArrange("Configurando mock para lancar excecao");
        ExecutionContext executionContext = CreateTestExecutionContext();
        RefreshToken entity = CreateTestRefreshToken(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, entity, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        LogAct("Chamando UpdateAsync");
        bool result = await _sut.UpdateAsync(
            executionContext, entity, CancellationToken.None);

        // Assert
        LogAssert("Verificando que retornou false e logou erro");
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

    #region ExistsAsync (via RepositoryBase)

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExistsAsync_ShouldDelegateToPostgreSqlRepository(bool expectedResult)
    {
        // Arrange
        LogArrange($"Configurando mock para retornar {expectedResult}");
        ExecutionContext executionContext = CreateTestExecutionContext();
        Id id = Id.CreateFromExistingInfo(Guid.NewGuid());

        _postgreSqlRepositoryMock
            .Setup(x => x.ExistsAsync(executionContext, id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        LogAct("Chamando ExistsAsync");
        bool result = await _sut.ExistsAsync(
            executionContext, id, CancellationToken.None);

        // Assert
        LogAssert($"Verificando que retornou {expectedResult}");
        result.ShouldBe(expectedResult);
    }

    #endregion

    #region GetByIdAsync (via RepositoryBase)

    [Fact]
    public async Task GetByIdAsync_WhenTokenExists_ShouldReturnRefreshToken()
    {
        // Arrange
        LogArrange("Configurando mock para retornar token");
        ExecutionContext executionContext = CreateTestExecutionContext();
        RefreshToken token = CreateTestRefreshToken(executionContext);
        Id id = token.EntityInfo.Id;

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByIdAsync(executionContext, id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        // Act
        LogAct("Chamando GetByIdAsync");
        RefreshToken? result = await _sut.GetByIdAsync(
            executionContext, id, CancellationToken.None);

        // Assert
        LogAssert("Verificando que token foi retornado");
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WhenTokenDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Configurando mock para retornar null");
        ExecutionContext executionContext = CreateTestExecutionContext();
        Id id = Id.CreateFromExistingInfo(Guid.NewGuid());

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByIdAsync(executionContext, id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        LogAct("Chamando GetByIdAsync");
        RefreshToken? result = await _sut.GetByIdAsync(
            executionContext, id, CancellationToken.None);

        // Assert
        LogAssert("Verificando que retornou null");
        result.ShouldBeNull();
    }

    #endregion

    #region RegisterNewAsync (via RepositoryBase)

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RegisterNewAsync_ShouldDelegateToPostgreSqlRepository(bool expectedResult)
    {
        // Arrange
        LogArrange($"Configurando mock para retornar {expectedResult}");
        ExecutionContext executionContext = CreateTestExecutionContext();
        RefreshToken entity = CreateTestRefreshToken(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, entity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        LogAct("Chamando RegisterNewAsync");
        bool result = await _sut.RegisterNewAsync(
            executionContext, entity, CancellationToken.None);

        // Assert
        LogAssert($"Verificando que retornou {expectedResult}");
        result.ShouldBe(expectedResult);
    }

    #endregion

    #region EnumerateAllAsync (via RepositoryBase - stub yield break)

    [Fact]
    public async Task EnumerateAllAsync_ShouldReturnTrueWithNoItems()
    {
        // Arrange
        LogArrange("Configurando handler para capturar itens");
        ExecutionContext executionContext = CreateTestExecutionContext();
        PaginationInfo paginationInfo = PaginationInfo.Create(1, 10);
        var itemsReceived = new List<RefreshToken>();
        EnumerateAllItemHandler<RefreshToken> handler = (ctx, item, pagination, ct) =>
        {
            itemsReceived.Add(item);
            return Task.FromResult(true);
        };

        // Act
        LogAct("Chamando EnumerateAllAsync");
        bool result = await _sut.EnumerateAllAsync(
            executionContext, paginationInfo, handler, CancellationToken.None);

        // Assert
        LogAssert("Verificando que retornou true e nenhum item foi recebido");
        result.ShouldBeTrue();
        itemsReceived.ShouldBeEmpty();
    }

    #endregion

    #region EnumerateModifiedSinceAsync (via RepositoryBase - stub yield break)

    [Fact]
    public async Task EnumerateModifiedSinceAsync_ShouldReturnTrueWithNoItems()
    {
        // Arrange
        LogArrange("Configurando handler para capturar itens");
        ExecutionContext executionContext = CreateTestExecutionContext();
        DateTimeOffset since = DateTimeOffset.UtcNow.AddHours(-1);
        var itemsReceived = new List<RefreshToken>();
        EnumerateModifiedSinceItemHandler<RefreshToken> handler = (ctx, item, tp, s, ct) =>
        {
            itemsReceived.Add(item);
            return Task.FromResult(true);
        };

        // Act
        LogAct("Chamando EnumerateModifiedSinceAsync");
        bool result = await _sut.EnumerateModifiedSinceAsync(
            executionContext, TimeProvider.System, since, handler, CancellationToken.None);

        // Assert
        LogAssert("Verificando que retornou true e nenhum item foi recebido");
        result.ShouldBeTrue();
        itemsReceived.ShouldBeEmpty();
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

    private static RefreshToken CreateTestRefreshToken(ExecutionContext executionContext)
    {
        EntityInfo entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(Guid.NewGuid()),
            tenantInfo: executionContext.TenantInfo,
            createdAt: DateTimeOffset.UtcNow,
            createdBy: executionContext.ExecutionUser,
            createdCorrelationId: executionContext.CorrelationId,
            createdExecutionOrigin: executionContext.ExecutionOrigin,
            createdBusinessOperationCode: executionContext.BusinessOperationCode,
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

    #endregion
}
