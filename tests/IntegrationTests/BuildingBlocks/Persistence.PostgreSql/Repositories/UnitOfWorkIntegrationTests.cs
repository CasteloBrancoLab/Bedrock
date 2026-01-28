using Bedrock.BuildingBlocks.Testing.Integration;
using Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.Repositories;

/// <summary>
/// Integration tests for PostgreSqlUnitOfWorkBase transaction management.
/// </summary>
[Collection("PostgresRepository")]
public class UnitOfWorkIntegrationTests : IntegrationTestBase
{
    private readonly PostgresRepositoryFixture _fixture;

    public UnitOfWorkIntegrationTests(
        PostgresRepositoryFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ExecuteAsync_Should_CommitTransaction_OnSuccess()
    {
        // Arrange
        LogArrange("Setting up UnitOfWork and Repository");
        var tenantCode = Guid.NewGuid();
        var entity = _fixture.CreateTestEntity(tenantCode: tenantCode);
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        // Act
        LogAct("Executing handler that returns true (success)");
        var result = await unitOfWork.ExecuteAsync(
            executionContext,
            entity,
            async (ctx, ent, ct) =>
            {
                var insertResult = await repository.InsertAsync(ctx, ent, ct);
                return insertResult;
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verifying transaction was committed and data persisted");
        result.ShouldBeTrue();
        executionContext.HasExceptions.ShouldBeFalse();

        var persistedEntity = await _fixture.GetTestEntityDirectlyAsync(entity.Id, tenantCode);
        persistedEntity.ShouldNotBeNull();
        persistedEntity.Name.ShouldBe(entity.Name);
        LogInfo("Transaction committed successfully");
    }

    [Fact]
    public async Task ExecuteAsync_Should_RollbackTransaction_OnHandlerFailure()
    {
        // Arrange
        LogArrange("Setting up UnitOfWork and Repository");
        var tenantCode = Guid.NewGuid();
        var entity = _fixture.CreateTestEntity(tenantCode: tenantCode);
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        // Act
        LogAct("Executing handler that returns false (failure)");
        var result = await unitOfWork.ExecuteAsync(
            executionContext,
            entity,
            async (ctx, ent, ct) =>
            {
                await repository.InsertAsync(ctx, ent, ct);
                return false; // Simulate failure
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verifying transaction was rolled back and data not persisted");
        result.ShouldBeFalse();

        var notPersistedEntity = await _fixture.GetTestEntityDirectlyAsync(entity.Id, tenantCode);
        notPersistedEntity.ShouldBeNull();
        LogInfo("Transaction rolled back correctly on handler failure");
    }

    [Fact]
    public async Task ExecuteAsync_Should_RollbackTransaction_OnException()
    {
        // Arrange
        LogArrange("Setting up UnitOfWork and Repository");
        var tenantCode = Guid.NewGuid();
        var entity = _fixture.CreateTestEntity(tenantCode: tenantCode);
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        // Act
        LogAct("Executing handler that throws exception");
        var result = await unitOfWork.ExecuteAsync(
            executionContext,
            entity,
            async (ctx, ent, ct) =>
            {
                await repository.InsertAsync(ctx, ent, ct);
                throw new InvalidOperationException("Test exception");
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verifying transaction was rolled back, exception logged, and data not persisted");
        result.ShouldBeFalse();
        executionContext.HasExceptions.ShouldBeTrue();

        var notPersistedEntity = await _fixture.GetTestEntityDirectlyAsync(entity.Id, tenantCode);
        notPersistedEntity.ShouldBeNull();
        LogInfo("Transaction rolled back correctly on exception");
    }

    [Fact]
    public async Task BeginTransactionAsync_Should_CreateTransaction()
    {
        // Arrange
        LogArrange("Setting up UnitOfWork");
        var executionContext = _fixture.CreateExecutionContext();
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();

        // Act
        LogAct("Opening connection and beginning transaction");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying transaction was created");
        result.ShouldBeTrue();
        unitOfWork.GetCurrentTransaction().ShouldNotBeNull();
        LogInfo("Transaction created successfully");
    }

    [Fact]
    public async Task BeginTransactionAsync_Should_BeIdempotent()
    {
        // Arrange
        LogArrange("Setting up UnitOfWork with open connection");
        var executionContext = _fixture.CreateExecutionContext();
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var firstTransaction = unitOfWork.GetCurrentTransaction();

        // Act
        LogAct("Calling BeginTransactionAsync again");
        var result = await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying second call returns true and same transaction");
        result.ShouldBeTrue();
        unitOfWork.GetCurrentTransaction().ShouldBe(firstTransaction);
        LogInfo("BeginTransactionAsync is idempotent");
    }

    [Fact]
    public async Task CommitAsync_Should_PersistChanges()
    {
        // Arrange
        LogArrange("Setting up UnitOfWork, Repository and inserting entity");
        var tenantCode = Guid.NewGuid();
        var entity = _fixture.CreateTestEntity(tenantCode: tenantCode);
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        await repository.InsertAsync(executionContext, entity, CancellationToken.None);

        // Act
        LogAct("Committing transaction");
        var result = await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying changes were persisted after commit");
        result.ShouldBeTrue();

        var persistedEntity = await _fixture.GetTestEntityDirectlyAsync(entity.Id, tenantCode);
        persistedEntity.ShouldNotBeNull();
        LogInfo("Commit persisted changes successfully");
    }

    [Fact]
    public async Task RollbackAsync_Should_DiscardChanges()
    {
        // Arrange
        LogArrange("Setting up UnitOfWork, Repository and inserting entity");
        var tenantCode = Guid.NewGuid();
        var entity = _fixture.CreateTestEntity(tenantCode: tenantCode);
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        await repository.InsertAsync(executionContext, entity, CancellationToken.None);

        // Act
        LogAct("Rolling back transaction");
        var result = await unitOfWork.RollbackAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying changes were discarded after rollback");
        result.ShouldBeTrue();

        var notPersistedEntity = await _fixture.GetTestEntityDirectlyAsync(entity.Id, tenantCode);
        notPersistedEntity.ShouldBeNull();
        LogInfo("Rollback discarded changes successfully");
    }

    [Fact]
    public async Task CreateNpgsqlCommand_Should_AttachConnectionAndTransaction()
    {
        // Arrange
        LogArrange("Setting up UnitOfWork with connection and transaction");
        var executionContext = _fixture.CreateExecutionContext();
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);

        // Act
        LogAct("Creating NpgsqlCommand");
        using var command = unitOfWork.CreateNpgsqlCommand("SELECT 1");

        // Assert
        LogAssert("Verifying command has connection and transaction attached");
        command.ShouldNotBeNull();
        command.Connection.ShouldBe(unitOfWork.GetCurrentConnection());
        command.Transaction.ShouldBe(unitOfWork.GetCurrentTransaction());
        LogInfo("Command created with correct connection and transaction");
    }

    [Fact]
    public async Task CloseConnectionAsync_Should_CloseAndDisposeTransaction()
    {
        // Arrange
        LogArrange("Setting up UnitOfWork with open connection and transaction");
        var executionContext = _fixture.CreateExecutionContext();
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);

        // Act
        LogAct("Closing connection");
        var result = await unitOfWork.CloseConnectionAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying connection is closed and transaction disposed");
        result.ShouldBeTrue();
        unitOfWork.GetCurrentTransaction().ShouldBeNull();
        unitOfWork.GetCurrentConnection().ShouldBeNull();
        LogInfo("Connection closed and transaction disposed correctly");
    }

    [Fact]
    public async Task DisposeAsync_Should_CleanupResources()
    {
        // Arrange
        LogArrange("Setting up UnitOfWork with open connection");
        var executionContext = _fixture.CreateExecutionContext();
        var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);

        // Act
        LogAct("Disposing UnitOfWork");
        await unitOfWork.DisposeAsync();

        // Assert
        LogAssert("Verifying resources are cleaned up");
        unitOfWork.GetCurrentTransaction().ShouldBeNull();
        unitOfWork.GetCurrentConnection().ShouldBeNull();
        LogInfo("DisposeAsync cleaned up resources correctly");
    }
}
