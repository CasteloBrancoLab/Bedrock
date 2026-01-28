using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Bedrock.BuildingBlocks.Testing.Benchmarks;

/// <summary>
/// Custom benchmark runner for sustained-loop benchmarks.
/// Unlike BenchmarkDotNet (designed for micro-benchmarks), this runner is optimized for
/// long-running I/O workloads (minutes) with continuous runtime metrics collection.
/// <para>
/// Discovers benchmark classes inheriting from <see cref="BenchmarkBase"/> in the given assembly
/// and executes each method marked with <c>[Benchmark]</c> from BenchmarkDotNet.Attributes.
/// Uses <c>[GlobalSetup]</c> and <c>[GlobalCleanup]</c> for lifecycle management.
/// </para>
/// <para>
/// Generates LLM-friendly pending files and samples JSON for the HTML report generator.
/// </para>
/// </summary>
public static class SustainedBenchmarkRunner
{
    private static readonly TimeSpan LogInterval = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Runs all discovered benchmarks from the assembly.
    /// </summary>
    /// <param name="assembly">Assembly containing benchmark classes.</param>
    /// <param name="args">Command-line arguments. Supports --filter pattern.</param>
    public static async Task RunAsync(Assembly assembly, string[] args)
    {
        var filter = ParseFilter(args);
        var benchmarks = DiscoverBenchmarks(assembly, filter);

        if (benchmarks.Count == 0)
        {
            Console.WriteLine("No benchmarks found.");
            return;
        }

        Console.WriteLine($"Found {benchmarks.Count} benchmark(s):");
        foreach (var (type, methods) in benchmarks)
        {
            foreach (var method in methods)
                Console.WriteLine($"  - {type.Name}.{method.Name}");
        }
        Console.WriteLine();

        var pendingDir = Path.Combine(Directory.GetCurrentDirectory(), "artifacts", "pending");
        Directory.CreateDirectory(pendingDir);

        foreach (var (type, methods) in benchmarks)
        {
            var instance = Activator.CreateInstance(type)!;

            // Run GlobalSetup
            InvokeLifecycle(instance, "BenchmarkDotNet.Attributes.GlobalSetupAttribute");

            foreach (var method in methods)
            {
                var fullName = $"{type.Name}.{method.Name}";
                var safeFileName = fullName.Replace('.', '_').Replace(' ', '_');

                Console.WriteLine($">>> Running: {fullName}");
                Console.WriteLine($"    Duration: {BenchmarkBase.DefaultDuration.TotalMinutes:F0} minutes");
                Console.WriteLine();

                // Start metrics tracker
                var tracker = new RuntimeMetricsTracker();
                var sw = Stopwatch.StartNew();

                // Run the benchmark method
                var result = method.Invoke(instance, null);
                if (result is Task task)
                    await task;

                sw.Stop();
                var elapsed = sw.Elapsed;

                // Analyze metrics
                var analysis = tracker.Analyze();
                tracker.Dispose();

                Console.WriteLine();
                Console.WriteLine($"    Elapsed: {elapsed:mm\\:ss\\.fff}");
                Console.WriteLine($"    Samples: {analysis.Samples.Count}");
                Console.WriteLine($"    Heap: {analysis.InitialHeapMb:F2} MB -> {analysis.FinalHeapMb:F2} MB ({analysis.HeapGrowthPercent:F2}%)");
                Console.WriteLine($"    Memory: {analysis.MemoryGrowth}");
                Console.WriteLine($"    CPU avg: {analysis.AvgCpuPercent:F1}%, peak: {analysis.PeakCpuPercent:F1}%");
                Console.WriteLine($"    GC: Gen0={analysis.TotalGen0} Gen1={analysis.TotalGen1} Gen2={analysis.TotalGen2}");
                Console.WriteLine();

                // Export pending file
                var status = analysis.MemoryGrowth == MemoryGrowthStatus.Growing ? "WARN" : "PASS";
                ExportPendingFile(pendingDir, safeFileName, fullName, status, elapsed, analysis);

                // Export samples JSON
                if (analysis.Samples.Count > 0)
                    ExportSamplesJson(pendingDir, safeFileName, analysis.Samples);
            }

            // Run GlobalCleanup
            InvokeLifecycle(instance, "BenchmarkDotNet.Attributes.GlobalCleanupAttribute");
        }

        // Generate summary
        GenerateSummary(pendingDir);
    }

    private static string? ParseFilter(string[] args)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--filter")
                return args[i + 1];
        }

        return null;
    }

    private static List<(Type Type, List<MethodInfo> Methods)> DiscoverBenchmarks(Assembly assembly, string? filter)
    {
        var results = new List<(Type, List<MethodInfo>)>();

        foreach (var type in assembly.GetTypes())
        {
            if (!type.IsSubclassOf(typeof(BenchmarkBase)) || type.IsAbstract)
                continue;

            if (filter is not null && filter != "*" && !MatchesFilter(type.Name, filter))
                continue;

            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.GetCustomAttributes()
                    .Any(a => a.GetType().FullName == "BenchmarkDotNet.Attributes.BenchmarkAttribute"))
                .ToList();

            if (methods.Count > 0)
                results.Add((type, methods));
        }

        return results;
    }

    private static bool MatchesFilter(string name, string pattern)
    {
        // Simple wildcard matching: *Name* style
        if (pattern.StartsWith('*') && pattern.EndsWith('*'))
            return name.Contains(pattern.Trim('*'), StringComparison.OrdinalIgnoreCase);
        if (pattern.StartsWith('*'))
            return name.EndsWith(pattern.TrimStart('*'), StringComparison.OrdinalIgnoreCase);
        if (pattern.EndsWith('*'))
            return name.StartsWith(pattern.TrimEnd('*'), StringComparison.OrdinalIgnoreCase);
        return string.Equals(name, pattern, StringComparison.OrdinalIgnoreCase);
    }

    private static void InvokeLifecycle(object instance, string attributeFullName)
    {
        var method = instance.GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(m => m.GetCustomAttributes()
                .Any(a => a.GetType().FullName == attributeFullName));

        if (method is null)
            return;

        var result = method.Invoke(instance, null);
        if (result is Task task)
            task.GetAwaiter().GetResult();
    }

    private static void ExportPendingFile(string dir, string safeFileName, string fullName, string status,
        TimeSpan elapsed, RuntimeAnalysisResult analysis)
    {
        var sb = new StringBuilder();
        sb.AppendLine(CultureInfo.InvariantCulture, $"BENCHMARK: {fullName}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"STATUS: {status}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"ELAPSED: {elapsed:mm\\:ss\\.fff}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"MEMORY_GROWTH: {analysis.MemoryGrowth.ToString().ToUpperInvariant()}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"MEMORY_SAMPLES: {analysis.Samples.Count}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"GC_GEN0: {analysis.TotalGen0}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"GC_GEN1: {analysis.TotalGen1}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"GC_GEN2: {analysis.TotalGen2}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"INITIAL_HEAP_MB: {analysis.InitialHeapMb:F2}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"FINAL_HEAP_MB: {analysis.FinalHeapMb:F2}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"HEAP_GROWTH_PERCENT: {analysis.HeapGrowthPercent:F2}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"AVG_CPU_PERCENT: {analysis.AvgCpuPercent:F2}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"PEAK_CPU_PERCENT: {analysis.PeakCpuPercent:F2}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"NETWORK_BYTES_SENT: {analysis.TotalNetworkBytesSent}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"NETWORK_BYTES_RECEIVED: {analysis.TotalNetworkBytesReceived}");

        var filePath = Path.Combine(dir, $"benchmark_{safeFileName}.txt");
        File.WriteAllText(filePath, sb.ToString());
        Console.WriteLine($"    [Pending] {filePath}");
    }

    private static void ExportSamplesJson(string dir, string safeFileName, IReadOnlyList<RuntimeSample> samples)
    {
        var filePath = Path.Combine(dir, $"benchmark_{safeFileName}_samples.json");

        using var stream = File.Create(filePath);
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
        Console.WriteLine($"    [Samples] {filePath} ({samples.Count} samples)");
    }

    private static void GenerateSummary(string dir)
    {
        var files = Directory.GetFiles(dir, "benchmark_*.txt").Where(f => !f.EndsWith("_samples.json")).OrderBy(f => f).ToList();
        if (files.Count == 0) return;

        var sb = new StringBuilder();

        // Read existing summary if present
        var summaryPath = Path.Combine(dir, "SUMMARY.txt");
        if (File.Exists(summaryPath))
            sb.Append(File.ReadAllText(summaryPath));

        sb.AppendLine();
        sb.AppendLine("----------------------------------------");
        sb.AppendLine("BENCHMARKS:");
        sb.AppendLine("----------------------------------------");
        sb.AppendLine(CultureInfo.InvariantCulture, $"TOTAL: {files.Count}");

        var warnCount = 0;
        foreach (var file in files)
        {
            var content = File.ReadAllText(file);
            if (content.Contains("STATUS: WARN", StringComparison.Ordinal))
                warnCount++;

            var benchmarkName = ExtractField(content, "BENCHMARK");
            var status = ExtractField(content, "STATUS");
            var memory = ExtractField(content, "MEMORY_GROWTH");
            var elapsed = ExtractField(content, "ELAPSED");

            sb.AppendLine(CultureInfo.InvariantCulture, $"  [{status}] {benchmarkName} - {elapsed} (memory: {memory})");
        }

        sb.AppendLine(CultureInfo.InvariantCulture, $"WARNINGS: {warnCount} (memory growth detected)");

        File.WriteAllText(summaryPath, sb.ToString());
        Console.WriteLine($"    [Summary] {summaryPath}");
    }

    private static string ExtractField(string content, string fieldName)
    {
        foreach (var line in content.Split('\n'))
        {
            if (line.StartsWith($"{fieldName}:", StringComparison.Ordinal))
                return line[($"{fieldName}:".Length)..].Trim();
        }

        return "N/A";
    }
}
