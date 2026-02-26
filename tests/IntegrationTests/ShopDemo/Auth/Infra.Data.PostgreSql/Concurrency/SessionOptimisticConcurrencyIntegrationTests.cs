using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using ShopDemo.Auth.Domain.Entities.Sessions;
using ShopDemo.Auth.Domain.Entities.Sessions.Inputs;
using ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Concurrency;

[Collection("AuthPostgreSql")]
[Feature("Session Optimistic Concurrency", "Concorrencia otimista no nivel de entidade")]
public class SessionOptimisticConcurrencyIntegrationTests : IntegrationTestBase
{
    private readonly AuthPostgreSqlFixture _fixture;

    public SessionOptimisticConcurrencyIntegrationTests(
        AuthPostgreSqlFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task UpdateAsync_Should_Succeed_WhenVersionIsCorrect()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo sessao e atualizando com versao correta");
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

        var revoked = session.Revoke(executionContext, new RevokeSessionInput());
        revoked.ShouldNotBeNull();

        // Act
        LogAct("Chamando UpdateAsync com versao correta");
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.UpdateAsync(executionContext, revoked, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o update teve sucesso");
        result.ShouldBeTrue();

        var persisted = await _fixture.GetSessionDirectlyAsync(dataModel.Id, tenantCode);
        persisted.ShouldNotBeNull();
        persisted.Status.ShouldBe((short)2); // Revoked
        persisted.RevokedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_Should_ReturnFalse_WhenDeletedBetweenReadAndUpdate()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo sessao, lendo como entity, e excluindo por fora antes do update");
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

        // Simulate deletion by another process
        await _fixture.DeleteSessionDirectlyAsync(dataModel.Id, tenantCode);

        var revoked = session.Revoke(executionContext, new RevokeSessionInput());
        revoked.ShouldNotBeNull();

        // Act
        LogAct("Chamando UpdateAsync apos exclusao por outro processo");
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.UpdateAsync(executionContext, revoked, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o update retornou false porque o registro nao existe mais");
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task DeleteAsync_Should_Fail_AfterConcurrentUpdate()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo sessao, atualizando versao, e tentando excluir com versao antiga");
        var tenantCode = Guid.NewGuid();
        var dataModel = _fixture.CreateTestSessionDataModel(tenantCode: tenantCode, entityVersion: 1);
        await _fixture.InsertSessionDirectlyAsync(dataModel);

        // Simulate another process updating the entity
        await _fixture.UpdateSessionEntityVersionDirectlyAsync(dataModel.Id, tenantCode, 3);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateSessionDataModelRepository(unitOfWork);

        // Act
        LogAct("Tentando excluir com versao desatualizada");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await dataModelRepo.DeleteAsync(executionContext, dataModel.Id, expectedVersion: 1, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a exclusao falhou por concorrencia");
        result.ShouldBeFalse();

        var stillExists = await _fixture.GetSessionDirectlyAsync(dataModel.Id, tenantCode);
        stillExists.ShouldNotBeNull();
        stillExists.EntityVersion.ShouldBe(3);
    }

    [Fact]
    public async Task UpdateAsync_Should_GenerateNewTimestampVersion()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo sessao com versao 1 e fazendo update");
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

        var revoked = session.Revoke(executionContext, new RevokeSessionInput());
        revoked.ShouldNotBeNull();

        // Act
        LogAct("Executando update e verificando que a versao mudou");
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        await repository.UpdateAsync(executionContext, revoked, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert - RegistryVersion gera versao baseada em timestamp, nao incremental
        LogAssert("Verificando que a versao mudou de 1 para um timestamp-based value");
        var persisted = await _fixture.GetSessionDirectlyAsync(dataModel.Id, tenantCode);
        persisted.ShouldNotBeNull();
        persisted.EntityVersion.ShouldNotBe(1);
        persisted.EntityVersion.ShouldBeGreaterThan(0);
    }
}
