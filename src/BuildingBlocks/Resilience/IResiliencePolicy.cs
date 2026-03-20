using Bedrock.BuildingBlocks.Resilience.Models;

namespace Bedrock.BuildingBlocks.Resilience;

/// <summary>
/// Defines the contract for a resilience policy that wraps handler execution
/// with retry and circuit breaker strategies.
/// </summary>
public interface IResiliencePolicy
{
    /// <summary>
    /// Executes the handler through the resilience pipeline with an input value.
    /// </summary>
    /// <typeparam name="TInput">The type of the input passed to the handler.</typeparam>
    /// <typeparam name="TOutput">The type of the handler's return value.</typeparam>
    /// <param name="executionContext">The execution context for distributed tracing and observability.</param>
    /// <param name="input">The input value to pass to the handler.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <param name="handler">The handler to execute within the resilience pipeline.</param>
    /// <returns>A result envelope containing the output or failure information.</returns>
    Task<ResiliencePolicyExecutionResult<TOutput>> ExecuteAsync<TInput, TOutput>(
        ExecutionContext executionContext,
        TInput input,
        CancellationToken cancellationToken,
        Func<ExecutionContext, TInput, CancellationToken, Task<TOutput>> handler);

    /// <summary>
    /// Executes the handler through the resilience pipeline without an input value.
    /// Delegates internally to the input-based overload with <c>null</c>.
    /// </summary>
    /// <typeparam name="TOutput">The type of the handler's return value.</typeparam>
    /// <param name="executionContext">The execution context for distributed tracing and observability.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <param name="handler">The handler to execute within the resilience pipeline.</param>
    /// <returns>A result envelope containing the output or failure information.</returns>
    Task<ResiliencePolicyExecutionResult<TOutput>> ExecuteAsync<TOutput>(
        ExecutionContext executionContext,
        CancellationToken cancellationToken,
        Func<ExecutionContext, CancellationToken, Task<TOutput>> handler);
}
