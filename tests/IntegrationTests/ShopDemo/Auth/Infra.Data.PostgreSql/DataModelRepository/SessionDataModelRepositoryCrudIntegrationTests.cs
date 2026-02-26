using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.DataModelRepository;

[Collection("AuthPostgreSql")]
[Feature("SessionDataModel CRUD", "Operacoes CRUD do repositorio de SessionDataModel")]
public class SessionDataModelRepositoryCrudIntegrationTests : IntegrationTestBase
{
    private readonly AuthPostgreSqlFixture _fixture;

    public SessionDataModelRepositoryCrudIntegrationTests(
        AuthPostgreSqlFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnSession_WhenExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando sessao de teste e inserindo diretamente");
        var tenantCode = Guid.NewGuid();
        var session = _fixture.CreateTestSessionDataModel(tenantCode: tenantCode);
        await _fixture.InsertSessionDirectlyAsync(session);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateSessionDataModelRepository(unitOfWork);

        // Act
        LogAct("Abrindo conexao e chamando GetByIdAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByIdAsync(executionContext, session.Id, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a sessao foi recuperada corretamente");
        result.ShouldNotBeNull();
        result.Id.ShouldBe(session.Id);
        result.TenantCode.ShouldBe(session.TenantCode);
        result.UserId.ShouldBe(session.UserId);
        result.RefreshTokenId.ShouldBe(session.RefreshTokenId);
        result.DeviceInfo.ShouldBe(session.DeviceInfo);
        result.IpAddress.ShouldBe(session.IpAddress);
        result.UserAgent.ShouldBe(session.UserAgent);
        result.Status.ShouldBe(session.Status);
        result.EntityVersion.ShouldBe(session.EntityVersion);
        executionContext.HasExceptions.ShouldBeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnNull_WhenNotExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Configurando contexto com ID de sessao inexistente");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateSessionDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando GetByIdAsync para sessao inexistente");
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
        LogArrange("Criando e inserindo sessao de teste");
        var tenantCode = Guid.NewGuid();
        var session = _fixture.CreateTestSessionDataModel(tenantCode: tenantCode);
        await _fixture.InsertSessionDirectlyAsync(session);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateSessionDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando ExistsAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.ExistsAsync(executionContext, session.Id, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a sessao existe");
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
        var repository = _fixture.CreateSessionDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando ExistsAsync para sessao inexistente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.ExistsAsync(executionContext, Guid.NewGuid(), CancellationToken.None);

        // Assert
        LogAssert("Verificando que a sessao nao existe");
        result.ShouldBeFalse();
        executionContext.HasExceptions.ShouldBeFalse();
    }

    [Fact]
    public async Task InsertAsync_Should_PersistAllFields()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando sessao de teste com todos os campos");
        var tenantCode = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var refreshTokenId = Guid.NewGuid();
        var expiresAt = DateTimeOffset.UtcNow.AddHours(2);
        var lastActivityAt = DateTimeOffset.UtcNow;
        var session = _fixture.CreateTestSessionDataModel(
            tenantCode: tenantCode,
            userId: userId,
            refreshTokenId: refreshTokenId,
            deviceInfo: "Chrome/Windows",
            ipAddress: "192.168.1.100",
            userAgent: "Mozilla/5.0",
            expiresAt: expiresAt,
            status: 1,
            lastActivityAt: lastActivityAt);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateSessionDataModelRepository(unitOfWork);

        // Act
        LogAct("Inserindo sessao pelo repositorio");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.InsertAsync(executionContext, session, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que todos os campos foram persistidos");
        result.ShouldBeTrue();
        executionContext.HasExceptions.ShouldBeFalse();

        var persisted = await _fixture.GetSessionDirectlyAsync(session.Id, tenantCode);
        persisted.ShouldNotBeNull();
        persisted.Id.ShouldBe(session.Id);
        persisted.TenantCode.ShouldBe(session.TenantCode);
        persisted.UserId.ShouldBe(userId);
        persisted.RefreshTokenId.ShouldBe(refreshTokenId);
        persisted.DeviceInfo.ShouldBe("Chrome/Windows");
        persisted.IpAddress.ShouldBe("192.168.1.100");
        persisted.UserAgent.ShouldBe("Mozilla/5.0");
        persisted.Status.ShouldBe((short)1);
        persisted.CreatedBy.ShouldBe(session.CreatedBy);
    }

    [Fact]
    public async Task InsertAsync_Should_HandleNullableFields()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando sessao de teste com campos opcionais nulos");
        var tenantCode = Guid.NewGuid();
        var session = _fixture.CreateTestSessionDataModel(
            tenantCode: tenantCode,
            deviceInfo: null,
            ipAddress: null,
            userAgent: null,
            revokedAt: null);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateSessionDataModelRepository(unitOfWork);

        // Act
        LogAct("Inserindo sessao com campos opcionais nulos");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.InsertAsync(executionContext, session, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que campos anulaveis sao armazenados como null");
        result.ShouldBeTrue();
        var persisted = await _fixture.GetSessionDirectlyAsync(session.Id, tenantCode);
        persisted.ShouldNotBeNull();
        persisted.DeviceInfo.ShouldBeNull();
        persisted.IpAddress.ShouldBeNull();
        persisted.UserAgent.ShouldBeNull();
        persisted.RevokedAt.ShouldBeNull();
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
        LogArrange("Criando e inserindo sessao de teste");
        var tenantCode = Guid.NewGuid();
        var originalVersion = 1L;
        var session = _fixture.CreateTestSessionDataModel(
            tenantCode: tenantCode,
            status: 1,
            entityVersion: originalVersion);
        await _fixture.InsertSessionDirectlyAsync(session);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateSessionDataModelRepository(unitOfWork);

        session.Status = 2;
        session.RevokedAt = DateTimeOffset.UtcNow;
        session.EntityVersion = originalVersion + 1;

        // Act
        LogAct("Atualizando sessao com versao correspondente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.UpdateAsync(executionContext, session, expectedVersion: originalVersion, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a sessao foi atualizada");
        result.ShouldBeTrue();
        executionContext.HasExceptions.ShouldBeFalse();

        var updated = await _fixture.GetSessionDirectlyAsync(session.Id, tenantCode);
        updated.ShouldNotBeNull();
        updated.Status.ShouldBe((short)2);
        updated.RevokedAt.ShouldNotBeNull();
        updated.EntityVersion.ShouldBe(2);
    }

    [Fact]
    public async Task UpdateAsync_Should_ReturnFalse_WhenVersionMismatch()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando e inserindo sessao de teste com versao 5");
        var tenantCode = Guid.NewGuid();
        var session = _fixture.CreateTestSessionDataModel(
            tenantCode: tenantCode,
            entityVersion: 5);
        await _fixture.InsertSessionDirectlyAsync(session);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateSessionDataModelRepository(unitOfWork);

        session.Status = 2;
        session.EntityVersion = 6;

        // Act
        LogAct("Tentando atualizacao com versao obsoleta");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.UpdateAsync(executionContext, session, expectedVersion: 1, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a atualizacao falhou por incompatibilidade de versao");
        result.ShouldBeFalse();

        var unchanged = await _fixture.GetSessionDirectlyAsync(session.Id, tenantCode);
        unchanged.ShouldNotBeNull();
        unchanged.Status.ShouldNotBe((short)2);
        unchanged.EntityVersion.ShouldBe(5);
    }

    [Fact]
    public async Task DeleteAsync_Should_RemoveSession_WhenVersionMatches()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando e inserindo sessao de teste");
        var tenantCode = Guid.NewGuid();
        var session = _fixture.CreateTestSessionDataModel(
            tenantCode: tenantCode,
            entityVersion: 1);
        await _fixture.InsertSessionDirectlyAsync(session);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateSessionDataModelRepository(unitOfWork);

        // Act
        LogAct("Excluindo sessao com versao correspondente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.DeleteAsync(executionContext, session.Id, expectedVersion: 1, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a sessao foi excluida");
        result.ShouldBeTrue();
        var deleted = await _fixture.GetSessionDirectlyAsync(session.Id, tenantCode);
        deleted.ShouldBeNull();
    }

    [Fact]
    public async Task DeleteAsync_Should_ReturnFalse_WhenVersionMismatch()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando e inserindo sessao com versao 3");
        var tenantCode = Guid.NewGuid();
        var session = _fixture.CreateTestSessionDataModel(
            tenantCode: tenantCode,
            entityVersion: 3);
        await _fixture.InsertSessionDirectlyAsync(session);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateSessionDataModelRepository(unitOfWork);

        // Act
        LogAct("Tentando exclusao com versao obsoleta");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.DeleteAsync(executionContext, session.Id, expectedVersion: 1, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a exclusao falhou");
        result.ShouldBeFalse();
        var stillExists = await _fixture.GetSessionDirectlyAsync(session.Id, tenantCode);
        stillExists.ShouldNotBeNull();
    }
}
