using System.Globalization;
using System.Text;
using System.Text.Json;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;

namespace Bedrock.BuildingBlocks.Testing.Benchmarks;

/// <summary>
/// Custom BenchmarkDotNet exporter that generates LLM-friendly text files
/// in the <c>artifacts/pending/</c> directory, following the same key-value format
/// used by <c>summarize.sh</c> for mutants, coverage, and SonarCloud issues.
/// <para>
/// Each benchmark produces a <c>benchmark_&lt;name&gt;.txt</c> file combining
/// BenchmarkDotNet performance metrics with <see cref="RuntimeMetricsStore"/> memory/CPU/network data.
/// </para>
/// </summary>
public sealed class BenchmarkPendingExporter : IExporter
{
    private const string ArtifactsPendingDir = "artifacts/pending";

    /// <inheritdoc />
    public string Name => "PendingExporter";

    /// <inheritdoc />
    public IEnumerable<string> ExportToFiles(Summary summary, ILogger consoleLogger)
    {
        var files = new List<string>();

        var pendingDir = Path.Combine(
            summary.ResultsDirectoryPath ?? Directory.GetCurrentDirectory(),
            "..", "..", "..", "..", ArtifactsPendingDir);

        // Also try from current working directory
        var cwdPendingDir = Path.Combine(Directory.GetCurrentDirectory(), ArtifactsPendingDir);
        var outputDir = Directory.Exists(Path.GetDirectoryName(cwdPendingDir))
            ? cwdPendingDir
            : pendingDir;

        Directory.CreateDirectory(outputDir);

        foreach (var report in summary.Reports)
        {
            var benchmarkName = report.BenchmarkCase.Descriptor.Type.Name;
            var methodName = report.BenchmarkCase.Descriptor.WorkloadMethod.Name;
            var fullName = $"{benchmarkName}.{methodName}";
            var safeFileName = fullName.Replace('.', '_').Replace(' ', '_');

            var sb = new StringBuilder();
            sb.AppendLine(CultureInfo.InvariantCulture, $"BENCHMARK: {fullName}");

            // Performance metrics from BenchmarkDotNet
            if (report.ResultStatistics is { } stats)
            {
                var meanNs = stats.Mean;
                var meanFormatted = FormatTime(meanNs);
                sb.AppendLine(CultureInfo.InvariantCulture, $"MEAN_TIME: {meanFormatted}");
                sb.AppendLine(CultureInfo.InvariantCulture, $"MEDIAN_TIME: {FormatTime(stats.Median)}");
                sb.AppendLine(CultureInfo.InvariantCulture, $"STDDEV: {FormatTime(stats.StandardDeviation)}");
            }

            // Memory allocation from MemoryDiagnoser
            if (report.GcStats.GetBytesAllocatedPerOperation(report.BenchmarkCase) is { } allocatedBytes)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"ALLOCATED: {FormatBytes(allocatedBytes)}");
            }

            // Runtime metrics from RuntimeMetricsStore
            var runtimeResult = RuntimeMetricsStore.Get(benchmarkName);
            if (runtimeResult is not null)
            {
                var status = runtimeResult.MemoryGrowth == MemoryGrowthStatus.Growing ? "WARN" : "PASS";
                sb.Insert(sb.ToString().IndexOf('\n') + 1,
                    $"STATUS: {status}{Environment.NewLine}");

                sb.AppendLine(CultureInfo.InvariantCulture, $"MEMORY_GROWTH: {runtimeResult.MemoryGrowth.ToString().ToUpperInvariant()}");
                sb.AppendLine(CultureInfo.InvariantCulture, $"MEMORY_SAMPLES: {runtimeResult.Samples.Count}");
                sb.AppendLine(CultureInfo.InvariantCulture, $"GC_GEN0: {runtimeResult.TotalGen0}");
                sb.AppendLine(CultureInfo.InvariantCulture, $"GC_GEN1: {runtimeResult.TotalGen1}");
                sb.AppendLine(CultureInfo.InvariantCulture, $"GC_GEN2: {runtimeResult.TotalGen2}");
                sb.AppendLine(CultureInfo.InvariantCulture, $"INITIAL_HEAP_MB: {runtimeResult.InitialHeapMb:F2}");
                sb.AppendLine(CultureInfo.InvariantCulture, $"FINAL_HEAP_MB: {runtimeResult.FinalHeapMb:F2}");
                sb.AppendLine(CultureInfo.InvariantCulture, $"HEAP_GROWTH_PERCENT: {runtimeResult.HeapGrowthPercent:F2}");
                sb.AppendLine(CultureInfo.InvariantCulture, $"AVG_CPU_PERCENT: {runtimeResult.AvgCpuPercent:F2}");
                sb.AppendLine(CultureInfo.InvariantCulture, $"PEAK_CPU_PERCENT: {runtimeResult.PeakCpuPercent:F2}");
                sb.AppendLine(CultureInfo.InvariantCulture, $"NETWORK_BYTES_SENT: {runtimeResult.TotalNetworkBytesSent}");
                sb.AppendLine(CultureInfo.InvariantCulture, $"NETWORK_BYTES_RECEIVED: {runtimeResult.TotalNetworkBytesReceived}");
            }
            else
            {
                sb.Insert(sb.ToString().IndexOf('\n') + 1,
                    $"STATUS: PASS{Environment.NewLine}");
            }

            var filePath = Path.Combine(outputDir, $"benchmark_{safeFileName}.txt");
            File.WriteAllText(filePath, sb.ToString());
            files.Add(filePath);

            consoleLogger.WriteLineInfo($"[PendingExporter] Generated: {filePath}");

            // Export runtime samples as JSON for timeline charts in the HTML report
            if (runtimeResult is not null && runtimeResult.Samples.Count > 0)
            {
                var samplesPath = Path.Combine(outputDir, $"benchmark_{safeFileName}_samples.json");
                ExportSamplesJson(runtimeResult.Samples, samplesPath);
                consoleLogger.WriteLineInfo($"[PendingExporter] Samples: {samplesPath} ({runtimeResult.Samples.Count} samples)");
            }
        }

        // Generate consolidated summary for benchmarks
        GenerateBenchmarkSummary(outputDir, files, consoleLogger);

        return files;
    }

    /// <inheritdoc />
    public void ExportToLog(Summary summary, ILogger logger)
    {
        // Not used — we export to files instead
    }

    private static void GenerateBenchmarkSummary(string outputDir, List<string> benchmarkFiles, ILogger logger)
    {
        var summaryPath = Path.Combine(outputDir, "SUMMARY.txt");
        var sb = new StringBuilder();

        // Read existing summary if present
        if (File.Exists(summaryPath))
        {
            sb.Append(File.ReadAllText(summaryPath));
        }

        sb.AppendLine();
        sb.AppendLine("----------------------------------------");
        sb.AppendLine("BENCHMARKS:");
        sb.AppendLine("----------------------------------------");
        sb.AppendLine(CultureInfo.InvariantCulture, $"TOTAL: {benchmarkFiles.Count}");

        var warnCount = 0;
        foreach (var file in benchmarkFiles)
        {
            var content = File.ReadAllText(file);
            if (content.Contains("STATUS: WARN", StringComparison.Ordinal))
                warnCount++;

            var benchmarkLine = ExtractField(content, "BENCHMARK");
            var statusLine = ExtractField(content, "STATUS");
            var meanLine = ExtractField(content, "MEAN_TIME");
            var memoryLine = ExtractField(content, "MEMORY_GROWTH");

            sb.AppendLine(CultureInfo.InvariantCulture, $"  [{statusLine}] {benchmarkLine} - {meanLine} (memory: {memoryLine})");
        }

        sb.Insert(sb.ToString().LastIndexOf("TOTAL:", StringComparison.Ordinal) + "TOTAL:".Length + benchmarkFiles.Count.ToString().Length + 2,
            $"WARNINGS: {warnCount} (memory growth detected){Environment.NewLine}");

        File.WriteAllText(summaryPath, sb.ToString());
        logger.WriteLineInfo($"[PendingExporter] Summary updated: {summaryPath}");
    }

    private static string ExtractField(string content, string fieldName)
    {
        foreach (var line in content.Split('\n'))
        {
            if (line.StartsWith($"{fieldName}:", StringComparison.Ordinal))
            {
                return line[($"{fieldName}:".Length)..].Trim();
            }
        }

        return "N/A";
    }

    private static void ExportSamplesJson(IReadOnlyList<RuntimeSample> samples, string path)
    {
        using var stream = File.Create(path);
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = false });

        writer.WriteStartArray();
        foreach (var s in samples)
        {
            writer.WriteStartObject();
            writer.WriteNumber("t", s.Timestamp.ToUnixTimeSeconds());
            writer.WriteNumber("heap", Math.Round(s.GcHeapSizeMb, 2));
            writer.WriteNumber("ws", Math.Round(s.WorkingSetMb, 2));
            writer.WriteNumber("cpu", Math.Round(s.CpuUsagePercent, 2));
            writer.WriteNumber("g0", s.Gen0Count);
            writer.WriteNumber("g1", s.Gen1Count);
            writer.WriteNumber("g2", s.Gen2Count);
            writer.WriteNumber("netS", s.NetworkBytesSent);
            writer.WriteNumber("netR", s.NetworkBytesReceived);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
    }

    private static string FormatTime(double nanoseconds)
    {
        return nanoseconds switch
        {
            < 1_000 => $"{nanoseconds:F2}ns",
            < 1_000_000 => $"{nanoseconds / 1_000:F2}us",
            < 1_000_000_000 => $"{nanoseconds / 1_000_000:F2}ms",
            _ => $"{nanoseconds / 1_000_000_000:F2}s"
        };
    }

    private static string FormatBytes(long bytes)
    {
        return bytes switch
        {
            < 1024 => $"{bytes}B",
            < 1024 * 1024 => $"{bytes / 1024.0:F2}KB",
            < 1024 * 1024 * 1024 => $"{bytes / (1024.0 * 1024):F2}MB",
            _ => $"{bytes / (1024.0 * 1024 * 1024):F2}GB"
        };
    }
}
