namespace Bedrock.BuildingBlocks.Resilience.Polly.Models;

/// <summary>
/// Fluent configuration for the circuit breaker strategy within a Polly resilience policy.
/// </summary>
public sealed class CircuitBreakerOptions
{
    internal double FailureRatio { get; private set; } = 0.5;
    internal int MinimumThroughput { get; private set; } = 10;
    internal TimeSpan BreakDuration { get; private set; } = TimeSpan.FromSeconds(30);
    internal TimeSpan SamplingDuration { get; private set; } = TimeSpan.FromSeconds(60);
    internal Action<ExecutionContext>? OnOpenedCallback { get; private set; }
    internal Action<ExecutionContext>? OnClosedCallback { get; private set; }
    internal Action<ExecutionContext>? OnHalfOpenedCallback { get; private set; }

    /// <summary>
    /// Sets the failure ratio threshold within the sampling window that triggers the circuit to open.
    /// Value must be between 0.0 (exclusive) and 1.0 (inclusive). Default is 0.5 (50%).
    /// </summary>
    public CircuitBreakerOptions WithFailureRatio(double ratio)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(ratio, 0.0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(ratio, 1.0);

        FailureRatio = ratio;
        return this;
    }

    /// <summary>
    /// Sets the minimum number of calls within the sampling window before the failure ratio is evaluated.
    /// Prevents the circuit from opening on insufficient data. Default is 10.
    /// </summary>
    public CircuitBreakerOptions WithMinimumThroughput(int throughput)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(throughput, 1);

        MinimumThroughput = throughput;
        return this;
    }

    /// <summary>
    /// Sets how long the circuit remains open before transitioning to half-open.
    /// During this period, all calls are rejected with <see cref="Models.ResiliencePolicyFailureReason.CircuitOpen"/>.
    /// Default is 30 seconds.
    /// </summary>
    public CircuitBreakerOptions WithBreakDuration(TimeSpan duration)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(duration, TimeSpan.Zero);

        BreakDuration = duration;
        return this;
    }

    /// <summary>
    /// Sets the sliding window duration used to evaluate the failure ratio.
    /// Default is 60 seconds.
    /// </summary>
    public CircuitBreakerOptions WithSamplingDuration(TimeSpan duration)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(duration, TimeSpan.Zero);

        SamplingDuration = duration;
        return this;
    }

    /// <summary>
    /// Registers a callback invoked when the circuit transitions to the <b>open</b> state.
    /// </summary>
    public CircuitBreakerOptions OnOpened(Action<ExecutionContext> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        OnOpenedCallback = callback;
        return this;
    }

    /// <summary>
    /// Registers a callback invoked when the circuit transitions back to the <b>closed</b> state.
    /// </summary>
    public CircuitBreakerOptions OnClosed(Action<ExecutionContext> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        OnClosedCallback = callback;
        return this;
    }

    /// <summary>
    /// Registers a callback invoked when the circuit transitions to the <b>half-open</b> state,
    /// allowing a single test call through.
    /// </summary>
    public CircuitBreakerOptions OnHalfOpened(Action<ExecutionContext> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        OnHalfOpenedCallback = callback;
        return this;
    }
}
