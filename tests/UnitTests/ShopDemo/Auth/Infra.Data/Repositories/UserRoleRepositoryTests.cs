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
using ShopDemo.Auth.Domain.Entities.UserRoles;
using ShopDemo.Auth.Domain.Entities.UserRoles.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Infra.Data.Repositories;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.Repositories;

public class UserRoleRepositoryTests : TestBase
{
    private readonly Mock<ILogger<UserRoleRepository>> _loggerMock;
    private readonly Mock<IUserRolePostgreSqlRepository> _postgreSqlRepositoryMock;
    private readonly UserRoleRepository _repository;

    public UserRoleRepositoryTests(ITestOutputHelper output) : base(output)
    {
        _loggerMock = new Mock<ILogger<UserRoleRepository>>();
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _postgreSqlRepositoryMock = new Mock<IUserRolePostgreSqlRepository>();
        _repository = new UserRoleRepository(_loggerMock.Object, _postgreSqlRepositoryMock.Object);
    }

    // Constructor Tests

    [Fact]
    public void Constructor_WhenPostgreSqlRepositoryIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Preparando logger valido e repositorio PostgreSql nulo");

        // Act
        LogAct("Instanciando UserRoleRepository com postgreSqlRepository nulo");
        Action act = () => new UserRoleRepository(_loggerMock.Object, null!);

        // Assert
        LogAssert("Verificando que ArgumentNullException foi lancada");
        act.ShouldThrow<ArgumentNullException>();
    }

    // GetByUserIdAsync Tests

    [Fact]
    public async Task GetByUserIdAsync_WhenRolesFound_ShouldReturnList()
    {
        // Arrange
        LogArrange("Preparando contexto e lista de user roles para retorno");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var userRole = CreateTestUserRole(executionContext);
        var expected = new List<UserRole> { userRole };
        _postgreSqlRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        LogAct("Chamando GetByUserIdAsync");
        var result = await _repository.GetByUserIdAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a lista retornada contem os user roles esperados");
        result.ShouldNotBeEmpty();
        result.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetByUserIdAsync_WhenNoRolesFound_ShouldReturnEmptyList()
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

    // GetByRoleIdAsync Tests

    [Fact]
    public async Task GetByRoleIdAsync_WhenRolesFound_ShouldReturnList()
    {
        // Arrange
        LogArrange("Preparando contexto e lista de user roles para retorno por role id");
        var executionContext = CreateTestExecutionContext();
        var roleId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var userRole = CreateTestUserRole(executionContext);
        var expected = new List<UserRole> { userRole };
        _postgreSqlRepositoryMock
            .Setup(x => x.GetByRoleIdAsync(executionContext, roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        LogAct("Chamando GetByRoleIdAsync");
        var result = await _repository.GetByRoleIdAsync(executionContext, roleId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a lista retornada contem os user roles esperados");
        result.ShouldNotBeEmpty();
        result.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetByRoleIdAsync_WhenNoRolesFound_ShouldReturnEmptyList()
    {
        // Arrange
        LogArrange("Preparando contexto e lista vazia para retorno");
        var executionContext = CreateTestExecutionContext();
        var roleId = Id.CreateFromExistingInfo(Guid.NewGuid());
        _postgreSqlRepositoryMock
            .Setup(x => x.GetByRoleIdAsync(executionContext, roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        LogAct("Chamando GetByRoleIdAsync");
        var result = await _repository.GetByRoleIdAsync(executionContext, roleId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a lista retornada esta vazia");
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetByRoleIdAsync_WhenExceptionThrown_ShouldLogAndReturnEmptyList()
    {
        // Arrange
        LogArrange("Preparando contexto e configurando excecao no repositorio PostgreSql");
        var executionContext = CreateTestExecutionContext();
        var roleId = Id.CreateFromExistingInfo(Guid.NewGuid());
        _postgreSqlRepositoryMock
            .Setup(x => x.GetByRoleIdAsync(executionContext, roleId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        LogAct("Chamando GetByRoleIdAsync esperando excecao");
        var result = await _repository.GetByRoleIdAsync(executionContext, roleId, CancellationToken.None);

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

    // DeleteAsync Tests

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DeleteAsync_WhenCalled_ShouldReturnExpectedResult(bool expectedResult)
    {
        // Arrange
        LogArrange("Preparando contexto e UserRole para deletar");
        var executionContext = CreateTestExecutionContext();
        var userRole = CreateTestUserRole(executionContext);
        _postgreSqlRepositoryMock
            .Setup(x => x.DeleteAsync(executionContext, userRole, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        LogAct("Chamando DeleteAsync");
        var result = await _repository.DeleteAsync(executionContext, userRole, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o resultado retornado e o esperado");
        result.ShouldBe(expectedResult);
    }

    [Fact]
    public async Task DeleteAsync_WhenExceptionThrown_ShouldLogAndReturnFalse()
    {
        // Arrange
        LogArrange("Preparando contexto e configurando excecao no repositorio PostgreSql");
        var executionContext = CreateTestExecutionContext();
        var userRole = CreateTestUserRole(executionContext);
        _postgreSqlRepositoryMock
            .Setup(x => x.DeleteAsync(executionContext, userRole, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        LogAct("Chamando DeleteAsync esperando excecao");
        var result = await _repository.DeleteAsync(executionContext, userRole, CancellationToken.None);

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
        LogArrange("Preparando contexto e id para buscar UserRole por id");
        var executionContext = CreateTestExecutionContext();
        var id = Id.CreateFromExistingInfo(Guid.NewGuid());
        var userRole = entityFound ? CreateTestUserRole(executionContext) : null;
        _postgreSqlRepositoryMock
            .Setup(x => x.GetByIdAsync(executionContext, id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userRole);

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
        LogArrange("Preparando contexto e UserRole para registrar");
        var executionContext = CreateTestExecutionContext();
        var userRole = CreateTestUserRole(executionContext);
        _postgreSqlRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, userRole, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        LogAct("Chamando RegisterNewAsync");
        var result = await _repository.RegisterNewAsync(executionContext, userRole, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o resultado retornado e o esperado");
        result.ShouldBe(expectedResult);
    }

    [Fact]
    public async Task EnumerateAllAsync_WhenCalled_ShouldReturnEmptyAsyncEnumerable()
    {
        // Arrange
        LogArrange("Preparando paginacao e handler para enumerar todos os UserRoles");
        var paginationInfo = PaginationInfo.All;
        var items = new List<UserRole>();
        EnumerateAllItemHandler<UserRole> handler = (_, item, _, _) =>
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
        LogArrange("Preparando contexto e handler para enumerar UserRoles modificados desde data");
        var executionContext = CreateTestExecutionContext();
        var since = DateTimeOffset.UtcNow.AddDays(-1);
        var items = new List<UserRole>();
        EnumerateModifiedSinceItemHandler<UserRole> handler = (_, item, _, _, _) =>
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

    private static UserRole CreateTestUserRole(ExecutionContext executionContext)
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
        return UserRole.CreateFromExistingInfo(
            new CreateFromExistingInfoUserRoleInput(
                entityInfo,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                Id.CreateFromExistingInfo(Guid.NewGuid())));
    }
}
