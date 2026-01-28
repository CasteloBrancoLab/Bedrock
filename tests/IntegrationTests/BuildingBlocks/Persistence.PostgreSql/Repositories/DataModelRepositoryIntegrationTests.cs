using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.Repositories;

/// <summary>
/// Integration tests for DataModelRepositoryBase CRUD operations.
/// </summary>
[Collection("PostgresRepository")]
[Feature("Repository CRUD", "Operações CRUD do repositório de DataModel")]
public class DataModelRepositoryIntegrationTests : IntegrationTestBase
{
    private readonly PostgresRepositoryFixture _fixture;

    public DataModelRepositoryIntegrationTests(
        PostgresRepositoryFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnEntity_WhenExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Creating test entity and inserting directly");
        var tenantCode = Guid.NewGuid();
        var entity = _fixture.CreateTestEntity(tenantCode: tenantCode);
        await _fixture.InsertTestEntityDirectlyAsync(entity);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        // Act
        LogAct("Opening connection and calling GetByIdAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByIdAsync(executionContext, entity.Id, CancellationToken.None);

        // Assert
        LogAssert("Verifying entity was retrieved correctly");
        result.ShouldNotBeNull();
        result.Id.ShouldBe(entity.Id);
        result.TenantCode.ShouldBe(entity.TenantCode);
        result.Name.ShouldBe(entity.Name);
        result.CreatedBy.ShouldBe(entity.CreatedBy);
        result.EntityVersion.ShouldBe(entity.EntityVersion);
        executionContext.HasExceptions.ShouldBeFalse();
        LogInfo("Entity retrieved successfully");
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnNull_WhenNotExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Setting up context with non-existent entity ID");
        var tenantCode = Guid.NewGuid();
        var nonExistentId = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        // Act
        LogAct("Opening connection and calling GetByIdAsync for non-existent entity");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByIdAsync(executionContext, nonExistentId, CancellationToken.None);

        // Assert
        LogAssert("Verifying null is returned");
        result.ShouldBeNull();
        executionContext.HasExceptions.ShouldBeFalse();
        LogInfo("GetByIdAsync correctly returned null for non-existent entity");
    }

    [Fact]
    public async Task ExistsAsync_Should_ReturnTrue_WhenEntityExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Creating and inserting test entity");
        var tenantCode = Guid.NewGuid();
        var entity = _fixture.CreateTestEntity(tenantCode: tenantCode);
        await _fixture.InsertTestEntityDirectlyAsync(entity);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        // Act
        LogAct("Calling ExistsAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.ExistsAsync(executionContext, entity.Id, CancellationToken.None);

        // Assert
        LogAssert("Verifying entity exists");
        result.ShouldBeTrue();
        executionContext.HasExceptions.ShouldBeFalse();
        LogInfo("ExistsAsync correctly returned true");
    }

    [Fact]
    public async Task ExistsAsync_Should_ReturnFalse_WhenEntityNotExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Setting up context with non-existent entity ID");
        var tenantCode = Guid.NewGuid();
        var nonExistentId = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        // Act
        LogAct("Calling ExistsAsync for non-existent entity");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.ExistsAsync(executionContext, nonExistentId, CancellationToken.None);

        // Assert
        LogAssert("Verifying entity does not exist");
        result.ShouldBeFalse();
        executionContext.HasExceptions.ShouldBeFalse();
        LogInfo("ExistsAsync correctly returned false");
    }

    [Fact]
    public async Task InsertAsync_Should_PersistEntity()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Creating test entity for insertion");
        var tenantCode = Guid.NewGuid();
        var entity = _fixture.CreateTestEntity(tenantCode: tenantCode);
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        // Act
        LogAct("Inserting entity via repository");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.InsertAsync(executionContext, entity, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying entity was persisted");
        result.ShouldBeTrue();
        executionContext.HasExceptions.ShouldBeFalse();

        var persistedEntity = await _fixture.GetTestEntityDirectlyAsync(entity.Id, tenantCode);
        persistedEntity.ShouldNotBeNull();
        persistedEntity.Id.ShouldBe(entity.Id);
        persistedEntity.Name.ShouldBe(entity.Name);
        LogInfo("Entity persisted successfully");
    }

    [Fact]
    public async Task InsertAsync_Should_PopulateAllBaseFields()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Creating test entity with all fields populated");
        var tenantCode = Guid.NewGuid();
        var entity = _fixture.CreateTestEntity(tenantCode: tenantCode);
        entity.LastChangedBy = "test_modifier";
        entity.LastChangedAt = DateTimeOffset.UtcNow;
        entity.LastChangedExecutionOrigin = "TestOrigin";
        entity.LastChangedCorrelationId = Guid.NewGuid();
        entity.LastChangedBusinessOperationCode = "TEST_OP";

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        // Act
        LogAct("Inserting entity with all fields");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.InsertAsync(executionContext, entity, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying all fields were persisted");
        result.ShouldBeTrue();
        var persistedEntity = await _fixture.GetTestEntityDirectlyAsync(entity.Id, tenantCode);
        persistedEntity.ShouldNotBeNull();
        persistedEntity.LastChangedBy.ShouldBe(entity.LastChangedBy);
        persistedEntity.LastChangedExecutionOrigin.ShouldBe(entity.LastChangedExecutionOrigin);
        persistedEntity.LastChangedCorrelationId.ShouldBe(entity.LastChangedCorrelationId);
        persistedEntity.LastChangedBusinessOperationCode.ShouldBe(entity.LastChangedBusinessOperationCode);
        LogInfo("All fields persisted correctly");
    }

    [Fact]
    public async Task InsertAsync_Should_HandleNullableFields()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Creating test entity with null optional fields");
        var tenantCode = Guid.NewGuid();
        var entity = _fixture.CreateTestEntity(tenantCode: tenantCode);
        // Leave optional fields as null

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        // Act
        LogAct("Inserting entity with null optional fields");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.InsertAsync(executionContext, entity, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying nullable fields are stored as null");
        result.ShouldBeTrue();
        var persistedEntity = await _fixture.GetTestEntityDirectlyAsync(entity.Id, tenantCode);
        persistedEntity.ShouldNotBeNull();
        persistedEntity.LastChangedBy.ShouldBeNull();
        persistedEntity.LastChangedAt.ShouldBeNull();
        persistedEntity.LastChangedExecutionOrigin.ShouldBeNull();
        persistedEntity.LastChangedCorrelationId.ShouldBeNull();
        persistedEntity.LastChangedBusinessOperationCode.ShouldBeNull();
        LogInfo("Nullable fields handled correctly");
    }

    [Fact]
    public async Task UpdateAsync_Should_ModifyEntity_WhenVersionMatches()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Creating and inserting test entity");
        var tenantCode = Guid.NewGuid();
        var originalVersion = 1L;
        var entity = _fixture.CreateTestEntity(tenantCode: tenantCode, entityVersion: originalVersion);
        await _fixture.InsertTestEntityDirectlyAsync(entity);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        // Modify the entity - keep same version because the update uses entity_version < new_version
        // The WHERE clause checks: entity_version < DataModel.EntityVersion
        // So we need DataModel.EntityVersion > current DB version
        entity.Name = "UpdatedName";
        entity.EntityVersion = originalVersion + 1; // New version must be > current version

        // Act
        LogAct("Updating entity with matching version");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        // Note: expectedVersion parameter is used in a second WHERE clause (entity_version = expectedVersion)
        // The full WHERE is: tenant_code AND id AND entity_version < new_version AND entity_version = expected_version
        // For this to work: expected_version must equal current DB version AND be < new DataModel version
        var result = await repository.UpdateAsync(executionContext, entity, expectedVersion: originalVersion, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying entity was updated");
        result.ShouldBeTrue();
        executionContext.HasExceptions.ShouldBeFalse();

        var updatedEntity = await _fixture.GetTestEntityDirectlyAsync(entity.Id, tenantCode);
        updatedEntity.ShouldNotBeNull();
        updatedEntity.Name.ShouldBe("UpdatedName");
        updatedEntity.EntityVersion.ShouldBe(2);
        LogInfo("Entity updated successfully");
    }

    [Fact]
    public async Task UpdateAsync_Should_ReturnFalse_WhenVersionMismatch()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Creating and inserting test entity with version 5");
        var tenantCode = Guid.NewGuid();
        var entity = _fixture.CreateTestEntity(tenantCode: tenantCode, entityVersion: 5);
        await _fixture.InsertTestEntityDirectlyAsync(entity);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        // Try to update with wrong expected version
        entity.Name = "ShouldNotUpdate";
        entity.EntityVersion = 6;

        // Act
        LogAct("Attempting update with stale version (expecting 1, actual is 5)");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.UpdateAsync(executionContext, entity, expectedVersion: 1, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying update failed due to version mismatch");
        result.ShouldBeFalse();
        executionContext.HasExceptions.ShouldBeFalse(); // No exception, just returns false

        var unchangedEntity = await _fixture.GetTestEntityDirectlyAsync(entity.Id, tenantCode);
        unchangedEntity.ShouldNotBeNull();
        unchangedEntity.Name.ShouldNotBe("ShouldNotUpdate");
        unchangedEntity.EntityVersion.ShouldBe(5); // Version unchanged
        LogInfo("Update correctly failed due to version mismatch");
    }

    [Fact]
    public async Task DeleteAsync_Should_RemoveEntity_WhenVersionMatches()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Creating and inserting test entity");
        var tenantCode = Guid.NewGuid();
        var entity = _fixture.CreateTestEntity(tenantCode: tenantCode, entityVersion: 1);
        await _fixture.InsertTestEntityDirectlyAsync(entity);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        // Act
        LogAct("Deleting entity with matching version");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.DeleteAsync(executionContext, entity.Id, expectedVersion: 1, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying entity was deleted");
        result.ShouldBeTrue();
        executionContext.HasExceptions.ShouldBeFalse();

        var deletedEntity = await _fixture.GetTestEntityDirectlyAsync(entity.Id, tenantCode);
        deletedEntity.ShouldBeNull();
        LogInfo("Entity deleted successfully");
    }

    [Fact]
    public async Task DeleteAsync_Should_ReturnFalse_WhenVersionMismatch()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Creating and inserting test entity with version 3");
        var tenantCode = Guid.NewGuid();
        var entity = _fixture.CreateTestEntity(tenantCode: tenantCode, entityVersion: 3);
        await _fixture.InsertTestEntityDirectlyAsync(entity);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        // Act
        LogAct("Attempting delete with stale version (expecting 1, actual is 3)");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.DeleteAsync(executionContext, entity.Id, expectedVersion: 1, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying delete failed due to version mismatch");
        result.ShouldBeFalse();
        executionContext.HasExceptions.ShouldBeFalse();

        var stillExistsEntity = await _fixture.GetTestEntityDirectlyAsync(entity.Id, tenantCode);
        stillExistsEntity.ShouldNotBeNull();
        LogInfo("Delete correctly failed due to version mismatch");
    }

    [Fact]
    public async Task DeleteAsync_Should_ReturnFalse_WhenEntityNotExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Setting up context with non-existent entity ID");
        var tenantCode = Guid.NewGuid();
        var nonExistentId = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        // Act
        LogAct("Attempting to delete non-existent entity");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.DeleteAsync(executionContext, nonExistentId, expectedVersion: 1, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying delete returned false for non-existent entity");
        result.ShouldBeFalse();
        executionContext.HasExceptions.ShouldBeFalse();
        LogInfo("Delete correctly returned false for non-existent entity");
    }
}
