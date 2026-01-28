using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.DataModels;
using Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.Repositories;

/// <summary>
/// Integration tests for the handler pattern used in enumeration methods.
/// </summary>
[Collection("PostgresRepository")]
[Feature("Handler Pattern", "Padrão de handler para enumeração de entidades")]
public class HandlerPatternIntegrationTests : IntegrationTestBase
{
    private readonly PostgresRepositoryFixture _fixture;

    public HandlerPatternIntegrationTests(
        PostgresRepositoryFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task EnumerateAllAsync_Should_CallHandler_ForEachEntity()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Creating 3 test entities");
        var tenantCode = Guid.NewGuid();
        var entities = new List<TestEntityDataModel>
        {
            _fixture.CreateTestEntity(tenantCode: tenantCode, name: "Entity1"),
            _fixture.CreateTestEntity(tenantCode: tenantCode, name: "Entity2"),
            _fixture.CreateTestEntity(tenantCode: tenantCode, name: "Entity3")
        };

        foreach (var entity in entities)
        {
            await _fixture.InsertTestEntityDirectlyAsync(entity);
        }

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        var handlerCallCount = 0;
        var enumeratedIds = new List<Guid>();

        // Act
        LogAct("Enumerating all entities");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.EnumerateAllAsync(
            executionContext,
            PaginationInfo.Create(page: 1, pageSize: 100),
            (entity, ct) =>
            {
                handlerCallCount++;
                enumeratedIds.Add(entity.Id);
                return Task.FromResult(true);
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verifying handler was called for each entity");
        result.ShouldBeTrue();
        handlerCallCount.ShouldBe(3);
        foreach (var entity in entities)
        {
            enumeratedIds.ShouldContain(entity.Id);
        }
        LogInfo($"Handler called {handlerCallCount} times for {entities.Count} entities");
    }

    [Fact]
    public async Task EnumerateAllAsync_Should_StopIteration_WhenHandlerReturnsFalse()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Creating 5 test entities");
        var tenantCode = Guid.NewGuid();
        for (int i = 0; i < 5; i++)
        {
            var entity = _fixture.CreateTestEntity(tenantCode: tenantCode, name: $"Entity{i}");
            await _fixture.InsertTestEntityDirectlyAsync(entity);
        }

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        var handlerCallCount = 0;

        // Act
        LogAct("Enumerating with handler that stops after 2 entities");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.EnumerateAllAsync(
            executionContext,
            PaginationInfo.Create(page: 1, pageSize: 100),
            (entity, ct) =>
            {
                handlerCallCount++;
                return Task.FromResult(handlerCallCount < 2); // Stop after 2nd call
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verifying iteration stopped after handler returned false");
        result.ShouldBeTrue();
        handlerCallCount.ShouldBe(2); // Handler called exactly 2 times
        LogInfo($"Iteration correctly stopped after {handlerCallCount} calls");
    }

    [Fact]
    public async Task EnumerateAllAsync_Should_ApplyPagination()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Creating 10 test entities");
        var tenantCode = Guid.NewGuid();
        for (int i = 0; i < 10; i++)
        {
            var entity = _fixture.CreateTestEntity(tenantCode: tenantCode, name: $"Entity{i:D2}");
            await _fixture.InsertTestEntityDirectlyAsync(entity);
        }

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        var page1Entities = new List<Guid>();
        var page2Entities = new List<Guid>();

        // Act
        LogAct("Enumerating page 1 (3 items)");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await repository.EnumerateAllAsync(
            executionContext,
            PaginationInfo.Create(page: 1, pageSize: 3),
            (entity, ct) =>
            {
                page1Entities.Add(entity.Id);
                return Task.FromResult(true);
            },
            CancellationToken.None);

        LogAct("Enumerating page 2 (3 items)");
        await repository.EnumerateAllAsync(
            executionContext,
            PaginationInfo.Create(page: 2, pageSize: 3),
            (entity, ct) =>
            {
                page2Entities.Add(entity.Id);
                return Task.FromResult(true);
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verifying pagination applied correctly");
        page1Entities.Count.ShouldBe(3);
        page2Entities.Count.ShouldBe(3);

        // Pages should have different entities
        foreach (var id in page1Entities)
        {
            page2Entities.ShouldNotContain(id);
        }
        LogInfo("Pagination applied correctly: Page 1 and Page 2 have different entities");
    }

    [Fact]
    public async Task EnumerateAllAsync_Should_ReturnTrue_OnEmptyResult()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Setting up context with no entities");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        var handlerCallCount = 0;

        // Act
        LogAct("Enumerating entities for empty tenant");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.EnumerateAllAsync(
            executionContext,
            PaginationInfo.Create(page: 1, pageSize: 100),
            (entity, ct) =>
            {
                handlerCallCount++;
                return Task.FromResult(true);
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verifying returns true with zero handler calls");
        result.ShouldBeTrue();
        handlerCallCount.ShouldBe(0);
        executionContext.HasExceptions.ShouldBeFalse();
        LogInfo("Empty enumeration handled correctly");
    }

    [Fact]
    public async Task EnumerateModifiedSinceAsync_Should_FilterByTimestamp()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Creating entities with different LastChangedAt timestamps");
        var tenantCode = Guid.NewGuid();
        var baseTime = DateTimeOffset.UtcNow;

        var oldEntity = _fixture.CreateTestEntity(tenantCode: tenantCode, name: "OldEntity");
        oldEntity.LastChangedAt = baseTime.AddDays(-10);
        oldEntity.LastChangedBy = "modifier";

        var recentEntity = _fixture.CreateTestEntity(tenantCode: tenantCode, name: "RecentEntity");
        recentEntity.LastChangedAt = baseTime.AddDays(-1);
        recentEntity.LastChangedBy = "modifier";

        var veryRecentEntity = _fixture.CreateTestEntity(tenantCode: tenantCode, name: "VeryRecentEntity");
        veryRecentEntity.LastChangedAt = baseTime;
        veryRecentEntity.LastChangedBy = "modifier";

        await _fixture.InsertTestEntityDirectlyAsync(oldEntity);
        await _fixture.InsertTestEntityDirectlyAsync(recentEntity);
        await _fixture.InsertTestEntityDirectlyAsync(veryRecentEntity);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        var enumeratedEntities = new List<string>();

        // Act
        LogAct("Enumerating entities modified since 5 days ago");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.EnumerateModifiedSinceAsync(
            executionContext,
            since: baseTime.AddDays(-5),
            (entity, ct) =>
            {
                enumeratedEntities.Add(entity.Name);
                return Task.FromResult(true);
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verifying only recent entities were enumerated");
        result.ShouldBeTrue();
        enumeratedEntities.Count.ShouldBe(2);
        enumeratedEntities.ShouldContain("RecentEntity");
        enumeratedEntities.ShouldContain("VeryRecentEntity");
        enumeratedEntities.ShouldNotContain("OldEntity");
        LogInfo("Timestamp filter applied correctly");
    }

    [Fact]
    public async Task EnumerateModifiedSinceAsync_Should_OrderByLastChangedAt()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Creating entities with specific LastChangedAt order");
        var tenantCode = Guid.NewGuid();
        var baseTime = DateTimeOffset.UtcNow;

        var entity1 = _fixture.CreateTestEntity(tenantCode: tenantCode, name: "Entity1");
        entity1.LastChangedAt = baseTime.AddHours(-3);
        entity1.LastChangedBy = "modifier";

        var entity2 = _fixture.CreateTestEntity(tenantCode: tenantCode, name: "Entity2");
        entity2.LastChangedAt = baseTime.AddHours(-1);
        entity2.LastChangedBy = "modifier";

        var entity3 = _fixture.CreateTestEntity(tenantCode: tenantCode, name: "Entity3");
        entity3.LastChangedAt = baseTime.AddHours(-2);
        entity3.LastChangedBy = "modifier";

        // Insert in random order
        await _fixture.InsertTestEntityDirectlyAsync(entity2);
        await _fixture.InsertTestEntityDirectlyAsync(entity1);
        await _fixture.InsertTestEntityDirectlyAsync(entity3);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        var enumeratedNames = new List<string>();

        // Act
        LogAct("Enumerating entities modified since 4 hours ago");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.EnumerateModifiedSinceAsync(
            executionContext,
            since: baseTime.AddHours(-4),
            (entity, ct) =>
            {
                enumeratedNames.Add(entity.Name);
                return Task.FromResult(true);
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verifying entities are in ascending order by LastChangedAt");
        result.ShouldBeTrue();
        enumeratedNames.Count.ShouldBe(3);
        enumeratedNames[0].ShouldBe("Entity1"); // -3 hours
        enumeratedNames[1].ShouldBe("Entity3"); // -2 hours
        enumeratedNames[2].ShouldBe("Entity2"); // -1 hour
        LogInfo("Entities enumerated in correct order by LastChangedAt");
    }

    [Fact]
    public async Task EnumerateModifiedSinceAsync_Should_StopIteration_WhenHandlerReturnsFalse()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Creating 5 entities with recent LastChangedAt");
        var tenantCode = Guid.NewGuid();
        var baseTime = DateTimeOffset.UtcNow;

        for (int i = 0; i < 5; i++)
        {
            var entity = _fixture.CreateTestEntity(tenantCode: tenantCode, name: $"Entity{i}");
            entity.LastChangedAt = baseTime.AddMinutes(-i);
            entity.LastChangedBy = "modifier";
            await _fixture.InsertTestEntityDirectlyAsync(entity);
        }

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        var handlerCallCount = 0;

        // Act
        LogAct("Enumerating with handler that stops after 3 entities");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.EnumerateModifiedSinceAsync(
            executionContext,
            since: baseTime.AddHours(-1),
            (entity, ct) =>
            {
                handlerCallCount++;
                return Task.FromResult(handlerCallCount < 3);
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verifying iteration stopped after handler returned false");
        result.ShouldBeTrue();
        handlerCallCount.ShouldBe(3);
        LogInfo($"Iteration correctly stopped after {handlerCallCount} calls");
    }

    [Fact]
    public async Task EnumerateAllAsync_Should_FilterByTenantCode()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Creating entities for different tenants");
        var tenantCodeA = Guid.NewGuid();
        var tenantCodeB = Guid.NewGuid();

        var entityA = _fixture.CreateTestEntity(tenantCode: tenantCodeA, name: "TenantAEntity");
        var entityB = _fixture.CreateTestEntity(tenantCode: tenantCodeB, name: "TenantBEntity");

        await _fixture.InsertTestEntityDirectlyAsync(entityA);
        await _fixture.InsertTestEntityDirectlyAsync(entityB);

        var executionContextA = _fixture.CreateExecutionContext(tenantCodeA);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        var enumeratedNames = new List<string>();

        // Act
        LogAct("Tenant A enumerates entities");
        await unitOfWork.OpenConnectionAsync(executionContextA, CancellationToken.None);
        var result = await repository.EnumerateAllAsync(
            executionContextA,
            PaginationInfo.Create(page: 1, pageSize: 100),
            (entity, ct) =>
            {
                enumeratedNames.Add(entity.Name);
                return Task.FromResult(true);
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verifying only Tenant A's entities were enumerated");
        result.ShouldBeTrue();
        enumeratedNames.Count.ShouldBe(1);
        enumeratedNames.ShouldContain("TenantAEntity");
        enumeratedNames.ShouldNotContain("TenantBEntity");
        LogInfo("TenantCode filter applied correctly in enumeration");
    }
}
