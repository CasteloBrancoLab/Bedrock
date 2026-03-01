using Bedrock.BuildingBlocks.Outbox.Models;
using Bedrock.BuildingBlocks.Outbox.PostgreSql;
using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using Bedrock.IntegrationTests.BuildingBlocks.Outbox.PostgreSql.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.IntegrationTests.BuildingBlocks.Outbox.PostgreSql.Repositories;

[Collection("OutboxPostgreSql")]
[Feature("Outbox Lifecycle", "MarkAsSent, MarkAsFailed e transicao para Dead")]
public class OutboxRepositoryLifecycleIntegrationTests : IntegrationTestBase
{
    private readonly OutboxPostgreSqlFixture _fixture;

    public OutboxRepositoryLifecycleIntegrationTests(
        OutboxPostgreSqlFixture fixture,
        ITestOutputHelper output) : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task MarkAsSentAsync_ShouldTransitionToSentAndClearLease()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["outbox"]);
        LogArrange("Inserting a processing entry");
        await _fixture.CleanupAsync();
        var entry = _fixture.CreateTestEntry(
            status: OutboxEntryStatus.Processing,
            isProcessing: true,
            processingExpiration: DateTimeOffset.UtcNow.AddMinutes(5));
        await _fixture.InsertEntryDirectlyAsync(entry);

        var executionContext = _fixture.CreateExecutionContext();
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        // Act
        LogAct("Marking entry as sent");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        await repository.MarkAsSentAsync(entry.Id, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying entry is Sent with processed_at and cleared lease");
        var persisted = await _fixture.GetEntryDirectlyAsync(entry.Id);
        persisted.ShouldNotBeNull();
        persisted.Status.ShouldBe(OutboxEntryStatus.Sent);
        persisted.ProcessedAt.ShouldNotBeNull();
        persisted.IsProcessing.ShouldBeFalse();
        persisted.ProcessingExpiration.ShouldBeNull();
    }

    [Fact]
    public async Task MarkAsFailedAsync_ShouldTransitionToFailedAndIncrementRetry()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["outbox"]);
        LogArrange("Inserting a processing entry with retry_count=0");
        await _fixture.CleanupAsync();
        var entry = _fixture.CreateTestEntry(
            status: OutboxEntryStatus.Processing,
            isProcessing: true,
            processingExpiration: DateTimeOffset.UtcNow.AddMinutes(5));
        await _fixture.InsertEntryDirectlyAsync(entry);

        var executionContext = _fixture.CreateExecutionContext();
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        // Act
        LogAct("Marking entry as failed");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        await repository.MarkAsFailedAsync(entry.Id, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying status=Failed, retry_count incremented, lease cleared");
        var persisted = await _fixture.GetEntryDirectlyAsync(entry.Id);
        persisted.ShouldNotBeNull();
        persisted.Status.ShouldBe(OutboxEntryStatus.Failed);
        persisted.RetryCount.ShouldBe((byte)1);
        persisted.IsProcessing.ShouldBeFalse();
        persisted.ProcessingExpiration.ShouldBeNull();
    }

    [Fact]
    public async Task MarkAsFailedAsync_WhenMaxRetriesReached_ShouldTransitionToDead()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["outbox"]);
        LogArrange("Inserting entry at retry_count=4 with MaxRetries=5");
        await _fixture.CleanupAsync();
        var entry = _fixture.CreateTestEntry(
            status: OutboxEntryStatus.Processing,
            retryCount: 4,
            isProcessing: true,
            processingExpiration: DateTimeOffset.UtcNow.AddMinutes(5));
        await _fixture.InsertEntryDirectlyAsync(entry);

        var executionContext = _fixture.CreateExecutionContext();
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork,
            options => options.WithMaxRetries(5));

        // Act
        LogAct("Marking entry as failed — should transition to Dead");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        await repository.MarkAsFailedAsync(entry.Id, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying status=Dead and retry_count=5");
        var persisted = await _fixture.GetEntryDirectlyAsync(entry.Id);
        persisted.ShouldNotBeNull();
        persisted.Status.ShouldBe(OutboxEntryStatus.Dead);
        persisted.RetryCount.ShouldBe((byte)5);
        persisted.IsProcessing.ShouldBeFalse();
    }

    [Fact]
    public async Task FullLifecycle_Pending_Claim_Sent()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["outbox"]);
        LogArrange("Setting up full lifecycle: Pending → Processing → Sent");
        await _fixture.CleanupAsync();
        var entry = _fixture.CreateTestEntry();
        await _fixture.InsertEntryDirectlyAsync(entry);

        var executionContext = _fixture.CreateExecutionContext();

        // Act — Claim
        LogAct("Step 1: Claiming the entry");
        await using var claimUow = _fixture.CreateAppUserUnitOfWork();
        var claimRepo = _fixture.CreateRepository(claimUow);
        await claimUow.OpenConnectionAsync(executionContext, CancellationToken.None);
        await claimUow.BeginTransactionAsync(executionContext, CancellationToken.None);
        var claimed = await claimRepo.ClaimNextBatchAsync(1, TimeSpan.FromMinutes(5), CancellationToken.None);
        await claimUow.CommitAsync(executionContext, CancellationToken.None);

        claimed.Count.ShouldBe(1);
        var claimedId = claimed[0].Id;

        // Act — MarkAsSent
        LogAct("Step 2: Marking as sent");
        await using var sentUow = _fixture.CreateAppUserUnitOfWork();
        var sentRepo = _fixture.CreateRepository(sentUow);
        await sentUow.OpenConnectionAsync(executionContext, CancellationToken.None);
        await sentUow.BeginTransactionAsync(executionContext, CancellationToken.None);
        await sentRepo.MarkAsSentAsync(claimedId, CancellationToken.None);
        await sentUow.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying full lifecycle completed: Sent with processed_at");
        var persisted = await _fixture.GetEntryDirectlyAsync(claimedId);
        persisted.ShouldNotBeNull();
        persisted.Status.ShouldBe(OutboxEntryStatus.Sent);
        persisted.ProcessedAt.ShouldNotBeNull();
        persisted.IsProcessing.ShouldBeFalse();
    }

    [Fact]
    public async Task FullLifecycle_Pending_Claim_Fail_Retry_Sent()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["outbox"]);
        LogArrange("Setting up lifecycle with retry: Pending → Processing → Failed → Processing → Sent");
        await _fixture.CleanupAsync();
        var entry = _fixture.CreateTestEntry();
        await _fixture.InsertEntryDirectlyAsync(entry);

        var executionContext = _fixture.CreateExecutionContext();

        // Step 1: Claim
        LogAct("Step 1: Claiming");
        await using var uow1 = _fixture.CreateAppUserUnitOfWork();
        var repo1 = _fixture.CreateRepository(uow1);
        await uow1.OpenConnectionAsync(executionContext, CancellationToken.None);
        await uow1.BeginTransactionAsync(executionContext, CancellationToken.None);
        var claimed = await repo1.ClaimNextBatchAsync(1, TimeSpan.FromMinutes(5), CancellationToken.None);
        await uow1.CommitAsync(executionContext, CancellationToken.None);
        var entryId = claimed[0].Id;

        // Step 2: Fail
        LogAct("Step 2: Marking as failed");
        await using var uow2 = _fixture.CreateAppUserUnitOfWork();
        var repo2 = _fixture.CreateRepository(uow2);
        await uow2.OpenConnectionAsync(executionContext, CancellationToken.None);
        await uow2.BeginTransactionAsync(executionContext, CancellationToken.None);
        await repo2.MarkAsFailedAsync(entryId, CancellationToken.None);
        await uow2.CommitAsync(executionContext, CancellationToken.None);

        // Step 3: Re-claim (Failed entries are eligible)
        LogAct("Step 3: Re-claiming failed entry");
        await using var uow3 = _fixture.CreateAppUserUnitOfWork();
        var repo3 = _fixture.CreateRepository(uow3);
        await uow3.OpenConnectionAsync(executionContext, CancellationToken.None);
        await uow3.BeginTransactionAsync(executionContext, CancellationToken.None);
        var reClaimed = await repo3.ClaimNextBatchAsync(1, TimeSpan.FromMinutes(5), CancellationToken.None);
        await uow3.CommitAsync(executionContext, CancellationToken.None);
        reClaimed.Count.ShouldBe(1);

        // Step 4: Send
        LogAct("Step 4: Marking as sent");
        await using var uow4 = _fixture.CreateAppUserUnitOfWork();
        var repo4 = _fixture.CreateRepository(uow4);
        await uow4.OpenConnectionAsync(executionContext, CancellationToken.None);
        await uow4.BeginTransactionAsync(executionContext, CancellationToken.None);
        await repo4.MarkAsSentAsync(entryId, CancellationToken.None);
        await uow4.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying Sent with retry_count=1");
        var persisted = await _fixture.GetEntryDirectlyAsync(entryId);
        persisted.ShouldNotBeNull();
        persisted.Status.ShouldBe(OutboxEntryStatus.Sent);
        persisted.RetryCount.ShouldBe((byte)1);
        persisted.ProcessedAt.ShouldNotBeNull();
    }
}
