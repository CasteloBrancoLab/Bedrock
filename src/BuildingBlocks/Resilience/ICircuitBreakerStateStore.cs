using Bedrock.BuildingBlocks.Resilience.Models;

namespace Bedrock.BuildingBlocks.Resilience;

/// <summary>
/// Defines the contract for a distributed circuit breaker state store.
/// Enables synchronization of circuit breaker state across application instances via polling.
/// </summary>
/// <remarks>
/// <para>
/// Implementations must be thread-safe and singleton-compatible, since resilience policies
/// are registered as singletons and access the store concurrently.
/// </para>
/// <para>
/// All methods receive <see cref="ExecutionContext"/> for distributed tracing and observability.
/// Implementations should log exceptions via <c>LogExceptionForDistributedTracing</c>
/// and register them via <see cref="ExecutionContext.AddException"/>.
/// </para>
/// </remarks>
public interface ICircuitBreakerStateStore
{
    /// <summary>
    /// Retrieves the current distributed state for a given policy.
    /// </summary>
    /// <param name="executionContext">The execution context for distributed tracing.</param>
    /// <param name="policyCode">The unique identifier of the resilience policy.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The state entry if found; otherwise, <c>null</c>.</returns>
    Task<CircuitBreakerStateEntry?> GetStateAsync(
        ExecutionContext executionContext,
        string policyCode,
        CancellationToken cancellationToken);

    /// <summary>
    /// Updates the distributed state for a given policy using optimistic concurrency.
    /// Only updates if the provided <paramref name="updatedAt"/> is newer than the stored value.
    /// </summary>
    /// <param name="executionContext">The execution context for distributed tracing.</param>
    /// <param name="policyCode">The unique identifier of the resilience policy.</param>
    /// <param name="state">The new circuit breaker state.</param>
    /// <param name="updatedAt">The timestamp of the state change, used for concurrency control.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><c>true</c> if the state was updated; <c>false</c> if concurrency check failed or an error occurred.</returns>
    Task<bool> UpdateStateAsync(
        ExecutionContext executionContext,
        string policyCode,
        CircuitBreakerDistributedState state,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken);
}
