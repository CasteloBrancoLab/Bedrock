using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Observability.ExtensionMethods;
using Bedrock.BuildingBlocks.Resilience.Models;
using Bedrock.BuildingBlocks.Resilience.Polly.Models;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace Bedrock.BuildingBlocks.Resilience.Polly;

/// <summary>
/// Abstract base class for Polly-based resilience policies.
/// Composes retry and circuit breaker strategies into a single thread-safe pipeline.
/// </summary>
/// <remarks>
/// <para>
/// Designed to be registered as a <b>singleton</b> in the DI container.
/// The resilience pipeline is built once during construction and reused across all concurrent calls.
/// All state (circuit breaker counters, timers) is managed internally by Polly's thread-safe infrastructure.
/// </para>
/// <para>
/// Subclasses override <see cref="ConfigureInternal"/> to define retry, circuit breaker, and jitter settings.
/// Logging uses the <c>*ForDistributedTracing</c> extension methods from <c>Bedrock.BuildingBlocks.Observability</c>.
/// </para>
/// <para>
/// Pipeline composition: CircuitBreaker (outer) → Retry (inner) → Handler.
/// The circuit breaker evaluates the final outcome after all retry attempts are exhausted.
/// </para>
/// <para>
/// <b>Distributed state synchronization:</b> When an <see cref="ICircuitBreakerStateStore"/> is provided,
/// circuit state changes are published to the store and a background polling task synchronizes
/// the local circuit from remote state. This enables circuit breaker coordination across instances.
/// A TTL safety net ensures that if the instance that opened the circuit crashes, other instances
/// can recover after <c>2× breakDuration</c>.
/// </para>
/// </remarks>
public abstract class PollyResiliencePolicyBase : IResiliencePolicy, IAsyncDisposable
{
    private static readonly ResiliencePropertyKey<ExecutionContext> ExecutionContextKey = new("Bedrock.ExecutionContext");
    private static readonly ResiliencePropertyKey<object?> InputKey = new("Bedrock.Input");

    // Core fields
    private readonly ILogger _logger;
    private readonly ResiliencePipeline _pipeline;
    private readonly bool _hasRetry;

    // Distributed state fields
    private readonly ICircuitBreakerStateStore? _stateStore;
    private readonly CircuitBreakerManualControl? _manualControl;
    private readonly CancellationTokenSource? _pollingCts;
    private readonly Task? _pollingTask;
    private readonly string _policyCode;
    private readonly TimeSpan _pollingInterval;
    private readonly TimeSpan _breakDuration;
    private readonly TimeProvider _timeProvider;
    private readonly bool _hasDistributedState;
    private volatile bool _isSynchronizingFromRemote;
    private CircuitBreakerDistributedState? _lastKnownRemoteState;

    /// <summary>
    /// Initializes the resilience policy by building the Polly pipeline from subclass configuration.
    /// </summary>
    /// <param name="logger">The logger for distributed tracing.</param>
    /// <param name="stateStore">
    /// Optional distributed state store for synchronizing circuit breaker state across instances.
    /// When provided, circuit state changes are published and a background polling task reads remote state.
    /// </param>
    protected PollyResiliencePolicyBase(ILogger logger, ICircuitBreakerStateStore? stateStore = null)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
        _stateStore = stateStore;

        var options = new PollyResiliencePolicyOptions();
        ConfigureInternal(options);

        _policyCode = options.PolicyCode ?? GetType().Name;
        _pollingInterval = options.DistributedStatePollingInterval;
        _breakDuration = options.CircuitBreaker?.BreakDuration ?? TimeSpan.FromSeconds(30);
        _timeProvider = options.TimeProvider ?? TimeProvider.System;
        _hasRetry = options.Retry is not null;
        _hasDistributedState = _stateStore is not null && options.CircuitBreaker is not null;

        if (_hasDistributedState)
            _manualControl = new CircuitBreakerManualControl();

        _pipeline = BuildPipeline(options);

        if (_hasDistributedState)
        {
            _pollingCts = new CancellationTokenSource();
            _pollingTask = PollDistributedStateAsync(_pollingCts.Token);
        }
    }

    /// <summary>
    /// Configures the resilience policy options. Called once during construction.
    /// </summary>
    /// <param name="options">The options to configure with retry, circuit breaker, and time provider settings.</param>
    protected abstract void ConfigureInternal(PollyResiliencePolicyOptions options);

    // ================================
    // IResiliencePolicy
    // ================================

    /// <inheritdoc />
    public async Task<ResiliencePolicyExecutionResult<TOutput>> ExecuteAsync<TInput, TOutput>(
        ExecutionContext executionContext,
        TInput input,
        CancellationToken cancellationToken,
        Func<ExecutionContext, TInput, CancellationToken, Task<TOutput>> handler)
    {
        ArgumentNullException.ThrowIfNull(executionContext);
        ArgumentNullException.ThrowIfNull(handler);

        var resilienceContext = ResilienceContextPool.Shared.Get(cancellationToken);

        try
        {
            SetResilienceContextProperties(resilienceContext, executionContext, input);
            LogExecutionStarted(executionContext);

            var result = await _pipeline.ExecuteAsync(
                async (ctx, state) => await state.handler(state.executionContext, state.input, ctx.CancellationToken).ConfigureAwait(false),
                resilienceContext,
                (executionContext, input, handler)).ConfigureAwait(false);

            LogExecutionSucceeded(executionContext);
            return ResiliencePolicyExecutionResult<TOutput>.CreateSuccess(result);
        }
        catch (BrokenCircuitException ex)
        {
            return HandleCircuitOpenFailure<TOutput>(executionContext, ex);
        }
        catch (Exception ex) when (_hasRetry)
        {
            return HandleRetriesExhaustedFailure<TOutput>(executionContext, ex);
        }
        catch (Exception ex)
        {
            return HandleHandlerException<TOutput>(executionContext, ex);
        }
        finally
        {
            ResilienceContextPool.Shared.Return(resilienceContext);
        }
    }

    /// <inheritdoc />
    public Task<ResiliencePolicyExecutionResult<TOutput>> ExecuteAsync<TOutput>(
        ExecutionContext executionContext,
        CancellationToken cancellationToken,
        Func<ExecutionContext, CancellationToken, Task<TOutput>> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        return ExecuteAsync<object?, TOutput>(
            executionContext,
            input: null,
            cancellationToken,
            handler: (ctx, _, ct) => handler(ctx, ct));
    }

    // ================================
    // Pipeline Construction
    // ================================

    private ResiliencePipeline BuildPipeline(PollyResiliencePolicyOptions options)
    {
        var builder = new ResiliencePipelineBuilder();

        ConfigureTimeProvider(builder, options);
        ConfigureCircuitBreakerStrategy(builder, options.CircuitBreaker);
        ConfigureRetryStrategy(builder, options.Retry);

        return builder.Build();
    }

    private static void ConfigureTimeProvider(ResiliencePipelineBuilder builder, PollyResiliencePolicyOptions options)
    {
        if (options.TimeProvider is not null)
            builder.TimeProvider = options.TimeProvider;
    }

    private void ConfigureCircuitBreakerStrategy(ResiliencePipelineBuilder builder, CircuitBreakerOptions? options)
    {
        if (options is null)
            return;

        var strategyOptions = new CircuitBreakerStrategyOptions
        {
            FailureRatio = options.FailureRatio,
            MinimumThroughput = options.MinimumThroughput,
            BreakDuration = options.BreakDuration,
            SamplingDuration = options.SamplingDuration,
            ShouldHandle = new PredicateBuilder().Handle<Exception>(),
            OnOpened = args =>
            {
                HandleCircuitOpened(args, options);
                return ValueTask.CompletedTask;
            },
            OnClosed = args =>
            {
                HandleCircuitClosed(args, options);
                return ValueTask.CompletedTask;
            },
            OnHalfOpened = args =>
            {
                HandleCircuitHalfOpened(args, options);
                return ValueTask.CompletedTask;
            }
        };

        if (_manualControl is not null)
            strategyOptions.ManualControl = _manualControl;

        builder.AddCircuitBreaker(strategyOptions);
    }

    private void ConfigureRetryStrategy(ResiliencePipelineBuilder builder, RetryOptions? options)
    {
        if (options is null)
            return;

        builder.AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = options.MaxAttempts,
            ShouldHandle = new PredicateBuilder().Handle<Exception>(),
            DelayGenerator = CreateDelayGenerator(options),
            OnRetry = args =>
            {
                HandleRetryAttemptFailed(args, options);
                return ValueTask.CompletedTask;
            }
        });
    }

    private static Func<RetryDelayGeneratorArguments<object>, ValueTask<TimeSpan?>>? CreateDelayGenerator(RetryOptions options)
    {
        if (options.JitterStrategy is null)
            return null;

        return args =>
        {
            var executionContext = GetExecutionContextFromResilienceContext(args.Context);
            var input = GetInputFromResilienceContext(args.Context);
            var delay = options.JitterStrategy(executionContext, input, args.AttemptNumber);

            return ValueTask.FromResult<TimeSpan?>(delay);
        };
    }

    // ================================
    // Pipeline Event Handlers
    // ================================

    private void HandleRetryAttemptFailed(OnRetryArguments<object> args, RetryOptions options)
    {
        var executionContext = GetExecutionContextFromResilienceContext(args.Context);

        _logger.LogWarningForDistributedTracing(
            executionContext,
            "Resilience policy {PolicyName} retry attempt {AttemptNumber}/{MaxAttempts} failed",
            GetType().Name,
            args.AttemptNumber + 1,
            options.MaxAttempts);
    }

    private void HandleCircuitOpened(OnCircuitOpenedArguments<object> args, CircuitBreakerOptions options)
    {
        var executionContext = GetExecutionContextFromResilienceContext(args.Context);

        _logger.LogErrorForDistributedTracing(
            executionContext,
            "Resilience policy {PolicyName} circuit breaker opened, break duration: {BreakDuration}",
            GetType().Name,
            options.BreakDuration);

        options.OnOpenedCallback?.Invoke(executionContext);
        PublishStateToStore(executionContext, CircuitBreakerDistributedState.Open);
    }

    private void HandleCircuitClosed(OnCircuitClosedArguments<object> args, CircuitBreakerOptions options)
    {
        var executionContext = GetExecutionContextFromResilienceContext(args.Context);

        _logger.LogInformationForDistributedTracing(
            executionContext,
            "Resilience policy {PolicyName} circuit breaker closed",
            GetType().Name);

        options.OnClosedCallback?.Invoke(executionContext);
        PublishStateToStore(executionContext, CircuitBreakerDistributedState.Closed);
    }

    private void HandleCircuitHalfOpened(OnCircuitHalfOpenedArguments args, CircuitBreakerOptions options)
    {
        var executionContext = GetExecutionContextFromResilienceContext(args.Context);

        _logger.LogWarningForDistributedTracing(
            executionContext,
            "Resilience policy {PolicyName} circuit breaker half-opened",
            GetType().Name);

        options.OnHalfOpenedCallback?.Invoke(executionContext);
        PublishStateToStore(executionContext, CircuitBreakerDistributedState.HalfOpen);
    }

    // ================================
    // Failure Handlers
    // ================================

    private ResiliencePolicyExecutionResult<TOutput> HandleCircuitOpenFailure<TOutput>(
        ExecutionContext executionContext,
        BrokenCircuitException exception)
    {
        _logger.LogWarningForDistributedTracing(
            executionContext,
            "Resilience policy {PolicyName} execution rejected, circuit breaker is open",
            GetType().Name);

        return ResiliencePolicyExecutionResult<TOutput>.CreateFailure(
            ResiliencePolicyFailureReason.CircuitOpen, exception);
    }

    private ResiliencePolicyExecutionResult<TOutput> HandleRetriesExhaustedFailure<TOutput>(
        ExecutionContext executionContext,
        Exception exception)
    {
        _logger.LogErrorForDistributedTracing(
            executionContext,
            "Resilience policy {PolicyName} all retry attempts exhausted",
            GetType().Name);

        return ResiliencePolicyExecutionResult<TOutput>.CreateFailure(
            ResiliencePolicyFailureReason.RetriesExhausted, exception);
    }

    private ResiliencePolicyExecutionResult<TOutput> HandleHandlerException<TOutput>(
        ExecutionContext executionContext,
        Exception exception)
    {
        _logger.LogExceptionForDistributedTracing(
            executionContext,
            exception,
            "Resilience policy {PolicyName} execution failed with unhandled exception",
            GetType().Name);

        return ResiliencePolicyExecutionResult<TOutput>.CreateFailure(
            ResiliencePolicyFailureReason.HandlerException, exception);
    }

    // ================================
    // Logging Helpers
    // ================================

    private void LogExecutionStarted(ExecutionContext executionContext)
    {
        _logger.LogDebugForDistributedTracing(
            executionContext,
            "Resilience policy {PolicyName} execution started",
            GetType().Name);
    }

    private void LogExecutionSucceeded(ExecutionContext executionContext)
    {
        _logger.LogDebugForDistributedTracing(
            executionContext,
            "Resilience policy {PolicyName} execution succeeded",
            GetType().Name);
    }

    // ================================
    // Resilience Context Helpers
    // ================================

    private static void SetResilienceContextProperties(
        ResilienceContext resilienceContext,
        ExecutionContext executionContext,
        object? input)
    {
        resilienceContext.Properties.Set(ExecutionContextKey, executionContext);
        resilienceContext.Properties.Set(InputKey, input);
    }

    private static ExecutionContext GetExecutionContextFromResilienceContext(ResilienceContext context)
    {
        return context.Properties.GetValue(ExecutionContextKey, null!);
    }

    private static object? GetInputFromResilienceContext(ResilienceContext context)
    {
        return context.Properties.GetValue(InputKey, null);
    }

    // ================================
    // Distributed State — Publishing
    // ================================

    private void PublishStateToStore(ExecutionContext executionContext, CircuitBreakerDistributedState state)
    {
        if (!_hasDistributedState || _isSynchronizingFromRemote)
            return;

        _ = PublishStateToStoreAsync(executionContext, state);
    }

    private async Task PublishStateToStoreAsync(ExecutionContext executionContext, CircuitBreakerDistributedState state)
    {
        try
        {
            var updatedAt = _timeProvider.GetUtcNow();
            await _stateStore!.UpdateStateAsync(executionContext, _policyCode, state, updatedAt, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogExceptionForDistributedTracing(
                executionContext,
                ex,
                "Resilience policy {PolicyName} failed to publish circuit state {State} to distributed store",
                GetType().Name,
                state);
        }
    }

    // ================================
    // Distributed State — Polling
    // ================================

    private ExecutionContext CreateInfrastructureExecutionContext()
    {
        return ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: TenantInfo.Create(Guid.Empty, "System"),
            executionUser: "Bedrock.Resilience",
            executionOrigin: GetType().Name,
            businessOperationCode: "CIRCUIT_BREAKER_SYNC",
            minimumMessageType: MessageType.Warning,
            timeProvider: _timeProvider);
    }

    private async Task PollDistributedStateAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(_pollingInterval);

        while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
        {
            try
            {
                var executionContext = CreateInfrastructureExecutionContext();
                await SynchronizeFromRemoteStateAsync(executionContext, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Resilience policy {PolicyName} failed to poll distributed circuit state",
                    GetType().Name);
            }
        }
    }

    private async Task SynchronizeFromRemoteStateAsync(ExecutionContext executionContext, CancellationToken cancellationToken)
    {
        var entry = await _stateStore!.GetStateAsync(executionContext, _policyCode, cancellationToken).ConfigureAwait(false);

        if (entry is null)
            return;

        if (IsExpiredOpenCircuit(entry))
        {
            await CloseCircuitFromRemoteAsync(cancellationToken).ConfigureAwait(false);
            await PublishStateToStoreAsync(executionContext, CircuitBreakerDistributedState.Closed).ConfigureAwait(false);
            _lastKnownRemoteState = CircuitBreakerDistributedState.Closed;
            return;
        }

        if (entry.State == _lastKnownRemoteState)
            return;

        _lastKnownRemoteState = entry.State;

        switch (entry.State)
        {
            case CircuitBreakerDistributedState.Open:
                await IsolateCircuitFromRemoteAsync(cancellationToken).ConfigureAwait(false);
                break;

            case CircuitBreakerDistributedState.Closed:
                await CloseCircuitFromRemoteAsync(cancellationToken).ConfigureAwait(false);
                break;

            // HalfOpen: let Polly manage the transition naturally — only the instance
            // that opened the circuit should perform the half-open test call.
        }
    }

    /// <summary>
    /// Safety net: if the instance that opened the circuit crashes before closing it,
    /// other instances detect expiration and close the circuit to recover.
    /// Uses 2× breakDuration as the TTL threshold.
    /// </summary>
    private bool IsExpiredOpenCircuit(CircuitBreakerStateEntry entry)
    {
        if (entry.State is not CircuitBreakerDistributedState.Open and not CircuitBreakerDistributedState.HalfOpen)
            return false;

        var expirationThreshold = entry.UpdatedAt + _breakDuration + _breakDuration;
        return _timeProvider.GetUtcNow() > expirationThreshold;
    }

    // ================================
    // Distributed State — Remote Sync
    // ================================

    private async Task IsolateCircuitFromRemoteAsync(CancellationToken cancellationToken)
    {
        _isSynchronizingFromRemote = true;
        try
        {
            await _manualControl!.IsolateAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _isSynchronizingFromRemote = false;
        }
    }

    private async Task CloseCircuitFromRemoteAsync(CancellationToken cancellationToken)
    {
        _isSynchronizingFromRemote = true;
        try
        {
            await _manualControl!.CloseAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _isSynchronizingFromRemote = false;
        }
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
