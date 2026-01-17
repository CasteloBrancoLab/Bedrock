namespace Bedrock.BuildingBlocks.Persistence.Abstractions.DistributedLock;

public interface IDistributedLock
{
    public Task<bool> IsLockedAsync(string lockKey, CancellationToken cancellationToken);
    public Task<bool> ReleaseAsync(string lockKey, CancellationToken cancellationToken);
}

public interface IDistributedLock<TDisposable>
    : IDistributedLock
    where TDisposable : IDisposable
{
    public Task<TDisposable?> AcquireAsync(string key, TimeSpan expiry, CancellationToken cancellationToken);
}
