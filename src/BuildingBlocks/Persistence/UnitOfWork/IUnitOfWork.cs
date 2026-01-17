namespace Bedrock.BuildingBlocks.Persistence.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    public string Name { get; }

    public Task<bool> OpenConnectionAsync(ExecutionContext executionContext, CancellationToken cancellationToken);
    public Task<bool> CloseConnectionAsync(ExecutionContext executionContext, CancellationToken cancellationToken);
    public Task<bool> BeginTransactionAsync(ExecutionContext executionContext, CancellationToken cancellationToken);
    public Task<bool> CommitAsync(ExecutionContext executionContext, CancellationToken cancellationToken);
    public Task<bool> RollbackAsync(ExecutionContext executionContext, CancellationToken cancellationToken);
    public Task<bool> ExecuteAsync<TInput>(ExecutionContext executionContext, TInput input, Func<ExecutionContext, TInput, CancellationToken, Task<bool>> handlerAsync, CancellationToken cancellationToken);
}
