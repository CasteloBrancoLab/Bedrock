using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.DataModelRepository;

[Collection("AuthPostgreSql")]
[Feature("RefreshTokenDataModel CRUD", "Operacoes CRUD do repositorio de RefreshTokenDataModel")]
public class RefreshTokenDataModelRepositoryCrudIntegrationTests : IntegrationTestBase
{
    private readonly AuthPostgreSqlFixture _fixture;

    public RefreshTokenDataModelRepositoryCrudIntegrationTests(
        AuthPostgreSqlFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnRefreshToken_WhenExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando refresh token de teste e inserindo diretamente");
        var tenantCode = Guid.NewGuid();
        var token = _fixture.CreateTestRefreshTokenDataModel(tenantCode: tenantCode);
        await _fixture.InsertRefreshTokenDirectlyAsync(token);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);

        // Act
        LogAct("Abrindo conexao e chamando GetByIdAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByIdAsync(executionContext, token.Id, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o refresh token foi recuperado corretamente");
        result.ShouldNotBeNull();
        result.Id.ShouldBe(token.Id);
        result.TenantCode.ShouldBe(token.TenantCode);
        result.UserId.ShouldBe(token.UserId);
        result.TokenHash.ShouldBe(token.TokenHash);
        result.FamilyId.ShouldBe(token.FamilyId);
        result.Status.ShouldBe(token.Status);
        result.EntityVersion.ShouldBe(token.EntityVersion);
        executionContext.HasExceptions.ShouldBeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnNull_WhenNotExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Configurando contexto com ID de refresh token inexistente");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando GetByIdAsync para refresh token inexistente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByIdAsync(executionContext, Guid.NewGuid(), CancellationToken.None);

        // Assert
        LogAssert("Verificando que null e retornado");
        result.ShouldBeNull();
        executionContext.HasExceptions.ShouldBeFalse();
    }

    [Fact]
    public async Task ExistsAsync_Should_ReturnTrue_WhenExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando e inserindo refresh token de teste");
        var tenantCode = Guid.NewGuid();
        var token = _fixture.CreateTestRefreshTokenDataModel(tenantCode: tenantCode);
        await _fixture.InsertRefreshTokenDirectlyAsync(token);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando ExistsAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.ExistsAsync(executionContext, token.Id, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o refresh token existe");
        result.ShouldBeTrue();
        executionContext.HasExceptions.ShouldBeFalse();
    }

    [Fact]
    public async Task ExistsAsync_Should_ReturnFalse_WhenNotExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Configurando contexto com ID inexistente");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando ExistsAsync para refresh token inexistente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.ExistsAsync(executionContext, Guid.NewGuid(), CancellationToken.None);

        // Assert
        LogAssert("Verificando que o refresh token nao existe");
        result.ShouldBeFalse();
        executionContext.HasExceptions.ShouldBeFalse();
    }

    [Fact]
    public async Task InsertAsync_Should_PersistAllFields()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando refresh token de teste com todos os campos");
        var tenantCode = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var expiresAt = DateTimeOffset.UtcNow.AddHours(2);
        var revokedAt = DateTimeOffset.UtcNow;
        var replacedByTokenId = Guid.NewGuid();
        var token = _fixture.CreateTestRefreshTokenDataModel(
            tenantCode: tenantCode,
            userId: userId,
            familyId: familyId,
            expiresAt: expiresAt,
            status: 3,
            revokedAt: revokedAt,
            replacedByTokenId: replacedByTokenId);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);

        // Act
        LogAct("Inserindo refresh token pelo repositorio");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.InsertAsync(executionContext, token, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que todos os campos foram persistidos");
        result.ShouldBeTrue();
        executionContext.HasExceptions.ShouldBeFalse();

        var persisted = await _fixture.GetRefreshTokenDirectlyAsync(token.Id, tenantCode);
        persisted.ShouldNotBeNull();
        persisted.Id.ShouldBe(token.Id);
        persisted.TenantCode.ShouldBe(token.TenantCode);
        persisted.UserId.ShouldBe(userId);
        persisted.TokenHash.ShouldBe(token.TokenHash);
        persisted.FamilyId.ShouldBe(familyId);
        persisted.Status.ShouldBe((short)3);
        persisted.RevokedAt.ShouldNotBeNull();
        persisted.ReplacedByTokenId.ShouldBe(replacedByTokenId);
        persisted.CreatedBy.ShouldBe(token.CreatedBy);
    }

    [Fact]
    public async Task InsertAsync_Should_HandleNullableFields()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando refresh token de teste com campos opcionais nulos");
        var tenantCode = Guid.NewGuid();
        var token = _fixture.CreateTestRefreshTokenDataModel(
            tenantCode: tenantCode,
            revokedAt: null,
            replacedByTokenId: null);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);

        // Act
        LogAct("Inserindo refresh token com campos opcionais nulos");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.InsertAsync(executionContext, token, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que campos anulaveis sao armazenados como null");
        result.ShouldBeTrue();
        var persisted = await _fixture.GetRefreshTokenDirectlyAsync(token.Id, tenantCode);
        persisted.ShouldNotBeNull();
        persisted.RevokedAt.ShouldBeNull();
        persisted.ReplacedByTokenId.ShouldBeNull();
        persisted.LastChangedBy.ShouldBeNull();
        persisted.LastChangedAt.ShouldBeNull();
        persisted.LastChangedExecutionOrigin.ShouldBeNull();
        persisted.LastChangedCorrelationId.ShouldBeNull();
        persisted.LastChangedBusinessOperationCode.ShouldBeNull();
    }

    [Fact]
    public async Task UpdateAsync_Should_ModifyFields_WhenVersionMatches()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando e inserindo refresh token de teste");
        var tenantCode = Guid.NewGuid();
        var originalVersion = 1L;
        var token = _fixture.CreateTestRefreshTokenDataModel(
            tenantCode: tenantCode,
            status: 1,
            entityVersion: originalVersion);
        await _fixture.InsertRefreshTokenDirectlyAsync(token);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);

        token.Status = 3;
        token.RevokedAt = DateTimeOffset.UtcNow;
        token.EntityVersion = originalVersion + 1;

        // Act
        LogAct("Atualizando refresh token com versao correspondente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.UpdateAsync(executionContext, token, expectedVersion: originalVersion, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o refresh token foi atualizado");
        result.ShouldBeTrue();
        executionContext.HasExceptions.ShouldBeFalse();

        var updated = await _fixture.GetRefreshTokenDirectlyAsync(token.Id, tenantCode);
        updated.ShouldNotBeNull();
        updated.Status.ShouldBe((short)3);
        updated.RevokedAt.ShouldNotBeNull();
        updated.EntityVersion.ShouldBe(2);
    }

    [Fact]
    public async Task UpdateAsync_Should_ReturnFalse_WhenVersionMismatch()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando e inserindo refresh token de teste com versao 5");
        var tenantCode = Guid.NewGuid();
        var token = _fixture.CreateTestRefreshTokenDataModel(
            tenantCode: tenantCode,
            entityVersion: 5);
        await _fixture.InsertRefreshTokenDirectlyAsync(token);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);

        token.Status = 3;
        token.EntityVersion = 6;

        // Act
        LogAct("Tentando atualizacao com versao obsoleta");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.UpdateAsync(executionContext, token, expectedVersion: 1, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a atualizacao falhou por incompatibilidade de versao");
        result.ShouldBeFalse();

        var unchanged = await _fixture.GetRefreshTokenDirectlyAsync(token.Id, tenantCode);
        unchanged.ShouldNotBeNull();
        unchanged.Status.ShouldNotBe((short)3);
        unchanged.EntityVersion.ShouldBe(5);
    }

    [Fact]
    public async Task DeleteAsync_Should_RemoveRefreshToken_WhenVersionMatches()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando e inserindo refresh token de teste");
        var tenantCode = Guid.NewGuid();
        var token = _fixture.CreateTestRefreshTokenDataModel(
            tenantCode: tenantCode,
            entityVersion: 1);
        await _fixture.InsertRefreshTokenDirectlyAsync(token);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);

        // Act
        LogAct("Excluindo refresh token com versao correspondente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.DeleteAsync(executionContext, token.Id, expectedVersion: 1, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o refresh token foi excluido");
        result.ShouldBeTrue();
        var deleted = await _fixture.GetRefreshTokenDirectlyAsync(token.Id, tenantCode);
        deleted.ShouldBeNull();
    }

    [Fact]
    public async Task DeleteAsync_Should_ReturnFalse_WhenVersionMismatch()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando e inserindo refresh token com versao 3");
        var tenantCode = Guid.NewGuid();
        var token = _fixture.CreateTestRefreshTokenDataModel(
            tenantCode: tenantCode,
            entityVersion: 3);
        await _fixture.InsertRefreshTokenDirectlyAsync(token);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);

        // Act
        LogAct("Tentando exclusao com versao obsoleta");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.DeleteAsync(executionContext, token.Id, expectedVersion: 1, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a exclusao falhou");
        result.ShouldBeFalse();
        var stillExists = await _fixture.GetRefreshTokenDirectlyAsync(token.Id, tenantCode);
        stillExists.ShouldNotBeNull();
    }
}
