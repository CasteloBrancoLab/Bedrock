using Bedrock.BuildingBlocks.Resilience.Models;

namespace Bedrock.BuildingBlocks.Resilience;

/// <summary>
/// Manages resilience policies lifecycle, distributed state synchronization,
/// and manual circuit breaker control for maintenance operations.
/// </summary>
/// <remarks>
/// <para>
/// Discovers all <see cref="IResiliencePolicy"/> instances registered in the DI container
/// and coordinates their circuit breaker state across application instances via polling.
/// </para>
/// <para>
/// The Manager is the <b>only</b> component that interacts with the <see cref="ICircuitBreakerStateStore"/>.
/// Policies notify the Manager of automatic state changes; the Manager publishes to the store.
/// </para>
/// </remarks>
public interface IResiliencePolicyManager
{
    /// <summary>
    /// Manually opens (isolates) a circuit breaker across all instances.
    /// Updates the distributed state store first, then forces the local circuit open.
    /// Useful for planned maintenance of external services.
    /// </summary>
    Task OpenCircuitAsync(ExecutionContext executionContext, string policyCode, CancellationToken cancellationToken);

    /// <summary>
    /// Manually closes a circuit breaker across all instances.
    /// Updates the distributed state store first, then forces the local circuit closed.
    /// </summary>
    Task CloseCircuitAsync(ExecutionContext executionContext, string policyCode, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the current distributed state for a given policy from the state store.
    /// </summary>
    Task<CircuitBreakerStateEntry?> GetCircuitStateAsync(ExecutionContext executionContext, string policyCode, CancellationToken cancellationToken);
}
