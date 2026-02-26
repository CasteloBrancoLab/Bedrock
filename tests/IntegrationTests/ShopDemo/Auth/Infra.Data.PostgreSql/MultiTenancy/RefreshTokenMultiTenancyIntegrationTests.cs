using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using ShopDemo.Auth.Domain.Entities.RefreshTokens;
using ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.MultiTenancy;

[Collection("AuthPostgreSql")]
[Feature("RefreshToken Multi-Tenancy", "Isolamento multi-tenant em todas as queries")]
public class RefreshTokenMultiTenancyIntegrationTests : IntegrationTestBase
{
    private readonly AuthPostgreSqlFixture _fixture;

    public RefreshTokenMultiTenancyIntegrationTests(
        AuthPostgreSqlFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetByIdAsync_Should_NotReturnTokenFromOtherTenant()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando refresh token no tenant A e buscando no tenant B");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var token = _fixture.CreateTestRefreshTokenDataModel(tenantCode: tenantA);
        await _fixture.InsertRefreshTokenDirectlyAsync(token);

        var executionContext = _fixture.CreateExecutionContext(tenantB);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);
        var repository = _fixture.CreateRefreshTokenPostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando GetByIdAsync com tenant diferente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByIdAsync(
            executionContext,
            Id.CreateFromExistingInfo(token.Id),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando isolamento cross-tenant");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByUserIdAsync_Should_NotReturnTokensFromOtherTenant()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando refresh token com userId no tenant A e buscando no tenant B");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var token = _fixture.CreateTestRefreshTokenDataModel(tenantCode: tenantA, userId: userId);
        await _fixture.InsertRefreshTokenDirectlyAsync(token);

        var executionContext = _fixture.CreateExecutionContext(tenantB);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);
        var repository = _fixture.CreateRefreshTokenPostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando GetByUserIdAsync com tenant diferente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByUserIdAsync(
            executionContext,
            Id.CreateFromExistingInfo(userId),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que nenhum token foi retornado para outro tenant");
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetByTokenHashAsync_Should_NotReturnTokenFromOtherTenant()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando refresh token com tokenHash no tenant A e buscando no tenant B");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var tokenHashBytes = new byte[64];
        Random.Shared.NextBytes(tokenHashBytes);
        var token = _fixture.CreateTestRefreshTokenDataModel(tenantCode: tenantA, tokenHash: tokenHashBytes);
        await _fixture.InsertRefreshTokenDirectlyAsync(token);

        var executionContext = _fixture.CreateExecutionContext(tenantB);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);
        var repository = _fixture.CreateRefreshTokenPostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando GetByTokenHashAsync com tenant diferente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByTokenHashAsync(
            executionContext,
            TokenHash.CreateNew(tokenHashBytes),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando isolamento cross-tenant");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetActiveByFamilyIdAsync_Should_NotReturnTokensFromOtherTenant()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando refresh token Active com familyId no tenant A e buscando no tenant B");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var token = _fixture.CreateTestRefreshTokenDataModel(
            tenantCode: tenantA,
            familyId: familyId,
            status: 1); // Active
        await _fixture.InsertRefreshTokenDirectlyAsync(token);

        var executionContext = _fixture.CreateExecutionContext(tenantB);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);
        var repository = _fixture.CreateRefreshTokenPostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando GetActiveByFamilyIdAsync com tenant diferente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetActiveByFamilyIdAsync(
            executionContext,
            TokenFamily.CreateFromExistingInfo(familyId),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que nenhum token foi retornado para outro tenant");
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task EnumerateAllAsync_Should_ReturnOnlyCurrentTenantTokens()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo refresh tokens em dois tenants diferentes");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        for (int i = 0; i < 3; i++)
            await _fixture.InsertRefreshTokenDirectlyAsync(_fixture.CreateTestRefreshTokenDataModel(tenantCode: tenantA));

        for (int i = 0; i < 2; i++)
            await _fixture.InsertRefreshTokenDirectlyAsync(_fixture.CreateTestRefreshTokenDataModel(tenantCode: tenantB));

        var executionContext = _fixture.CreateExecutionContext(tenantA);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);
        var repository = _fixture.CreateRefreshTokenPostgreSqlRepository(dataModelRepo);

        var enumerated = new List<RefreshToken>();

        // Act
        LogAct("Chamando EnumerateAllAsync para tenant A");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await repository.EnumerateAllAsync(
            executionContext,
            PaginationInfo.All,
            (ctx, token, pagination, ct) =>
            {
                enumerated.Add(token);
                return Task.FromResult(true);
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que apenas refresh tokens do tenant A foram retornados");
        enumerated.Count.ShouldBe(3);
    }

    [Fact]
    public async Task DeleteAsync_Should_NotDeleteTokenFromOtherTenant()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando refresh token no tenant A e tentando excluir com contexto do tenant B");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var dataModel = _fixture.CreateTestRefreshTokenDataModel(tenantCode: tenantA, entityVersion: 1);
        await _fixture.InsertRefreshTokenDirectlyAsync(dataModel);

        var executionContextB = _fixture.CreateExecutionContext(tenantB);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);

        // Act
        LogAct("Tentando excluir refresh token de outro tenant");
        await unitOfWork.OpenConnectionAsync(executionContextB, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContextB, CancellationToken.None);
        var result = await dataModelRepo.DeleteAsync(executionContextB, dataModel.Id, expectedVersion: 1, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContextB, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a exclusao falhou e o refresh token original permanece");
        result.ShouldBeFalse();

        var stillExists = await _fixture.GetRefreshTokenDirectlyAsync(dataModel.Id, tenantA);
        stillExists.ShouldNotBeNull();
    }
}
