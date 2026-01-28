using System.Collections.Concurrent;

namespace Bedrock.BuildingBlocks.Testing.Benchmarks;

/// <summary>
/// Thread-safe static store for passing <see cref="RuntimeAnalysisResult"/> from benchmark
/// [GlobalCleanup] methods to the <see cref="BenchmarkPendingExporter"/>.
/// BenchmarkDotNet runs benchmarks in a separate process, so results are stored statically
/// and consumed during the export phase within the same process lifetime.
/// </summary>
public static class RuntimeMetricsStore
{
    private static readonly ConcurrentDictionary<string, RuntimeAnalysisResult> Results = new();

    /// <summary>
    /// Records an analysis result for a benchmark.
    /// Called from [GlobalCleanup] after <see cref="RuntimeMetricsTracker.Analyze()"/>.
    /// </summary>
    /// <param name="benchmarkName">Unique name identifying the benchmark class.</param>
    /// <param name="result">The consolidated runtime analysis result.</param>
    public static void Record(string benchmarkName, RuntimeAnalysisResult result)
    {
        Results[benchmarkName] = result;
    }

    /// <summary>
    /// Retrieves a previously recorded analysis result for a benchmark.
    /// Called by <see cref="BenchmarkPendingExporter"/> during export.
    /// </summary>
    /// <param name="benchmarkName">Unique name identifying the benchmark class.</param>
    /// <returns>The analysis result, or null if not found.</returns>
    public static RuntimeAnalysisResult? Get(string benchmarkName)
    {
        return Results.GetValueOrDefault(benchmarkName);
    }

    /// <summary>
    /// Clears all stored results. Useful for test isolation.
    /// </summary>
    public static void Clear()
    {
        Results.Clear();
    }
}
