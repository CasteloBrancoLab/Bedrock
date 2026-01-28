namespace Bedrock.BuildingBlocks.Testing.Benchmarks;

/// <summary>
/// Consolidated analysis of runtime metrics collected during a benchmark execution.
/// Includes memory growth assessment, GC statistics, CPU usage, and network I/O totals.
/// </summary>
/// <param name="MemoryGrowth">Whether heap memory stabilized or continued growing.</param>
/// <param name="Samples">All runtime samples collected during the benchmark.</param>
/// <param name="InitialHeapMb">Average heap size from the first quarter of samples.</param>
/// <param name="FinalHeapMb">Average heap size from the last quarter of samples.</param>
/// <param name="HeapGrowthPercent">Percentage growth from initial to final heap size.</param>
/// <param name="TotalGen0">Total Gen 0 GC collections during the benchmark.</param>
/// <param name="TotalGen1">Total Gen 1 GC collections during the benchmark.</param>
/// <param name="TotalGen2">Total Gen 2 GC collections during the benchmark.</param>
/// <param name="AvgCpuPercent">Average CPU usage percentage across all samples.</param>
/// <param name="PeakCpuPercent">Peak CPU usage percentage observed.</param>
/// <param name="TotalNetworkBytesSent">Total network bytes sent during the benchmark.</param>
/// <param name="TotalNetworkBytesReceived">Total network bytes received during the benchmark.</param>
public sealed record RuntimeAnalysisResult(
    MemoryGrowthStatus MemoryGrowth,
    IReadOnlyList<RuntimeSample> Samples,
    double InitialHeapMb,
    double FinalHeapMb,
    double HeapGrowthPercent,
    long TotalGen0,
    long TotalGen1,
    long TotalGen2,
    double AvgCpuPercent,
    double PeakCpuPercent,
    long TotalNetworkBytesSent,
    long TotalNetworkBytesReceived);
