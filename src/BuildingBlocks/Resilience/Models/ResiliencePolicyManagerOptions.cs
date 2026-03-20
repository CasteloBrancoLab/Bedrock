namespace Bedrock.BuildingBlocks.Resilience.Models;

/// <summary>
/// Fluent configuration for the <see cref="IResiliencePolicyManager"/>.
/// </summary>
public sealed class ResiliencePolicyManagerOptions
{
    internal TimeSpan PollingInterval { get; private set; } = TimeSpan.FromSeconds(5);
    internal TimeSpan OpenCircuitExpirationThreshold { get; private set; } = TimeSpan.FromMinutes(2);

    /// <summary>
    /// Sets the interval for polling the distributed state store.
    /// Default is 5 seconds.
    /// </summary>
    public ResiliencePolicyManagerOptions WithPollingInterval(TimeSpan interval)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(interval, TimeSpan.Zero);

        PollingInterval = interval;
        return this;
    }

    /// <summary>
    /// Sets the TTL threshold for open circuits in the state store.
    /// If a circuit has been open longer than this threshold, it is considered expired
    /// (the instance that opened it may have crashed) and other instances close it.
    /// Default is 2 minutes.
    /// </summary>
    public ResiliencePolicyManagerOptions WithOpenCircuitExpirationThreshold(TimeSpan threshold)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(threshold, TimeSpan.Zero);

        OpenCircuitExpirationThreshold = threshold;
        return this;
    }
}
