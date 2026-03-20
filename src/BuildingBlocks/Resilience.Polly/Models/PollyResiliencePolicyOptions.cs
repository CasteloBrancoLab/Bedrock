namespace Bedrock.BuildingBlocks.Resilience.Polly.Models;

/// <summary>
/// Root fluent configuration for a Polly-based resilience policy.
/// Composes retry and circuit breaker strategies into a single pipeline.
/// </summary>
public sealed class PollyResiliencePolicyOptions
{
    internal RetryOptions? Retry { get; private set; }
    internal CircuitBreakerOptions? CircuitBreaker { get; private set; }
    internal TimeProvider? TimeProvider { get; private set; }

    /// <summary>
    /// Configures the retry strategy.
    /// </summary>
    public PollyResiliencePolicyOptions WithRetry(Action<RetryOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var options = new RetryOptions();
        configure(options);
        Retry = options;
        return this;
    }

    /// <summary>
    /// Configures the circuit breaker strategy.
    /// The circuit breaker wraps the retry — it evaluates the final outcome after all retry attempts.
    /// </summary>
    public PollyResiliencePolicyOptions WithCircuitBreaker(Action<CircuitBreakerOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var options = new CircuitBreakerOptions();
        configure(options);
        CircuitBreaker = options;
        return this;
    }

    /// <summary>
    /// Sets the <see cref="System.TimeProvider"/> used by the resilience pipeline
    /// for measuring retry delays and circuit breaker windows.
    /// Essential for testability.
    /// </summary>
    public PollyResiliencePolicyOptions WithTimeProvider(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        TimeProvider = timeProvider;
        return this;
    }
}
