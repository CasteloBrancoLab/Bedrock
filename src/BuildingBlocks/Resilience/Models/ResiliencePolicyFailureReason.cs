namespace Bedrock.BuildingBlocks.Resilience.Models;

/// <summary>
/// Describes the reason a resilience policy execution failed.
/// </summary>
public enum ResiliencePolicyFailureReason
{
    /// <summary>
    /// No failure — the execution succeeded.
    /// </summary>
    None = 0,

    /// <summary>
    /// All retry attempts were exhausted without a successful outcome.
    /// The circuit breaker may open as a consequence if the failure ratio threshold is reached.
    /// </summary>
    RetriesExhausted = 1,

    /// <summary>
    /// The execution was rejected because the circuit breaker was already open,
    /// waiting for the half-open window before allowing a test call.
    /// </summary>
    CircuitOpen = 2,

    /// <summary>
    /// The handler threw an unhandled exception that was not covered by a retry policy.
    /// </summary>
    HandlerException = 3
}
