using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Messages;
using Bedrock.BuildingBlocks.Core.Tenants;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Domain.Entities.Models.Inputs;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using Bedrock.BuildingBlocks.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using ShopDemo.Auth.Domain.Entities.SigningKeys;
using ShopDemo.Auth.Domain.Entities.SigningKeys.Enums;
using ShopDemo.Auth.Domain.Entities.SigningKeys.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Infra.Data.Repositories;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.Repositories;

public class SigningKeyRepositoryTests : TestBase
{
    private readonly Mock<ILogger<SigningKeyRepository>> _loggerMock;
    private readonly Mock<ISigningKeyPostgreSqlRepository> _postgreSqlRepositoryMock;
    private readonly SigningKeyRepository _repository;

    public SigningKeyRepositoryTests(ITestOutputHelper output) : base(output)
    {
        _loggerMock = new Mock<ILogger<SigningKeyRepository>>();
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _postgreSqlRepositoryMock = new Mock<ISigningKeyPostgreSqlRepository>();
        _repository = new SigningKeyRepository(_loggerMock.Object, _postgreSqlRepositoryMock.Object);
    }

    // Constructor Tests

    [Fact]
    public void Constructor_WhenPostgreSqlRepositoryIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Preparando logger valido e repositorio PostgreSql nulo");

        // Act
        LogAct("Instanciando SigningKeyRepository com postgreSqlRepository nulo");
        Action act = () => new SigningKeyRepository(_loggerMock.Object, null!);

        // Assert
        LogAssert("Verificando que ArgumentNullException foi lancada");
        act.ShouldThrow<ArgumentNullException>();
    }

    // GetActiveAsync Tests

    [Fact]
    public async Task GetActiveAsync_WhenSigningKeyFound_ShouldReturnSigningKey()
    {
        // Arrange
        LogArrange("Preparando contexto e chave de assinatura ativa para retorno");
        var executionContext = CreateTestExecutionContext();
        var signingKey = CreateTestSigningKey(executionContext);
        _postgreSqlRepositoryMock
            .Setup(x => x.GetActiveAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(signingKey);

        // Act
        LogAct("Chamando GetActiveAsync");
        var result = await _repository.GetActiveAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a chave de assinatura ativa retornada nao e nula");
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetActiveAsync_WhenNoSigningKeyFound_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Preparando contexto e retorno nulo para chave de assinatura ativa nao encontrada");
        var executionContext = CreateTestExecutionContext();
        _postgreSqlRepositoryMock
            .Setup(x => x.GetActiveAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SigningKey?)null);

        // Act
        LogAct("Chamando GetActiveAsync");
        var result = await _repository.GetActiveAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o resultado retornado e nulo");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetActiveAsync_WhenExceptionThrown_ShouldLogAndReturnNull()
    {
        // Arrange
        LogArrange("Preparando contexto e configurando excecao no repositorio PostgreSql");
        var executionContext = CreateTestExecutionContext();
        _postgreSqlRepositoryMock
            .Setup(x => x.GetActiveAsync(executionContext, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        LogAct("Chamando GetActiveAsync esperando excecao");
        var result = await _repository.GetActiveAsync(executionContext, CancellationToken.None);

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

    // GetByKidAsync Tests

    [Fact]
    public async Task GetByKidAsync_WhenSigningKeyFound_ShouldReturnSigningKey()
    {
        // Arrange
        LogArrange("Preparando contexto e chave de assinatura por kid para retorno");
        var executionContext = CreateTestExecutionContext();
        var kid = Kid.CreateFromExistingInfo("test-kid");
        var signingKey = CreateTestSigningKey(executionContext);
        _postgreSqlRepositoryMock
            .Setup(x => x.GetByKidAsync(executionContext, kid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(signingKey);

        // Act
        LogAct("Chamando GetByKidAsync");
        var result = await _repository.GetByKidAsync(executionContext, kid, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a chave de assinatura retornada nao e nula");
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetByKidAsync_WhenSigningKeyNotFound_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Preparando contexto e retorno nulo para chave de assinatura nao encontrada");
        var executionContext = CreateTestExecutionContext();
        var kid = Kid.CreateFromExistingInfo("nonexistent-kid");
        _postgreSqlRepositoryMock
            .Setup(x => x.GetByKidAsync(executionContext, kid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SigningKey?)null);

        // Act
        LogAct("Chamando GetByKidAsync");
        var result = await _repository.GetByKidAsync(executionContext, kid, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o resultado retornado e nulo");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByKidAsync_WhenExceptionThrown_ShouldLogAndReturnNull()
    {
        // Arrange
        LogArrange("Preparando contexto e configurando excecao no repositorio PostgreSql");
        var executionContext = CreateTestExecutionContext();
        var kid = Kid.CreateFromExistingInfo("test-kid");
        _postgreSqlRepositoryMock
            .Setup(x => x.GetByKidAsync(executionContext, kid, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        LogAct("Chamando GetByKidAsync esperando excecao");
        var result = await _repository.GetByKidAsync(executionContext, kid, CancellationToken.None);

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

    // GetAllValidAsync Tests

    [Fact]
    public async Task GetAllValidAsync_WhenSigningKeysFound_ShouldReturnList()
    {
        // Arrange
        LogArrange("Preparando contexto e lista de chaves de assinatura validas para retorno");
        var executionContext = CreateTestExecutionContext();
        var signingKey = CreateTestSigningKey(executionContext);
        var expected = new List<SigningKey> { signingKey };
        _postgreSqlRepositoryMock
            .Setup(x => x.GetAllValidAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        LogAct("Chamando GetAllValidAsync");
        var result = await _repository.GetAllValidAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a lista retornada contem as chaves de assinatura esperadas");
        result.ShouldNotBeEmpty();
        result.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetAllValidAsync_WhenNoSigningKeysFound_ShouldReturnEmptyList()
    {
        // Arrange
        LogArrange("Preparando contexto e lista vazia para retorno");
        var executionContext = CreateTestExecutionContext();
        _postgreSqlRepositoryMock
            .Setup(x => x.GetAllValidAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        LogAct("Chamando GetAllValidAsync");
        var result = await _repository.GetAllValidAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a lista retornada esta vazia");
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAllValidAsync_WhenExceptionThrown_ShouldLogAndReturnEmptyList()
    {
        // Arrange
        LogArrange("Preparando contexto e configurando excecao no repositorio PostgreSql");
        var executionContext = CreateTestExecutionContext();
        _postgreSqlRepositoryMock
            .Setup(x => x.GetAllValidAsync(executionContext, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        LogAct("Chamando GetAllValidAsync esperando excecao");
        var result = await _repository.GetAllValidAsync(executionContext, CancellationToken.None);

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

    // UpdateAsync Tests

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UpdateAsync_WhenCalled_ShouldReturnExpectedResult(bool expectedResult)
    {
        // Arrange
        LogArrange("Preparando contexto e chave de assinatura para atualizar");
        var executionContext = CreateTestExecutionContext();
        var signingKey = CreateTestSigningKey(executionContext);
        _postgreSqlRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, signingKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        LogAct("Chamando UpdateAsync");
        var result = await _repository.UpdateAsync(executionContext, signingKey, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o resultado retornado e o esperado");
        result.ShouldBe(expectedResult);
    }

    [Fact]
    public async Task UpdateAsync_WhenExceptionThrown_ShouldLogAndReturnFalse()
    {
        // Arrange
        LogArrange("Preparando contexto e configurando excecao no repositorio PostgreSql");
        var executionContext = CreateTestExecutionContext();
        var signingKey = CreateTestSigningKey(executionContext);
        _postgreSqlRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, signingKey, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        LogAct("Chamando UpdateAsync esperando excecao");
        var result = await _repository.UpdateAsync(executionContext, signingKey, CancellationToken.None);

        // Assert
        LogAssert("Verificando que false foi retornado e o erro foi logado");
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
        LogArrange("Preparando contexto e id para buscar chave de assinatura por id");
        var executionContext = CreateTestExecutionContext();
        var id = Id.CreateFromExistingInfo(Guid.NewGuid());
        var signingKey = entityFound ? CreateTestSigningKey(executionContext) : null;
        _postgreSqlRepositoryMock
            .Setup(x => x.GetByIdAsync(executionContext, id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(signingKey);

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
        LogArrange("Preparando contexto e chave de assinatura para registrar");
        var executionContext = CreateTestExecutionContext();
        var signingKey = CreateTestSigningKey(executionContext);
        _postgreSqlRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, signingKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        LogAct("Chamando RegisterNewAsync");
        var result = await _repository.RegisterNewAsync(executionContext, signingKey, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o resultado retornado e o esperado");
        result.ShouldBe(expectedResult);
    }

    [Fact]
    public async Task EnumerateAllAsync_WhenCalled_ShouldReturnEmptyAsyncEnumerable()
    {
        // Arrange
        LogArrange("Preparando paginacao e handler para enumerar todas as chaves de assinatura");
        var paginationInfo = Bedrock.BuildingBlocks.Core.Paginations.PaginationInfo.All;
        var items = new List<SigningKey>();
        EnumerateAllItemHandler<SigningKey> handler = (item, ct) =>
        {
            items.Add(item);
            return ValueTask.CompletedTask;
        };

        // Act
        LogAct("Chamando EnumerateAllAsync");
        await _repository.EnumerateAllAsync(paginationInfo, handler, CancellationToken.None);

        // Assert
        LogAssert("Verificando que nenhum item foi enumerado (stub com yield break)");
        items.ShouldBeEmpty();
    }

    [Fact]
    public async Task EnumerateModifiedSinceAsync_WhenCalled_ShouldReturnEmptyAsyncEnumerable()
    {
        // Arrange
        LogArrange("Preparando contexto e handler para enumerar chaves de assinatura modificadas desde data");
        var executionContext = CreateTestExecutionContext();
        var since = DateTimeOffset.UtcNow.AddDays(-1);
        var items = new List<SigningKey>();
        EnumerateModifiedSinceItemHandler<SigningKey> handler = (item, ct) =>
        {
            items.Add(item);
            return ValueTask.CompletedTask;
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

    private static SigningKey CreateTestSigningKey(ExecutionContext executionContext)
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
        return SigningKey.CreateFromExistingInfo(
            new CreateFromExistingInfoSigningKeyInput(
                entityInfo,
                Kid.CreateFromExistingInfo("test-kid"),
                "RS256",
                "public-key-data",
                "encrypted-private-key-data",
                SigningKeyStatus.Active,
                null,
                DateTimeOffset.UtcNow.AddYears(1)));
    }
}
