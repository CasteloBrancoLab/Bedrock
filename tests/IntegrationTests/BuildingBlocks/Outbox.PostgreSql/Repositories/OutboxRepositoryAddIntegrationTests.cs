using Bedrock.BuildingBlocks.Outbox.Models;
using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using Bedrock.IntegrationTests.BuildingBlocks.Outbox.PostgreSql.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.IntegrationTests.BuildingBlocks.Outbox.PostgreSql.Repositories;

[Collection("OutboxPostgreSql")]
[Feature("Outbox AddAsync", "Persiste OutboxEntry na tabela do PostgreSQL")]
public class OutboxRepositoryAddIntegrationTests : IntegrationTestBase
{
    private readonly OutboxPostgreSqlFixture _fixture;

    public OutboxRepositoryAddIntegrationTests(
        OutboxPostgreSqlFixture fixture,
        ITestOutputHelper output) : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AddAsync_ShouldPersistEntry()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["outbox"]);
        LogArrange("Creating outbox entry");
        var entry = _fixture.CreateTestEntry();
        var executionContext = _fixture.CreateExecutionContext();

        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        // Act
        LogAct("Adding entry via repository within transaction");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        await repository.AddAsync(entry, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying entry was persisted");
        var persisted = await _fixture.GetEntryDirectlyAsync(entry.Id);
        persisted.ShouldNotBeNull();
        persisted.Id.ShouldBe(entry.Id);
        persisted.TenantCode.ShouldBe(entry.TenantCode);
        persisted.CorrelationId.ShouldBe(entry.CorrelationId);
        persisted.PayloadType.ShouldBe(entry.PayloadType);
        persisted.ContentType.ShouldBe(entry.ContentType);
        persisted.Payload.ShouldBe(entry.Payload);
        persisted.Status.ShouldBe(OutboxEntryStatus.Pending);
        persisted.RetryCount.ShouldBe((byte)0);
        persisted.IsProcessing.ShouldBeFalse();
        persisted.ProcessingExpiration.ShouldBeNull();
        persisted.ProcessedAt.ShouldBeNull();
    }

    [Fact]
    public async Task AddAsync_ShouldPersistMultipleEntries()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["outbox"]);
        LogArrange("Creating two outbox entries");
        var entry1 = _fixture.CreateTestEntry();
        var entry2 = _fixture.CreateTestEntry();
        var executionContext = _fixture.CreateExecutionContext();

        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        // Act
        LogAct("Adding two entries in the same transaction");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        await repository.AddAsync(entry1, CancellationToken.None);
        await repository.AddAsync(entry2, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying both entries were persisted");
        var persisted1 = await _fixture.GetEntryDirectlyAsync(entry1.Id);
        var persisted2 = await _fixture.GetEntryDirectlyAsync(entry2.Id);
        persisted1.ShouldNotBeNull();
        persisted2.ShouldNotBeNull();
    }

    [Fact]
    public async Task AddAsync_WhenRolledBack_ShouldNotPersist()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["outbox"]);
        LogArrange("Creating entry for rollback test");
        var entry = _fixture.CreateTestEntry();
        var executionContext = _fixture.CreateExecutionContext();

        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        // Act
        LogAct("Adding entry then rolling back transaction");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        await repository.AddAsync(entry, CancellationToken.None);
        await unitOfWork.RollbackAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying entry was NOT persisted after rollback");
        var persisted = await _fixture.GetEntryDirectlyAsync(entry.Id);
        persisted.ShouldBeNull();
    }
}
