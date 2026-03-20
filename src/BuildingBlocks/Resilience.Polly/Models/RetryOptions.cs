namespace Bedrock.BuildingBlocks.Resilience.Polly.Models;

/// <summary>
/// Fluent configuration for the retry strategy within a Polly resilience policy.
/// </summary>
public sealed class RetryOptions
{
    internal int MaxAttempts { get; private set; } = 3;
    internal Func<ExecutionContext, object?, int, TimeSpan>? JitterStrategy { get; private set; }

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
}
