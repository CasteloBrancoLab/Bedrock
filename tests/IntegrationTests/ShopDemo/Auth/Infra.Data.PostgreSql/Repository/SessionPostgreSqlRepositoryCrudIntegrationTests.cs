using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using ShopDemo.Auth.Domain.Entities.Sessions;
using ShopDemo.Auth.Domain.Entities.Sessions.Enums;
using ShopDemo.Auth.Domain.Entities.Sessions.Inputs;
using ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Repository;

[Collection("AuthPostgreSql")]
[Feature("SessionPostgreSqlRepository CRUD", "Round-trip completo Domain Entity <-> DataModel <-> SQL")]
public class SessionPostgreSqlRepositoryCrudIntegrationTests : IntegrationTestBase
{
    private readonly AuthPostgreSqlFixture _fixture;

    public SessionPostgreSqlRepositoryCrudIntegrationTests(
        AuthPostgreSqlFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnSessionEntity_WhenExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo session via raw SQL e buscando como entity");
        var tenantCode = Guid.NewGuid();
        var dataModel = _fixture.CreateTestSessionDataModel(tenantCode: tenantCode);
        await _fixture.InsertSessionDirectlyAsync(dataModel);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateSessionDataModelRepository(unitOfWork);
        var repository = _fixture.CreateSessionPostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando GetByIdAsync no repositorio de dominio");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByIdAsync(
            executionContext,
            Id.CreateFromExistingInfo(dataModel.Id),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que a entidade Session foi construida corretamente");
        result.ShouldNotBeNull();
        result.EntityInfo.Id.Value.ShouldBe(dataModel.Id);
        result.UserId.Value.ShouldBe(dataModel.UserId);
        result.RefreshTokenId.Value.ShouldBe(dataModel.RefreshTokenId);
        result.DeviceInfo.ShouldBe(dataModel.DeviceInfo);
        result.IpAddress.ShouldBe(dataModel.IpAddress);
        result.UserAgent.ShouldBe(dataModel.UserAgent);
        result.Status.ShouldBe((SessionStatus)dataModel.Status);
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
        var dataModelRepo = _fixture.CreateSessionDataModelRepository(unitOfWork);
        var repository = _fixture.CreateSessionPostgreSqlRepository(dataModelRepo);

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
    public async Task RegisterNewAsync_Should_PersistSessionEntity()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando Session entity via RegisterNew");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);

        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var refreshTokenId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);

        var input = new RegisterNewSessionInput(userId, refreshTokenId, "TestDevice", "127.0.0.1", "TestAgent/1.0", expiresAt);
        var session = Session.RegisterNew(executionContext, input);
        session.ShouldNotBeNull();

        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateSessionDataModelRepository(unitOfWork);
        var repository = _fixture.CreateSessionPostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando RegisterNewAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.RegisterNewAsync(executionContext, session, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a entidade foi persistida via raw SQL");
        result.ShouldBeTrue();

        var persisted = await _fixture.GetSessionDirectlyAsync(session.EntityInfo.Id.Value, tenantCode);
        persisted.ShouldNotBeNull();
        persisted.UserId.ShouldBe(userId.Value);
        persisted.RefreshTokenId.ShouldBe(refreshTokenId.Value);
        persisted.DeviceInfo.ShouldBe("TestDevice");
        persisted.IpAddress.ShouldBe("127.0.0.1");
        persisted.UserAgent.ShouldBe("TestAgent/1.0");
        persisted.Status.ShouldBe((short)SessionStatus.Active);
    }

    [Fact]
    public async Task ExistsAsync_Should_ReturnTrue_WhenExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo session e verificando existencia via repositorio de dominio");
        var tenantCode = Guid.NewGuid();
        var dataModel = _fixture.CreateTestSessionDataModel(tenantCode: tenantCode);
        await _fixture.InsertSessionDirectlyAsync(dataModel);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateSessionDataModelRepository(unitOfWork);
        var repository = _fixture.CreateSessionPostgreSqlRepository(dataModelRepo);

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
        LogArrange("Inserindo session, recuperando como entity e revogando");
        var tenantCode = Guid.NewGuid();
        var dataModel = _fixture.CreateTestSessionDataModel(tenantCode: tenantCode, entityVersion: 1);
        await _fixture.InsertSessionDirectlyAsync(dataModel);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateSessionDataModelRepository(unitOfWork);
        var repository = _fixture.CreateSessionPostgreSqlRepository(dataModelRepo);

        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);

        var session = await repository.GetByIdAsync(
            executionContext,
            Id.CreateFromExistingInfo(dataModel.Id),
            CancellationToken.None);
        session.ShouldNotBeNull();

        var revokedSession = session.Revoke(
            executionContext,
            new RevokeSessionInput());
        revokedSession.ShouldNotBeNull();

        // Act
        LogAct("Chamando UpdateAsync");
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.UpdateAsync(executionContext, revokedSession, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a atualizacao foi persistida com Status=Revoked e RevokedAt definido");
        result.ShouldBeTrue();

        var persisted = await _fixture.GetSessionDirectlyAsync(dataModel.Id, tenantCode);
        persisted.ShouldNotBeNull();
        persisted.Status.ShouldBe((short)SessionStatus.Revoked);
        persisted.RevokedAt.ShouldNotBeNull();
        persisted.EntityVersion.ShouldNotBe(1L);
        persisted.EntityVersion.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task UpdateAsync_Should_ReturnFalse_WhenNotExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando session sem persistir e tentando update");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);

        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var refreshTokenId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);

        var session = Session.RegisterNew(
            executionContext,
            new RegisterNewSessionInput(userId, refreshTokenId, "TestDevice", "127.0.0.1", "TestAgent/1.0", expiresAt));
        session.ShouldNotBeNull();

        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateSessionDataModelRepository(unitOfWork);
        var repository = _fixture.CreateSessionPostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando UpdateAsync para session inexistente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.UpdateAsync(executionContext, session, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que retorna false");
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task RegisterNewAsync_Should_PersistNullableFieldsCorrectly()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando Session com deviceInfo, ipAddress e userAgent nulos");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);

        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var refreshTokenId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);

        var input = new RegisterNewSessionInput(userId, refreshTokenId, null, null, null, expiresAt);
        var session = Session.RegisterNew(executionContext, input);
        session.ShouldNotBeNull();

        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateSessionDataModelRepository(unitOfWork);
        var repository = _fixture.CreateSessionPostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Persistindo session com campos nullable nulos");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.RegisterNewAsync(executionContext, session, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que os campos nullable foram persistidos como NULL");
        result.ShouldBeTrue();

        var persisted = await _fixture.GetSessionDirectlyAsync(session.EntityInfo.Id.Value, tenantCode);
        persisted.ShouldNotBeNull();
        persisted.DeviceInfo.ShouldBeNull();
        persisted.IpAddress.ShouldBeNull();
        persisted.UserAgent.ShouldBeNull();
        persisted.Status.ShouldBe((short)SessionStatus.Active);
    }
}
