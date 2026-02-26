using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using ShopDemo.Auth.Domain.Entities.Sessions;
using ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.MultiTenancy;

[Collection("AuthPostgreSql")]
[Feature("Session Multi-Tenancy", "Isolamento multi-tenant em todas as queries")]
public class SessionMultiTenancyIntegrationTests : IntegrationTestBase
{
    private readonly AuthPostgreSqlFixture _fixture;

    public SessionMultiTenancyIntegrationTests(
        AuthPostgreSqlFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetByIdAsync_Should_NotReturnSessionFromOtherTenant()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando sessao no tenant A e buscando no tenant B");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var session = _fixture.CreateTestSessionDataModel(tenantCode: tenantA);
        await _fixture.InsertSessionDirectlyAsync(session);

        var executionContext = _fixture.CreateExecutionContext(tenantB);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateSessionDataModelRepository(unitOfWork);
        var repository = _fixture.CreateSessionPostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando GetByIdAsync com tenant diferente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByIdAsync(
            executionContext,
            Id.CreateFromExistingInfo(session.Id),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando isolamento cross-tenant");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByUserIdAsync_Should_NotReturnSessionsFromOtherTenant()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando sessao com userId no tenant A e buscando no tenant B");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var session = _fixture.CreateTestSessionDataModel(tenantCode: tenantA, userId: userId);
        await _fixture.InsertSessionDirectlyAsync(session);

        var executionContext = _fixture.CreateExecutionContext(tenantB);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateSessionDataModelRepository(unitOfWork);
        var repository = _fixture.CreateSessionPostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando GetByUserIdAsync com tenant diferente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByUserIdAsync(
            executionContext,
            Id.CreateFromExistingInfo(userId),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando isolamento cross-tenant");
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetActiveByUserIdAsync_Should_NotReturnSessionsFromOtherTenant()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando sessao ativa com userId no tenant A e buscando no tenant B");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var session = _fixture.CreateTestSessionDataModel(tenantCode: tenantA, userId: userId, status: 1);
        await _fixture.InsertSessionDirectlyAsync(session);

        var executionContext = _fixture.CreateExecutionContext(tenantB);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateSessionDataModelRepository(unitOfWork);
        var repository = _fixture.CreateSessionPostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando GetActiveByUserIdAsync com tenant diferente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetActiveByUserIdAsync(
            executionContext,
            Id.CreateFromExistingInfo(userId),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando isolamento cross-tenant");
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task CountActiveByUserIdAsync_Should_ReturnZeroForOtherTenant()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando sessao ativa no tenant A e contando no tenant B");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var session = _fixture.CreateTestSessionDataModel(tenantCode: tenantA, userId: userId, status: 1);
        await _fixture.InsertSessionDirectlyAsync(session);

        var executionContext = _fixture.CreateExecutionContext(tenantB);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateSessionDataModelRepository(unitOfWork);
        var repository = _fixture.CreateSessionPostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando CountActiveByUserIdAsync com tenant diferente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.CountActiveByUserIdAsync(
            executionContext,
            Id.CreateFromExistingInfo(userId),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que retorna zero para outro tenant");
        result.ShouldBe(0);
    }

    [Fact]
    public async Task EnumerateAllAsync_Should_ReturnOnlyCurrentTenantSessions()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo sessoes em dois tenants diferentes");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        for (int i = 0; i < 3; i++)
            await _fixture.InsertSessionDirectlyAsync(_fixture.CreateTestSessionDataModel(tenantCode: tenantA));

        for (int i = 0; i < 2; i++)
            await _fixture.InsertSessionDirectlyAsync(_fixture.CreateTestSessionDataModel(tenantCode: tenantB));

        var executionContext = _fixture.CreateExecutionContext(tenantA);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateSessionDataModelRepository(unitOfWork);
        var repository = _fixture.CreateSessionPostgreSqlRepository(dataModelRepo);

        var enumerated = new List<Session>();

        // Act
        LogAct("Chamando EnumerateAllAsync para tenant A");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await repository.EnumerateAllAsync(
            executionContext,
            PaginationInfo.All,
            (ctx, session, pagination, ct) =>
            {
                enumerated.Add(session);
                return Task.FromResult(true);
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que apenas sessoes do tenant A foram retornadas");
        enumerated.Count.ShouldBe(3);
    }

    [Fact]
    public async Task UpdateAsync_Should_NotModifySessionFromOtherTenant()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando sessao no tenant A e tentando atualizar com contexto do tenant B");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var dataModel = _fixture.CreateTestSessionDataModel(tenantCode: tenantA, entityVersion: 1);
        await _fixture.InsertSessionDirectlyAsync(dataModel);

        var executionContextB = _fixture.CreateExecutionContext(tenantB);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateSessionDataModelRepository(unitOfWork);
        var repository = _fixture.CreateSessionPostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Tentando buscar e atualizar sessao de outro tenant");
        await unitOfWork.OpenConnectionAsync(executionContextB, CancellationToken.None);
        var fetchedSession = await repository.GetByIdAsync(
            executionContextB,
            Id.CreateFromExistingInfo(dataModel.Id),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que a sessao nao foi encontrada no tenant B");
        fetchedSession.ShouldBeNull();

        var original = await _fixture.GetSessionDirectlyAsync(dataModel.Id, tenantA);
        original.ShouldNotBeNull();
        original.Status.ShouldBe(dataModel.Status);
    }

    [Fact]
    public async Task DeleteAsync_Should_NotDeleteSessionFromOtherTenant()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando sessao no tenant A e tentando excluir com contexto do tenant B");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var dataModel = _fixture.CreateTestSessionDataModel(tenantCode: tenantA, entityVersion: 1);
        await _fixture.InsertSessionDirectlyAsync(dataModel);

        var executionContextB = _fixture.CreateExecutionContext(tenantB);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateSessionDataModelRepository(unitOfWork);

        // Act
        LogAct("Tentando excluir sessao de outro tenant");
        await unitOfWork.OpenConnectionAsync(executionContextB, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContextB, CancellationToken.None);
        var result = await dataModelRepo.DeleteAsync(executionContextB, dataModel.Id, expectedVersion: 1, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContextB, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a exclusao falhou e a sessao original permanece");
        result.ShouldBeFalse();

        var stillExists = await _fixture.GetSessionDirectlyAsync(dataModel.Id, tenantA);
        stillExists.ShouldNotBeNull();
    }
}
