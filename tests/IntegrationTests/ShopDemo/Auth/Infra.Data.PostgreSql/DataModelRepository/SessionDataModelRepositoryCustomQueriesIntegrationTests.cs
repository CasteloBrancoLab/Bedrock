using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.DataModelRepository;

[Collection("AuthPostgreSql")]
[Feature("SessionDataModel Custom Queries", "Queries customizadas do repositorio de SessionDataModel")]
public class SessionDataModelRepositoryCustomQueriesIntegrationTests : IntegrationTestBase
{
    private readonly AuthPostgreSqlFixture _fixture;

    public SessionDataModelRepositoryCustomQueriesIntegrationTests(
        AuthPostgreSqlFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetByUserIdAsync_Should_ReturnSessions_WhenExist()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando 3 sessoes para o mesmo userId");
        var tenantCode = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var session1 = _fixture.CreateTestSessionDataModel(tenantCode: tenantCode, userId: userId);
        var session2 = _fixture.CreateTestSessionDataModel(tenantCode: tenantCode, userId: userId);
        var session3 = _fixture.CreateTestSessionDataModel(tenantCode: tenantCode, userId: userId);

        await _fixture.InsertSessionDirectlyAsync(session1);
        await _fixture.InsertSessionDirectlyAsync(session2);
        await _fixture.InsertSessionDirectlyAsync(session3);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateSessionDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando GetByUserIdAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByUserIdAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que 3 sessoes foram retornadas");
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
        result.ShouldAllBe(s => s.UserId == userId);
        executionContext.HasExceptions.ShouldBeFalse();
    }

    [Fact]
    public async Task GetByUserIdAsync_Should_ReturnEmpty_WhenNoSessions()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Configurando contexto para userId sem sessoes");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateSessionDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando GetByUserIdAsync para userId sem sessoes");
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
        LogArrange("Criando sessao em tenant A e buscando em tenant B");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var session = _fixture.CreateTestSessionDataModel(tenantCode: tenantA, userId: userId);
        await _fixture.InsertSessionDirectlyAsync(session);

        var executionContext = _fixture.CreateExecutionContext(tenantB);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateSessionDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando GetByUserIdAsync com tenant diferente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByUserIdAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que nao encontra sessoes de outro tenant");
        result.ShouldNotBeNull();
        result.Count.ShouldBe(0);
        executionContext.HasExceptions.ShouldBeFalse();
    }

    [Fact]
    public async Task GetByUserIdAsync_Should_PopulateAllFields()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando sessao com todos os campos preenchidos");
        var tenantCode = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var session = _fixture.CreateTestSessionDataModel(tenantCode: tenantCode, userId: userId);
        session.LastChangedBy = "modifier_user";
        session.LastChangedAt = DateTimeOffset.UtcNow;
        session.LastChangedExecutionOrigin = "TestOrigin";
        session.LastChangedCorrelationId = Guid.NewGuid();
        session.LastChangedBusinessOperationCode = "UPDATE_OP";
        await _fixture.InsertSessionDirectlyAsync(session);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateSessionDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando GetByUserIdAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByUserIdAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que todos os campos foram populados");
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        var returned = result[0];
        returned.Id.ShouldBe(session.Id);
        returned.TenantCode.ShouldBe(session.TenantCode);
        returned.UserId.ShouldBe(session.UserId);
        returned.RefreshTokenId.ShouldBe(session.RefreshTokenId);
        returned.DeviceInfo.ShouldBe(session.DeviceInfo);
        returned.IpAddress.ShouldBe(session.IpAddress);
        returned.UserAgent.ShouldBe(session.UserAgent);
        returned.Status.ShouldBe(session.Status);
        returned.CreatedBy.ShouldBe(session.CreatedBy);
        returned.EntityVersion.ShouldBe(session.EntityVersion);
        executionContext.HasExceptions.ShouldBeFalse();
    }

    [Fact]
    public async Task GetActiveByUserIdAsync_Should_ReturnActiveSessions()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando 2 sessoes ativas para o mesmo userId");
        var tenantCode = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var activeSession1 = _fixture.CreateTestSessionDataModel(
            tenantCode: tenantCode, userId: userId, status: 1);
        var activeSession2 = _fixture.CreateTestSessionDataModel(
            tenantCode: tenantCode, userId: userId, status: 1);

        await _fixture.InsertSessionDirectlyAsync(activeSession1);
        await _fixture.InsertSessionDirectlyAsync(activeSession2);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateSessionDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando GetActiveByUserIdAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetActiveByUserIdAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que 2 sessoes ativas foram retornadas");
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result.ShouldAllBe(s => s.UserId == userId);
        result.ShouldAllBe(s => s.Status == 1);
        executionContext.HasExceptions.ShouldBeFalse();
    }

    [Fact]
    public async Task GetActiveByUserIdAsync_Should_NotReturnRevokedSessions()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando sessoes com status Active(1) e Revoked(2) para o mesmo userId");
        var tenantCode = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var activeSession = _fixture.CreateTestSessionDataModel(
            tenantCode: tenantCode, userId: userId, status: 1);
        var revokedSession = _fixture.CreateTestSessionDataModel(
            tenantCode: tenantCode, userId: userId, status: 2,
            revokedAt: DateTimeOffset.UtcNow);

        await _fixture.InsertSessionDirectlyAsync(activeSession);
        await _fixture.InsertSessionDirectlyAsync(revokedSession);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateSessionDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando GetActiveByUserIdAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetActiveByUserIdAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que apenas a sessao ativa foi retornada");
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(activeSession.Id);
        result[0].Status.ShouldBe((short)1);
        executionContext.HasExceptions.ShouldBeFalse();
    }

    [Fact]
    public async Task GetActiveByUserIdAsync_Should_FilterByTenant()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando sessao ativa em tenant A e buscando em tenant B");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var session = _fixture.CreateTestSessionDataModel(
            tenantCode: tenantA, userId: userId, status: 1);
        await _fixture.InsertSessionDirectlyAsync(session);

        var executionContext = _fixture.CreateExecutionContext(tenantB);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateSessionDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando GetActiveByUserIdAsync com tenant diferente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetActiveByUserIdAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que nao encontra sessoes de outro tenant");
        result.ShouldNotBeNull();
        result.Count.ShouldBe(0);
        executionContext.HasExceptions.ShouldBeFalse();
    }

    [Fact]
    public async Task CountActiveByUserIdAsync_Should_ReturnCount()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando 3 sessoes ativas para o mesmo userId");
        var tenantCode = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var session1 = _fixture.CreateTestSessionDataModel(
            tenantCode: tenantCode, userId: userId, status: 1);
        var session2 = _fixture.CreateTestSessionDataModel(
            tenantCode: tenantCode, userId: userId, status: 1);
        var session3 = _fixture.CreateTestSessionDataModel(
            tenantCode: tenantCode, userId: userId, status: 1);

        await _fixture.InsertSessionDirectlyAsync(session1);
        await _fixture.InsertSessionDirectlyAsync(session2);
        await _fixture.InsertSessionDirectlyAsync(session3);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateSessionDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando CountActiveByUserIdAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.CountActiveByUserIdAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a contagem retorna 3");
        result.ShouldBe(3);
        executionContext.HasExceptions.ShouldBeFalse();
    }

    [Fact]
    public async Task CountActiveByUserIdAsync_Should_ReturnZero_WhenNoActiveSessions()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Configurando contexto para userId sem sessoes ativas");
        var tenantCode = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var revokedSession = _fixture.CreateTestSessionDataModel(
            tenantCode: tenantCode, userId: userId, status: 2,
            revokedAt: DateTimeOffset.UtcNow);
        await _fixture.InsertSessionDirectlyAsync(revokedSession);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateSessionDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando CountActiveByUserIdAsync para userId sem sessoes ativas");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.CountActiveByUserIdAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a contagem retorna zero");
        result.ShouldBe(0);
        executionContext.HasExceptions.ShouldBeFalse();
    }

    [Fact]
    public async Task CountActiveByUserIdAsync_Should_FilterByTenant()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando sessao ativa em tenant A e contando em tenant B");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var session = _fixture.CreateTestSessionDataModel(
            tenantCode: tenantA, userId: userId, status: 1);
        await _fixture.InsertSessionDirectlyAsync(session);

        var executionContext = _fixture.CreateExecutionContext(tenantB);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateSessionDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando CountActiveByUserIdAsync com tenant diferente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.CountActiveByUserIdAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que nao conta sessoes de outro tenant");
        result.ShouldBe(0);
        executionContext.HasExceptions.ShouldBeFalse();
    }
}
