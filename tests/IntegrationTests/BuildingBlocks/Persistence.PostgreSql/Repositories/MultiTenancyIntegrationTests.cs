using Bedrock.BuildingBlocks.Testing.Integration;
using Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.Repositories;

/// <summary>
/// Integration tests for multi-tenancy isolation via TenantCode filtering.
/// </summary>
[Collection("PostgresRepository")]
public class MultiTenancyIntegrationTests : IntegrationTestBase
{
    private readonly PostgresRepositoryFixture _fixture;

    public MultiTenancyIntegrationTests(
        PostgresRepositoryFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetByIdAsync_TenantA_CannotSeeTenantB_Entities()
    {
        // Arrange
        LogArrange("Creating entity for Tenant B");
        var tenantCodeA = Guid.NewGuid();
        var tenantCodeB = Guid.NewGuid();
        var entityB = _fixture.CreateTestEntity(tenantCode: tenantCodeB, name: "TenantBEntity");
        await _fixture.InsertTestEntityDirectlyAsync(entityB);

        var executionContextA = _fixture.CreateExecutionContext(tenantCodeA);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        // Act
        LogAct("Tenant A tries to read Tenant B's entity by ID");
        await unitOfWork.OpenConnectionAsync(executionContextA, CancellationToken.None);
        var result = await repository.GetByIdAsync(executionContextA, entityB.Id, CancellationToken.None);

        // Assert
        LogAssert("Verifying Tenant A cannot see Tenant B's entity");
        result.ShouldBeNull();
        executionContextA.HasExceptions.ShouldBeFalse();
        LogInfo("Multi-tenancy isolation verified: Tenant A cannot see Tenant B's entity");
    }

    [Fact]
    public async Task ExistsAsync_TenantA_CannotSeeTenantB_Entities()
    {
        // Arrange
        LogArrange("Creating entity for Tenant B");
        var tenantCodeA = Guid.NewGuid();
        var tenantCodeB = Guid.NewGuid();
        var entityB = _fixture.CreateTestEntity(tenantCode: tenantCodeB);
        await _fixture.InsertTestEntityDirectlyAsync(entityB);

        var executionContextA = _fixture.CreateExecutionContext(tenantCodeA);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        // Act
        LogAct("Tenant A checks if Tenant B's entity exists");
        await unitOfWork.OpenConnectionAsync(executionContextA, CancellationToken.None);
        var result = await repository.ExistsAsync(executionContextA, entityB.Id, CancellationToken.None);

        // Assert
        LogAssert("Verifying Tenant A sees entity as non-existent");
        result.ShouldBeFalse();
        executionContextA.HasExceptions.ShouldBeFalse();
        LogInfo("Multi-tenancy isolation verified: ExistsAsync returns false for other tenant's entity");
    }

    [Fact]
    public async Task EnumerateAllAsync_OnlyReturnsCurrentTenantEntities()
    {
        // Arrange
        LogArrange("Creating entities for Tenant A and Tenant B");
        var tenantCodeA = Guid.NewGuid();
        var tenantCodeB = Guid.NewGuid();

        var entityA1 = _fixture.CreateTestEntity(tenantCode: tenantCodeA, name: "TenantA_Entity1");
        var entityA2 = _fixture.CreateTestEntity(tenantCode: tenantCodeA, name: "TenantA_Entity2");
        var entityB1 = _fixture.CreateTestEntity(tenantCode: tenantCodeB, name: "TenantB_Entity1");

        await _fixture.InsertTestEntityDirectlyAsync(entityA1);
        await _fixture.InsertTestEntityDirectlyAsync(entityA2);
        await _fixture.InsertTestEntityDirectlyAsync(entityB1);

        var executionContextA = _fixture.CreateExecutionContext(tenantCodeA);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        var enumeratedEntities = new List<Guid>();

        // Act
        LogAct("Tenant A enumerates all entities");
        await unitOfWork.OpenConnectionAsync(executionContextA, CancellationToken.None);
        var result = await repository.EnumerateAllAsync(
            executionContextA,
            Bedrock.BuildingBlocks.Core.Paginations.PaginationInfo.Create(page: 1, pageSize: 100),
            (entity, ct) =>
            {
                enumeratedEntities.Add(entity.Id);
                return Task.FromResult(true);
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verifying only Tenant A's entities were enumerated");
        result.ShouldBeTrue();
        enumeratedEntities.Count.ShouldBe(2);
        enumeratedEntities.ShouldContain(entityA1.Id);
        enumeratedEntities.ShouldContain(entityA2.Id);
        enumeratedEntities.ShouldNotContain(entityB1.Id);
        LogInfo("Multi-tenancy isolation verified: EnumerateAllAsync only returns current tenant's entities");
    }

    [Fact]
    public async Task UpdateAsync_TenantA_CannotModifyTenantB_Entities()
    {
        // Arrange
        LogArrange("Creating entity for Tenant B");
        var tenantCodeA = Guid.NewGuid();
        var tenantCodeB = Guid.NewGuid();
        var entityB = _fixture.CreateTestEntity(tenantCode: tenantCodeB, name: "OriginalName", entityVersion: 1);
        await _fixture.InsertTestEntityDirectlyAsync(entityB);

        var executionContextA = _fixture.CreateExecutionContext(tenantCodeA);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        // Prepare modified entity (trying to update Tenant B's entity as Tenant A)
        var modifiedEntity = _fixture.CreateTestEntity(
            id: entityB.Id,
            tenantCode: tenantCodeA, // Tenant A's context has TenantCodeA
            name: "ModifiedByTenantA",
            entityVersion: 2);
        modifiedEntity.CreatedBy = entityB.CreatedBy;
        modifiedEntity.CreatedAt = entityB.CreatedAt;

        // Act
        LogAct("Tenant A tries to update Tenant B's entity");
        await unitOfWork.OpenConnectionAsync(executionContextA, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContextA, CancellationToken.None);
        var result = await repository.UpdateAsync(executionContextA, modifiedEntity, expectedVersion: 1, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContextA, CancellationToken.None);

        // Assert
        LogAssert("Verifying Tenant A cannot modify Tenant B's entity");
        result.ShouldBeFalse(); // No rows affected because WHERE clause includes TenantCode from ExecutionContext

        var unchangedEntity = await _fixture.GetTestEntityDirectlyAsync(entityB.Id, tenantCodeB);
        unchangedEntity.ShouldNotBeNull();
        unchangedEntity.Name.ShouldBe("OriginalName");
        LogInfo("Multi-tenancy isolation verified: Tenant A cannot modify Tenant B's entity");
    }

    [Fact]
    public async Task DeleteAsync_TenantA_CannotDeleteTenantB_Entities()
    {
        // Arrange
        LogArrange("Creating entity for Tenant B");
        var tenantCodeA = Guid.NewGuid();
        var tenantCodeB = Guid.NewGuid();
        var entityB = _fixture.CreateTestEntity(tenantCode: tenantCodeB, entityVersion: 1);
        await _fixture.InsertTestEntityDirectlyAsync(entityB);

        var executionContextA = _fixture.CreateExecutionContext(tenantCodeA);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        // Act
        LogAct("Tenant A tries to delete Tenant B's entity");
        await unitOfWork.OpenConnectionAsync(executionContextA, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContextA, CancellationToken.None);
        var result = await repository.DeleteAsync(executionContextA, entityB.Id, expectedVersion: 1, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContextA, CancellationToken.None);

        // Assert
        LogAssert("Verifying Tenant A cannot delete Tenant B's entity");
        result.ShouldBeFalse(); // No rows affected because WHERE clause includes TenantCode

        var stillExistsEntity = await _fixture.GetTestEntityDirectlyAsync(entityB.Id, tenantCodeB);
        stillExistsEntity.ShouldNotBeNull();
        LogInfo("Multi-tenancy isolation verified: Tenant A cannot delete Tenant B's entity");
    }

    [Fact]
    public async Task InsertAsync_Should_UseExecutionContextTenantCode()
    {
        // Arrange
        LogArrange("Creating entity and setting ExecutionContext with specific TenantCode");
        var tenantCode = Guid.NewGuid();
        var entity = _fixture.CreateTestEntity(tenantCode: tenantCode);
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        // Act
        LogAct("Inserting entity");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.InsertAsync(executionContext, entity, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying entity was stored with correct TenantCode");
        result.ShouldBeTrue();

        var persistedEntity = await _fixture.GetTestEntityDirectlyAsync(entity.Id, tenantCode);
        persistedEntity.ShouldNotBeNull();
        persistedEntity.TenantCode.ShouldBe(tenantCode);
        LogInfo("Entity stored with correct TenantCode from DataModel");
    }
}
