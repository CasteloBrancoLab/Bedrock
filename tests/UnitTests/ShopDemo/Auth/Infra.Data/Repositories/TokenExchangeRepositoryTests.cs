using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using Bedrock.BuildingBlocks.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using ShopDemo.Auth.Domain.Entities.TokenExchanges;
using ShopDemo.Auth.Domain.Entities.TokenExchanges.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Infra.Data.Repositories;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.Repositories;

public class TokenExchangeRepositoryTests : TestBase
{
    private readonly Mock<ILogger<TokenExchangeRepository>> _loggerMock;
    private readonly Mock<ITokenExchangePostgreSqlRepository> _postgreSqlRepositoryMock;
    private readonly TokenExchangeRepository _repository;

    public TokenExchangeRepositoryTests(ITestOutputHelper output) : base(output)
    {
        _loggerMock = new Mock<ILogger<TokenExchangeRepository>>();
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _postgreSqlRepositoryMock = new Mock<ITokenExchangePostgreSqlRepository>();
        _repository = new TokenExchangeRepository(_loggerMock.Object, _postgreSqlRepositoryMock.Object);
    }

    // Constructor Tests

    [Fact]
    public void Constructor_WhenPostgreSqlRepositoryIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Preparando logger valido e repositorio PostgreSql nulo");

        // Act
        LogAct("Instanciando TokenExchangeRepository com postgreSqlRepository nulo");
        Action act = () => new TokenExchangeRepository(_loggerMock.Object, null!);

        // Assert
        LogAssert("Verificando que ArgumentNullException foi lancada");
        act.ShouldThrow<ArgumentNullException>();
    }

    // GetByUserIdAsync Tests

    [Fact]
    public async Task GetByUserIdAsync_WhenTokenExchangesFound_ShouldReturnList()
    {
        // Arrange
        LogArrange("Preparando contexto e lista de trocas de token para retorno");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var tokenExchange = CreateTestTokenExchange(executionContext);
        var expected = new List<TokenExchange> { tokenExchange };
        _postgreSqlRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        LogAct("Chamando GetByUserIdAsync");
        var result = await _repository.GetByUserIdAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a lista retornada contem as trocas de token esperadas");
        result.ShouldNotBeEmpty();
        result.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetByUserIdAsync_WhenNoTokenExchangesFound_ShouldReturnEmptyList()
    {
        // Arrange
        LogArrange("Preparando contexto e lista vazia para retorno");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        _postgreSqlRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        LogAct("Chamando GetByUserIdAsync");
        var result = await _repository.GetByUserIdAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a lista retornada esta vazia");
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetByUserIdAsync_WhenExceptionThrown_ShouldLogAndReturnEmptyList()
    {
        // Arrange
        LogArrange("Preparando contexto e configurando excecao no repositorio PostgreSql");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        _postgreSqlRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        LogAct("Chamando GetByUserIdAsync esperando excecao");
        var result = await _repository.GetByUserIdAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a lista vazia foi retornada e o erro foi logado");
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

    // GetByIssuedTokenJtiAsync Tests

    [Fact]
    public async Task GetByIssuedTokenJtiAsync_WhenTokenExchangeFound_ShouldReturnTokenExchange()
    {
        // Arrange
        LogArrange("Preparando contexto e troca de token por JTI para retorno");
        var executionContext = CreateTestExecutionContext();
        var issuedTokenJti = "test-jti-value";
        var tokenExchange = CreateTestTokenExchange(executionContext);
        _postgreSqlRepositoryMock
            .Setup(x => x.GetByIssuedTokenJtiAsync(executionContext, issuedTokenJti, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tokenExchange);

        // Act
        LogAct("Chamando GetByIssuedTokenJtiAsync");
        var result = await _repository.GetByIssuedTokenJtiAsync(executionContext, issuedTokenJti, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a troca de token retornada nao e nula");
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetByIssuedTokenJtiAsync_WhenTokenExchangeNotFound_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Preparando contexto e retorno nulo para troca de token nao encontrada");
        var executionContext = CreateTestExecutionContext();
        var issuedTokenJti = "nonexistent-jti-value";
        _postgreSqlRepositoryMock
            .Setup(x => x.GetByIssuedTokenJtiAsync(executionContext, issuedTokenJti, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TokenExchange?)null);

        // Act
        LogAct("Chamando GetByIssuedTokenJtiAsync");
        var result = await _repository.GetByIssuedTokenJtiAsync(executionContext, issuedTokenJti, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o resultado retornado e nulo");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByIssuedTokenJtiAsync_WhenExceptionThrown_ShouldLogAndReturnNull()
    {
        // Arrange
        LogArrange("Preparando contexto e configurando excecao no repositorio PostgreSql");
        var executionContext = CreateTestExecutionContext();
        var issuedTokenJti = "test-jti-value";
        _postgreSqlRepositoryMock
            .Setup(x => x.GetByIssuedTokenJtiAsync(executionContext, issuedTokenJti, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        LogAct("Chamando GetByIssuedTokenJtiAsync esperando excecao");
        var result = await _repository.GetByIssuedTokenJtiAsync(executionContext, issuedTokenJti, CancellationToken.None);

        // Assert
        LogAssert("Verificando que null foi retornado e o erro foi logado");
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

    // Base Class Method Tests

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExistsAsync_WhenCalled_ShouldReturnExpectedResult(bool expectedResult)
    {
        // Arrange
        LogArrange("Preparando contexto e id para verificar existencia");
        var executionContext = CreateTestExecutionContext();
        var id = Id.CreateFromExistingInfo(Guid.NewGuid());
        _postgreSqlRepositoryMock
            .Setup(x => x.ExistsAsync(executionContext, id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        LogAct("Chamando ExistsAsync");
        var result = await _repository.ExistsAsync(executionContext, id, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o resultado retornado e o esperado");
        result.ShouldBe(expectedResult);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetByIdAsync_WhenCalled_ShouldReturnExpectedResult(bool entityFound)
    {
        // Arrange
        LogArrange("Preparando contexto e id para buscar troca de token por id");
        var executionContext = CreateTestExecutionContext();
        var id = Id.CreateFromExistingInfo(Guid.NewGuid());
        var tokenExchange = entityFound ? CreateTestTokenExchange(executionContext) : null;
        _postgreSqlRepositoryMock
            .Setup(x => x.GetByIdAsync(executionContext, id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tokenExchange);

        // Act
        LogAct("Chamando GetByIdAsync");
        var result = await _repository.GetByIdAsync(executionContext, id, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o resultado retornado e o esperado");
        if (entityFound)
            result.ShouldNotBeNull();
        else
            result.ShouldBeNull();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RegisterNewAsync_WhenCalled_ShouldReturnExpectedResult(bool expectedResult)
    {
        // Arrange
        LogArrange("Preparando contexto e troca de token para registrar");
        var executionContext = CreateTestExecutionContext();
        var tokenExchange = CreateTestTokenExchange(executionContext);
        _postgreSqlRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, tokenExchange, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        LogAct("Chamando RegisterNewAsync");
        var result = await _repository.RegisterNewAsync(executionContext, tokenExchange, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o resultado retornado e o esperado");
        result.ShouldBe(expectedResult);
    }

    [Fact]
    public async Task EnumerateAllAsync_WhenCalled_ShouldReturnEmptyAsyncEnumerable()
    {
        // Arrange
        LogArrange("Preparando paginacao e handler para enumerar todas as trocas de token");
        var paginationInfo = Bedrock.BuildingBlocks.Core.Paginations.PaginationInfo.All;
        var items = new List<TokenExchange>();
        EnumerateAllItemHandler<TokenExchange> handler = (_, item, _, _) =>
        {
            items.Add(item);
            return Task.FromResult(true);
        };

        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Chamando EnumerateAllAsync");
        await _repository.EnumerateAllAsync(executionContext, paginationInfo, handler, CancellationToken.None);

        // Assert
        LogAssert("Verificando que nenhum item foi enumerado (stub com yield break)");
        items.ShouldBeEmpty();
    }

    [Fact]
    public async Task EnumerateModifiedSinceAsync_WhenCalled_ShouldReturnEmptyAsyncEnumerable()
    {
        // Arrange
        LogArrange("Preparando contexto e handler para enumerar trocas de token modificadas desde data");
        var executionContext = CreateTestExecutionContext();
        var since = DateTimeOffset.UtcNow.AddDays(-1);
        var items = new List<TokenExchange>();
        EnumerateModifiedSinceItemHandler<TokenExchange> handler = (_, item, _, _, _) =>
        {
            items.Add(item);
            return Task.FromResult(true);
        };

        // Act
        LogAct("Chamando EnumerateModifiedSinceAsync");
        await _repository.EnumerateModifiedSinceAsync(executionContext, TimeProvider.System, since, handler, CancellationToken.None);

        // Assert
        LogAssert("Verificando que nenhum item foi enumerado (stub com yield break)");
        items.ShouldBeEmpty();
    }

    // Helper Methods

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

    private static TokenExchange CreateTestTokenExchange(ExecutionContext executionContext)
    {
        var entityInfo = EntityInfo.CreateFromExistingInfo(
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
        return TokenExchange.CreateFromExistingInfo(
            new CreateFromExistingInfoTokenExchangeInput(
                entityInfo,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                "subject-token-jti",
                "test-audience",
                "issued-token-jti",
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddHours(1)));
    }
}
