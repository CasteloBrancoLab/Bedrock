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
/// </remarks>
public abstract class PollyResiliencePolicyBase : IResiliencePolicy
{
    private static readonly ResiliencePropertyKey<ExecutionContext> ExecutionContextKey = new("Bedrock.ExecutionContext");
    private static readonly ResiliencePropertyKey<object?> InputKey = new("Bedrock.Input");

    private readonly ILogger _logger;
    private readonly ResiliencePipeline _pipeline;
    private readonly bool _hasRetry;

    /// <summary>
    /// Initializes the resilience policy by building the Polly pipeline from subclass configuration.
    /// </summary>
    /// <param name="logger">The logger for distributed tracing.</param>
    protected PollyResiliencePolicyBase(ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;

        var options = new PollyResiliencePolicyOptions();
        ConfigureInternal(options);

        _hasRetry = options.Retry is not null;
        _pipeline = BuildPipeline(options);
    }

    /// <summary>
    /// Configures the resilience policy options. Called once during construction.
    /// </summary>
    /// <param name="options">The options to configure with retry, circuit breaker, and time provider settings.</param>
    protected abstract void ConfigureInternal(PollyResiliencePolicyOptions options);

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

        builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions
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
        });
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
    }

    private void HandleCircuitClosed(OnCircuitClosedArguments<object> args, CircuitBreakerOptions options)
    {
        var executionContext = GetExecutionContextFromResilienceContext(args.Context);

        _logger.LogInformationForDistributedTracing(
            executionContext,
            "Resilience policy {PolicyName} circuit breaker closed",
            GetType().Name);

        options.OnClosedCallback?.Invoke(executionContext);
    }

    private void HandleCircuitHalfOpened(OnCircuitHalfOpenedArguments args, CircuitBreakerOptions options)
    {
        var executionContext = GetExecutionContextFromResilienceContext(args.Context);

        _logger.LogWarningForDistributedTracing(
            executionContext,
            "Resilience policy {PolicyName} circuit breaker half-opened",
            GetType().Name);

        options.OnHalfOpenedCallback?.Invoke(executionContext);
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
        _logger.LogInformationForDistributedTracing(
            executionContext,
            "Resilience policy {PolicyName} execution started",
            GetType().Name);
    }

    private void LogExecutionSucceeded(ExecutionContext executionContext)
    {
        _logger.LogInformationForDistributedTracing(
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
