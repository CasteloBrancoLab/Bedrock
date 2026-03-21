using Bedrock.BuildingBlocks.Resilience.Models;

namespace Bedrock.BuildingBlocks.Resilience;

/// <summary>
/// Defines the contract for a resilience policy that wraps handler execution
/// with retry, circuit breaker, and timeout strategies.
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
    /// <param name="fallback">
    /// Optional fallback handler invoked when the primary handler fails.
    /// Receives the execution context, the failure reason, and the exception.
    /// When provided and primary execution fails, the fallback result is returned with
    /// <see cref="ResiliencePolicyExecutionResult{TOutput}.IsFallback"/> set to <c>true</c>.
    /// </param>
    Task<ResiliencePolicyExecutionResult<TOutput>> ExecuteAsync<TInput, TOutput>(
        ExecutionContext executionContext,
        TInput input,
        CancellationToken cancellationToken,
        Func<ExecutionContext, TInput, CancellationToken, Task<TOutput>> handler,
        Func<ExecutionContext, ResiliencePolicyFailureReason, Exception?, Task<TOutput>>? fallback = null);

    /// <summary>
    /// Executes the handler through the resilience pipeline without an input value.
    /// </summary>
    /// <param name="fallback">
    /// Optional fallback handler invoked when the primary handler fails.
    /// </param>
    Task<ResiliencePolicyExecutionResult<TOutput>> ExecuteAsync<TOutput>(
        ExecutionContext executionContext,
        CancellationToken cancellationToken,
        Func<ExecutionContext, CancellationToken, Task<TOutput>> handler,
        Func<ExecutionContext, ResiliencePolicyFailureReason, Exception?, Task<TOutput>>? fallback = null);

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
