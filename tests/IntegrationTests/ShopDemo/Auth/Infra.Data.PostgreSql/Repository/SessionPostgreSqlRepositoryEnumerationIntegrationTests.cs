using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using ShopDemo.Auth.Domain.Entities.Sessions;
using ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Repository;

[Collection("AuthPostgreSql")]
[Feature("SessionPostgreSqlRepository Enumeration", "EnumerateAllAsync e EnumerateModifiedSinceAsync")]
public class SessionPostgreSqlRepositoryEnumerationIntegrationTests : IntegrationTestBase
{
    private readonly AuthPostgreSqlFixture _fixture;

    public SessionPostgreSqlRepositoryEnumerationIntegrationTests(
        AuthPostgreSqlFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task EnumerateAllAsync_Should_EnumerateAllSessions()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo 3 sessions no mesmo tenant");
        var tenantCode = Guid.NewGuid();
        for (int i = 0; i < 3; i++)
        {
            var session = _fixture.CreateTestSessionDataModel(tenantCode: tenantCode);
            await _fixture.InsertSessionDirectlyAsync(session);
        }

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateSessionDataModelRepository(unitOfWork);
        var repository = _fixture.CreateSessionPostgreSqlRepository(dataModelRepo);

        var enumerated = new List<Session>();

        // Act
        LogAct("Chamando EnumerateAllAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.EnumerateAllAsync(
            executionContext,
            PaginationInfo.All,
            (ctx, session, pagination, ct) =>
            {
                enumerated.Add(session);
                return Task.FromResult(true);
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que 3 sessions foram enumeradas");
        result.ShouldBeTrue();
        enumerated.Count.ShouldBe(3);
    }

    [Fact]
    public async Task EnumerateAllAsync_Should_StopOnFalse()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo 5 sessions no mesmo tenant");
        var tenantCode = Guid.NewGuid();
        for (int i = 0; i < 5; i++)
        {
            var session = _fixture.CreateTestSessionDataModel(tenantCode: tenantCode);
            await _fixture.InsertSessionDirectlyAsync(session);
        }

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateSessionDataModelRepository(unitOfWork);
        var repository = _fixture.CreateSessionPostgreSqlRepository(dataModelRepo);

        var count = 0;

        // Act
        LogAct("Chamando EnumerateAllAsync com handler que para apos 2 itens");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.EnumerateAllAsync(
            executionContext,
            PaginationInfo.All,
            (ctx, session, pagination, ct) =>
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
    public async Task EnumerateAllAsync_Should_ConvertToSessionEntities()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo session com dados conhecidos");
        var tenantCode = Guid.NewGuid();
        var knownUserId = Guid.NewGuid();
        var knownRefreshTokenId = Guid.NewGuid();

        var dataModel = _fixture.CreateTestSessionDataModel(
            tenantCode: tenantCode,
            userId: knownUserId,
            refreshTokenId: knownRefreshTokenId,
            deviceInfo: "KnownDevice",
            ipAddress: "192.168.1.1",
            userAgent: "KnownAgent/2.0");
        await _fixture.InsertSessionDirectlyAsync(dataModel);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateSessionDataModelRepository(unitOfWork);
        var repository = _fixture.CreateSessionPostgreSqlRepository(dataModelRepo);

        Session? capturedSession = null;

        // Act
        LogAct("Chamando EnumerateAllAsync e capturando entidade");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await repository.EnumerateAllAsync(
            executionContext,
            PaginationInfo.All,
            (ctx, session, pagination, ct) =>
            {
                capturedSession = session;
                return Task.FromResult(true);
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que a entidade Session foi convertida corretamente");
        capturedSession.ShouldNotBeNull();
        capturedSession.UserId.Value.ShouldBe(knownUserId);
        capturedSession.RefreshTokenId.Value.ShouldBe(knownRefreshTokenId);
        capturedSession.DeviceInfo.ShouldBe("KnownDevice");
        capturedSession.IpAddress.ShouldBe("192.168.1.1");
        capturedSession.UserAgent.ShouldBe("KnownAgent/2.0");
    }

    [Fact]
    public async Task EnumerateModifiedSinceAsync_Should_FilterByTimestamp()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo session com last_changed_at definido");
        var tenantCode = Guid.NewGuid();
        var sinceTime = DateTimeOffset.UtcNow.AddMinutes(-5);

        var session = _fixture.CreateTestSessionDataModel(tenantCode: tenantCode);
        session.LastChangedBy = "modifier";
        session.LastChangedAt = DateTimeOffset.UtcNow;
        session.LastChangedExecutionOrigin = "TestOrigin";
        session.LastChangedCorrelationId = Guid.NewGuid();
        session.LastChangedBusinessOperationCode = "TEST_OP";
        await _fixture.InsertSessionDirectlyAsync(session);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateSessionDataModelRepository(unitOfWork);
        var repository = _fixture.CreateSessionPostgreSqlRepository(dataModelRepo);

        var enumerated = new List<Session>();

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
        LogAssert("Verificando que a session modificada foi enumerada");
        result.ShouldBeTrue();
        enumerated.Count.ShouldBeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task EnumerateModifiedSinceAsync_Should_NotReturnUnmodifiedSessions()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo session sem last_changed_at e buscando modificadas");
        var tenantCode = Guid.NewGuid();
        var session = _fixture.CreateTestSessionDataModel(tenantCode: tenantCode);
        await _fixture.InsertSessionDirectlyAsync(session);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateSessionDataModelRepository(unitOfWork);
        var repository = _fixture.CreateSessionPostgreSqlRepository(dataModelRepo);

        var enumerated = new List<Session>();

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
        LogAssert("Verificando que nenhuma session foi enumerada");
        result.ShouldBeTrue();
        enumerated.ShouldBeEmpty();
    }

    [Fact]
    public async Task EnumerateModifiedSinceAsync_Should_ConvertToSessionEntities()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo session modificada com dados conhecidos");
        var tenantCode = Guid.NewGuid();
        var knownUserId = Guid.NewGuid();
        var knownRefreshTokenId = Guid.NewGuid();

        var dataModel = _fixture.CreateTestSessionDataModel(
            tenantCode: tenantCode,
            userId: knownUserId,
            refreshTokenId: knownRefreshTokenId,
            deviceInfo: "ModifiedDevice",
            ipAddress: "10.0.0.1",
            userAgent: "ModifiedAgent/3.0");
        dataModel.LastChangedBy = "modifier";
        dataModel.LastChangedAt = DateTimeOffset.UtcNow;
        dataModel.LastChangedExecutionOrigin = "TestOrigin";
        dataModel.LastChangedCorrelationId = Guid.NewGuid();
        dataModel.LastChangedBusinessOperationCode = "TEST_OP";
        await _fixture.InsertSessionDirectlyAsync(dataModel);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateSessionDataModelRepository(unitOfWork);
        var repository = _fixture.CreateSessionPostgreSqlRepository(dataModelRepo);

        Session? capturedSession = null;

        // Act
        LogAct("Chamando EnumerateModifiedSinceAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await repository.EnumerateModifiedSinceAsync(
            executionContext,
            TimeProvider.System,
            DateTimeOffset.UtcNow.AddMinutes(-5),
            (ctx, entity, timeProvider, since, ct) =>
            {
                capturedSession = entity;
                return Task.FromResult(true);
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que a entidade Session foi convertida corretamente");
        capturedSession.ShouldNotBeNull();
        capturedSession.UserId.Value.ShouldBe(knownUserId);
        capturedSession.RefreshTokenId.Value.ShouldBe(knownRefreshTokenId);
        capturedSession.DeviceInfo.ShouldBe("ModifiedDevice");
        capturedSession.IpAddress.ShouldBe("10.0.0.1");
        capturedSession.UserAgent.ShouldBe("ModifiedAgent/3.0");
    }
}
