namespace Bedrock.BuildingBlocks.Resilience.Models;

/// <summary>
/// Represents a circuit breaker state entry retrieved from the distributed state store.
/// </summary>
/// <param name="State">The current distributed state of the circuit breaker.</param>
/// <param name="UpdatedAt">The timestamp when the state was last updated, used for optimistic concurrency.</param>
public sealed record CircuitBreakerStateEntry(
    CircuitBreakerDistributedState State,
    DateTimeOffset UpdatedAt);
