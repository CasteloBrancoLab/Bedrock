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
using ShopDemo.Auth.Domain.Entities.Sessions;
using ShopDemo.Auth.Domain.Entities.Sessions.Enums;
using ShopDemo.Auth.Domain.Entities.Sessions.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Infra.Data.Repositories;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.Repositories;

public class SessionRepositoryTests : TestBase
{
    private readonly Mock<ILogger<SessionRepository>> _loggerMock;
    private readonly Mock<ISessionPostgreSqlRepository> _postgreSqlRepositoryMock;
    private readonly SessionRepository _repository;

    public SessionRepositoryTests(ITestOutputHelper output) : base(output)
    {
        _loggerMock = new Mock<ILogger<SessionRepository>>();
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _postgreSqlRepositoryMock = new Mock<ISessionPostgreSqlRepository>();
        _repository = new SessionRepository(_loggerMock.Object, _postgreSqlRepositoryMock.Object);
    }

    // Constructor Tests

    [Fact]
    public void Constructor_WhenPostgreSqlRepositoryIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Preparando logger valido e repositorio PostgreSql nulo");

        // Act
        LogAct("Instanciando SessionRepository com postgreSqlRepository nulo");
        Action act = () => new SessionRepository(_loggerMock.Object, null!);

        // Assert
        LogAssert("Verificando que ArgumentNullException foi lancada");
        act.ShouldThrow<ArgumentNullException>();
    }

    // GetByUserIdAsync Tests

    [Fact]
    public async Task GetByUserIdAsync_WhenSessionsFound_ShouldReturnList()
    {
        // Arrange
        LogArrange("Preparando contexto e lista de sessoes para retorno");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var session = CreateTestSession(executionContext);
        var expected = new List<Session> { session };
        _postgreSqlRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        LogAct("Chamando GetByUserIdAsync");
        var result = await _repository.GetByUserIdAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a lista retornada contem as sessoes esperadas");
        result.ShouldNotBeEmpty();
        result.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetByUserIdAsync_WhenNoSessionsFound_ShouldReturnEmptyList()
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

    // GetActiveByUserIdAsync Tests

    [Fact]
    public async Task GetActiveByUserIdAsync_WhenSessionsFound_ShouldReturnList()
    {
        // Arrange
        LogArrange("Preparando contexto e lista de sessoes ativas para retorno");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var session = CreateTestSession(executionContext);
        var expected = new List<Session> { session };
        _postgreSqlRepositoryMock
            .Setup(x => x.GetActiveByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        LogAct("Chamando GetActiveByUserIdAsync");
        var result = await _repository.GetActiveByUserIdAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a lista retornada contem as sessoes ativas esperadas");
        result.ShouldNotBeEmpty();
        result.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetActiveByUserIdAsync_WhenNoSessionsFound_ShouldReturnEmptyList()
    {
        // Arrange
        LogArrange("Preparando contexto e lista vazia para retorno");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        _postgreSqlRepositoryMock
            .Setup(x => x.GetActiveByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        LogAct("Chamando GetActiveByUserIdAsync");
        var result = await _repository.GetActiveByUserIdAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a lista retornada esta vazia");
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetActiveByUserIdAsync_WhenExceptionThrown_ShouldLogAndReturnEmptyList()
    {
        // Arrange
        LogArrange("Preparando contexto e configurando excecao no repositorio PostgreSql");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        _postgreSqlRepositoryMock
            .Setup(x => x.GetActiveByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        LogAct("Chamando GetActiveByUserIdAsync esperando excecao");
        var result = await _repository.GetActiveByUserIdAsync(executionContext, userId, CancellationToken.None);

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

    // CountActiveByUserIdAsync Tests

    [Fact]
    public async Task CountActiveByUserIdAsync_WhenSessionsExist_ShouldReturnCount()
    {
        // Arrange
        LogArrange("Preparando contexto e contagem de sessoes ativas para retorno");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        _postgreSqlRepositoryMock
            .Setup(x => x.CountActiveByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        // Act
        LogAct("Chamando CountActiveByUserIdAsync");
        var result = await _repository.CountActiveByUserIdAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a contagem retornada e a esperada");
        result.ShouldBe(3);
    }

    [Fact]
    public async Task CountActiveByUserIdAsync_WhenNoSessionsExist_ShouldReturnZero()
    {
        // Arrange
        LogArrange("Preparando contexto e contagem zero para retorno");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        _postgreSqlRepositoryMock
            .Setup(x => x.CountActiveByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        LogAct("Chamando CountActiveByUserIdAsync");
        var result = await _repository.CountActiveByUserIdAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a contagem retornada e zero");
        result.ShouldBe(0);
    }

    [Fact]
    public async Task CountActiveByUserIdAsync_WhenExceptionThrown_ShouldLogAndReturnZero()
    {
        // Arrange
        LogArrange("Preparando contexto e configurando excecao no repositorio PostgreSql");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        _postgreSqlRepositoryMock
            .Setup(x => x.CountActiveByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        LogAct("Chamando CountActiveByUserIdAsync esperando excecao");
        var result = await _repository.CountActiveByUserIdAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que zero foi retornado e o erro foi logado");
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

    // UpdateAsync Tests

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UpdateAsync_WhenCalled_ShouldReturnExpectedResult(bool expectedResult)
    {
        // Arrange
        LogArrange("Preparando contexto e sessao para atualizar");
        var executionContext = CreateTestExecutionContext();
        var session = CreateTestSession(executionContext);
        _postgreSqlRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, session, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        LogAct("Chamando UpdateAsync");
        var result = await _repository.UpdateAsync(executionContext, session, CancellationToken.None);

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
        var session = CreateTestSession(executionContext);
        _postgreSqlRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, session, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        LogAct("Chamando UpdateAsync esperando excecao");
        var result = await _repository.UpdateAsync(executionContext, session, CancellationToken.None);

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
        LogArrange("Preparando contexto e id para buscar sessao por id");
        var executionContext = CreateTestExecutionContext();
        var id = Id.CreateFromExistingInfo(Guid.NewGuid());
        var session = entityFound ? CreateTestSession(executionContext) : null;
        _postgreSqlRepositoryMock
            .Setup(x => x.GetByIdAsync(executionContext, id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

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
        LogArrange("Preparando contexto e sessao para registrar");
        var executionContext = CreateTestExecutionContext();
        var session = CreateTestSession(executionContext);
        _postgreSqlRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, session, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        LogAct("Chamando RegisterNewAsync");
        var result = await _repository.RegisterNewAsync(executionContext, session, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o resultado retornado e o esperado");
        result.ShouldBe(expectedResult);
    }

    [Fact]
    public async Task EnumerateAllAsync_WhenCalled_ShouldReturnEmptyAsyncEnumerable()
    {
        // Arrange
        LogArrange("Preparando paginacao e handler para enumerar todas as sessoes");
        var paginationInfo = Bedrock.BuildingBlocks.Core.Paginations.PaginationInfo.All;
        var items = new List<Session>();
        EnumerateAllItemHandler<Session> handler = (_, item, _, _) =>
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
        LogArrange("Preparando contexto e handler para enumerar sessoes modificadas desde data");
        var executionContext = CreateTestExecutionContext();
        var since = DateTimeOffset.UtcNow.AddDays(-1);
        var items = new List<Session>();
        EnumerateModifiedSinceItemHandler<Session> handler = (_, item, _, _, _) =>
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

    private static Session CreateTestSession(ExecutionContext executionContext)
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
        return Session.CreateFromExistingInfo(
            new CreateFromExistingInfoSessionInput(
                entityInfo,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                null,
                null,
                null,
                DateTimeOffset.UtcNow.AddHours(1),
                SessionStatus.Active,
                DateTimeOffset.UtcNow,
                null));
    }
}
