using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using ShopDemo.Auth.Domain.Entities.Sessions;
using ShopDemo.Auth.Domain.Entities.Sessions.Enums;
using ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Repository;

[Collection("AuthPostgreSql")]
[Feature("SessionPostgreSqlRepository Custom Queries", "Queries customizadas de dominio")]
public class SessionPostgreSqlRepositoryCustomQueriesIntegrationTests : IntegrationTestBase
{
    private readonly AuthPostgreSqlFixture _fixture;

    public SessionPostgreSqlRepositoryCustomQueriesIntegrationTests(
        AuthPostgreSqlFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetByUserIdAsync_Should_ReturnEntities_WhenExist()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo 2 sessions para o mesmo userId");
        var tenantCode = Guid.NewGuid();
        var userId = Guid.NewGuid();

        for (int i = 0; i < 2; i++)
        {
            var dataModel = _fixture.CreateTestSessionDataModel(
                tenantCode: tenantCode,
                userId: userId);
            await _fixture.InsertSessionDirectlyAsync(dataModel);
        }

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateSessionDataModelRepository(unitOfWork);
        var repository = _fixture.CreateSessionPostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando GetByUserIdAsync no repositorio de dominio");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByUserIdAsync(
            executionContext,
            Id.CreateFromExistingInfo(userId),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que 2 entidades Session foram retornadas");
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result.ShouldAllBe(s => s.UserId.Value == userId);
    }

    [Fact]
    public async Task GetByUserIdAsync_Should_ReturnEmpty_WhenNotExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Configurando contexto para userId inexistente");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateSessionDataModelRepository(unitOfWork);
        var repository = _fixture.CreateSessionPostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando GetByUserIdAsync para userId inexistente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByUserIdAsync(
            executionContext,
            Id.CreateFromExistingInfo(Guid.NewGuid()),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que lista vazia e retornada");
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetActiveByUserIdAsync_Should_ReturnActiveEntities()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo 2 sessions Active para o mesmo userId");
        var tenantCode = Guid.NewGuid();
        var userId = Guid.NewGuid();

        for (int i = 0; i < 2; i++)
        {
            var dataModel = _fixture.CreateTestSessionDataModel(
                tenantCode: tenantCode,
                userId: userId,
                status: (short)SessionStatus.Active);
            await _fixture.InsertSessionDirectlyAsync(dataModel);
        }

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateSessionDataModelRepository(unitOfWork);
        var repository = _fixture.CreateSessionPostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando GetActiveByUserIdAsync no repositorio de dominio");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetActiveByUserIdAsync(
            executionContext,
            Id.CreateFromExistingInfo(userId),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que 2 entidades Active foram retornadas");
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result.ShouldAllBe(s => s.UserId.Value == userId);
        result.ShouldAllBe(s => s.Status == SessionStatus.Active);
    }

    [Fact]
    public async Task GetActiveByUserIdAsync_Should_NotReturnRevokedEntities()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo 1 Active e 1 Revoked com mesmo userId");
        var tenantCode = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var activeSession = _fixture.CreateTestSessionDataModel(
            tenantCode: tenantCode,
            userId: userId,
            status: (short)SessionStatus.Active);
        await _fixture.InsertSessionDirectlyAsync(activeSession);

        var revokedSession = _fixture.CreateTestSessionDataModel(
            tenantCode: tenantCode,
            userId: userId,
            status: (short)SessionStatus.Revoked,
            revokedAt: DateTimeOffset.UtcNow);
        await _fixture.InsertSessionDirectlyAsync(revokedSession);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateSessionDataModelRepository(unitOfWork);
        var repository = _fixture.CreateSessionPostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando GetActiveByUserIdAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetActiveByUserIdAsync(
            executionContext,
            Id.CreateFromExistingInfo(userId),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que apenas a session Active foi retornada, excluindo a Revoked");
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result[0].EntityInfo.Id.Value.ShouldBe(activeSession.Id);
        result[0].Status.ShouldBe(SessionStatus.Active);
    }

    [Fact]
    public async Task CountActiveByUserIdAsync_Should_ReturnCount()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo 3 sessions Active para o mesmo userId");
        var tenantCode = Guid.NewGuid();
        var userId = Guid.NewGuid();

        for (int i = 0; i < 3; i++)
        {
            var dataModel = _fixture.CreateTestSessionDataModel(
                tenantCode: tenantCode,
                userId: userId,
                status: (short)SessionStatus.Active);
            await _fixture.InsertSessionDirectlyAsync(dataModel);
        }

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateSessionDataModelRepository(unitOfWork);
        var repository = _fixture.CreateSessionPostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando CountActiveByUserIdAsync no repositorio de dominio");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.CountActiveByUserIdAsync(
            executionContext,
            Id.CreateFromExistingInfo(userId),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que o count retornado e 3");
        result.ShouldBe(3);
    }

    [Fact]
    public async Task CountActiveByUserIdAsync_Should_ReturnZero_WhenNone()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Configurando contexto para userId sem sessions ativas");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateSessionDataModelRepository(unitOfWork);
        var repository = _fixture.CreateSessionPostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando CountActiveByUserIdAsync para userId sem sessions");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.CountActiveByUserIdAsync(
            executionContext,
            Id.CreateFromExistingInfo(Guid.NewGuid()),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que o count retornado e 0");
        result.ShouldBe(0);
    }
}
