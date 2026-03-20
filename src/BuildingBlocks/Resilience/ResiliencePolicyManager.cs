using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Observability.ExtensionMethods;
using Bedrock.BuildingBlocks.Resilience.Models;
using Microsoft.Extensions.Logging;

namespace Bedrock.BuildingBlocks.Resilience;

/// <summary>
/// Manages resilience policies lifecycle, distributed state synchronization,
/// and manual circuit breaker control.
/// </summary>
/// <remarks>
/// <para>
/// Registered as a singleton. Discovers all <see cref="IResiliencePolicy"/> instances via DI,
/// registers state change callbacks, and starts a background polling task for distributed sync.
/// </para>
/// <para>
/// The Manager is the <b>only</b> component that interacts with the <see cref="ICircuitBreakerStateStore"/>.
/// Feedback loop prevention: when synchronizing from remote state (polling), the
/// <c>_isSynchronizingFromRemote</c> flag suppresses re-publishing to the store.
/// </para>
/// <para>
/// TTL safety net: if an open circuit in the state store exceeds the expiration threshold,
/// it is assumed the instance that opened it has crashed, and the Manager closes the circuit.
/// </para>
/// </remarks>
public sealed class ResiliencePolicyManager : IResiliencePolicyManager, IAsyncDisposable
{
    private readonly ILogger<ResiliencePolicyManager> _logger;
    private readonly ICircuitBreakerStateStore? _stateStore;
    private readonly TimeProvider _timeProvider;
    private readonly TimeSpan _pollingInterval;
    private readonly TimeSpan _openCircuitExpirationThreshold;
    private readonly Dictionary<string, IResiliencePolicy> _policies = new();
    private readonly Dictionary<string, CircuitBreakerDistributedState?> _lastKnownStates = new();
    private CancellationTokenSource? _pollingCts;
    private Task? _pollingTask;
    private volatile bool _isSynchronizingFromRemote;

    internal ResiliencePolicyManager(
        ILogger<ResiliencePolicyManager> logger,
        ICircuitBreakerStateStore? stateStore,
        ResiliencePolicyManagerOptions options,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _logger = logger;
        _stateStore = stateStore;
        _timeProvider = timeProvider;
        _pollingInterval = options.PollingInterval;
        _openCircuitExpirationThreshold = options.OpenCircuitExpirationThreshold;
    }

    // ================================
    // Registration & Startup
    // ================================

    internal void Register(IResiliencePolicy policy)
    {
        _policies[policy.PolicyCode] = policy;
        _lastKnownStates[policy.PolicyCode] = null;
        policy.RegisterCircuitStateChangedCallback(HandlePolicyCircuitStateChanged);
    }

    internal void StartPolling()
    {
        if (_stateStore is null)
            return;

        _pollingCts = new CancellationTokenSource();
        _pollingTask = PollDistributedStateAsync(_pollingCts.Token);
    }

    // ================================
    // IResiliencePolicyManager
    // ================================

    /// <inheritdoc />
    public async Task OpenCircuitAsync(
        ExecutionContext executionContext,
        string policyCode,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(executionContext);
        ArgumentException.ThrowIfNullOrWhiteSpace(policyCode);

        if (!_policies.TryGetValue(policyCode, out var policy))
            return;

        if (_stateStore is not null)
        {
            await _stateStore.UpdateStateAsync(
                executionContext, policyCode,
                CircuitBreakerDistributedState.Open,
                _timeProvider.GetUtcNow(),
                cancellationToken).ConfigureAwait(false);
        }

        await ForceCircuitFromManagerAsync(policy, isolate: true, cancellationToken).ConfigureAwait(false);

        _logger.LogInformationForDistributedTracing(
            executionContext,
            "Resilience policy {PolicyCode} circuit manually opened",
            policyCode);
    }

    /// <inheritdoc />
    public async Task CloseCircuitAsync(
        ExecutionContext executionContext,
        string policyCode,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(executionContext);
        ArgumentException.ThrowIfNullOrWhiteSpace(policyCode);

        if (!_policies.TryGetValue(policyCode, out var policy))
            return;

        if (_stateStore is not null)
        {
            await _stateStore.UpdateStateAsync(
                executionContext, policyCode,
                CircuitBreakerDistributedState.Closed,
                _timeProvider.GetUtcNow(),
                cancellationToken).ConfigureAwait(false);
        }

        await ForceCircuitFromManagerAsync(policy, isolate: false, cancellationToken).ConfigureAwait(false);

        _logger.LogInformationForDistributedTracing(
            executionContext,
            "Resilience policy {PolicyCode} circuit manually closed",
            policyCode);
    }

    /// <inheritdoc />
    public async Task<CircuitBreakerStateEntry?> GetCircuitStateAsync(
        ExecutionContext executionContext,
        string policyCode,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(executionContext);
        ArgumentException.ThrowIfNullOrWhiteSpace(policyCode);

        if (_stateStore is null)
            return null;

        return await _stateStore.GetStateAsync(executionContext, policyCode, cancellationToken).ConfigureAwait(false);
    }

    // ================================
    // Policy State Change Callback
    // ================================

    private void HandlePolicyCircuitStateChanged(
        string policyCode,
        CircuitBreakerDistributedState newState,
        ExecutionContext executionContext)
    {
        if (_stateStore is null || _isSynchronizingFromRemote)
            return;

        _ = PublishStateToStoreAsync(executionContext, policyCode, newState);
    }

    private async Task PublishStateToStoreAsync(
        ExecutionContext executionContext,
        string policyCode,
        CircuitBreakerDistributedState state)
    {
        try
        {
            var updatedAt = _timeProvider.GetUtcNow();
            await _stateStore!.UpdateStateAsync(executionContext, policyCode, state, updatedAt, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogExceptionForDistributedTracing(
                executionContext,
                ex,
                "Resilience policy {PolicyCode} failed to publish circuit state {State} to distributed store",
                policyCode,
                state);
        }
    }

    // ================================
    // Distributed State — Polling
    // ================================

    private async Task PollDistributedStateAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(_pollingInterval);

        while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
        {
            try
            {
                var executionContext = CreateInfrastructureExecutionContext();
                await SynchronizeAllPoliciesAsync(executionContext, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "ResiliencePolicyManager failed to poll distributed circuit state");
            }
        }
    }

    private async Task SynchronizeAllPoliciesAsync(
        ExecutionContext executionContext,
        CancellationToken cancellationToken)
    {
        foreach (var (policyCode, policy) in _policies)
        {
            await SynchronizePolicyAsync(executionContext, policyCode, policy, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task SynchronizePolicyAsync(
        ExecutionContext executionContext,
        string policyCode,
        IResiliencePolicy policy,
        CancellationToken cancellationToken)
    {
        var entry = await _stateStore!.GetStateAsync(executionContext, policyCode, cancellationToken).ConfigureAwait(false);

        if (entry is null)
            return;

        if (IsExpiredOpenCircuit(entry))
        {
            await ForceCircuitFromManagerAsync(policy, isolate: false, cancellationToken).ConfigureAwait(false);
            await PublishStateToStoreAsync(executionContext, policyCode, CircuitBreakerDistributedState.Closed).ConfigureAwait(false);
            _lastKnownStates[policyCode] = CircuitBreakerDistributedState.Closed;
            return;
        }

        if (entry.State == _lastKnownStates[policyCode])
            return;

        _lastKnownStates[policyCode] = entry.State;

        switch (entry.State)
        {
            case CircuitBreakerDistributedState.Open:
                await ForceCircuitFromManagerAsync(policy, isolate: true, cancellationToken).ConfigureAwait(false);
                break;

            case CircuitBreakerDistributedState.Closed:
                await ForceCircuitFromManagerAsync(policy, isolate: false, cancellationToken).ConfigureAwait(false);
                break;

            // HalfOpen: let the policy manage the transition naturally
        }
    }

    private bool IsExpiredOpenCircuit(CircuitBreakerStateEntry entry)
    {
        if (entry.State is not CircuitBreakerDistributedState.Open and not CircuitBreakerDistributedState.HalfOpen)
            return false;

        var expirationThreshold = entry.UpdatedAt + _openCircuitExpirationThreshold;
        return _timeProvider.GetUtcNow() > expirationThreshold;
    }

    // ================================
    // Circuit Control Helpers
    // ================================

    private async Task ForceCircuitFromManagerAsync(
        IResiliencePolicy policy,
        bool isolate,
        CancellationToken cancellationToken)
    {
        _isSynchronizingFromRemote = true;
        try
        {
            if (isolate)
                await policy.ForceOpenCircuitAsync(cancellationToken).ConfigureAwait(false);
            else
                await policy.ForceCloseCircuitAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _isSynchronizingFromRemote = false;
        }
    }

    // ================================
    // Infrastructure
    // ================================

    private ExecutionContext CreateInfrastructureExecutionContext()
    {
        return ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: TenantInfo.Create(Guid.Empty, "System"),
            executionUser: "Bedrock.Resilience",
            executionOrigin: "ResiliencePolicyManager",
            businessOperationCode: "CIRCUIT_BREAKER_SYNC",
            minimumMessageType: MessageType.Warning,
            timeProvider: _timeProvider);
    }

    // ================================
    // IAsyncDisposable
    // ================================

    /// <summary>
    /// Stops the background polling task and releases resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_pollingCts is not null)
        {
            await _pollingCts.CancelAsync().ConfigureAwait(false);
            _pollingCts.Dispose();
        }

        if (_pollingTask is not null)
        {
            try
            {
                await _pollingTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Expected during shutdown
            }
        }

        GC.SuppressFinalize(this);
    }
}
