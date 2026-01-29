using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.Repositories;

/// <summary>
/// Integration tests for optimistic concurrency control via EntityVersion.
/// </summary>
[Collection("PostgresRepository")]
[Feature("Optimistic Concurrency", "Controle de concorrência otimista via EntityVersion")]
public class OptimisticConcurrencyIntegrationTests : IntegrationTestBase
{
    private readonly PostgresRepositoryFixture _fixture;

    public OptimisticConcurrencyIntegrationTests(
        PostgresRepositoryFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task UpdateAsync_Should_Succeed_WithMatchingVersion()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Criando entidade com versão 1");
        var tenantCode = Guid.NewGuid();
        var entity = _fixture.CreateTestEntity(tenantCode: tenantCode, entityVersion: 1);
        await _fixture.InsertTestEntityDirectlyAsync(entity);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        entity.Name = "UpdatedName";
        entity.EntityVersion = 2;

        // Act
        LogAct("Atualizando com versão esperada 1");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.UpdateAsync(executionContext, entity, expectedVersion: 1, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a atualização foi bem-sucedida");
        result.ShouldBeTrue();

        var updatedEntity = await _fixture.GetTestEntityDirectlyAsync(entity.Id, tenantCode);
        updatedEntity.ShouldNotBeNull();
        updatedEntity.Name.ShouldBe("UpdatedName");
        updatedEntity.EntityVersion.ShouldBe(2);
        LogInfo("Update with matching version succeeded");
    }

    [Fact]
    public async Task UpdateAsync_Should_Fail_WithStaleVersion()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Criando entidade com versão 10");
        var tenantCode = Guid.NewGuid();
        var entity = _fixture.CreateTestEntity(tenantCode: tenantCode, entityVersion: 10);
        await _fixture.InsertTestEntityDirectlyAsync(entity);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        var originalName = entity.Name;
        entity.Name = "ShouldNotUpdate";
        entity.EntityVersion = 11;

        // Act
        LogAct("Tentando atualização com versão obsoleta (esperada 5, real é 10)");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.UpdateAsync(executionContext, entity, expectedVersion: 5, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a atualização falhou (nenhuma linha afetada)");
        result.ShouldBeFalse();
        executionContext.HasExceptions.ShouldBeFalse();

        var unchangedEntity = await _fixture.GetTestEntityDirectlyAsync(entity.Id, tenantCode);
        unchangedEntity.ShouldNotBeNull();
        unchangedEntity.Name.ShouldBe(originalName);
        unchangedEntity.EntityVersion.ShouldBe(10);
        LogInfo("Update with stale version correctly failed");
    }

    [Fact]
    public async Task UpdateAsync_ConcurrentUpdates_FirstWinsSecondFails()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Criando entidade com versão 1");
        var tenantCode = Guid.NewGuid();
        var entity = _fixture.CreateTestEntity(tenantCode: tenantCode, entityVersion: 1);
        await _fixture.InsertTestEntityDirectlyAsync(entity);

        var executionContext1 = _fixture.CreateExecutionContext(tenantCode);
        var executionContext2 = _fixture.CreateExecutionContext(tenantCode);

        // Simulate first "user" reading and updating
        await using var unitOfWork1 = _fixture.CreateAppUserUnitOfWork();
        var repository1 = _fixture.CreateRepository(unitOfWork1);

        // Simulate second "user" reading and updating
        await using var unitOfWork2 = _fixture.CreateAppUserUnitOfWork();
        var repository2 = _fixture.CreateRepository(unitOfWork2);

        // Act
        LogAct("Primeiro usuário atualiza a entidade");
        await unitOfWork1.OpenConnectionAsync(executionContext1, CancellationToken.None);
        await unitOfWork1.BeginTransactionAsync(executionContext1, CancellationToken.None);

        var entity1 = _fixture.CreateTestEntity(
            id: entity.Id,
            tenantCode: tenantCode,
            name: "FirstUserUpdate",
            entityVersion: 2);
        entity1.CreatedBy = entity.CreatedBy;
        entity1.CreatedAt = entity.CreatedAt;

        var result1 = await repository1.UpdateAsync(executionContext1, entity1, expectedVersion: 1, CancellationToken.None);
        await unitOfWork1.CommitAsync(executionContext1, CancellationToken.None);

        LogAct("Segundo usuário tenta atualizar com a mesma versão original");
        await unitOfWork2.OpenConnectionAsync(executionContext2, CancellationToken.None);
        await unitOfWork2.BeginTransactionAsync(executionContext2, CancellationToken.None);

        var entity2 = _fixture.CreateTestEntity(
            id: entity.Id,
            tenantCode: tenantCode,
            name: "SecondUserUpdate",
            entityVersion: 2);
        entity2.CreatedBy = entity.CreatedBy;
        entity2.CreatedAt = entity.CreatedAt;

        var result2 = await repository2.UpdateAsync(executionContext2, entity2, expectedVersion: 1, CancellationToken.None);
        await unitOfWork2.CommitAsync(executionContext2, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a primeira atualização foi bem-sucedida e a segunda falhou");
        result1.ShouldBeTrue();
        result2.ShouldBeFalse();

        var finalEntity = await _fixture.GetTestEntityDirectlyAsync(entity.Id, tenantCode);
        finalEntity.ShouldNotBeNull();
        finalEntity.Name.ShouldBe("FirstUserUpdate");
        finalEntity.EntityVersion.ShouldBe(2);
        LogInfo("Concurrent update scenario handled correctly - first wins");
    }

    [Fact]
    public async Task DeleteAsync_Should_Succeed_WithMatchingVersion()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Criando entidade com versão 1");
        var tenantCode = Guid.NewGuid();
        var entity = _fixture.CreateTestEntity(tenantCode: tenantCode, entityVersion: 1);
        await _fixture.InsertTestEntityDirectlyAsync(entity);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        // Act
        LogAct("Excluindo com versão esperada 1");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.DeleteAsync(executionContext, entity.Id, expectedVersion: 1, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a exclusão foi bem-sucedida");
        result.ShouldBeTrue();

        var deletedEntity = await _fixture.GetTestEntityDirectlyAsync(entity.Id, tenantCode);
        deletedEntity.ShouldBeNull();
        LogInfo("Delete with matching version succeeded");
    }

    [Fact]
    public async Task DeleteAsync_Should_Fail_WithStaleVersion()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Criando entidade com versão 5");
        var tenantCode = Guid.NewGuid();
        var entity = _fixture.CreateTestEntity(tenantCode: tenantCode, entityVersion: 5);
        await _fixture.InsertTestEntityDirectlyAsync(entity);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        // Act
        LogAct("Tentando exclusão com versão obsoleta (esperada 1, real é 5)");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.DeleteAsync(executionContext, entity.Id, expectedVersion: 1, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a exclusão falhou (nenhuma linha afetada)");
        result.ShouldBeFalse();
        executionContext.HasExceptions.ShouldBeFalse();

        var stillExistsEntity = await _fixture.GetTestEntityDirectlyAsync(entity.Id, tenantCode);
        stillExistsEntity.ShouldNotBeNull();
        LogInfo("Delete with stale version correctly failed");
    }

    [Fact]
    public async Task DeleteAsync_Should_Fail_WhenConcurrentUpdateOccurs()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Criando entidade com versão 1");
        var tenantCode = Guid.NewGuid();
        var entity = _fixture.CreateTestEntity(tenantCode: tenantCode, entityVersion: 1);
        await _fixture.InsertTestEntityDirectlyAsync(entity);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        // Simulate concurrent update by another process
        LogAct("Outro processo atualiza a entidade para versão 2");
        await _fixture.UpdateEntityVersionDirectlyAsync(entity.Id, tenantCode, 2);

        // Now try to delete with stale version
        LogAct("Tentando exclusão com versão original 1");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.DeleteAsync(executionContext, entity.Id, expectedVersion: 1, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a exclusão falhou devido a atualização concorrente");
        result.ShouldBeFalse();

        var stillExistsEntity = await _fixture.GetTestEntityDirectlyAsync(entity.Id, tenantCode);
        stillExistsEntity.ShouldNotBeNull();
        stillExistsEntity.EntityVersion.ShouldBe(2);
        LogInfo("Delete correctly failed after concurrent update");
    }
}
