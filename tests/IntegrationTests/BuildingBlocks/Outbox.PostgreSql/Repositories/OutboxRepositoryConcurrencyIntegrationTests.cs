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
[Feature("Outbox Concurrency", "FOR UPDATE SKIP LOCKED, lease expiration e concurrent access")]
public class OutboxRepositoryConcurrencyIntegrationTests : IntegrationTestBase
{
    private readonly OutboxPostgreSqlFixture _fixture;

    public OutboxRepositoryConcurrencyIntegrationTests(
        OutboxPostgreSqlFixture fixture,
        ITestOutputHelper output) : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ConcurrentClaim_TwoWorkers_ShouldNotReturnOverlappingEntries()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["outbox"]);
        LogArrange("Inserting 6 pending entries for two concurrent workers");
        await _fixture.CleanupAsync();
        for (int i = 0; i < 6; i++)
            await _fixture.InsertEntryDirectlyAsync(_fixture.CreateTestEntry());

        var executionContext = _fixture.CreateExecutionContext();

        // Act — Two workers claim concurrently
        LogAct("Two workers claiming batch of 6 simultaneously");
        var worker1Task = Task.Run(async () =>
        {
            await using var uow = _fixture.CreateAppUserUnitOfWork();
            var repo = _fixture.CreateRepository(uow);
            await uow.OpenConnectionAsync(executionContext, CancellationToken.None);
            await uow.BeginTransactionAsync(executionContext, CancellationToken.None);
            var claimed = await repo.ClaimNextBatchAsync(6, TimeSpan.FromMinutes(5), CancellationToken.None);
            await uow.CommitAsync(executionContext, CancellationToken.None);
            return claimed;
        });

        var worker2Task = Task.Run(async () =>
        {
            await using var uow = _fixture.CreateAppUserUnitOfWork();
            var repo = _fixture.CreateRepository(uow);
            await uow.OpenConnectionAsync(executionContext, CancellationToken.None);
            await uow.BeginTransactionAsync(executionContext, CancellationToken.None);
            var claimed = await repo.ClaimNextBatchAsync(6, TimeSpan.FromMinutes(5), CancellationToken.None);
            await uow.CommitAsync(executionContext, CancellationToken.None);
            return claimed;
        });

        var results = await Task.WhenAll(worker1Task, worker2Task);
        var worker1Claimed = results[0];
        var worker2Claimed = results[1];

        // Assert
        LogAssert("Verifying no overlap: total claimed = 6, zero duplicates");
        var allClaimedIds = worker1Claimed.Select(e => e.Id)
            .Concat(worker2Claimed.Select(e => e.Id))
            .ToList();

        allClaimedIds.Count.ShouldBe(6, "Total claimed across both workers should be 6");
        allClaimedIds.Distinct().Count().ShouldBe(6, "No duplicate IDs — zero overlap");
    }

    [Fact]
    public async Task ConcurrentClaim_LimitedEntries_ShouldDistributeWithoutOverlap()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["outbox"]);
        LogArrange("Inserting 3 entries, two workers each requesting 3");
        await _fixture.CleanupAsync();
        for (int i = 0; i < 3; i++)
            await _fixture.InsertEntryDirectlyAsync(_fixture.CreateTestEntry());

        var executionContext = _fixture.CreateExecutionContext();

        // Act
        LogAct("Two workers competing for 3 entries");
        var worker1Task = Task.Run(async () =>
        {
            await using var uow = _fixture.CreateAppUserUnitOfWork();
            var repo = _fixture.CreateRepository(uow);
            await uow.OpenConnectionAsync(executionContext, CancellationToken.None);
            await uow.BeginTransactionAsync(executionContext, CancellationToken.None);
            var claimed = await repo.ClaimNextBatchAsync(3, TimeSpan.FromMinutes(5), CancellationToken.None);
            await uow.CommitAsync(executionContext, CancellationToken.None);
            return claimed;
        });

        var worker2Task = Task.Run(async () =>
        {
            await using var uow = _fixture.CreateAppUserUnitOfWork();
            var repo = _fixture.CreateRepository(uow);
            await uow.OpenConnectionAsync(executionContext, CancellationToken.None);
            await uow.BeginTransactionAsync(executionContext, CancellationToken.None);
            var claimed = await repo.ClaimNextBatchAsync(3, TimeSpan.FromMinutes(5), CancellationToken.None);
            await uow.CommitAsync(executionContext, CancellationToken.None);
            return claimed;
        });

        var results = await Task.WhenAll(worker1Task, worker2Task);

        // Assert
        LogAssert("Verifying sum = 3, no overlap, each worker got a disjoint subset");
        var total = results[0].Count + results[1].Count;
        total.ShouldBe(3, "Total claimed should equal total available entries");

        var allIds = results[0].Select(e => e.Id).Concat(results[1].Select(e => e.Id)).ToList();
        allIds.Distinct().Count().ShouldBe(3, "No duplicate claims");
    }

    [Fact]
    public async Task ConcurrentAddAndClaim_ShouldNotInterfere()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["outbox"]);
        LogArrange("Inserting 2 initial entries, then adding more concurrently with claiming");
        await _fixture.CleanupAsync();
        var initial1 = _fixture.CreateTestEntry();
        var initial2 = _fixture.CreateTestEntry();
        await _fixture.InsertEntryDirectlyAsync(initial1);
        await _fixture.InsertEntryDirectlyAsync(initial2);

        var executionContext = _fixture.CreateExecutionContext();

        // Act — Add and Claim concurrently
        LogAct("Worker A adds 3 entries while Worker B claims");
        var addedEntries = new List<OutboxEntry>();
        var addTask = Task.Run(async () =>
        {
            await using var uow = _fixture.CreateAppUserUnitOfWork();
            var repo = _fixture.CreateRepository(uow);
            await uow.OpenConnectionAsync(executionContext, CancellationToken.None);
            await uow.BeginTransactionAsync(executionContext, CancellationToken.None);
            for (int i = 0; i < 3; i++)
            {
                var entry = _fixture.CreateTestEntry();
                addedEntries.Add(entry);
                await repo.AddAsync(entry, CancellationToken.None);
            }
            await uow.CommitAsync(executionContext, CancellationToken.None);
        });

        var claimTask = Task.Run(async () =>
        {
            await using var uow = _fixture.CreateAppUserUnitOfWork();
            var repo = _fixture.CreateRepository(uow);
            await uow.OpenConnectionAsync(executionContext, CancellationToken.None);
            await uow.BeginTransactionAsync(executionContext, CancellationToken.None);
            var claimed = await repo.ClaimNextBatchAsync(10, TimeSpan.FromMinutes(5), CancellationToken.None);
            await uow.CommitAsync(executionContext, CancellationToken.None);
            return claimed;
        });

        await Task.WhenAll(addTask, claimTask);
        var claimed = await claimTask;

        // Assert
        LogAssert("Verifying claim got at least the 2 initial entries without errors");
        claimed.Count.ShouldBeGreaterThanOrEqualTo(2,
            "Should claim at least the 2 pre-existing entries (may include newly added if committed first)");

        foreach (var entry in claimed)
        {
            entry.Status.ShouldBe(OutboxEntryStatus.Processing);
            entry.IsProcessing.ShouldBeTrue();
        }
    }

    [Fact]
    public async Task LeaseExpiration_AnotherWorkerReclaims_ShouldGetNewLease()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["outbox"]);
        LogArrange("Inserting entry with expired lease from Worker A");
        await _fixture.CleanupAsync();
        var entry = _fixture.CreateTestEntry(
            status: OutboxEntryStatus.Processing,
            isProcessing: true,
            processingExpiration: DateTimeOffset.UtcNow.AddMinutes(-5));
        await _fixture.InsertEntryDirectlyAsync(entry);

        var executionContext = _fixture.CreateExecutionContext();

        // Act — Worker B reclaims the expired entry
        LogAct("Worker B reclaiming expired-lease entry with new 10-minute lease");
        await using var uow = _fixture.CreateAppUserUnitOfWork();
        var repo = _fixture.CreateRepository(uow);
        await uow.OpenConnectionAsync(executionContext, CancellationToken.None);
        await uow.BeginTransactionAsync(executionContext, CancellationToken.None);
        var claimed = await repo.ClaimNextBatchAsync(1, TimeSpan.FromMinutes(10), CancellationToken.None);
        await uow.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying reclaimed entry has new lease, future expiration, and incremented retry_count");
        claimed.Count.ShouldBe(1);
        claimed[0].Id.ShouldBe(entry.Id);
        claimed[0].IsProcessing.ShouldBeTrue();
        claimed[0].ProcessingExpiration.ShouldNotBeNull();
        claimed[0].ProcessingExpiration!.Value.ShouldBeGreaterThan(DateTimeOffset.UtcNow.AddMinutes(5),
            "New lease should expire well in the future");
        claimed[0].RetryCount.ShouldBe((byte)1,
            "Expired lease reclaim should increment retry_count");
    }

    [Fact]
    public async Task LeaseExpiration_OriginalWorkerMarksSent_StillSucceeds()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["outbox"]);
        LogArrange("Setting up: entry claimed by Worker A, lease expires, Worker B reclaims, Worker A tries MarkAsSent");
        await _fixture.CleanupAsync();
        var entry = _fixture.CreateTestEntry(
            status: OutboxEntryStatus.Processing,
            isProcessing: true,
            processingExpiration: DateTimeOffset.UtcNow.AddMinutes(-1));
        await _fixture.InsertEntryDirectlyAsync(entry);

        var executionContext = _fixture.CreateExecutionContext();

        // Worker B reclaims
        LogAct("Worker B reclaims the expired entry");
        await using var uowB = _fixture.CreateAppUserUnitOfWork();
        var repoB = _fixture.CreateRepository(uowB);
        await uowB.OpenConnectionAsync(executionContext, CancellationToken.None);
        await uowB.BeginTransactionAsync(executionContext, CancellationToken.None);
        var reclaimed = await repoB.ClaimNextBatchAsync(1, TimeSpan.FromMinutes(10), CancellationToken.None);
        await uowB.CommitAsync(executionContext, CancellationToken.None);
        reclaimed.Count.ShouldBe(1);

        // Worker A (original) tries to mark as sent — UPDATE has no status guard
        LogAct("Worker A marks as sent after lease expired (no guard on UPDATE)");
        await using var uowA = _fixture.CreateAppUserUnitOfWork();
        var repoA = _fixture.CreateRepository(uowA);
        await uowA.OpenConnectionAsync(executionContext, CancellationToken.None);
        await uowA.BeginTransactionAsync(executionContext, CancellationToken.None);
        await repoA.MarkAsSentAsync(entry.Id, CancellationToken.None);
        await uowA.CommitAsync(executionContext, CancellationToken.None);

        // Assert — entry is Sent (last writer wins)
        LogAssert("Verifying entry ended as Sent (last-writer-wins behavior)");
        var persisted = await _fixture.GetEntryDirectlyAsync(entry.Id);
        persisted.ShouldNotBeNull();
        persisted.Status.ShouldBe(OutboxEntryStatus.Sent);
        persisted.IsProcessing.ShouldBeFalse();
        persisted.ProcessedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task ActiveLease_ShouldNotBeReclaimable()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["outbox"]);
        LogArrange("Inserting entry with active lease (future expiration)");
        await _fixture.CleanupAsync();
        var entry = _fixture.CreateTestEntry(
            status: OutboxEntryStatus.Processing,
            isProcessing: true,
            processingExpiration: DateTimeOffset.UtcNow.AddMinutes(30));
        await _fixture.InsertEntryDirectlyAsync(entry);

        var executionContext = _fixture.CreateExecutionContext();

        // Act
        LogAct("Attempting to claim — active lease should be protected");
        await using var uow = _fixture.CreateAppUserUnitOfWork();
        var repo = _fixture.CreateRepository(uow);
        await uow.OpenConnectionAsync(executionContext, CancellationToken.None);
        await uow.BeginTransactionAsync(executionContext, CancellationToken.None);
        var claimed = await repo.ClaimNextBatchAsync(10, TimeSpan.FromMinutes(5), CancellationToken.None);
        await uow.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying active-lease entry was NOT claimed");
        claimed.ShouldBeEmpty();

        var persisted = await _fixture.GetEntryDirectlyAsync(entry.Id);
        persisted.ShouldNotBeNull();
        persisted.ProcessingExpiration!.Value.ShouldBeGreaterThan(DateTimeOffset.UtcNow.AddMinutes(25),
            "Original lease should be untouched");
    }

    [Fact]
    public async Task ConcurrentMarkAsSent_SameEntry_ShouldNotThrow()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["outbox"]);
        LogArrange("Inserting a processing entry for concurrent MarkAsSent");
        await _fixture.CleanupAsync();
        var entry = _fixture.CreateTestEntry(
            status: OutboxEntryStatus.Processing,
            isProcessing: true,
            processingExpiration: DateTimeOffset.UtcNow.AddMinutes(5));
        await _fixture.InsertEntryDirectlyAsync(entry);

        var executionContext = _fixture.CreateExecutionContext();

        // Act — Two workers try to mark the same entry as sent concurrently
        LogAct("Two workers marking same entry as sent simultaneously");
        var task1 = Task.Run(async () =>
        {
            await using var uow = _fixture.CreateAppUserUnitOfWork();
            var repo = _fixture.CreateRepository(uow);
            await uow.OpenConnectionAsync(executionContext, CancellationToken.None);
            await uow.BeginTransactionAsync(executionContext, CancellationToken.None);
            await repo.MarkAsSentAsync(entry.Id, CancellationToken.None);
            await uow.CommitAsync(executionContext, CancellationToken.None);
        });

        var task2 = Task.Run(async () =>
        {
            await using var uow = _fixture.CreateAppUserUnitOfWork();
            var repo = _fixture.CreateRepository(uow);
            await uow.OpenConnectionAsync(executionContext, CancellationToken.None);
            await uow.BeginTransactionAsync(executionContext, CancellationToken.None);
            await repo.MarkAsSentAsync(entry.Id, CancellationToken.None);
            await uow.CommitAsync(executionContext, CancellationToken.None);
        });

        // Assert — Neither should throw
        LogAssert("Verifying no exceptions and entry is Sent");
        await Should.NotThrowAsync(() => Task.WhenAll(task1, task2));

        var persisted = await _fixture.GetEntryDirectlyAsync(entry.Id);
        persisted.ShouldNotBeNull();
        persisted.Status.ShouldBe(OutboxEntryStatus.Sent);
    }

    [Fact]
    public async Task LeaseExpiration_WhenMaxRetriesReached_ShouldTransitionToDead()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["outbox"]);
        LogArrange("Inserting expired-lease entry at retry_count=4 with MaxRetries=5");
        await _fixture.CleanupAsync();
        var entry = _fixture.CreateTestEntry(
            status: OutboxEntryStatus.Processing,
            retryCount: 4,
            isProcessing: true,
            processingExpiration: DateTimeOffset.UtcNow.AddMinutes(-1));
        await _fixture.InsertEntryDirectlyAsync(entry);

        var executionContext = _fixture.CreateExecutionContext();

        // Act — Claim should detect expired Processing + retry_count+1 >= MaxRetries → Dead
        LogAct("Claiming expired entry that reached MaxRetries — should transition to Dead");
        await using var uow = _fixture.CreateAppUserUnitOfWork();
        var repo = _fixture.CreateRepository(uow,
            options => options.WithMaxRetries(5));
        await uow.OpenConnectionAsync(executionContext, CancellationToken.None);
        await uow.BeginTransactionAsync(executionContext, CancellationToken.None);
        var claimed = await repo.ClaimNextBatchAsync(10, TimeSpan.FromMinutes(5), CancellationToken.None);
        await uow.CommitAsync(executionContext, CancellationToken.None);

        // Assert — entry transitioned to Dead, returned in RETURNING but as Dead
        LogAssert("Verifying entry is Dead with retry_count=5 and no lease");
        var persisted = await _fixture.GetEntryDirectlyAsync(entry.Id);
        persisted.ShouldNotBeNull();
        persisted.Status.ShouldBe(OutboxEntryStatus.Dead);
        persisted.RetryCount.ShouldBe((byte)5);
        persisted.ProcessingExpiration.ShouldBeNull("Dead entries should not have a lease");
    }

    [Fact]
    public async Task LeaseExpiration_RetryCountIncrements_AcrossMultipleExpirations()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["outbox"]);
        LogArrange("Simulating 3 consecutive lease expirations via claim");
        await _fixture.CleanupAsync();
        var entry = _fixture.CreateTestEntry();
        await _fixture.InsertEntryDirectlyAsync(entry);

        var executionContext = _fixture.CreateExecutionContext();

        // Claim 1: Pending → Processing (retry_count stays 0)
        LogAct("Claim 1: initial claim from Pending");
        await using var uow1 = _fixture.CreateAppUserUnitOfWork();
        var repo1 = _fixture.CreateRepository(uow1);
        await uow1.OpenConnectionAsync(executionContext, CancellationToken.None);
        await uow1.BeginTransactionAsync(executionContext, CancellationToken.None);
        var claimed1 = await repo1.ClaimNextBatchAsync(1, TimeSpan.FromSeconds(1), CancellationToken.None);
        await uow1.CommitAsync(executionContext, CancellationToken.None);
        claimed1.Count.ShouldBe(1);

        // Wait for lease to expire
        await Task.Delay(TimeSpan.FromSeconds(2));

        // Claim 2: expired Processing → Processing (retry_count 0 → 1)
        LogAct("Claim 2: reclaim after first lease expiration");
        await using var uow2 = _fixture.CreateAppUserUnitOfWork();
        var repo2 = _fixture.CreateRepository(uow2);
        await uow2.OpenConnectionAsync(executionContext, CancellationToken.None);
        await uow2.BeginTransactionAsync(executionContext, CancellationToken.None);
        var claimed2 = await repo2.ClaimNextBatchAsync(1, TimeSpan.FromSeconds(1), CancellationToken.None);
        await uow2.CommitAsync(executionContext, CancellationToken.None);
        claimed2.Count.ShouldBe(1);
        claimed2[0].RetryCount.ShouldBe((byte)1);

        // Wait for lease to expire again
        await Task.Delay(TimeSpan.FromSeconds(2));

        // Claim 3: expired Processing → Processing (retry_count 1 → 2)
        LogAct("Claim 3: reclaim after second lease expiration");
        await using var uow3 = _fixture.CreateAppUserUnitOfWork();
        var repo3 = _fixture.CreateRepository(uow3);
        await uow3.OpenConnectionAsync(executionContext, CancellationToken.None);
        await uow3.BeginTransactionAsync(executionContext, CancellationToken.None);
        var claimed3 = await repo3.ClaimNextBatchAsync(1, TimeSpan.FromSeconds(1), CancellationToken.None);
        await uow3.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying retry_count incremented to 2 after two lease expirations");
        claimed3.Count.ShouldBe(1);
        claimed3[0].RetryCount.ShouldBe((byte)2);

        var persisted = await _fixture.GetEntryDirectlyAsync(entry.Id);
        persisted.ShouldNotBeNull();
        persisted.RetryCount.ShouldBe((byte)2);
        persisted.Status.ShouldBe(OutboxEntryStatus.Processing);
    }

    [Fact]
    public async Task SentAndDeadEntries_ShouldNeverBeClaimed()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["outbox"]);
        LogArrange("Inserting Sent and Dead entries — neither should be claimable");
        await _fixture.CleanupAsync();
        var sentEntry = _fixture.CreateTestEntry(status: OutboxEntryStatus.Sent);
        var deadEntry = _fixture.CreateTestEntry(status: OutboxEntryStatus.Dead);
        await _fixture.InsertEntryDirectlyAsync(sentEntry);
        await _fixture.InsertEntryDirectlyAsync(deadEntry);

        var executionContext = _fixture.CreateExecutionContext();

        // Act
        LogAct("Claiming — Sent and Dead entries should be excluded");
        await using var uow = _fixture.CreateAppUserUnitOfWork();
        var repo = _fixture.CreateRepository(uow);
        await uow.OpenConnectionAsync(executionContext, CancellationToken.None);
        await uow.BeginTransactionAsync(executionContext, CancellationToken.None);
        var claimed = await repo.ClaimNextBatchAsync(10, TimeSpan.FromMinutes(5), CancellationToken.None);
        await uow.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying zero entries claimed");
        claimed.ShouldBeEmpty();
    }
}
