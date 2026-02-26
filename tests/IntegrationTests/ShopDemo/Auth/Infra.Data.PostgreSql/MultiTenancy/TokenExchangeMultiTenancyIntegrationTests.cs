using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using ShopDemo.Auth.Domain.Entities.TokenExchanges;
using ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.MultiTenancy;

[Collection("AuthPostgreSql")]
[Feature("TokenExchange Multi-Tenancy", "Isolamento multi-tenant em todas as queries")]
public class TokenExchangeMultiTenancyIntegrationTests : IntegrationTestBase
{
    private readonly AuthPostgreSqlFixture _fixture;

    public TokenExchangeMultiTenancyIntegrationTests(
        AuthPostgreSqlFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetByIdAsync_Should_NotReturnExchangeFromOtherTenant()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando token exchange no tenant A e buscando no tenant B");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var exchange = _fixture.CreateTestTokenExchangeDataModel(tenantCode: tenantA);
        await _fixture.InsertTokenExchangeDirectlyAsync(exchange);

        var executionContext = _fixture.CreateExecutionContext(tenantB);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateTokenExchangeDataModelRepository(unitOfWork);
        var repository = _fixture.CreateTokenExchangePostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando GetByIdAsync com tenant diferente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByIdAsync(
            executionContext,
            Id.CreateFromExistingInfo(exchange.Id),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando isolamento cross-tenant");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByUserIdAsync_Should_NotReturnExchangesFromOtherTenant()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando token exchange com userId no tenant A e buscando no tenant B");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var exchange = _fixture.CreateTestTokenExchangeDataModel(tenantCode: tenantA, userId: userId);
        await _fixture.InsertTokenExchangeDirectlyAsync(exchange);

        var executionContext = _fixture.CreateExecutionContext(tenantB);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateTokenExchangeDataModelRepository(unitOfWork);
        var repository = _fixture.CreateTokenExchangePostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando GetByUserIdAsync com tenant diferente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByUserIdAsync(
            executionContext,
            Id.CreateFromExistingInfo(userId),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que nenhum token exchange foi retornado para outro tenant");
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetByIssuedTokenJtiAsync_Should_NotReturnExchangeFromOtherTenant()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando token exchange com issuedTokenJti no tenant A e buscando no tenant B");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var issuedTokenJti = $"jti_{Guid.NewGuid():N}";
        var exchange = _fixture.CreateTestTokenExchangeDataModel(tenantCode: tenantA, issuedTokenJti: issuedTokenJti);
        await _fixture.InsertTokenExchangeDirectlyAsync(exchange);

        var executionContext = _fixture.CreateExecutionContext(tenantB);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateTokenExchangeDataModelRepository(unitOfWork);
        var repository = _fixture.CreateTokenExchangePostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando GetByIssuedTokenJtiAsync com tenant diferente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByIssuedTokenJtiAsync(
            executionContext,
            issuedTokenJti,
            CancellationToken.None);

        // Assert
        LogAssert("Verificando isolamento cross-tenant");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task EnumerateAllAsync_Should_ReturnOnlyCurrentTenantExchanges()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo token exchanges em dois tenants diferentes");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        for (int i = 0; i < 3; i++)
            await _fixture.InsertTokenExchangeDirectlyAsync(_fixture.CreateTestTokenExchangeDataModel(tenantCode: tenantA));

        for (int i = 0; i < 2; i++)
            await _fixture.InsertTokenExchangeDirectlyAsync(_fixture.CreateTestTokenExchangeDataModel(tenantCode: tenantB));

        var executionContext = _fixture.CreateExecutionContext(tenantA);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateTokenExchangeDataModelRepository(unitOfWork);
        var repository = _fixture.CreateTokenExchangePostgreSqlRepository(dataModelRepo);

        var enumerated = new List<TokenExchange>();

        // Act
        LogAct("Chamando EnumerateAllAsync para tenant A");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await repository.EnumerateAllAsync(
            executionContext,
            PaginationInfo.All,
            (ctx, exchange, pagination, ct) =>
            {
                enumerated.Add(exchange);
                return Task.FromResult(true);
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que apenas token exchanges do tenant A foram retornados");
        enumerated.Count.ShouldBe(3);
    }

    [Fact]
    public async Task DeleteAsync_Should_NotDeleteExchangeFromOtherTenant()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando token exchange no tenant A e tentando excluir com contexto do tenant B");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var dataModel = _fixture.CreateTestTokenExchangeDataModel(tenantCode: tenantA, entityVersion: 1);
        await _fixture.InsertTokenExchangeDirectlyAsync(dataModel);

        var executionContextB = _fixture.CreateExecutionContext(tenantB);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateTokenExchangeDataModelRepository(unitOfWork);

        // Act
        LogAct("Tentando excluir token exchange de outro tenant");
        await unitOfWork.OpenConnectionAsync(executionContextB, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContextB, CancellationToken.None);
        var result = await dataModelRepo.DeleteAsync(executionContextB, dataModel.Id, expectedVersion: 1, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContextB, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a exclusao falhou e o token exchange original permanece");
        result.ShouldBeFalse();

        var stillExists = await _fixture.GetTokenExchangeDirectlyAsync(dataModel.Id, tenantA);
        stillExists.ShouldNotBeNull();
    }
}
