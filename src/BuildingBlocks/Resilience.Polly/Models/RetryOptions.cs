using Polly;

namespace Bedrock.BuildingBlocks.Resilience.Polly.Models;

/// <summary>
/// Fluent configuration for the retry strategy within a Polly resilience policy.
/// </summary>
public sealed class RetryOptions
{
    private readonly List<Action<PredicateBuilder>> _exceptionFilters = [];

    internal int MaxAttempts { get; private set; } = 3;
    internal Func<ExecutionContext, object?, int, TimeSpan>? JitterStrategy { get; private set; }
    internal bool HasExceptionFilters => _exceptionFilters.Count > 0;
    internal IReadOnlyList<Action<PredicateBuilder>> ExceptionFilters => _exceptionFilters;

    /// <summary>
    /// Sets the maximum number of retry attempts after the initial call.
    /// Default is 3 (4 total attempts including the initial call).
    /// </summary>
    public RetryOptions WithMaxAttempts(int maxAttempts)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxAttempts, 1);

        MaxAttempts = maxAttempts;
        return this;
    }

    /// <summary>
    /// Sets a custom jitter strategy that computes the delay before each retry attempt.
    /// </summary>
    /// <param name="jitterStrategy">
    /// A function receiving the execution context, the input value (or <c>null</c>),
    /// and the zero-based attempt number; returns the delay <see cref="TimeSpan"/> before the next retry.
    /// </param>
    public RetryOptions WithJitterStrategy(Func<ExecutionContext, object?, int, TimeSpan> jitterStrategy)
    {
        ArgumentNullException.ThrowIfNull(jitterStrategy);

        JitterStrategy = jitterStrategy;
        return this;
    }

    /// <summary>
    /// Adds an exception type that the retry strategy should handle.
    /// Only exceptions matching at least one <c>Handle</c> filter are retried;
    /// unmatched exceptions propagate immediately.
    /// If no filters are configured, all exceptions are retried (default behavior).
    /// </summary>
    public RetryOptions Handle<TException>() where TException : Exception
    {
        _exceptionFilters.Add(pb => pb.Handle<TException>());
        return this;
    }

    /// <summary>
    /// Adds a conditional exception filter for the retry strategy.
    /// Only exceptions of the specified type that match the predicate are retried.
    /// </summary>
    public RetryOptions Handle<TException>(Func<TException, bool> predicate) where TException : Exception
    {
        ArgumentNullException.ThrowIfNull(predicate);

        _exceptionFilters.Add(pb => pb.Handle(predicate));
        return this;
    }
}
