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
using ShopDemo.Auth.Domain.Entities.Tenants;
using ShopDemo.Auth.Domain.Entities.Tenants.Enums;
using ShopDemo.Auth.Domain.Entities.Tenants.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Infra.Data.Repositories;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.Repositories;

public class TenantRepositoryTests : TestBase
{
    private readonly Mock<ILogger<TenantRepository>> _loggerMock;
    private readonly Mock<ITenantPostgreSqlRepository> _postgreSqlRepositoryMock;
    private readonly TenantRepository _repository;

    public TenantRepositoryTests(ITestOutputHelper output) : base(output)
    {
        _loggerMock = new Mock<ILogger<TenantRepository>>();
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _postgreSqlRepositoryMock = new Mock<ITenantPostgreSqlRepository>();
        _repository = new TenantRepository(_loggerMock.Object, _postgreSqlRepositoryMock.Object);
    }

    // Constructor Tests

    [Fact]
    public void Constructor_WhenPostgreSqlRepositoryIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Preparando logger valido e repositorio PostgreSql nulo");

        // Act
        LogAct("Instanciando TenantRepository com postgreSqlRepository nulo");
        Action act = () => new TenantRepository(_loggerMock.Object, null!);

        // Assert
        LogAssert("Verificando que ArgumentNullException foi lancada");
        act.ShouldThrow<ArgumentNullException>();
    }

    // GetByDomainAsync Tests

    [Fact]
    public async Task GetByDomainAsync_WhenTenantFound_ShouldReturnTenant()
    {
        // Arrange
        LogArrange("Preparando contexto e tenant para retorno");
        var executionContext = CreateTestExecutionContext();
        var domain = "test.example.com";
        var tenant = CreateTestTenant(executionContext);
        _postgreSqlRepositoryMock
            .Setup(x => x.GetByDomainAsync(executionContext, domain, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        // Act
        LogAct("Chamando GetByDomainAsync");
        var result = await _repository.GetByDomainAsync(executionContext, domain, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o tenant retornado nao e nulo");
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetByDomainAsync_WhenTenantNotFound_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Preparando contexto e retorno nulo para tenant nao encontrado");
        var executionContext = CreateTestExecutionContext();
        var domain = "nonexistent.example.com";
        _postgreSqlRepositoryMock
            .Setup(x => x.GetByDomainAsync(executionContext, domain, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        // Act
        LogAct("Chamando GetByDomainAsync");
        var result = await _repository.GetByDomainAsync(executionContext, domain, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o resultado retornado e nulo");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByDomainAsync_WhenExceptionThrown_ShouldLogAndReturnNull()
    {
        // Arrange
        LogArrange("Preparando contexto e configurando excecao no repositorio PostgreSql");
        var executionContext = CreateTestExecutionContext();
        var domain = "test.example.com";
        _postgreSqlRepositoryMock
            .Setup(x => x.GetByDomainAsync(executionContext, domain, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        LogAct("Chamando GetByDomainAsync esperando excecao");
        var result = await _repository.GetByDomainAsync(executionContext, domain, CancellationToken.None);

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

    // GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WhenTenantsFound_ShouldReturnList()
    {
        // Arrange
        LogArrange("Preparando contexto e lista de tenants para retorno");
        var executionContext = CreateTestExecutionContext();
        var tenant = CreateTestTenant(executionContext);
        var expected = new List<Tenant> { tenant };
        _postgreSqlRepositoryMock
            .Setup(x => x.GetAllAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        LogAct("Chamando GetAllAsync");
        var result = await _repository.GetAllAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a lista retornada contem os tenants esperados");
        result.ShouldNotBeEmpty();
        result.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoTenantsFound_ShouldReturnEmptyList()
    {
        // Arrange
        LogArrange("Preparando contexto e lista vazia para retorno");
        var executionContext = CreateTestExecutionContext();
        _postgreSqlRepositoryMock
            .Setup(x => x.GetAllAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        LogAct("Chamando GetAllAsync");
        var result = await _repository.GetAllAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a lista retornada esta vazia");
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WhenExceptionThrown_ShouldLogAndReturnEmptyList()
    {
        // Arrange
        LogArrange("Preparando contexto e configurando excecao no repositorio PostgreSql");
        var executionContext = CreateTestExecutionContext();
        _postgreSqlRepositoryMock
            .Setup(x => x.GetAllAsync(executionContext, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        LogAct("Chamando GetAllAsync esperando excecao");
        var result = await _repository.GetAllAsync(executionContext, CancellationToken.None);

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
        LogArrange("Preparando contexto e tenant para atualizar");
        var executionContext = CreateTestExecutionContext();
        var tenant = CreateTestTenant(executionContext);
        _postgreSqlRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, tenant, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        LogAct("Chamando UpdateAsync");
        var result = await _repository.UpdateAsync(executionContext, tenant, CancellationToken.None);

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
        var tenant = CreateTestTenant(executionContext);
        _postgreSqlRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, tenant, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        LogAct("Chamando UpdateAsync esperando excecao");
        var result = await _repository.UpdateAsync(executionContext, tenant, CancellationToken.None);

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
        LogArrange("Preparando contexto e id para buscar tenant por id");
        var executionContext = CreateTestExecutionContext();
        var id = Id.CreateFromExistingInfo(Guid.NewGuid());
        var tenant = entityFound ? CreateTestTenant(executionContext) : null;
        _postgreSqlRepositoryMock
            .Setup(x => x.GetByIdAsync(executionContext, id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

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
        LogArrange("Preparando contexto e tenant para registrar");
        var executionContext = CreateTestExecutionContext();
        var tenant = CreateTestTenant(executionContext);
        _postgreSqlRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, tenant, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        LogAct("Chamando RegisterNewAsync");
        var result = await _repository.RegisterNewAsync(executionContext, tenant, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o resultado retornado e o esperado");
        result.ShouldBe(expectedResult);
    }

    [Fact]
    public async Task EnumerateAllAsync_WhenCalled_ShouldReturnEmptyAsyncEnumerable()
    {
        // Arrange
        LogArrange("Preparando paginacao e handler para enumerar todos os tenants");
        var paginationInfo = Bedrock.BuildingBlocks.Core.Paginations.PaginationInfo.All;
        var items = new List<Tenant>();
        EnumerateAllItemHandler<Tenant> handler = (_, item, _, _) =>
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
        LogArrange("Preparando contexto e handler para enumerar tenants modificados desde data");
        var executionContext = CreateTestExecutionContext();
        var since = DateTimeOffset.UtcNow.AddDays(-1);
        var items = new List<Tenant>();
        EnumerateModifiedSinceItemHandler<Tenant> handler = (_, item, _, _, _) =>
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

    private static Tenant CreateTestTenant(ExecutionContext executionContext)
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
        return Tenant.CreateFromExistingInfo(
            new CreateFromExistingInfoTenantInput(
                entityInfo,
                "Test Tenant",
                "test.example.com",
                "test_schema",
                TenantStatus.Active,
                TenantTier.Professional,
                null));
    }
}
