using System.Diagnostics;
using System.Diagnostics.Metrics;
using Bedrock.BuildingBlocks.Observability.ExtensionMethods;
using Bedrock.BuildingBlocks.Resilience.Models;
using Bedrock.BuildingBlocks.Resilience.Polly.Models;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace Bedrock.BuildingBlocks.Resilience.Polly;

/// <summary>
/// Abstract base class for Polly-based resilience policies.
/// Composes retry, circuit breaker, and timeout strategies into a single thread-safe pipeline.
/// </summary>
/// <remarks>
/// <para>
/// Designed to be registered as a <b>singleton</b> in the DI container.
/// The resilience pipeline is built once during construction and reused across all concurrent calls.
/// </para>
/// <para>
/// Pipeline composition: CircuitBreaker (outer) → Retry → Timeout (inner) → Handler.
/// The circuit breaker evaluates the final outcome after all retry attempts are exhausted.
/// The timeout applies per-attempt, not to the total operation.
/// </para>
/// <para>
/// Distributed state synchronization is handled externally by <see cref="IResiliencePolicyManager"/>.
/// </para>
/// </remarks>
public abstract class PollyResiliencePolicyBase : IResiliencePolicy
{
    // ================================
    // Metrics (OpenTelemetry-compatible via System.Diagnostics.Metrics)
    // ================================

    private static readonly Meter ResilienceMeter = new("Bedrock.BuildingBlocks.Resilience", "1.0.0");
    private static readonly Counter<long> ExecutionCounter = ResilienceMeter.CreateCounter<long>("bedrock.resilience.executions", description: "Total resilience policy executions");
    private static readonly Counter<long> SuccessCounter = ResilienceMeter.CreateCounter<long>("bedrock.resilience.successes", description: "Successful executions (including fallback)");
    private static readonly Counter<long> FailureCounter = ResilienceMeter.CreateCounter<long>("bedrock.resilience.failures", description: "Failed executions");
    private static readonly Counter<long> FallbackCounter = ResilienceMeter.CreateCounter<long>("bedrock.resilience.fallbacks", description: "Executions that used the fallback handler");
    private static readonly Counter<long> RetryCounter = ResilienceMeter.CreateCounter<long>("bedrock.resilience.retries", description: "Individual retry attempts");
    private static readonly Counter<long> CircuitOpenCounter = ResilienceMeter.CreateCounter<long>("bedrock.resilience.circuit_opens", description: "Circuit breaker open events");
    private static readonly Counter<long> TimeoutCounter = ResilienceMeter.CreateCounter<long>("bedrock.resilience.timeouts", description: "Per-attempt timeout events");
    private static readonly Histogram<double> ExecutionDuration = ResilienceMeter.CreateHistogram<double>("bedrock.resilience.execution_duration_ms", "ms", "Execution duration including retries");

    // ================================
    // Fields
    // ================================

    private static readonly ResiliencePropertyKey<ExecutionContext> ExecutionContextKey = new("Bedrock.ExecutionContext");
    private static readonly ResiliencePropertyKey<object?> InputKey = new("Bedrock.Input");

    private readonly ILogger _logger;
    private readonly ResiliencePipeline _pipeline;
    private readonly bool _hasRetry;
    private readonly CircuitBreakerManualControl? _manualControl;
    private readonly string _policyCode;
    private CircuitStateChangedHandler? _circuitStateChangedCallback;

    /// <summary>
    /// Initializes the resilience policy by building the Polly pipeline from subclass configuration.
    /// </summary>
    protected PollyResiliencePolicyBase(ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;

        var options = new PollyResiliencePolicyOptions();
        ConfigureInternal(options);

        _policyCode = options.PolicyCode ?? GetType().Name;
        _hasRetry = options.Retry is not null;

        if (options.CircuitBreaker is not null)
            _manualControl = new CircuitBreakerManualControl();

        _pipeline = BuildPipeline(options);
    }

    /// <summary>
    /// Configures the resilience policy options. Called once during construction.
    /// </summary>
    protected abstract void ConfigureInternal(PollyResiliencePolicyOptions options);

    // ================================
    // IResiliencePolicy — Identity
    // ================================

    /// <inheritdoc />
    public string PolicyCode => _policyCode;

    // ================================
    // IResiliencePolicy — Execution
    // ================================

    /// <inheritdoc />
    public async Task<ResiliencePolicyExecutionResult<TOutput>> ExecuteAsync<TInput, TOutput>(
        ExecutionContext executionContext,
        TInput input,
        CancellationToken cancellationToken,
        Func<ExecutionContext, TInput, CancellationToken, Task<TOutput>> handler,
        Func<ExecutionContext, ResiliencePolicyFailureReason, Exception?, Task<TOutput>>? fallback = null)
    {
        ArgumentNullException.ThrowIfNull(executionContext);
        ArgumentNullException.ThrowIfNull(handler);

        var resilienceContext = ResilienceContextPool.Shared.Get(cancellationToken);
        var stopwatch = Stopwatch.StartNew();
        var tags = new TagList { { "policy", _policyCode } };

        ExecutionCounter.Add(1, tags);

        try
        {
            SetResilienceContextProperties(resilienceContext, executionContext, input);
            LogExecutionStarted(executionContext);

            var result = await _pipeline.ExecuteAsync(
                async (ctx, state) => await state.handler(state.executionContext, state.input, ctx.CancellationToken).ConfigureAwait(false),
                resilienceContext,
                (executionContext, input, handler)).ConfigureAwait(false);

            LogExecutionSucceeded(executionContext);
            SuccessCounter.Add(1, tags);
            return ResiliencePolicyExecutionResult<TOutput>.CreateSuccess(result);
        }
        catch (BrokenCircuitException ex)
        {
            return await HandleFailureWithOptionalFallback<TOutput>(
                executionContext, ResiliencePolicyFailureReason.CircuitOpen, ex, fallback, tags).ConfigureAwait(false);
        }
        catch (TimeoutRejectedException ex)
        {
            TimeoutCounter.Add(1, tags);
            var reason = _hasRetry
                ? ResiliencePolicyFailureReason.RetriesExhausted
                : ResiliencePolicyFailureReason.Timeout;

            return await HandleFailureWithOptionalFallback<TOutput>(
                executionContext, reason, ex, fallback, tags).ConfigureAwait(false);
        }
        catch (Exception ex) when (_hasRetry)
        {
            return await HandleFailureWithOptionalFallback<TOutput>(
                executionContext, ResiliencePolicyFailureReason.RetriesExhausted, ex, fallback, tags).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return await HandleFailureWithOptionalFallback<TOutput>(
                executionContext, ResiliencePolicyFailureReason.HandlerException, ex, fallback, tags).ConfigureAwait(false);
        }
        finally
        {
            stopwatch.Stop();
            ExecutionDuration.Record(stopwatch.Elapsed.TotalMilliseconds, tags);
            ResilienceContextPool.Shared.Return(resilienceContext);
        }
    }

    /// <inheritdoc />
    public Task<ResiliencePolicyExecutionResult<TOutput>> ExecuteAsync<TOutput>(
        ExecutionContext executionContext,
        CancellationToken cancellationToken,
        Func<ExecutionContext, CancellationToken, Task<TOutput>> handler,
        Func<ExecutionContext, ResiliencePolicyFailureReason, Exception?, Task<TOutput>>? fallback = null)
    {
        ArgumentNullException.ThrowIfNull(handler);

        return ExecuteAsync<object?, TOutput>(
            executionContext,
            input: null,
            cancellationToken,
            handler: (ctx, _, ct) => handler(ctx, ct),
            fallback);
    }

    // ================================
    // IResiliencePolicy — Circuit Management
    // ================================

    /// <inheritdoc />
    public async Task ForceOpenCircuitAsync(CancellationToken cancellationToken)
    {
        if (_manualControl is not null)
            await _manualControl.IsolateAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task ForceCloseCircuitAsync(CancellationToken cancellationToken)
    {
        if (_manualControl is not null)
            await _manualControl.CloseAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void RegisterCircuitStateChangedCallback(CircuitStateChangedHandler handler)
    {
        _circuitStateChangedCallback = handler;
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
        ConfigureTimeoutStrategy(builder, options.Timeout);

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
            ShouldHandle = BuildShouldHandlePredicate(options.HasExceptionFilters, options.ExceptionFilters),
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
            ShouldHandle = BuildShouldHandlePredicate(options.HasExceptionFilters, options.ExceptionFilters),
            DelayGenerator = CreateDelayGenerator(options),
            OnRetry = args =>
            {
                HandleRetryAttemptFailed(args, options);
                return ValueTask.CompletedTask;
            }
        });
    }

    private static void ConfigureTimeoutStrategy(ResiliencePipelineBuilder builder, TimeoutOptions? options)
    {
        if (options is null)
            return;

        builder.AddTimeout(options.Duration);
    }

    private static PredicateBuilder BuildShouldHandlePredicate(
        bool hasFilters,
        IReadOnlyList<Action<PredicateBuilder>> filters)
    {
        var predicateBuilder = new PredicateBuilder();

        if (!hasFilters)
        {
            predicateBuilder.Handle<Exception>();
            return predicateBuilder;
        }

        foreach (var filter in filters)
            filter(predicateBuilder);

        return predicateBuilder;
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
        RetryCounter.Add(1, new TagList { { "policy", _policyCode } });

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
        CircuitOpenCounter.Add(1, new TagList { { "policy", _policyCode } });

        _logger.LogErrorForDistributedTracing(
            executionContext,
            "Resilience policy {PolicyName} circuit breaker opened, break duration: {BreakDuration}",
            GetType().Name,
            options.BreakDuration);

        options.OnOpenedCallback?.Invoke(executionContext);
        _circuitStateChangedCallback?.Invoke(_policyCode, CircuitBreakerDistributedState.Open, executionContext);
    }

    private void HandleCircuitClosed(OnCircuitClosedArguments<object> args, CircuitBreakerOptions options)
    {
        var executionContext = GetExecutionContextFromResilienceContext(args.Context);

        _logger.LogInformationForDistributedTracing(
            executionContext,
            "Resilience policy {PolicyName} circuit breaker closed",
            GetType().Name);

        options.OnClosedCallback?.Invoke(executionContext);
        _circuitStateChangedCallback?.Invoke(_policyCode, CircuitBreakerDistributedState.Closed, executionContext);
    }

    private void HandleCircuitHalfOpened(OnCircuitHalfOpenedArguments args, CircuitBreakerOptions options)
    {
        var executionContext = GetExecutionContextFromResilienceContext(args.Context);

        _logger.LogWarningForDistributedTracing(
            executionContext,
            "Resilience policy {PolicyName} circuit breaker half-opened",
            GetType().Name);

        options.OnHalfOpenedCallback?.Invoke(executionContext);
        _circuitStateChangedCallback?.Invoke(_policyCode, CircuitBreakerDistributedState.HalfOpen, executionContext);
    }

    // ================================
    // Failure Handling with Optional Fallback
    // ================================

    private async Task<ResiliencePolicyExecutionResult<TOutput>> HandleFailureWithOptionalFallback<TOutput>(
        ExecutionContext executionContext,
        ResiliencePolicyFailureReason reason,
        Exception exception,
        Func<ExecutionContext, ResiliencePolicyFailureReason, Exception?, Task<TOutput>>? fallback,
        TagList tags)
    {
        LogFailure(executionContext, reason, exception);

        if (fallback is not null)
        {
            return await ExecuteFallback(executionContext, reason, exception, fallback, tags).ConfigureAwait(false);
        }

        FailureCounter.Add(1, new TagList { { "policy", _policyCode }, { "reason", reason.ToString() } });
        return ResiliencePolicyExecutionResult<TOutput>.CreateFailure(reason, exception);
    }

    private async Task<ResiliencePolicyExecutionResult<TOutput>> ExecuteFallback<TOutput>(
        ExecutionContext executionContext,
        ResiliencePolicyFailureReason originalReason,
        Exception originalException,
        Func<ExecutionContext, ResiliencePolicyFailureReason, Exception?, Task<TOutput>> fallback,
        TagList tags)
    {
        try
        {
            var fallbackValue = await fallback(executionContext, originalReason, originalException).ConfigureAwait(false);

            _logger.LogInformationForDistributedTracing(
                executionContext,
                "Resilience policy {PolicyName} fallback executed successfully for {Reason}",
                GetType().Name,
                originalReason);

            FallbackCounter.Add(1, tags);
            SuccessCounter.Add(1, tags);
            return ResiliencePolicyExecutionResult<TOutput>.CreateFallback(fallbackValue, originalReason, originalException);
        }
        catch (Exception fallbackException)
        {
            _logger.LogExceptionForDistributedTracing(
                executionContext,
                fallbackException,
                "Resilience policy {PolicyName} fallback also failed for {Reason}",
                GetType().Name,
                originalReason);

            FailureCounter.Add(1, new TagList { { "policy", _policyCode }, { "reason", originalReason.ToString() } });
            return ResiliencePolicyExecutionResult<TOutput>.CreateFailure(originalReason, originalException);
        }
    }

    // ================================
    // Logging Helpers
    // ================================

    private void LogFailure(ExecutionContext executionContext, ResiliencePolicyFailureReason reason, Exception exception)
    {
        switch (reason)
        {
            case ResiliencePolicyFailureReason.CircuitOpen:
                _logger.LogWarningForDistributedTracing(
                    executionContext,
                    "Resilience policy {PolicyName} execution rejected, circuit breaker is open",
                    GetType().Name);
                break;

            case ResiliencePolicyFailureReason.RetriesExhausted:
                _logger.LogErrorForDistributedTracing(
                    executionContext,
                    "Resilience policy {PolicyName} all retry attempts exhausted",
                    GetType().Name);
                break;

            case ResiliencePolicyFailureReason.Timeout:
                _logger.LogErrorForDistributedTracing(
                    executionContext,
                    "Resilience policy {PolicyName} handler exceeded timeout",
                    GetType().Name);
                break;

            case ResiliencePolicyFailureReason.HandlerException:
                _logger.LogExceptionForDistributedTracing(
                    executionContext,
                    exception,
                    "Resilience policy {PolicyName} execution failed with unhandled exception",
                    GetType().Name);
                break;
        }
    }

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
}
