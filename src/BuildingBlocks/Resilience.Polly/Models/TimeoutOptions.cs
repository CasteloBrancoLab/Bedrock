namespace Bedrock.BuildingBlocks.Resilience.Polly.Models;

/// <summary>
/// Fluent configuration for the per-attempt timeout strategy within a Polly resilience policy.
/// </summary>
/// <remarks>
/// The timeout wraps each individual handler invocation (per-attempt, not total).
/// If a handler exceeds the timeout, the attempt is cancelled and the retry strategy
/// can retry with a fresh timeout window.
/// </remarks>
public sealed class TimeoutOptions
{
    internal TimeSpan Duration { get; private set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Sets the maximum duration for each handler invocation.
    /// If the handler exceeds this duration, a <c>TimeoutRejectedException</c> is thrown.
    /// Default is 30 seconds.
    /// </summary>
    public TimeoutOptions WithDuration(TimeSpan duration)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(duration, TimeSpan.Zero);

        Duration = duration;
        return this;
    }
}
