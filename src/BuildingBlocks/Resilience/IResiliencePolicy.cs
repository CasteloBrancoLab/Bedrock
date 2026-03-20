using Bedrock.BuildingBlocks.Resilience.Models;

namespace Bedrock.BuildingBlocks.Resilience;

/// <summary>
/// Defines the contract for a resilience policy that wraps handler execution
/// with retry and circuit breaker strategies.
/// </summary>
/// <remarks>
/// Every resilience policy is inherently manageable — there is no separate marking interface.
/// The <see cref="IResiliencePolicyManager"/> discovers all registered policies via DI
/// and manages their distributed state.
/// </remarks>
public interface IResiliencePolicy
{
    /// <summary>
    /// Gets the unique identifier of this policy, used as the key in the distributed state store.
    /// </summary>
    string PolicyCode { get; }

    // ================================
    // Execution
    // ================================

    /// <summary>
    /// Executes the handler through the resilience pipeline with an input value.
    /// </summary>
    Task<ResiliencePolicyExecutionResult<TOutput>> ExecuteAsync<TInput, TOutput>(
        ExecutionContext executionContext,
        TInput input,
        CancellationToken cancellationToken,
        Func<ExecutionContext, TInput, CancellationToken, Task<TOutput>> handler);

    /// <summary>
    /// Executes the handler through the resilience pipeline without an input value.
    /// </summary>
    Task<ResiliencePolicyExecutionResult<TOutput>> ExecuteAsync<TOutput>(
        ExecutionContext executionContext,
        CancellationToken cancellationToken,
        Func<ExecutionContext, CancellationToken, Task<TOutput>> handler);

    // ================================
    // Circuit Management
    // ================================

    /// <summary>
    /// Forces the circuit breaker to the open (isolated) state.
    /// All subsequent calls are rejected until <see cref="ForceCloseCircuitAsync"/> is called.
    /// </summary>
    Task ForceOpenCircuitAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Forces the circuit breaker to the closed state, allowing all calls through.
    /// </summary>
    Task ForceCloseCircuitAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Registers a callback invoked when the circuit breaker changes state automatically
    /// (due to failures, not manual control). Set by the <see cref="IResiliencePolicyManager"/>.
    /// </summary>
    void RegisterCircuitStateChangedCallback(CircuitStateChangedHandler handler);
}
