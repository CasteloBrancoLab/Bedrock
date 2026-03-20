namespace Bedrock.BuildingBlocks.Resilience.Models;

/// <summary>
/// Delegate invoked when a resilience policy's circuit breaker changes state.
/// Used by <see cref="IResiliencePolicyManager"/> to synchronize distributed state.
/// </summary>
/// <param name="policyCode">The unique identifier of the resilience policy.</param>
/// <param name="newState">The new circuit breaker state.</param>
/// <param name="executionContext">The execution context from the operation that triggered the state change.</param>
public delegate void CircuitStateChangedHandler(
    string policyCode,
    CircuitBreakerDistributedState newState,
    ExecutionContext executionContext);
