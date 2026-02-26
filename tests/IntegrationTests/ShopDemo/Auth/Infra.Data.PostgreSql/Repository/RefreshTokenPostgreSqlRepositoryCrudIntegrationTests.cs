using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using ShopDemo.Auth.Domain.Entities.RefreshTokens;
using ShopDemo.Auth.Domain.Entities.RefreshTokens.Enums;
using ShopDemo.Auth.Domain.Entities.RefreshTokens.Inputs;
using ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Repository;

[Collection("AuthPostgreSql")]
[Feature("RefreshTokenPostgreSqlRepository CRUD", "Round-trip completo Domain Entity <-> DataModel <-> SQL")]
public class RefreshTokenPostgreSqlRepositoryCrudIntegrationTests : IntegrationTestBase
{
    private readonly AuthPostgreSqlFixture _fixture;

    public RefreshTokenPostgreSqlRepositoryCrudIntegrationTests(
        AuthPostgreSqlFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnRefreshTokenEntity_WhenExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo refresh token via raw SQL e buscando como entity");
        var tenantCode = Guid.NewGuid();
        var dataModel = _fixture.CreateTestRefreshTokenDataModel(tenantCode: tenantCode);
        await _fixture.InsertRefreshTokenDirectlyAsync(dataModel);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);
        var repository = _fixture.CreateRefreshTokenPostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando GetByIdAsync no repositorio de dominio");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByIdAsync(
            executionContext,
            Id.CreateFromExistingInfo(dataModel.Id),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que a entidade RefreshToken foi construida corretamente");
        result.ShouldNotBeNull();
        result.EntityInfo.Id.Value.ShouldBe(dataModel.Id);
        result.UserId.Value.ShouldBe(dataModel.UserId);
        result.TokenHash.Value.ToArray().ShouldBe(dataModel.TokenHash);
        result.FamilyId.Value.ShouldBe(dataModel.FamilyId);
        result.Status.ShouldBe((RefreshTokenStatus)dataModel.Status);
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnNull_WhenNotExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Configurando contexto para ID inexistente");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);
        var repository = _fixture.CreateRefreshTokenPostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando GetByIdAsync para ID inexistente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByIdAsync(
            executionContext,
            Id.CreateFromExistingInfo(Guid.NewGuid()),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que null e retornado");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task RegisterNewAsync_Should_PersistRefreshTokenEntity()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando RefreshToken entity via RegisterNew");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);

        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var tokenHash = TokenHash.CreateNew(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
        var familyId = TokenFamily.CreateNew();
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);

        var input = new RegisterNewRefreshTokenInput(userId, tokenHash, familyId, expiresAt);
        var refreshToken = RefreshToken.RegisterNew(executionContext, input);
        refreshToken.ShouldNotBeNull();

        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);
        var repository = _fixture.CreateRefreshTokenPostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando RegisterNewAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.RegisterNewAsync(executionContext, refreshToken, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a entidade foi persistida via raw SQL");
        result.ShouldBeTrue();

        var persisted = await _fixture.GetRefreshTokenDirectlyAsync(refreshToken.EntityInfo.Id.Value, tenantCode);
        persisted.ShouldNotBeNull();
        persisted.UserId.ShouldBe(userId.Value);
        persisted.TokenHash.ShouldBe(tokenHash.Value.ToArray());
        persisted.FamilyId.ShouldBe(familyId.Value);
        persisted.Status.ShouldBe((short)RefreshTokenStatus.Active);
    }

    [Fact]
    public async Task ExistsAsync_Should_ReturnTrue_WhenExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo refresh token e verificando existencia via repositorio de dominio");
        var tenantCode = Guid.NewGuid();
        var dataModel = _fixture.CreateTestRefreshTokenDataModel(tenantCode: tenantCode);
        await _fixture.InsertRefreshTokenDirectlyAsync(dataModel);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);
        var repository = _fixture.CreateRefreshTokenPostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando ExistsAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.ExistsAsync(
            executionContext,
            Id.CreateFromExistingInfo(dataModel.Id),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que retorna true");
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateAsync_Should_PersistChanges()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo refresh token, recuperando como entity e revogando");
        var tenantCode = Guid.NewGuid();
        var dataModel = _fixture.CreateTestRefreshTokenDataModel(tenantCode: tenantCode, entityVersion: 1);
        await _fixture.InsertRefreshTokenDirectlyAsync(dataModel);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);
        var repository = _fixture.CreateRefreshTokenPostgreSqlRepository(dataModelRepo);

        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);

        var refreshToken = await repository.GetByIdAsync(
            executionContext,
            Id.CreateFromExistingInfo(dataModel.Id),
            CancellationToken.None);
        refreshToken.ShouldNotBeNull();

        var revokedToken = refreshToken.Revoke(
            executionContext,
            new RevokeRefreshTokenInput());
        revokedToken.ShouldNotBeNull();

        // Act
        LogAct("Chamando UpdateAsync");
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.UpdateAsync(executionContext, revokedToken, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a atualizacao foi persistida com Status=Revoked e RevokedAt definido");
        result.ShouldBeTrue();

        var persisted = await _fixture.GetRefreshTokenDirectlyAsync(dataModel.Id, tenantCode);
        persisted.ShouldNotBeNull();
        persisted.Status.ShouldBe((short)RefreshTokenStatus.Revoked);
        persisted.RevokedAt.ShouldNotBeNull();
        persisted.EntityVersion.ShouldNotBe(1L);
        persisted.EntityVersion.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task UpdateAsync_Should_ReturnFalse_WhenNotExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando refresh token sem persistir e tentando update");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);

        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var tokenHash = TokenHash.CreateNew(new byte[] { 1, 2, 3, 4, 5 });
        var familyId = TokenFamily.CreateNew();
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);

        var refreshToken = RefreshToken.RegisterNew(
            executionContext,
            new RegisterNewRefreshTokenInput(userId, tokenHash, familyId, expiresAt));
        refreshToken.ShouldNotBeNull();

        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);
        var repository = _fixture.CreateRefreshTokenPostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando UpdateAsync para refresh token inexistente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.UpdateAsync(executionContext, refreshToken, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que retorna false");
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task RegisterNewAsync_Should_PersistTokenHashAsBytea()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando RefreshToken com token hash de 64 bytes");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);

        var hashBytes = new byte[64];
        for (int i = 0; i < hashBytes.Length; i++)
            hashBytes[i] = (byte)(i % 256);

        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var tokenHash = TokenHash.CreateNew(hashBytes);
        var familyId = TokenFamily.CreateNew();
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);

        var refreshToken = RefreshToken.RegisterNew(
            executionContext,
            new RegisterNewRefreshTokenInput(userId, tokenHash, familyId, expiresAt));
        refreshToken.ShouldNotBeNull();

        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);
        var repository = _fixture.CreateRefreshTokenPostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Persistindo e recuperando via raw SQL");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        await repository.RegisterNewAsync(executionContext, refreshToken, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o token hash foi persistido corretamente como BYTEA");
        var persisted = await _fixture.GetRefreshTokenDirectlyAsync(refreshToken.EntityInfo.Id.Value, tenantCode);
        persisted.ShouldNotBeNull();
        persisted.TokenHash.ShouldBe(hashBytes);
    }
}
