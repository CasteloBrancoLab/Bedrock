namespace Bedrock.BuildingBlocks.Testing.Benchmarks;

/// <summary>
/// Abstract base class for all Bedrock sustained benchmarks.
/// Provides shared configuration for time-based benchmark loops.
/// <para>
/// Benchmarks should:
/// <list type="bullet">
///   <item>Use <c>[Benchmark]</c> attribute from BenchmarkDotNet.Attributes for discovery</item>
///   <item>Use <c>[GlobalSetup]</c> and <c>[GlobalCleanup]</c> for lifecycle</item>
///   <item>Loop internally for <see cref="DefaultDuration"/></item>
/// </list>
/// </para>
/// <para>
/// The <see cref="SustainedBenchmarkRunner"/> discovers and executes these benchmarks,
/// collecting runtime metrics via <see cref="RuntimeMetricsTracker"/> automatically.
/// </para>
/// </summary>
/// <example>
/// <code>
/// public class MyBenchmark : BenchmarkBase
/// {
///     [GlobalSetup]
///     public void Setup() { /* init resources */ }
///
///     [Benchmark]
///     public async Task MyOperation()
///     {
///         var sw = Stopwatch.StartNew();
///         while (sw.Elapsed &lt; DefaultDuration)
///         {
///             // ... operation ...
///         }
///     }
///
///     [GlobalCleanup]
///     public void Cleanup() { /* dispose resources */ }
/// }
/// </code>
/// </example>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable",
    Justification = "Base class for benchmarks — must be inheritable for discovery")]
public abstract class BenchmarkBase
{
    /// <summary>
    /// Default duration for sustained-loop benchmarks.
    /// Each [Benchmark] method should loop internally for this duration,
    /// allowing RuntimeMetricsTracker to collect enough samples for trend analysis.
    /// </summary>
    public static readonly TimeSpan DefaultDuration = TimeSpan.FromMinutes(2);
}
