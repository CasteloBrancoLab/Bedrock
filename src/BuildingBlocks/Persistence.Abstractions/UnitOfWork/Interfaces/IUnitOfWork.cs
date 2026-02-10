namespace Bedrock.BuildingBlocks.Persistence.Abstractions.UnitOfWork.Interfaces;

/// <summary>
/// Defines the contract for unit of work pattern implementations.
/// </summary>
/// <remarks>
/// <para>
/// <b>Thread Safety:</b> Implementations of this interface are NOT thread-safe by design.
/// Each async operation should use its own IUnitOfWork instance. Do not share instances across threads.
/// </para>
/// <para>
/// <b>Resource Management:</b> This interface extends both IDisposable and IAsyncDisposable.
/// Prefer using <c>await using</c> for async contexts to ensure proper async cleanup of database resources.
/// </para>
/// </remarks>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    public string Name { get; }

    public Task<bool> OpenConnectionAsync(ExecutionContext executionContext, CancellationToken cancellationToken);
    public Task<bool> CloseConnectionAsync(ExecutionContext executionContext, CancellationToken cancellationToken);
    public Task<bool> BeginTransactionAsync(ExecutionContext executionContext, CancellationToken cancellationToken);
    public Task<bool> CommitAsync(ExecutionContext executionContext, CancellationToken cancellationToken);
    public Task<bool> RollbackAsync(ExecutionContext executionContext, CancellationToken cancellationToken);
    public Task<bool> ExecuteAsync<TInput>(ExecutionContext executionContext, TInput input, Func<ExecutionContext, TInput, CancellationToken, Task<bool>> handlerAsync, CancellationToken cancellationToken);
}
