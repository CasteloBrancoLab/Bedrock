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
using ShopDemo.Auth.Domain.Entities.ServiceClients;
using ShopDemo.Auth.Domain.Entities.ServiceClients.Enums;
using ShopDemo.Auth.Domain.Entities.ServiceClients.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Infra.Data.Repositories;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.Repositories;

public class ServiceClientRepositoryTests : TestBase
{
    private readonly Mock<ILogger<ServiceClientRepository>> _loggerMock;
    private readonly Mock<IServiceClientPostgreSqlRepository> _postgreSqlRepositoryMock;
    private readonly ServiceClientRepository _repository;

    public ServiceClientRepositoryTests(ITestOutputHelper output) : base(output)
    {
        _loggerMock = new Mock<ILogger<ServiceClientRepository>>();
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _postgreSqlRepositoryMock = new Mock<IServiceClientPostgreSqlRepository>();
        _repository = new ServiceClientRepository(_loggerMock.Object, _postgreSqlRepositoryMock.Object);
    }

    // Constructor Tests

    [Fact]
    public void Constructor_WhenPostgreSqlRepositoryIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Preparando logger valido e repositorio PostgreSql nulo");

        // Act
        LogAct("Instanciando ServiceClientRepository com postgreSqlRepository nulo");
        Action act = () => new ServiceClientRepository(_loggerMock.Object, null!);

        // Assert
        LogAssert("Verificando que ArgumentNullException foi lancada");
        act.ShouldThrow<ArgumentNullException>();
    }

    // GetByClientIdAsync Tests

    [Fact]
    public async Task GetByClientIdAsync_WhenServiceClientFound_ShouldReturnServiceClient()
    {
        // Arrange
        LogArrange("Preparando contexto e cliente de servico para retorno");
        var executionContext = CreateTestExecutionContext();
        var clientId = "test-client-id";
        var serviceClient = CreateTestServiceClient(executionContext);
        _postgreSqlRepositoryMock
            .Setup(x => x.GetByClientIdAsync(executionContext, clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceClient);

        // Act
        LogAct("Chamando GetByClientIdAsync");
        var result = await _repository.GetByClientIdAsync(executionContext, clientId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o cliente de servico retornado nao e nulo");
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetByClientIdAsync_WhenServiceClientNotFound_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Preparando contexto e retorno nulo para cliente de servico nao encontrado");
        var executionContext = CreateTestExecutionContext();
        var clientId = "nonexistent-client-id";
        _postgreSqlRepositoryMock
            .Setup(x => x.GetByClientIdAsync(executionContext, clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ServiceClient?)null);

        // Act
        LogAct("Chamando GetByClientIdAsync");
        var result = await _repository.GetByClientIdAsync(executionContext, clientId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o resultado retornado e nulo");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByClientIdAsync_WhenExceptionThrown_ShouldLogAndReturnNull()
    {
        // Arrange
        LogArrange("Preparando contexto e configurando excecao no repositorio PostgreSql");
        var executionContext = CreateTestExecutionContext();
        var clientId = "test-client-id";
        _postgreSqlRepositoryMock
            .Setup(x => x.GetByClientIdAsync(executionContext, clientId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        LogAct("Chamando GetByClientIdAsync esperando excecao");
        var result = await _repository.GetByClientIdAsync(executionContext, clientId, CancellationToken.None);

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

    // GetByCreatorUserIdAsync Tests

    [Fact]
    public async Task GetByCreatorUserIdAsync_WhenServiceClientsFound_ShouldReturnList()
    {
        // Arrange
        LogArrange("Preparando contexto e lista de clientes de servico para retorno");
        var executionContext = CreateTestExecutionContext();
        var createdByUserId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var serviceClient = CreateTestServiceClient(executionContext);
        var expected = new List<ServiceClient> { serviceClient };
        _postgreSqlRepositoryMock
            .Setup(x => x.GetByCreatorUserIdAsync(executionContext, createdByUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        LogAct("Chamando GetByCreatorUserIdAsync");
        var result = await _repository.GetByCreatorUserIdAsync(executionContext, createdByUserId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a lista retornada contem os clientes de servico esperados");
        result.ShouldNotBeEmpty();
        result.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetByCreatorUserIdAsync_WhenNoServiceClientsFound_ShouldReturnEmptyList()
    {
        // Arrange
        LogArrange("Preparando contexto e lista vazia para retorno");
        var executionContext = CreateTestExecutionContext();
        var createdByUserId = Id.CreateFromExistingInfo(Guid.NewGuid());
        _postgreSqlRepositoryMock
            .Setup(x => x.GetByCreatorUserIdAsync(executionContext, createdByUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        LogAct("Chamando GetByCreatorUserIdAsync");
        var result = await _repository.GetByCreatorUserIdAsync(executionContext, createdByUserId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a lista retornada esta vazia");
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetByCreatorUserIdAsync_WhenExceptionThrown_ShouldLogAndReturnEmptyList()
    {
        // Arrange
        LogArrange("Preparando contexto e configurando excecao no repositorio PostgreSql");
        var executionContext = CreateTestExecutionContext();
        var createdByUserId = Id.CreateFromExistingInfo(Guid.NewGuid());
        _postgreSqlRepositoryMock
            .Setup(x => x.GetByCreatorUserIdAsync(executionContext, createdByUserId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        LogAct("Chamando GetByCreatorUserIdAsync esperando excecao");
        var result = await _repository.GetByCreatorUserIdAsync(executionContext, createdByUserId, CancellationToken.None);

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
        LogArrange("Preparando contexto e cliente de servico para atualizar");
        var executionContext = CreateTestExecutionContext();
        var serviceClient = CreateTestServiceClient(executionContext);
        _postgreSqlRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, serviceClient, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        LogAct("Chamando UpdateAsync");
        var result = await _repository.UpdateAsync(executionContext, serviceClient, CancellationToken.None);

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
        var serviceClient = CreateTestServiceClient(executionContext);
        _postgreSqlRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, serviceClient, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        LogAct("Chamando UpdateAsync esperando excecao");
        var result = await _repository.UpdateAsync(executionContext, serviceClient, CancellationToken.None);

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
        LogArrange("Preparando contexto e id para buscar cliente de servico por id");
        var executionContext = CreateTestExecutionContext();
        var id = Id.CreateFromExistingInfo(Guid.NewGuid());
        var serviceClient = entityFound ? CreateTestServiceClient(executionContext) : null;
        _postgreSqlRepositoryMock
            .Setup(x => x.GetByIdAsync(executionContext, id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceClient);

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
        LogArrange("Preparando contexto e cliente de servico para registrar");
        var executionContext = CreateTestExecutionContext();
        var serviceClient = CreateTestServiceClient(executionContext);
        _postgreSqlRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, serviceClient, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        LogAct("Chamando RegisterNewAsync");
        var result = await _repository.RegisterNewAsync(executionContext, serviceClient, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o resultado retornado e o esperado");
        result.ShouldBe(expectedResult);
    }

    [Fact]
    public async Task EnumerateAllAsync_WhenCalled_ShouldReturnEmptyAsyncEnumerable()
    {
        // Arrange
        LogArrange("Preparando paginacao e handler para enumerar todos os clientes de servico");
        var paginationInfo = Bedrock.BuildingBlocks.Core.Paginations.PaginationInfo.All;
        var items = new List<ServiceClient>();
        EnumerateAllItemHandler<ServiceClient> handler = (item, ct) =>
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
        LogArrange("Preparando contexto e handler para enumerar clientes de servico modificados desde data");
        var executionContext = CreateTestExecutionContext();
        var since = DateTimeOffset.UtcNow.AddDays(-1);
        var items = new List<ServiceClient>();
        EnumerateModifiedSinceItemHandler<ServiceClient> handler = (item, ct) =>
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

    private static ServiceClient CreateTestServiceClient(ExecutionContext executionContext)
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
        return ServiceClient.CreateFromExistingInfo(
            new CreateFromExistingInfoServiceClientInput(
                entityInfo,
                "test-client-id",
                new byte[] { 1, 2, 3 },
                "Test Service Client",
                ServiceClientStatus.Active,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                null,
                null));
    }
}
