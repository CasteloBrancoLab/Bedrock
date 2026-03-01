using Bedrock.BuildingBlocks.Outbox.Models;
using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using Bedrock.IntegrationTests.BuildingBlocks.Outbox.PostgreSql.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.IntegrationTests.BuildingBlocks.Outbox.PostgreSql.Repositories;

[Collection("OutboxPostgreSql")]
[Feature("Outbox ClaimNextBatch", "Lease pattern com FOR UPDATE SKIP LOCKED")]
public class OutboxRepositoryClaimIntegrationTests : IntegrationTestBase
{
    private readonly OutboxPostgreSqlFixture _fixture;

    public OutboxRepositoryClaimIntegrationTests(
        OutboxPostgreSqlFixture fixture,
        ITestOutputHelper output) : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ClaimNextBatchAsync_ShouldReturnPendingEntries()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["outbox"]);
        LogArrange("Inserting 3 pending entries directly");
        await _fixture.CleanupAsync();
        var entry1 = _fixture.CreateTestEntry();
        var entry2 = _fixture.CreateTestEntry();
        var entry3 = _fixture.CreateTestEntry();
        await _fixture.InsertEntryDirectlyAsync(entry1);
        await _fixture.InsertEntryDirectlyAsync(entry2);
        await _fixture.InsertEntryDirectlyAsync(entry3);

        var executionContext = _fixture.CreateExecutionContext();
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        // Act
        LogAct("Claiming batch of 2");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var claimed = await repository.ClaimNextBatchAsync(2, TimeSpan.FromMinutes(5), CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying 2 entries claimed with Processing status and lease");
        claimed.Count.ShouldBe(2);
        foreach (var entry in claimed)
        {
            entry.Status.ShouldBe(OutboxEntryStatus.Processing);
            entry.IsProcessing.ShouldBeTrue();
            entry.ProcessingExpiration.ShouldNotBeNull();
        }
    }

    [Fact]
    public async Task ClaimNextBatchAsync_WhenNoEntries_ShouldReturnEmpty()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["outbox"]);
        LogArrange("Cleaning up all entries");
        await _fixture.CleanupAsync();

        var executionContext = _fixture.CreateExecutionContext();
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        // Act
        LogAct("Claiming batch from empty table");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var claimed = await repository.ClaimNextBatchAsync(10, TimeSpan.FromMinutes(5), CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying empty result");
        claimed.ShouldBeEmpty();
    }

    [Fact]
    public async Task ClaimNextBatchAsync_ShouldNotClaimAlreadyProcessingEntries()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["outbox"]);
        LogArrange("Inserting entry with active lease");
        await _fixture.CleanupAsync();
        var processingEntry = _fixture.CreateTestEntry(
            status: OutboxEntryStatus.Processing,
            isProcessing: true,
            processingExpiration: DateTimeOffset.UtcNow.AddMinutes(10));
        await _fixture.InsertEntryDirectlyAsync(processingEntry);

        var executionContext = _fixture.CreateExecutionContext();
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        // Act
        LogAct("Claiming batch — active lease should be skipped");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var claimed = await repository.ClaimNextBatchAsync(10, TimeSpan.FromMinutes(5), CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying no entries claimed");
        claimed.ShouldBeEmpty();
    }

    [Fact]
    public async Task ClaimNextBatchAsync_ShouldClaimExpiredLeaseEntries()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["outbox"]);
        LogArrange("Inserting entry with expired lease");
        await _fixture.CleanupAsync();
        var expiredEntry = _fixture.CreateTestEntry(
            status: OutboxEntryStatus.Processing,
            isProcessing: true,
            processingExpiration: DateTimeOffset.UtcNow.AddMinutes(-1));
        await _fixture.InsertEntryDirectlyAsync(expiredEntry);

        var executionContext = _fixture.CreateExecutionContext();
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        // Act
        LogAct("Claiming batch — expired lease should be re-claimed");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var claimed = await repository.ClaimNextBatchAsync(10, TimeSpan.FromMinutes(5), CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying expired entry was re-claimed");
        claimed.Count.ShouldBe(1);
        claimed[0].Id.ShouldBe(expiredEntry.Id);
        claimed[0].Status.ShouldBe(OutboxEntryStatus.Processing);
    }

    [Fact]
    public async Task ClaimNextBatchAsync_ShouldClaimFailedEntriesForRetry()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["outbox"]);
        LogArrange("Inserting failed entry eligible for retry");
        await _fixture.CleanupAsync();
        var failedEntry = _fixture.CreateTestEntry(
            status: OutboxEntryStatus.Failed,
            retryCount: 2);
        await _fixture.InsertEntryDirectlyAsync(failedEntry);

        var executionContext = _fixture.CreateExecutionContext();
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        // Act
        LogAct("Claiming batch — failed entry should be picked up");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var claimed = await repository.ClaimNextBatchAsync(10, TimeSpan.FromMinutes(5), CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying failed entry was claimed");
        claimed.Count.ShouldBe(1);
        claimed[0].Id.ShouldBe(failedEntry.Id);
    }

    [Fact]
    public async Task ClaimNextBatchAsync_ShouldRespectBatchSizeLimit()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["outbox"]);
        LogArrange("Inserting 5 pending entries");
        await _fixture.CleanupAsync();
        for (int i = 0; i < 5; i++)
            await _fixture.InsertEntryDirectlyAsync(_fixture.CreateTestEntry());

        var executionContext = _fixture.CreateExecutionContext();
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        // Act
        LogAct("Claiming batch of 3 from 5 available");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var claimed = await repository.ClaimNextBatchAsync(3, TimeSpan.FromMinutes(5), CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying exactly 3 entries claimed");
        claimed.Count.ShouldBe(3);
    }
}
