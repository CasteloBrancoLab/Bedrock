namespace Bedrock.BuildingBlocks.Resilience.Models;

/// <summary>
/// Represents the distributed state of a circuit breaker, synchronized across application instances.
/// </summary>
public enum CircuitBreakerDistributedState : short
{
    /// <summary>
    /// The circuit is closed — all calls are allowed through.
    /// </summary>
    Closed = 0,

    /// <summary>
    /// The circuit is open — all calls are rejected until the break duration expires.
    /// </summary>
    Open = 1,

    /// <summary>
    /// The circuit is half-open — a single test call is allowed to determine if the circuit should close.
    /// </summary>
    HalfOpen = 2
}
