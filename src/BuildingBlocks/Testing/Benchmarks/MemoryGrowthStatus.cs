namespace Bedrock.BuildingBlocks.Testing.Benchmarks;

/// <summary>
/// Indicates whether memory usage stabilized or continued growing during a benchmark run.
/// Determined by comparing the first half vs second half of collected heap samples.
/// </summary>
public enum MemoryGrowthStatus
{
    /// <summary>
    /// Heap size stabilized — no significant upward trend detected.
    /// </summary>
    Stable,

    /// <summary>
    /// Heap size showed a significant upward trend — possible memory leak.
    /// </summary>
    Growing
}
