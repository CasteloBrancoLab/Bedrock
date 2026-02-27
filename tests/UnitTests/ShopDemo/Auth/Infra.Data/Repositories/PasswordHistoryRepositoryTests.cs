using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using Bedrock.BuildingBlocks.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using ShopDemo.Auth.Domain.Entities.PasswordHistories;
using ShopDemo.Auth.Domain.Entities.PasswordHistories.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Infra.Data.Repositories;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.Repositories;

public class PasswordHistoryRepositoryTests : TestBase
{
    private readonly Mock<ILogger<PasswordHistoryRepository>> _loggerMock;
    private readonly Mock<IPasswordHistoryPostgreSqlRepository> _postgreSqlRepositoryMock;
    private readonly PasswordHistoryRepository _repository;

    public PasswordHistoryRepositoryTests(ITestOutputHelper output) : base(output)
    {
        _loggerMock = new Mock<ILogger<PasswordHistoryRepository>>();
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _postgreSqlRepositoryMock = new Mock<IPasswordHistoryPostgreSqlRepository>();
        _repository = new PasswordHistoryRepository(_loggerMock.Object, _postgreSqlRepositoryMock.Object);
    }

    // Constructor Tests

    [Fact]
    public void Constructor_WhenPostgreSqlRepositoryIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Preparando logger valido e repositorio PostgreSql nulo");

        // Act
        LogAct("Instanciando PasswordHistoryRepository com postgreSqlRepository nulo");
        Action act = () => new PasswordHistoryRepository(_loggerMock.Object, null!);

        // Assert
        LogAssert("Verificando que ArgumentNullException foi lancada");
        act.ShouldThrow<ArgumentNullException>();
    }

    // GetLatestByUserIdAsync Tests

    [Fact]
    public async Task GetLatestByUserIdAsync_WhenHistoriesFound_ShouldReturnList()
    {
        // Arrange
        LogArrange("Preparando contexto e lista de password histories para retorno");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var count = 5;
        var passwordHistory = CreateTestPasswordHistory(executionContext);
        var expected = new List<PasswordHistory> { passwordHistory };
        _postgreSqlRepositoryMock
            .Setup(x => x.GetLatestByUserIdAsync(executionContext, userId, count, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        LogAct("Chamando GetLatestByUserIdAsync");
        var result = await _repository.GetLatestByUserIdAsync(executionContext, userId, count, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a lista retornada contem os password histories esperados");
        result.ShouldNotBeEmpty();
        result.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetLatestByUserIdAsync_WhenNoHistoriesFound_ShouldReturnEmptyList()
    {
        // Arrange
        LogArrange("Preparando contexto e lista vazia para retorno");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var count = 5;
        _postgreSqlRepositoryMock
            .Setup(x => x.GetLatestByUserIdAsync(executionContext, userId, count, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        LogAct("Chamando GetLatestByUserIdAsync");
        var result = await _repository.GetLatestByUserIdAsync(executionContext, userId, count, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a lista retornada esta vazia");
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetLatestByUserIdAsync_WhenExceptionThrown_ShouldLogAndReturnEmptyList()
    {
        // Arrange
        LogArrange("Preparando contexto e configurando excecao no repositorio PostgreSql");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var count = 5;
        _postgreSqlRepositoryMock
            .Setup(x => x.GetLatestByUserIdAsync(executionContext, userId, count, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        LogAct("Chamando GetLatestByUserIdAsync esperando excecao");
        var result = await _repository.GetLatestByUserIdAsync(executionContext, userId, count, CancellationToken.None);

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
        LogArrange("Preparando contexto e id para buscar PasswordHistory por id");
        var executionContext = CreateTestExecutionContext();
        var id = Id.CreateFromExistingInfo(Guid.NewGuid());
        var passwordHistory = entityFound ? CreateTestPasswordHistory(executionContext) : null;
        _postgreSqlRepositoryMock
            .Setup(x => x.GetByIdAsync(executionContext, id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(passwordHistory);

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
        LogArrange("Preparando contexto e PasswordHistory para registrar");
        var executionContext = CreateTestExecutionContext();
        var passwordHistory = CreateTestPasswordHistory(executionContext);
        _postgreSqlRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, passwordHistory, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        LogAct("Chamando RegisterNewAsync");
        var result = await _repository.RegisterNewAsync(executionContext, passwordHistory, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o resultado retornado e o esperado");
        result.ShouldBe(expectedResult);
    }

    [Fact]
    public async Task EnumerateAllAsync_WhenCalled_ShouldReturnEmptyAsyncEnumerable()
    {
        // Arrange
        LogArrange("Preparando paginacao e handler para enumerar todos os PasswordHistories");
        var paginationInfo = PaginationInfo.All;
        var items = new List<PasswordHistory>();
        EnumerateAllItemHandler<PasswordHistory> handler = (_, item, _, _) =>
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
        LogArrange("Preparando contexto e handler para enumerar PasswordHistories modificados desde data");
        var executionContext = CreateTestExecutionContext();
        var since = DateTimeOffset.UtcNow.AddDays(-1);
        var items = new List<PasswordHistory>();
        EnumerateModifiedSinceItemHandler<PasswordHistory> handler = (_, item, _, _, _) =>
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

    private static PasswordHistory CreateTestPasswordHistory(ExecutionContext executionContext)
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
        return PasswordHistory.CreateFromExistingInfo(
            new CreateFromExistingInfoPasswordHistoryInput(
                entityInfo,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                "hashed_password_value",
                DateTimeOffset.UtcNow));
    }
}
