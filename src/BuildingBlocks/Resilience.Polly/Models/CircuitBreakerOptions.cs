using Polly;

namespace Bedrock.BuildingBlocks.Resilience.Polly.Models;

/// <summary>
/// Fluent configuration for the circuit breaker strategy within a Polly resilience policy.
/// </summary>
public sealed class CircuitBreakerOptions
{
    private readonly List<Action<PredicateBuilder>> _exceptionFilters = [];

    internal double FailureRatio { get; private set; } = 0.5;
    internal int MinimumThroughput { get; private set; } = 10;
    internal TimeSpan BreakDuration { get; private set; } = TimeSpan.FromSeconds(30);
    internal TimeSpan SamplingDuration { get; private set; } = TimeSpan.FromSeconds(60);
    internal Action<ExecutionContext>? OnOpenedCallback { get; private set; }
    internal Action<ExecutionContext>? OnClosedCallback { get; private set; }
    internal Action<ExecutionContext>? OnHalfOpenedCallback { get; private set; }
    internal bool HasExceptionFilters => _exceptionFilters.Count > 0;
    internal IReadOnlyList<Action<PredicateBuilder>> ExceptionFilters => _exceptionFilters;

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

    /// <summary>
    /// Adds an exception type that the circuit breaker should count toward its failure ratio.
    /// Only exceptions matching at least one <c>Handle</c> filter are counted;
    /// unmatched exceptions do not affect the circuit state.
    /// If no filters are configured, all exceptions are counted (default behavior).
    /// </summary>
    public CircuitBreakerOptions Handle<TException>() where TException : Exception
    {
        _exceptionFilters.Add(pb => pb.Handle<TException>());
        return this;
    }

    /// <summary>
    /// Adds a conditional exception filter for the circuit breaker.
    /// Only exceptions of the specified type that match the predicate are counted.
    /// </summary>
    public CircuitBreakerOptions Handle<TException>(Func<TException, bool> predicate) where TException : Exception
    {
        ArgumentNullException.ThrowIfNull(predicate);

        _exceptionFilters.Add(pb => pb.Handle(predicate));
        return this;
    }
}
