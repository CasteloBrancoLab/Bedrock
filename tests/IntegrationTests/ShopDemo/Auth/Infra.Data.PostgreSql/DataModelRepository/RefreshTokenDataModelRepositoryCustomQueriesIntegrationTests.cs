using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.DataModelRepository;

[Collection("AuthPostgreSql")]
[Feature("RefreshTokenDataModel Custom Queries", "Queries customizadas do repositorio de RefreshTokenDataModel")]
public class RefreshTokenDataModelRepositoryCustomQueriesIntegrationTests : IntegrationTestBase
{
    private readonly AuthPostgreSqlFixture _fixture;

    public RefreshTokenDataModelRepositoryCustomQueriesIntegrationTests(
        AuthPostgreSqlFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetByUserIdAsync_Should_ReturnTokens_WhenExist()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando 3 refresh tokens para o mesmo userId");
        var tenantCode = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var token1 = _fixture.CreateTestRefreshTokenDataModel(tenantCode: tenantCode, userId: userId);
        var token2 = _fixture.CreateTestRefreshTokenDataModel(tenantCode: tenantCode, userId: userId);
        var token3 = _fixture.CreateTestRefreshTokenDataModel(tenantCode: tenantCode, userId: userId);

        await _fixture.InsertRefreshTokenDirectlyAsync(token1);
        await _fixture.InsertRefreshTokenDirectlyAsync(token2);
        await _fixture.InsertRefreshTokenDirectlyAsync(token3);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando GetByUserIdAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByUserIdAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que 3 tokens foram retornados");
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
        result.ShouldAllBe(t => t.UserId == userId);
        executionContext.HasExceptions.ShouldBeFalse();
    }

    [Fact]
    public async Task GetByUserIdAsync_Should_ReturnEmpty_WhenNoTokens()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Configurando contexto para userId sem tokens");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando GetByUserIdAsync para userId sem tokens");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByUserIdAsync(executionContext, Guid.NewGuid(), CancellationToken.None);

        // Assert
        LogAssert("Verificando que lista vazia e retornada");
        result.ShouldNotBeNull();
        result.Count.ShouldBe(0);
        executionContext.HasExceptions.ShouldBeFalse();
    }

    [Fact]
    public async Task GetByUserIdAsync_Should_FilterByTenant()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando token em tenant A e buscando em tenant B");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var token = _fixture.CreateTestRefreshTokenDataModel(tenantCode: tenantA, userId: userId);
        await _fixture.InsertRefreshTokenDirectlyAsync(token);

        var executionContext = _fixture.CreateExecutionContext(tenantB);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando GetByUserIdAsync com tenant diferente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByUserIdAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que nao encontra tokens de outro tenant");
        result.ShouldNotBeNull();
        result.Count.ShouldBe(0);
        executionContext.HasExceptions.ShouldBeFalse();
    }

    [Fact]
    public async Task GetByTokenHashAsync_Should_ReturnToken_WhenExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando refresh token com tokenHash especifico");
        var tenantCode = Guid.NewGuid();
        var tokenHash = new byte[64];
        Random.Shared.NextBytes(tokenHash);

        var token = _fixture.CreateTestRefreshTokenDataModel(tenantCode: tenantCode, tokenHash: tokenHash);
        await _fixture.InsertRefreshTokenDirectlyAsync(token);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando GetByTokenHashAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByTokenHashAsync(executionContext, tokenHash, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o token foi encontrado pelo hash");
        result.ShouldNotBeNull();
        result.Id.ShouldBe(token.Id);
        result.TokenHash.ShouldBe(tokenHash);
        executionContext.HasExceptions.ShouldBeFalse();
    }

    [Fact]
    public async Task GetByTokenHashAsync_Should_ReturnNull_WhenNotExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Configurando contexto para tokenHash inexistente");
        var tenantCode = Guid.NewGuid();
        var nonExistentHash = new byte[64];
        Random.Shared.NextBytes(nonExistentHash);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando GetByTokenHashAsync para hash inexistente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByTokenHashAsync(executionContext, nonExistentHash, CancellationToken.None);

        // Assert
        LogAssert("Verificando que null e retornado");
        result.ShouldBeNull();
        executionContext.HasExceptions.ShouldBeFalse();
    }

    [Fact]
    public async Task GetByTokenHashAsync_Should_FilterByTenant()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando token em tenant A e buscando em tenant B");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var tokenHash = new byte[64];
        Random.Shared.NextBytes(tokenHash);

        var token = _fixture.CreateTestRefreshTokenDataModel(tenantCode: tenantA, tokenHash: tokenHash);
        await _fixture.InsertRefreshTokenDirectlyAsync(token);

        var executionContext = _fixture.CreateExecutionContext(tenantB);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando GetByTokenHashAsync com tenant diferente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByTokenHashAsync(executionContext, tokenHash, CancellationToken.None);

        // Assert
        LogAssert("Verificando que nao encontra token de outro tenant");
        result.ShouldBeNull();
        executionContext.HasExceptions.ShouldBeFalse();
    }

    [Fact]
    public async Task GetByTokenHashAsync_Should_PopulateAllFields()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando refresh token com todos os campos preenchidos");
        var tenantCode = Guid.NewGuid();
        var tokenHash = new byte[64];
        Random.Shared.NextBytes(tokenHash);

        var token = _fixture.CreateTestRefreshTokenDataModel(tenantCode: tenantCode, tokenHash: tokenHash);
        token.LastChangedBy = "modifier_user";
        token.LastChangedAt = DateTimeOffset.UtcNow;
        token.LastChangedExecutionOrigin = "TestOrigin";
        token.LastChangedCorrelationId = Guid.NewGuid();
        token.LastChangedBusinessOperationCode = "UPDATE_OP";
        await _fixture.InsertRefreshTokenDirectlyAsync(token);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando GetByTokenHashAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByTokenHashAsync(executionContext, tokenHash, CancellationToken.None);

        // Assert
        LogAssert("Verificando que todos os campos foram populados");
        result.ShouldNotBeNull();
        result.Id.ShouldBe(token.Id);
        result.TenantCode.ShouldBe(token.TenantCode);
        result.UserId.ShouldBe(token.UserId);
        result.TokenHash.ShouldBe(token.TokenHash);
        result.FamilyId.ShouldBe(token.FamilyId);
        result.Status.ShouldBe(token.Status);
        result.CreatedBy.ShouldBe(token.CreatedBy);
        result.EntityVersion.ShouldBe(token.EntityVersion);
        executionContext.HasExceptions.ShouldBeFalse();
    }

    [Fact]
    public async Task GetActiveByFamilyIdAsync_Should_ReturnActiveTokens()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando 2 tokens ativos com mesmo familyId");
        var tenantCode = Guid.NewGuid();
        var familyId = Guid.NewGuid();

        var activeToken1 = _fixture.CreateTestRefreshTokenDataModel(
            tenantCode: tenantCode, familyId: familyId, status: 1);
        var activeToken2 = _fixture.CreateTestRefreshTokenDataModel(
            tenantCode: tenantCode, familyId: familyId, status: 1);

        await _fixture.InsertRefreshTokenDirectlyAsync(activeToken1);
        await _fixture.InsertRefreshTokenDirectlyAsync(activeToken2);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando GetActiveByFamilyIdAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetActiveByFamilyIdAsync(executionContext, familyId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que 2 tokens ativos foram retornados");
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result.ShouldAllBe(t => t.FamilyId == familyId);
        result.ShouldAllBe(t => t.Status == 1);
        executionContext.HasExceptions.ShouldBeFalse();
    }

    [Fact]
    public async Task GetActiveByFamilyIdAsync_Should_NotReturnUsedOrRevokedTokens()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando tokens com status Active(1), Used(2) e Revoked(3) com mesmo familyId");
        var tenantCode = Guid.NewGuid();
        var familyId = Guid.NewGuid();

        var activeToken = _fixture.CreateTestRefreshTokenDataModel(
            tenantCode: tenantCode, familyId: familyId, status: 1);
        var usedToken = _fixture.CreateTestRefreshTokenDataModel(
            tenantCode: tenantCode, familyId: familyId, status: 2);
        var revokedToken = _fixture.CreateTestRefreshTokenDataModel(
            tenantCode: tenantCode, familyId: familyId, status: 3,
            revokedAt: DateTimeOffset.UtcNow);

        await _fixture.InsertRefreshTokenDirectlyAsync(activeToken);
        await _fixture.InsertRefreshTokenDirectlyAsync(usedToken);
        await _fixture.InsertRefreshTokenDirectlyAsync(revokedToken);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando GetActiveByFamilyIdAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetActiveByFamilyIdAsync(executionContext, familyId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que apenas o token ativo foi retornado");
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(activeToken.Id);
        result[0].Status.ShouldBe((short)1);
        executionContext.HasExceptions.ShouldBeFalse();
    }

    [Fact]
    public async Task GetActiveByFamilyIdAsync_Should_FilterByTenant()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando token ativo em tenant A e buscando em tenant B");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var familyId = Guid.NewGuid();

        var token = _fixture.CreateTestRefreshTokenDataModel(
            tenantCode: tenantA, familyId: familyId, status: 1);
        await _fixture.InsertRefreshTokenDirectlyAsync(token);

        var executionContext = _fixture.CreateExecutionContext(tenantB);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando GetActiveByFamilyIdAsync com tenant diferente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetActiveByFamilyIdAsync(executionContext, familyId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que nao encontra tokens de outro tenant");
        result.ShouldNotBeNull();
        result.Count.ShouldBe(0);
        executionContext.HasExceptions.ShouldBeFalse();
    }
}
