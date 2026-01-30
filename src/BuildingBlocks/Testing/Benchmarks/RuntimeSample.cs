namespace Bedrock.BuildingBlocks.Testing.Benchmarks;

/// <summary>
/// Represents a single point-in-time sample of runtime metrics collected via EventCounters.
/// Captured at regular intervals during benchmark execution for trend analysis.
/// </summary>
/// <param name="Timestamp">When the sample was captured.</param>
/// <param name="GcHeapSizeMb">GC heap size in megabytes (from System.Runtime gc-heap-size).</param>
/// <param name="WorkingSetMb">Process working set in megabytes (from System.Runtime working-set).</param>
/// <param name="Gen0Count">Cumulative Gen 0 GC collection count.</param>
/// <param name="Gen1Count">Cumulative Gen 1 GC collection count.</param>
/// <param name="Gen2Count">Cumulative Gen 2 GC collection count.</param>
/// <param name="CpuUsagePercent">CPU usage percentage (from System.Runtime cpu-usage).</param>
/// <param name="NetworkBytesSent">Cumulative network bytes sent (from System.Net.Sockets bytes-sent).</param>
/// <param name="NetworkBytesReceived">Cumulative network bytes received (from System.Net.Sockets bytes-received).</param>
/// <param name="GcPauseTimePercent">Cumulative percentage of time spent in GC pauses since process start (from GCMemoryInfo.PauseTimePercentage).</param>
/// <param name="GcPauseDurationMs">Cumulative total GC pause duration in milliseconds since process start (from GC.GetTotalPauseDuration).</param>
public sealed record RuntimeSample(
    DateTimeOffset Timestamp,
    double GcHeapSizeMb,
    double WorkingSetMb,
    long Gen0Count,
    long Gen1Count,
    long Gen2Count,
    double CpuUsagePercent,
    long NetworkBytesSent,
    long NetworkBytesReceived,
    double GcPauseTimePercent,
    double GcPauseDurationMs);
