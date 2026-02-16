using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using ShopDemo.Auth.Domain.Entities.Users;
using ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Repository;

[Collection("AuthPostgreSql")]
[Feature("UserPostgreSqlRepository Enumeration", "EnumerateAllAsync e EnumerateModifiedSinceAsync")]
public class UserPostgreSqlRepositoryEnumerationIntegrationTests : IntegrationTestBase
{
    private readonly AuthPostgreSqlFixture _fixture;

    public UserPostgreSqlRepositoryEnumerationIntegrationTests(
        AuthPostgreSqlFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task EnumerateAllAsync_Should_EnumerateAllUsers()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo 3 usuarios no mesmo tenant");
        var tenantCode = Guid.NewGuid();
        for (int i = 0; i < 3; i++)
        {
            var user = _fixture.CreateTestUserDataModel(tenantCode: tenantCode);
            await _fixture.InsertUserDirectlyAsync(user);
        }

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateDataModelRepository(unitOfWork);
        var repository = _fixture.CreatePostgreSqlRepository(dataModelRepo);

        var enumerated = new List<User>();

        // Act
        LogAct("Chamando EnumerateAllAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.EnumerateAllAsync(
            executionContext,
            PaginationInfo.All,
            (ctx, user, pagination, ct) =>
            {
                enumerated.Add(user);
                return Task.FromResult(true);
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que 3 usuarios foram enumerados");
        result.ShouldBeTrue();
        enumerated.Count.ShouldBe(3);
    }

    [Fact]
    public async Task EnumerateAllAsync_Should_StopOnFalse()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo 5 usuarios no mesmo tenant");
        var tenantCode = Guid.NewGuid();
        for (int i = 0; i < 5; i++)
        {
            var user = _fixture.CreateTestUserDataModel(tenantCode: tenantCode);
            await _fixture.InsertUserDirectlyAsync(user);
        }

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateDataModelRepository(unitOfWork);
        var repository = _fixture.CreatePostgreSqlRepository(dataModelRepo);

        var count = 0;

        // Act
        LogAct("Chamando EnumerateAllAsync com handler que para apos 2 itens");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.EnumerateAllAsync(
            executionContext,
            PaginationInfo.All,
            (ctx, user, pagination, ct) =>
            {
                count++;
                return Task.FromResult(count < 2);
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que a enumeracao parou apos 2 itens");
        result.ShouldBeTrue();
        count.ShouldBe(2);
    }

    [Fact]
    public async Task EnumerateAllAsync_Should_ConvertToUserEntities()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo usuario com dados conhecidos");
        var tenantCode = Guid.NewGuid();
        var dataModel = _fixture.CreateTestUserDataModel(
            tenantCode: tenantCode,
            username: "enumerate_user",
            email: "enumerate@example.com");
        await _fixture.InsertUserDirectlyAsync(dataModel);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateDataModelRepository(unitOfWork);
        var repository = _fixture.CreatePostgreSqlRepository(dataModelRepo);

        User? capturedUser = null;

        // Act
        LogAct("Chamando EnumerateAllAsync e capturando entidade");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await repository.EnumerateAllAsync(
            executionContext,
            PaginationInfo.All,
            (ctx, user, pagination, ct) =>
            {
                capturedUser = user;
                return Task.FromResult(true);
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que a entidade User foi convertida corretamente");
        capturedUser.ShouldNotBeNull();
        capturedUser.Username.ShouldBe("enumerate_user");
        capturedUser.Email.Value.ShouldBe("enumerate@example.com");
    }

    [Fact]
    public async Task EnumerateModifiedSinceAsync_Should_FilterByTimestamp()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo usuario com last_changed_at definido");
        var tenantCode = Guid.NewGuid();
        var sinceTime = DateTimeOffset.UtcNow.AddMinutes(-5);

        var user = _fixture.CreateTestUserDataModel(tenantCode: tenantCode);
        user.LastChangedBy = "modifier";
        user.LastChangedAt = DateTimeOffset.UtcNow;
        user.LastChangedExecutionOrigin = "TestOrigin";
        user.LastChangedCorrelationId = Guid.NewGuid();
        user.LastChangedBusinessOperationCode = "TEST_OP";
        await _fixture.InsertUserDirectlyAsync(user);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateDataModelRepository(unitOfWork);
        var repository = _fixture.CreatePostgreSqlRepository(dataModelRepo);

        var enumerated = new List<User>();

        // Act
        LogAct("Chamando EnumerateModifiedSinceAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.EnumerateModifiedSinceAsync(
            executionContext,
            TimeProvider.System,
            sinceTime,
            (ctx, entity, timeProvider, since, ct) =>
            {
                enumerated.Add(entity);
                return Task.FromResult(true);
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que o usuario modificado foi enumerado");
        result.ShouldBeTrue();
        enumerated.Count.ShouldBeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task EnumerateModifiedSinceAsync_Should_NotReturnUnmodifiedUsers()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo usuario sem last_changed_at e buscando modificados");
        var tenantCode = Guid.NewGuid();
        var user = _fixture.CreateTestUserDataModel(tenantCode: tenantCode);
        await _fixture.InsertUserDirectlyAsync(user);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateDataModelRepository(unitOfWork);
        var repository = _fixture.CreatePostgreSqlRepository(dataModelRepo);

        var enumerated = new List<User>();

        // Act
        LogAct("Chamando EnumerateModifiedSinceAsync com timestamp futuro");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.EnumerateModifiedSinceAsync(
            executionContext,
            TimeProvider.System,
            DateTimeOffset.UtcNow.AddMinutes(10),
            (ctx, entity, timeProvider, since, ct) =>
            {
                enumerated.Add(entity);
                return Task.FromResult(true);
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que nenhum usuario foi enumerado");
        result.ShouldBeTrue();
        enumerated.ShouldBeEmpty();
    }

    [Fact]
    public async Task EnumerateModifiedSinceAsync_Should_ConvertToUserEntities()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo usuario modificado com dados conhecidos");
        var tenantCode = Guid.NewGuid();
        var dataModel = _fixture.CreateTestUserDataModel(
            tenantCode: tenantCode,
            username: "modified_user",
            email: "modified@example.com");
        dataModel.LastChangedBy = "modifier";
        dataModel.LastChangedAt = DateTimeOffset.UtcNow;
        dataModel.LastChangedExecutionOrigin = "TestOrigin";
        dataModel.LastChangedCorrelationId = Guid.NewGuid();
        dataModel.LastChangedBusinessOperationCode = "TEST_OP";
        await _fixture.InsertUserDirectlyAsync(dataModel);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateDataModelRepository(unitOfWork);
        var repository = _fixture.CreatePostgreSqlRepository(dataModelRepo);

        User? capturedUser = null;

        // Act
        LogAct("Chamando EnumerateModifiedSinceAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await repository.EnumerateModifiedSinceAsync(
            executionContext,
            TimeProvider.System,
            DateTimeOffset.UtcNow.AddMinutes(-5),
            (ctx, entity, timeProvider, since, ct) =>
            {
                capturedUser = entity;
                return Task.FromResult(true);
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que a entidade User foi convertida corretamente");
        capturedUser.ShouldNotBeNull();
        capturedUser.Username.ShouldBe("modified_user");
        capturedUser.Email.Value.ShouldBe("modified@example.com");
    }
}
