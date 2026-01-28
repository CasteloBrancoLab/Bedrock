using System.Collections.Concurrent;
using System.Diagnostics;

namespace Bedrock.BuildingBlocks.Testing.Benchmarks;

/// <summary>
/// Cross-platform runtime metrics tracker using polling.
/// Actively collects GC, memory, and CPU metrics at 1-second intervals
/// throughout the lifetime of a benchmark execution.
/// <para>
/// Metrics collected:
/// <list type="bullet">
///   <item>GC heap size (MB) via <c>GC.GetTotalMemory</c></item>
///   <item>Working set (MB) via <c>Environment.WorkingSet</c></item>
///   <item>GC collection counts (Gen 0/1/2) via <c>GC.CollectionCount</c></item>
///   <item>CPU usage (%) via <c>Process.TotalProcessorTime</c> delta</item>
///   <item>Network bytes sent/received (placeholder — requires EventSource for real data)</item>
/// </list>
/// </para>
/// <para>
/// Uses a <see cref="Timer"/> for polling instead of <c>EventListener</c>
/// to ensure compatibility with BenchmarkDotNet's InProcess toolchain.
/// </para>
/// </summary>
public sealed class RuntimeMetricsTracker : IDisposable
{
    private const int SampleIntervalMs = 1000;

    /// <summary>
    /// Threshold percentage for heap growth to be considered "Growing".
    /// If the final heap average exceeds the initial heap average by more than this percentage,
    /// the memory growth status is set to Growing.
    /// </summary>
    private const double GrowthThresholdPercent = 20.0;

    private readonly ConcurrentBag<RuntimeSample> _samples = [];
    private readonly Timer _timer;
    private readonly Process _process;
    private readonly int _processorCount;
    private TimeSpan _lastCpuTime;
    private DateTime _lastCpuCheck;
    private bool _disposed;

    /// <summary>
    /// Creates a new RuntimeMetricsTracker and starts collecting samples immediately.
    /// </summary>
    public RuntimeMetricsTracker()
    {
        _process = Process.GetCurrentProcess();
        _processorCount = Environment.ProcessorCount;
        _lastCpuTime = _process.TotalProcessorTime;
        _lastCpuCheck = DateTime.UtcNow;

        // Start polling immediately, then every interval
        _timer = new Timer(CaptureSample, null, 0, SampleIntervalMs);
    }

    /// <summary>
    /// Gets all samples collected so far.
    /// </summary>
    public IReadOnlyList<RuntimeSample> Samples => [.. _samples.OrderBy(s => s.Timestamp)];

    /// <summary>
    /// Analyzes all collected samples and produces a consolidated <see cref="RuntimeAnalysisResult"/>.
    /// Compares the first quarter vs last quarter of heap samples to determine memory growth trend.
    /// </summary>
    /// <returns>Consolidated analysis of all runtime metrics collected during the benchmark.</returns>
    public RuntimeAnalysisResult Analyze()
    {
        var orderedSamples = Samples;

        if (orderedSamples.Count == 0)
        {
            return new RuntimeAnalysisResult(
                MemoryGrowth: MemoryGrowthStatus.Stable,
                Samples: orderedSamples,
                InitialHeapMb: 0, FinalHeapMb: 0, HeapGrowthPercent: 0,
                TotalGen0: 0, TotalGen1: 0, TotalGen2: 0,
                AvgCpuPercent: 0, PeakCpuPercent: 0,
                TotalNetworkBytesSent: 0, TotalNetworkBytesReceived: 0);
        }

        // Divide samples into quarters for trend analysis
        var quarterSize = Math.Max(1, orderedSamples.Count / 4);
        var firstQuarter = orderedSamples.Take(quarterSize).ToList();
        var lastQuarter = orderedSamples.Skip(orderedSamples.Count - quarterSize).ToList();

        var initialHeap = firstQuarter.Average(s => s.GcHeapSizeMb);
        var finalHeap = lastQuarter.Average(s => s.GcHeapSizeMb);
        var growthPercent = initialHeap > 0 ? ((finalHeap - initialHeap) / initialHeap) * 100.0 : 0;

        var memoryGrowth = growthPercent > GrowthThresholdPercent
            ? MemoryGrowthStatus.Growing
            : MemoryGrowthStatus.Stable;

        // GC totals: difference between last and first sample
        var firstSample = orderedSamples[0];
        var lastSample = orderedSamples[^1];
        var totalGen0 = lastSample.Gen0Count - firstSample.Gen0Count;
        var totalGen1 = lastSample.Gen1Count - firstSample.Gen1Count;
        var totalGen2 = lastSample.Gen2Count - firstSample.Gen2Count;

        // CPU statistics
        var avgCpu = orderedSamples.Average(s => s.CpuUsagePercent);
        var peakCpu = orderedSamples.Max(s => s.CpuUsagePercent);

        // Network totals from last sample (cumulative counters)
        var totalBytesSent = lastSample.NetworkBytesSent;
        var totalBytesReceived = lastSample.NetworkBytesReceived;

        return new RuntimeAnalysisResult(
            MemoryGrowth: memoryGrowth,
            Samples: orderedSamples,
            InitialHeapMb: Math.Round(initialHeap, 2),
            FinalHeapMb: Math.Round(finalHeap, 2),
            HeapGrowthPercent: Math.Round(growthPercent, 2),
            TotalGen0: totalGen0,
            TotalGen1: totalGen1,
            TotalGen2: totalGen2,
            AvgCpuPercent: Math.Round(avgCpu, 2),
            PeakCpuPercent: Math.Round(peakCpu, 2),
            TotalNetworkBytesSent: totalBytesSent,
            TotalNetworkBytesReceived: totalBytesReceived);
    }

    private void CaptureSample(object? state)
    {
        if (_disposed)
            return;

        try
        {
            // Memory
            var heapBytes = GC.GetTotalMemory(false);
            var heapMb = heapBytes / (1024.0 * 1024.0);
            var workingSetMb = Environment.WorkingSet / (1024.0 * 1024.0);

            // GC counts
            var gen0 = (long)GC.CollectionCount(0);
            var gen1 = (long)GC.CollectionCount(1);
            var gen2 = (long)GC.CollectionCount(2);

            // CPU usage via delta
            var now = DateTime.UtcNow;
            _process.Refresh();
            var currentCpuTime = _process.TotalProcessorTime;
            var elapsedTime = now - _lastCpuCheck;
            var cpuTimeDelta = currentCpuTime - _lastCpuTime;

            var cpuPercent = elapsedTime.TotalMilliseconds > 0
                ? (cpuTimeDelta.TotalMilliseconds / (elapsedTime.TotalMilliseconds * _processorCount)) * 100.0
                : 0.0;

            cpuPercent = Math.Clamp(cpuPercent, 0, 100);

            _lastCpuTime = currentCpuTime;
            _lastCpuCheck = now;

            _samples.Add(new RuntimeSample(
                Timestamp: DateTimeOffset.UtcNow,
                GcHeapSizeMb: Math.Round(heapMb, 2),
                WorkingSetMb: Math.Round(workingSetMb, 2),
                Gen0Count: gen0,
                Gen1Count: gen1,
                Gen2Count: gen2,
                CpuUsagePercent: Math.Round(cpuPercent, 2),
                NetworkBytesSent: 0,
                NetworkBytesReceived: 0));
        }
        catch (InvalidOperationException)
        {
            // Process may have exited — ignore
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _timer.Dispose();
    }
}
