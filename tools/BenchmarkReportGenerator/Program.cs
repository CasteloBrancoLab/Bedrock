using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;

// Gerador de relatorio HTML para benchmarks de performance
// Uso: BenchmarkReportGenerator <benchmark-dir> <pending-dir> <output-file> <git-branch> <git-commit>

var benchmarkDir = args.Length > 0 ? args[0] : "artifacts/benchmark";
var pendingDir = args.Length > 1 ? args[1] : "artifacts/pending";
var outputFile = args.Length > 2 ? args[2] : "artifacts/benchmark-report/index.html";
var gitBranch = args.Length > 3 ? args[3] : "unknown";
var gitCommit = args.Length > 4 ? args[4] : "unknown";

Console.WriteLine(">>> Gerando Relatorio de Benchmarks...");

// 1. Read pending benchmark files (LLM-friendly format)
// Look in both pendingDir (local runs) and benchmarkDir (CI - downloaded from artifact)
var benchmarkResults = new List<BenchmarkResult>();
var dirsToSearch = new[] { pendingDir, benchmarkDir }.Where(Directory.Exists).Distinct();

foreach (var dir in dirsToSearch)
{
    foreach (var file in Directory.GetFiles(dir, "benchmark_*.txt").OrderBy(f => f))
    {
        var result = ParsePendingFile(file);
        if (result is not null)
        {
            // Load samples JSON if available
            var samplesFile = Path.ChangeExtension(file, null) + "_samples.json";
            if (File.Exists(samplesFile))
                result.SamplesJson = File.ReadAllText(samplesFile);

            benchmarkResults.Add(result);
        }
    }
}

// 2. Read BenchmarkDotNet JSON reports for detailed data
var detailedResults = new Dictionary<string, BdnReport>();
if (Directory.Exists(benchmarkDir))
{
    foreach (var jsonFile in Directory.GetFiles(benchmarkDir, "*-report-full.json", SearchOption.AllDirectories))
    {
        try
        {
            var json = File.ReadAllText(jsonFile);
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("Benchmarks", out var benchmarks))
            {
                foreach (var bm in benchmarks.EnumerateArray())
                {
                    var fullName = bm.TryGetProperty("FullName", out var fn) ? fn.GetString() ?? "" : "";
                    var method = bm.TryGetProperty("Method", out var m) ? m.GetString() ?? "" : "";
                    var type = bm.TryGetProperty("Type", out var t) ? t.GetString() ?? "" : "";
                    var key = $"{type}.{method}";

                    var stats = bm.TryGetProperty("Statistics", out var s) ? s : (JsonElement?)null;
                    var memory = bm.TryGetProperty("Memory", out var mem) ? mem : (JsonElement?)null;

                    detailedResults[key] = new BdnReport
                    {
                        FullName = fullName,
                        Type = type,
                        Method = method,
                        Mean = stats?.TryGetProperty("Mean", out var mean) == true ? mean.GetDouble() : 0,
                        Median = stats?.TryGetProperty("Median", out var median) == true ? median.GetDouble() : 0,
                        StdDev = stats?.TryGetProperty("StandardDeviation", out var sd) == true ? sd.GetDouble() : 0,
                        Min = stats?.TryGetProperty("Min", out var min) == true ? min.GetDouble() : 0,
                        Max = stats?.TryGetProperty("Max", out var max) == true ? max.GetDouble() : 0,
                        AllocatedBytes = memory?.TryGetProperty("BytesAllocatedPerOperation", out var alloc) == true ? alloc.GetInt64() : 0,
                        Gen0Collections = memory?.TryGetProperty("Gen0Collections", out var g0) == true ? g0.GetInt32() : 0,
                        Gen1Collections = memory?.TryGetProperty("Gen1Collections", out var g1) == true ? g1.GetInt32() : 0,
                        Gen2Collections = memory?.TryGetProperty("Gen2Collections", out var g2) == true ? g2.GetInt32() : 0,
                        Parameters = bm.TryGetProperty("Parameters", out var p) ? p.GetString() ?? "" : ""
                    };
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Aviso: Erro ao ler {jsonFile}: {ex.Message}");
        }
    }
}

if (benchmarkResults.Count == 0 && detailedResults.Count == 0)
{
    Console.WriteLine("Nenhum resultado de benchmark encontrado.");
    return 1;
}

// 3. Merge data: enrich pending results with BDN detailed data
foreach (var result in benchmarkResults)
{
    if (detailedResults.TryGetValue(result.Name, out var detail))
    {
        result.Median = detail.Median;
        result.StdDev = detail.StdDev;
        result.Min = detail.Min;
        result.Max = detail.Max;
        result.Parameters = detail.Parameters;
    }
}

// If we only have BDN data (no pending files), create results from it
if (benchmarkResults.Count == 0)
{
    foreach (var (key, detail) in detailedResults)
    {
        benchmarkResults.Add(new BenchmarkResult
        {
            Name = key,
            Status = "PASS",
            MeanTime = FormatTime(detail.Mean),
            MedianTime = FormatTime(detail.Median),
            Allocated = FormatBytes(detail.AllocatedBytes),
            MemoryGrowth = "N/A",
            Median = detail.Median,
            StdDev = detail.StdDev,
            Min = detail.Min,
            Max = detail.Max,
            Parameters = detail.Parameters
        });
    }
}

// 4. Generate HTML
var html = GenerateHtml(benchmarkResults, gitBranch, gitCommit);

var dir = Path.GetDirectoryName(outputFile);
if (!string.IsNullOrEmpty(dir))
    Directory.CreateDirectory(dir);

File.WriteAllText(outputFile, html);
Console.WriteLine($"Relatorio gerado: {outputFile}");
Console.WriteLine($"  Benchmarks: {benchmarkResults.Count}");
return 0;

// --- Helper methods ---

static BenchmarkResult? ParsePendingFile(string filePath)
{
    try
    {
        var lines = File.ReadAllLines(filePath);
        var result = new BenchmarkResult();

        foreach (var line in lines)
        {
            var colonIdx = line.IndexOf(':');
            if (colonIdx < 0) continue;

            var key = line[..colonIdx].Trim();
            var value = line[(colonIdx + 1)..].Trim();

            switch (key)
            {
                case "BENCHMARK": result.Name = value; break;
                case "STATUS": result.Status = value; break;
                case "MEAN_TIME": result.MeanTime = value; break;
                case "MEDIAN_TIME": result.MedianTime = value; break;
                case "STDDEV": result.StdDevFormatted = value; break;
                case "ALLOCATED": result.Allocated = value; break;
                case "MEMORY_GROWTH": result.MemoryGrowth = value; break;
                case "MEMORY_SAMPLES": result.MemorySamples = int.TryParse(value, out var ms) ? ms : 0; break;
                case "GC_GEN0": result.GcGen0 = long.TryParse(value, out var g0) ? g0 : 0; break;
                case "GC_GEN1": result.GcGen1 = long.TryParse(value, out var g1) ? g1 : 0; break;
                case "GC_GEN2": result.GcGen2 = long.TryParse(value, out var g2) ? g2 : 0; break;
                case "INITIAL_HEAP_MB": result.InitialHeapMb = double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var ih) ? ih : 0; break;
                case "FINAL_HEAP_MB": result.FinalHeapMb = double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var fh) ? fh : 0; break;
                case "HEAP_GROWTH_PERCENT": result.HeapGrowthPercent = double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var hg) ? hg : 0; break;
                case "AVG_CPU_PERCENT": result.AvgCpuPercent = double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var ac) ? ac : 0; break;
                case "PEAK_CPU_PERCENT": result.PeakCpuPercent = double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var pc) ? pc : 0; break;
                case "NETWORK_BYTES_SENT": result.NetworkBytesSent = long.TryParse(value, out var ns) ? ns : 0; break;
                case "NETWORK_BYTES_RECEIVED": result.NetworkBytesReceived = long.TryParse(value, out var nr) ? nr : 0; break;
                case "AVG_GC_PAUSE_PERCENT": result.AvgGcPausePercent = double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var agp) ? agp : 0; break;
                case "PEAK_GC_PAUSE_PERCENT": result.PeakGcPausePercent = double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var pgp) ? pgp : 0; break;
                case "TOTAL_GC_PAUSE_MS": result.TotalGcPauseMs = double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var tgp) ? tgp : 0; break;
            }
        }

        return string.IsNullOrEmpty(result.Name) ? null : result;
    }
    catch
    {
        return null;
    }
}

static string FormatTime(double nanoseconds)
{
    return nanoseconds switch
    {
        < 1_000 => $"{nanoseconds:F2}ns",
        < 1_000_000 => $"{nanoseconds / 1_000:F2}us",
        < 1_000_000_000 => $"{nanoseconds / 1_000_000:F2}ms",
        _ => $"{nanoseconds / 1_000_000_000:F2}s"
    };
}

static string FormatPauseMs(double ms)
{
    return ms switch
    {
        0 => "0ms",
        < 1 => $"{ms * 1000:F0}μs",
        < 1_000 => $"{ms:F1}ms",
        < 60_000 => $"{ms / 1_000:F2}s",
        _ => $"{ms / 60_000:F2}min"
    };
}

static string FormatBytes(long bytes)
{
    return bytes switch
    {
        < 1024 => $"{bytes}B",
        < 1024 * 1024 => $"{bytes / 1024.0:F2}KB",
        < 1024 * 1024 * 1024 => $"{bytes / (1024.0 * 1024):F2}MB",
        _ => $"{bytes / (1024.0 * 1024 * 1024):F2}GB"
    };
}

static string GenerateHtml(List<BenchmarkResult> results, string gitBranch, string gitCommit)
{
    var totalBenchmarks = results.Count;
    var warnCount = results.Count(r => r.Status == "WARN");
    var passCount = results.Count(r => r.Status == "PASS");

    var sb = new StringBuilder();

    // --- HTML Head ---
    sb.Append("""
        <!DOCTYPE html>
        <html lang="pt-BR">
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <title>Bedrock - Relatorio de Benchmarks</title>
            <style>
                :root{--passed:#10b981;--failed:#ef4444;--warn:#f59e0b;--bg:#0f172a;--card:#1e293b;--text:#f1f5f9;--muted:#94a3b8;--border:#334155;--header-bg:linear-gradient(to right,#1e1b4b,#312e81);--feature-bg:linear-gradient(to right,#1e293b,#334155);--table-header-bg:#1e293b;--table-row-bg:#1e293b;--badge-pass-bg:#065f46;--badge-pass-text:#6ee7b7;--badge-warn-bg:#78350f;--badge-warn-text:#fcd34d;--badge-total-bg:#374151;--badge-total-text:#e5e7eb;--chart-legend:#e5e7eb;--stable-bg:#065f46;--stable-text:#6ee7b7;--growing-bg:#78350f;--growing-text:#fcd34d}
                .light-theme{--bg:#f9fafb;--card:#fff;--text:#1f2937;--muted:#6b7280;--border:#e5e7eb;--header-bg:linear-gradient(to right,#eef2ff,#e0e7ff);--feature-bg:linear-gradient(to right,#f8fafc,#f1f5f9);--table-header-bg:#f8fafc;--table-row-bg:#f8fafc;--badge-pass-bg:#d1fae5;--badge-pass-text:#065f46;--badge-warn-bg:#fef3c7;--badge-warn-text:#92400e;--badge-total-bg:#e5e7eb;--badge-total-text:#1f2937;--chart-legend:#374151;--stable-bg:#d1fae5;--stable-text:#065f46;--growing-bg:#fef3c7;--growing-text:#92400e}
                *{box-sizing:border-box;margin:0;padding:0}
                body{font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,sans-serif;background:var(--bg);color:var(--text);line-height:1.6;transition:background .3s,color .3s}
                .container{max-width:1400px;margin:0 auto;padding:2rem}
                .header{text-align:center;margin-bottom:2rem;border-bottom:2px solid var(--border);padding-bottom:1.5rem;position:relative}
                .header h1{font-size:2rem;font-weight:700}
                .subtitle{color:var(--muted);font-size:.875rem}
                .theme-toggle{position:absolute;top:0;right:0;background:var(--card);border:1px solid var(--border);border-radius:.5rem;padding:.5rem .75rem;cursor:pointer;display:flex;align-items:center;gap:.5rem;font-size:.875rem;color:var(--text);transition:all .2s}
                .theme-toggle:hover{background:var(--border)}
                .theme-toggle svg{width:1.25rem;height:1.25rem}
                .theme-toggle .icon-sun{display:none}
                .theme-toggle .icon-moon{display:block}
                .light-theme .theme-toggle .icon-sun{display:block}
                .light-theme .theme-toggle .icon-moon{display:none}
                .cards{display:grid;grid-template-columns:repeat(auto-fit,minmax(160px,1fr));gap:1rem;margin-bottom:2rem}
                .card{background:var(--card);padding:1.25rem;border-radius:.75rem;box-shadow:0 1px 3px rgba(0,0,0,.1);text-align:center}
                .card-value{font-size:2.5rem;font-weight:700}
                .card-label{color:var(--muted);font-size:.75rem;text-transform:uppercase}
                .card.pass{border-left:4px solid var(--passed)}.card.pass .card-value{color:var(--passed)}
                .card.warn{border-left:4px solid var(--warn)}.card.warn .card-value{color:var(--warn)}

                .env-section{background:var(--card);padding:1.25rem 1.5rem;border-radius:.75rem;box-shadow:0 1px 3px rgba(0,0,0,.1);margin-bottom:2rem}
                .env-section h3{font-size:1rem;margin-bottom:.75rem;border-bottom:1px solid var(--border);padding-bottom:.5rem}
                .env-section dl{display:flex;flex-wrap:wrap;gap:.5rem 2rem;font-size:.875rem}
                .env-section dt{color:var(--muted);font-size:.75rem;text-transform:uppercase}.env-section dd{font-weight:500;margin-right:1rem}
                .chart-section{display:grid;grid-template-columns:repeat(auto-fit,minmax(300px,1fr));gap:2rem;margin-bottom:2rem;background:var(--card);padding:1.5rem;border-radius:.75rem;box-shadow:0 1px 3px rgba(0,0,0,.1)}
                @media(max-width:768px){.chart-section{grid-template-columns:1fr}}
                .chart-container{position:relative;height:300px}

                .percentile-table{margin-bottom:0}
                .percentile-tables{overflow-x:auto}
                .pct-up{color:var(--warn);font-size:.7rem;font-weight:600}
                .pct-down{color:var(--passed);font-size:.7rem;font-weight:600}
                .pct-neutral{color:var(--muted);font-size:.7rem}

                /* Correlation matrix drag-and-drop */
                .corr-section{margin-top:1.5rem}
                .corr-chips{display:flex;flex-wrap:wrap;gap:.5rem;margin-bottom:.75rem}
                .corr-chip{padding:.3rem .7rem;border-radius:9999px;font-size:.75rem;font-weight:600;cursor:grab;user-select:none;border:1px solid var(--border);background:var(--card);color:var(--text);transition:all .15s}
                .corr-chip:active{cursor:grabbing;opacity:.7}
                .corr-chip.used{opacity:.4;pointer-events:none}
                .corr-dropzones{display:grid;grid-template-columns:1fr 1fr;gap:1rem;margin-bottom:1rem}
                .corr-dropzone{min-height:48px;border:2px dashed var(--border);border-radius:.5rem;padding:.5rem;display:flex;flex-wrap:wrap;gap:.4rem;align-items:center;transition:border-color .2s}
                .corr-dropzone.drag-over{border-color:var(--passed)}
                .corr-dropzone-label{font-size:.7rem;text-transform:uppercase;color:var(--muted);margin-bottom:.25rem}
                .corr-dropped{padding:.25rem .6rem;border-radius:9999px;font-size:.7rem;font-weight:600;background:var(--badge-pass-bg);color:var(--badge-pass-text);cursor:pointer;display:flex;align-items:center;gap:.3rem}
                .corr-dropped:hover{opacity:.7}
                .corr-dropped::after{content:'\00d7';font-size:.85rem}
                .corr-matrix{overflow-x:auto;margin-top:.75rem}
                .corr-matrix table{border-collapse:collapse;width:auto}
                .corr-matrix th,.corr-matrix td{padding:.4rem .6rem;text-align:center;font-size:.75rem;border:1px solid var(--border);font-family:monospace}
                .corr-matrix th{background:var(--table-header-bg);color:var(--muted);font-weight:600;white-space:nowrap}
                .corr-matrix td{min-width:60px}

                .section-header{font-size:1.25rem;font-weight:600;margin-bottom:1rem;border-bottom:2px solid var(--border);padding-bottom:.5rem}

                /* Dicas de analise */
                .tips-section{background:var(--card);border-radius:.75rem;box-shadow:0 1px 3px rgba(0,0,0,.1);margin-bottom:2rem;overflow:hidden}
                .tips-header{padding:1rem 1.5rem;background:linear-gradient(135deg,#312e81,#4338ca);color:#e0e7ff;font-weight:600;font-size:1.1rem;display:flex;align-items:center;gap:.75rem;cursor:pointer;transition:filter .2s}
                .tips-header:hover{filter:brightness(1.15)}
                .light-theme .tips-header{background:linear-gradient(135deg,#eef2ff,#c7d2fe);color:#3730a3}
                .tips-toggle{font-size:.75rem;transition:transform .2s}
                .tips-section.collapsed .tips-toggle{transform:rotate(-90deg)}
                .tips-body{max-height:8000px;overflow:hidden;transition:max-height .4s;padding:1.5rem}
                .tips-section.collapsed .tips-body{max-height:0;padding:0 1.5rem}
                .tip-group{margin-bottom:1.5rem}
                .tip-group:last-child{margin-bottom:0}
                .tip-group-header{font-size:.95rem;font-weight:600;margin-bottom:.75rem;display:flex;align-items:center;gap:.5rem;cursor:pointer;padding:.5rem .75rem;border-radius:.5rem;transition:background .2s}
                .tip-group-header:hover{background:rgba(148,163,184,0.1)}
                .tip-group-toggle{font-size:.65rem;color:var(--muted);transition:transform .2s}
                .tip-group.collapsed .tip-group-toggle{transform:rotate(-90deg)}
                .tip-group-body{max-height:4000px;overflow:hidden;transition:max-height .3s}
                .tip-group.collapsed .tip-group-body{max-height:0}
                .tip-card{background:var(--bg);border-radius:.5rem;padding:1rem 1.25rem;margin-bottom:.75rem;border-left:4px solid var(--border)}
                .tip-card.tip-good{border-left-color:var(--passed)}
                .tip-card.tip-warn{border-left-color:var(--warn)}
                .tip-card.tip-danger{border-left-color:var(--failed)}
                .tip-card.tip-info{border-left-color:#6366f1}
                .tip-title{font-weight:600;font-size:.85rem;margin-bottom:.35rem;display:flex;align-items:center;gap:.5rem}
                .tip-text{font-size:.8rem;color:var(--muted);line-height:1.7}
                .tip-text b{color:var(--text)}
                .tip-icon{font-size:1.1rem}
                .tip-example{background:var(--card);border:1px solid var(--border);border-radius:.375rem;padding:.5rem .75rem;margin-top:.5rem;font-size:.75rem;font-family:monospace;color:var(--muted)}
                .tip-scale{display:flex;gap:.5rem;margin-top:.5rem;flex-wrap:wrap}
                .tip-scale-item{display:flex;align-items:center;gap:.3rem;font-size:.7rem;color:var(--muted)}
                .tip-scale-dot{width:10px;height:10px;border-radius:50%;display:inline-block}

                /* Diagnostic panel */
                .diag-panel{margin-bottom:1.5rem;border:1px solid var(--border);border-radius:.5rem;overflow:hidden}
                .diag-header{padding:.75rem 1rem;background:var(--table-header-bg);font-weight:600;font-size:.85rem;display:flex;align-items:center;gap:.5rem;cursor:pointer}
                .diag-header:hover{filter:brightness(1.1)}
                .diag-toggle{font-size:.65rem;color:var(--muted);transition:transform .2s}
                .diag-panel.collapsed .diag-toggle{transform:rotate(-90deg)}
                .diag-body{max-height:3000px;overflow:hidden;transition:max-height .3s;padding:.75rem 1rem}
                .diag-panel.collapsed .diag-body{max-height:0;padding:0 1rem}
                .diag-items{display:grid;grid-template-columns:repeat(auto-fit,minmax(280px,1fr));gap:.75rem}
                .diag-item{display:flex;align-items:flex-start;gap:.6rem;padding:.6rem .75rem;border-radius:.375rem;background:var(--bg);font-size:.8rem;line-height:1.5}
                .diag-icon{font-size:1.2rem;flex-shrink:0;margin-top:.1rem}
                .diag-label{font-weight:600;font-size:.75rem;color:var(--muted);text-transform:uppercase;margin-bottom:.15rem}
                .diag-msg{color:var(--text)}

                /* Tabela de benchmarks */
                .bench-table{width:100%;border-collapse:collapse;background:var(--card);border-radius:.75rem;overflow:hidden;box-shadow:0 1px 3px rgba(0,0,0,.1);margin-bottom:2rem}
                .bench-table th,.bench-table td{padding:.75rem 1rem;text-align:left;border-bottom:1px solid var(--border)}
                .bench-table th{background:var(--table-header-bg);font-weight:600;font-size:.75rem;text-transform:uppercase;color:var(--muted)}
                .bench-table tr:last-child td{border-bottom:none}
                .bench-table .num{text-align:right;font-family:monospace;font-size:.85rem}

                .badge{display:inline-block;padding:.2rem .6rem;border-radius:9999px;font-size:.7rem;font-weight:600}
                .badge-stable{background:var(--stable-bg);color:var(--stable-text)}
                .badge-growing{background:var(--growing-bg);color:var(--growing-text)}
                .badge-pass{background:var(--badge-pass-bg);color:var(--badge-pass-text)}
                .badge-warn{background:var(--badge-warn-bg);color:var(--badge-warn-text)}

                /* Detalhes colapsaveis */
                .detail{background:var(--card);margin-bottom:.75rem;border-radius:.75rem;box-shadow:0 1px 3px rgba(0,0,0,.1);overflow:hidden}
                .detail-header{padding:1rem 1.5rem;background:var(--feature-bg);font-weight:600;display:flex;align-items:center;gap:.75rem;cursor:pointer;transition:background .2s}
                .detail-header:hover{filter:brightness(1.1)}
                .detail-toggle{font-size:.75rem;color:var(--muted);transition:transform .2s}
                .detail.collapsed .detail-toggle{transform:rotate(-90deg)}
                .detail-name{flex:1}
                .detail-badges{display:flex;gap:.5rem}
                .detail-content{max-height:5000px;overflow:hidden;transition:max-height .3s;padding:1.5rem}
                .detail.collapsed .detail-content{max-height:0;padding:0 1.5rem}

                .metrics-grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(200px,1fr));gap:1rem;margin-bottom:1.5rem}
                .metric{background:var(--bg);padding:1rem;border-radius:.5rem}
                .metric-label{font-size:.7rem;text-transform:uppercase;color:var(--muted);margin-bottom:.25rem}
                .metric-value{font-size:1.1rem;font-weight:600;font-family:monospace}

                .timeline-chart{position:relative;height:320px;margin-bottom:1.5rem}
                .chart-detail{height:200px;margin-bottom:1rem}
                .chart-label{font-size:.85rem;font-weight:600;margin-bottom:.5rem;color:var(--muted)}

                .footer{text-align:center;padding:2rem;color:var(--muted);font-size:.75rem;border-top:1px solid var(--border);margin-top:2rem}
                @media print{body{background:#fff;font-size:12px}.container{max-width:none;padding:1rem}.card,.detail,.chart-section,.bench-table{box-shadow:none;border:1px solid var(--border)}.detail{break-inside:avoid}.detail.collapsed .detail-content{max-height:none}.detail-toggle{display:none}}
            </style>
        </head>
        <body>
        <div class="container">
            <header class="header">
                <button class="theme-toggle" onclick="toggleTheme()" title="Alternar tema claro/escuro">
                    <svg class="icon-sun" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor"><circle cx="12" cy="12" r="4"/><path stroke-linecap="round" d="M12 2v2m0 16v2M4 12H2m20 0h-2m-2.05-6.95 1.41-1.41M4.64 19.36l1.41-1.41m0-11.9L4.64 4.64m14.72 14.72-1.41-1.41"/></svg>
                    <svg class="icon-moon" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z"/></svg>
                </button>
                <h1>Bedrock - Relatorio de Benchmarks</h1>
                <p class="subtitle">Gerado em:
        """);
    sb.Append(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
    sb.Append("""
         UTC</p>
            </header>
        """);

    // --- Summary Cards ---
    sb.Append($"""
            <section class="cards">
                <div class="card"><div class="card-value">{totalBenchmarks}</div><div class="card-label">Benchmarks</div></div>
                <div class="card pass"><div class="card-value">{passCount}</div><div class="card-label">OK</div></div>
                <div class="card warn"><div class="card-value">{warnCount}</div><div class="card-label">Warnings</div></div>
            </section>
        """);

    // --- Environment ---
    sb.Append("""
            <section class="env-section">
                <h3>Ambiente</h3>
                <dl>
                    <dt>Maquina</dt><dd>
        """);
    sb.Append(WebUtility.HtmlEncode(Environment.MachineName));
    sb.Append("</dd><dt>SO</dt><dd>");
    sb.Append(WebUtility.HtmlEncode(Environment.OSVersion.ToString()));
    sb.Append("</dd><dt>.NET</dt><dd>");
    sb.Append(WebUtility.HtmlEncode(Environment.Version.ToString()));
    sb.Append("</dd><dt>Branch</dt><dd>");
    sb.Append(WebUtility.HtmlEncode(gitBranch));
    sb.Append("</dd><dt>Commit</dt><dd>");
    sb.Append(WebUtility.HtmlEncode(gitCommit.Length >= 7 ? gitCommit[..7] : gitCommit));
    sb.Append("""
                    </dd>
                </dl>
            </section>
        """);

    // --- Charts ---
    var hasMeanTimeData = results.Any(r => r.Median > 0 || !string.IsNullOrEmpty(r.MeanTime));

    sb.Append("""
            <section class="chart-section">
        """);

    if (hasMeanTimeData)
    {
        sb.Append("""
                <div class="chart-container"><canvas id="meanChart"></canvas></div>
        """);
    }
    else
    {
        sb.Append("""
                <div class="chart-container"><canvas id="heapChart"></canvas></div>
                <div class="chart-container"><canvas id="cpuChart"></canvas></div>
                <div class="chart-container"><canvas id="networkChart"></canvas></div>
        """);
    }

    sb.Append("""
            </section>
        """);

    // --- Pre-compute medians from samples for summary table ---
    var medianData = new Dictionary<string, (double CpuMedian, double WsMedian, double GcPauseMedian)>();
    foreach (var r in results)
    {
        if (!string.IsNullOrEmpty(r.SamplesJson))
        {
            var smp = ParseSamplesForPercentiles(r.SamplesJson);
            if (smp.Count > 0)
            {
                var cpuSorted = smp.Select(s => s.Cpu).OrderBy(v => v).ToArray();
                var wsSorted = smp.Select(s => s.Ws).OrderBy(v => v).ToArray();
                var gcPauseSorted = smp.Select(s => s.GcPause).OrderBy(v => v).ToArray();
                medianData[r.Name] = (Percentile(cpuSorted, 50), Percentile(wsSorted, 50), Percentile(gcPauseSorted, 50));
            }
        }
    }

    // --- Benchmark Summary Table ---
    sb.Append("""
            <section>
                <h2 class="section-header">Sumario de Benchmarks</h2>
                <table class="bench-table">
                    <thead>
                        <tr>
                            <th>Status</th>
                            <th>Benchmark</th>
        """);

    if (hasMeanTimeData)
    {
        sb.Append("""
                            <th class="num">Tempo Medio</th>
                            <th class="num">Alocado</th>
        """);
    }

    sb.Append("""
                            <th>Memoria</th>
                            <th class="num">Working Set (P50)</th>
                            <th class="num">CPU (P50)</th>
                            <th class="num">Rede I/O</th>
                            <th class="num">GC (0/1/2)</th>
                            <th class="num">GC Pause (P50)</th>
                        </tr>
                    </thead>
                    <tbody>
        """);

    foreach (var r in results)
    {
        var statusBadge = r.Status == "WARN"
            ? """<span class="badge badge-warn">WARN</span>"""
            : """<span class="badge badge-pass">PASS</span>""";
        var memBadge = r.MemoryGrowth?.ToUpperInvariant() switch
        {
            "STABLE" => """<span class="badge badge-stable">STABLE</span>""",
            "GROWING" => """<span class="badge badge-growing">GROWING</span>""",
            _ => "<span>-</span>"
        };
        var networkIO = r.NetworkBytesSent > 0 || r.NetworkBytesReceived > 0
            ? $"E:{FormatBytes(r.NetworkBytesSent)} R:{FormatBytes(r.NetworkBytesReceived)}"
            : "-";

        sb.Append($"""
                        <tr>
                            <td>{statusBadge}</td>
                            <td>{WebUtility.HtmlEncode(r.Name)}{(string.IsNullOrEmpty(r.Parameters) ? "" : $" <small>({WebUtility.HtmlEncode(r.Parameters)})</small>")}</td>
            """);

        if (hasMeanTimeData)
        {
            sb.Append($"""
                            <td class="num">{WebUtility.HtmlEncode(r.MeanTime ?? "-")}</td>
                            <td class="num">{WebUtility.HtmlEncode(r.Allocated ?? "-")}</td>
            """);
        }

        var md = medianData.TryGetValue(r.Name, out var m) ? m : default;

        sb.Append($"""
                            <td>{memBadge}</td>
                            <td class="num">{(md.WsMedian > 0 ? $"{md.WsMedian:F1} MB" : "-")}</td>
                            <td class="num">{(md.CpuMedian > 0 ? $"{md.CpuMedian:F1}%" : "-")}</td>
                            <td class="num" style="font-size:.75rem">{networkIO}</td>
                            <td class="num">{r.GcGen0}/{r.GcGen1}/{r.GcGen2}</td>
                            <td class="num">{(md.GcPauseMedian > 0 || r.TotalGcPauseMs > 0 ? $"{md.GcPauseMedian:F2}% ({FormatPauseMs(r.TotalGcPauseMs)})" : "-")}</td>
                        </tr>
            """);
    }

    sb.Append("""
                    </tbody>
                </table>
            </section>
        """);

    // --- Dicas de Analise ---
    sb.Append("""
            <div class="tips-section collapsed">
                <div class="tips-header" onclick="this.closest('.tips-section').classList.toggle('collapsed')">
                    <span class="tips-toggle">&#9660;</span>
                    <span>&#128161;</span>
                    <span>Dicas de Analise — Como interpretar este relatorio</span>
                </div>
                <div class="tips-body">

                    <div class="tip-group">
                        <div class="tip-group-header" onclick="this.closest('.tip-group').classList.toggle('collapsed')">
                            <span class="tip-group-toggle">&#9660;</span>
                            <span>&#129504;</span>
                            <span>Memoria e Heap — O aplicativo esta vazando memoria?</span>
                        </div>
                        <div class="tip-group-body">
                            <div class="tip-card tip-info">
                                <div class="tip-title"><span class="tip-icon">&#128203;</span> O que e Heap GC?</div>
                                <div class="tip-text">
                                    O <b>Heap GC</b> e a area de memoria gerenciada pelo .NET onde os objetos vivem.
                                    Quando voce cria um <code>new Object()</code>, ele vai para o Heap.
                                    O <b>Garbage Collector (GC)</b> limpa periodicamente os objetos que nao estao mais em uso.
                                </div>
                            </div>
                            <div class="tip-card tip-good">
                                <div class="tip-title"><span class="tip-icon">&#9989;</span> STABLE = Tudo certo</div>
                                <div class="tip-text">
                                    Se o badge mostra <b>STABLE</b>, significa que a memoria se estabilizou ao longo do tempo.
                                    O GC esta conseguindo limpar os objetos na mesma velocidade em que sao criados.
                                    <b>A linha de tendencia do Heap deve estar plana ou levemente subindo.</b>
                                </div>
                            </div>
                            <div class="tip-card tip-warn">
                                <div class="tip-title"><span class="tip-icon">&#9888;&#65039;</span> GROWING = Atencao</div>
                                <div class="tip-text">
                                    Se o badge mostra <b>GROWING</b>, o Heap cresceu mais de 20% entre o inicio e o fim do benchmark.
                                    Isso <b>pode</b> indicar um vazamento de memoria (memory leak), mas tambem pode ser apenas o
                                    warm-up normal da aplicacao. <b>Pergunte-se:</b>
                                </div>
                                <div class="tip-example">
                                    &#8226; A linha de tendencia do Heap esta subindo continuamente? &#8594; Possivel leak<br>
                                    &#8226; O Heap subiu e depois estabilizou? &#8594; Provavelmente warm-up normal<br>
                                    &#8226; O Working Set tambem esta subindo? &#8594; O SO esta alocando mais memoria fisica
                                </div>
                            </div>
                            <div class="tip-card tip-info">
                                <div class="tip-title"><span class="tip-icon">&#128200;</span> Working Set vs Heap GC</div>
                                <div class="tip-text">
                                    O <b>Working Set</b> e a memoria fisica total do processo (inclui Heap + codigo + buffers nativos).
                                    Se o Working Set cresce mas o Heap nao, pode ser memoria nativa (ex: buffers de I/O, conexoes).
                                    Se ambos crescem juntos, o crescimento e do codigo gerenciado.
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="tip-group collapsed">
                        <div class="tip-group-header" onclick="this.closest('.tip-group').classList.toggle('collapsed')">
                            <span class="tip-group-toggle">&#9660;</span>
                            <span>&#9889;</span>
                            <span>CPU — O benchmark esta usando processamento de forma eficiente?</span>
                        </div>
                        <div class="tip-group-body">
                            <div class="tip-card tip-info">
                                <div class="tip-title"><span class="tip-icon">&#128203;</span> Como interpretar CPU (%)</div>
                                <div class="tip-text">
                                    O valor de <b>CPU (%)</b> indica quanto do processador o benchmark esta usando.
                                    O valor e normalizado pelo numero de nucleos: 100% = todos os nucleos em uso maximo.
                                    <b>Um valor baixo e esperado</b> para benchmarks de I/O (rede, disco).
                                    <b>Um valor alto e esperado</b> para benchmarks de computacao (algoritmos, serializacao).
                                </div>
                            </div>
                            <div class="tip-card tip-good">
                                <div class="tip-title"><span class="tip-icon">&#9989;</span> Tendencia estavel</div>
                                <div class="tip-text">
                                    Se a <b>linha de tendencia da CPU</b> esta plana, o benchmark tem consumo previsivel.
                                    Variacao e normal (o SO agenda outros processos), mas a mediana (P50) deve representar bem o uso tipico.
                                </div>
                            </div>
                            <div class="tip-card tip-warn">
                                <div class="tip-title"><span class="tip-icon">&#9888;&#65039;</span> CPU subindo ao longo do tempo</div>
                                <div class="tip-text">
                                    Se a tendencia sobe continuamente, pode indicar que o benchmark esta ficando mais pesado
                                    (ex: listas crescendo, cache inflando, mais threads competindo). Correlacione com o Heap
                                    na <b>Matriz de Correlacao</b> para investigar.
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="tip-group collapsed">
                        <div class="tip-group-header" onclick="this.closest('.tip-group').classList.toggle('collapsed')">
                            <span class="tip-group-toggle">&#9660;</span>
                            <span>&#9851;&#65039;</span>
                            <span>Garbage Collector — As pausas estao impactando a performance?</span>
                        </div>
                        <div class="tip-group-body">
                            <div class="tip-card tip-info">
                                <div class="tip-title"><span class="tip-icon">&#128203;</span> Geracoes do GC: Gen0, Gen1, Gen2</div>
                                <div class="tip-text">
                                    O GC do .NET organiza objetos em 3 geracoes:<br>
                                    <b>Gen0</b> — Objetos recem-criados. Coletas rapidas e frequentes. &#9989; Normal ter muitas.<br>
                                    <b>Gen1</b> — Objetos que sobreviveram a Gen0. Coletas moderadas.<br>
                                    <b>Gen2</b> — Objetos de longa duracao. Coletas caras e lentas. &#9888;&#65039; Muitas Gen2 = problema.
                                </div>
                            </div>
                            <div class="tip-card tip-info">
                                <div class="tip-title"><span class="tip-icon">&#9200;</span> GC Pause — O que significa?</div>
                                <div class="tip-text">
                                    <b>GC Pause (%)</b> e a porcentagem do tempo que a aplicacao ficou <b>parada</b> esperando o GC limpar a memoria.
                                    <b>GC Pause (ms)</b> e o tempo absoluto de pausa. Quanto menor, melhor.<br>
                                    <b>Referencia:</b>
                                </div>
                                <div class="tip-scale">
                                    <span class="tip-scale-item"><span class="tip-scale-dot" style="background:var(--passed)"></span> &lt; 1% Excelente</span>
                                    <span class="tip-scale-item"><span class="tip-scale-dot" style="background:#f59e0b"></span> 1-5% Aceitavel</span>
                                    <span class="tip-scale-item"><span class="tip-scale-dot" style="background:var(--failed)"></span> &gt; 5% Investigar</span>
                                </div>
                            </div>
                            <div class="tip-card tip-warn">
                                <div class="tip-title"><span class="tip-icon">&#128269;</span> Correlacao: GC Pause vs Heap</div>
                                <div class="tip-text">
                                    Se voce viu na <b>Matriz de Correlacao</b> que Heap GC e GC Pause tem correlacao negativa,
                                    isso e <b>normal</b>: o GC pausa para limpar, o que reduz o Heap. Quando o Heap cresce livremente
                                    (sem coleta), as pausas sao menores. E o ciclo natural do GC.
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="tip-group collapsed">
                        <div class="tip-group-header" onclick="this.closest('.tip-group').classList.toggle('collapsed')">
                            <span class="tip-group-toggle">&#9660;</span>
                            <span>&#127760;</span>
                            <span>Rede — O trafego de rede esta dentro do esperado?</span>
                        </div>
                        <div class="tip-group-body">
                            <div class="tip-card tip-info">
                                <div class="tip-title"><span class="tip-icon">&#128203;</span> Enviado vs Recebido</div>
                                <div class="tip-text">
                                    <b>Enviado</b> e <b>Recebido</b> mostram o volume total de dados trafegados pelo processo.
                                    Para benchmarks que nao usam rede (computacao pura), esses valores devem ser <b>zero</b>.
                                    Para benchmarks de API/HTTP, um crescimento linear e esperado.
                                </div>
                            </div>
                            <div class="tip-card tip-good">
                                <div class="tip-title"><span class="tip-icon">&#9989;</span> Crescimento linear = OK</div>
                                <div class="tip-text">
                                    Se a linha do grafico de rede sobe de forma constante (reta), significa que o benchmark
                                    esta enviando/recebendo dados a uma taxa estavel. A <b>linha de tendencia</b> deve
                                    ser praticamente colada na serie original.
                                </div>
                            </div>
                            <div class="tip-card tip-warn">
                                <div class="tip-title"><span class="tip-icon">&#9888;&#65039;</span> Rede inesperada?</div>
                                <div class="tip-text">
                                    Se um benchmark que <b>nao deveria usar rede</b> mostra trafego, investigue:
                                    pode ser telemetria, logging remoto, ou dependencias externas nao-mockadas.
                                    Use o <b>&#916; Enviado/Recebido</b> (oculto por padrao na legenda) para ver a taxa por segundo.
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="tip-group collapsed">
                        <div class="tip-group-header" onclick="this.closest('.tip-group').classList.toggle('collapsed')">
                            <span class="tip-group-toggle">&#9660;</span>
                            <span>&#128202;</span>
                            <span>Percentis e Estatisticas — O que significam P50, P99 etc?</span>
                        </div>
                        <div class="tip-group-body">
                            <div class="tip-card tip-info">
                                <div class="tip-title"><span class="tip-icon">&#128203;</span> O que e um Percentil?</div>
                                <div class="tip-text">
                                    O percentil indica que X% das amostras ficaram <b>abaixo</b> daquele valor.<br>
                                    <b>P50 (Mediana)</b> — Metade das amostras ficou abaixo, metade acima. E o valor "tipico".<br>
                                    <b>P95</b> — 95% das amostras ficaram abaixo. Mostra o comportamento quase-pior.<br>
                                    <b>P99</b> — 99% das amostras ficaram abaixo. Captura outliers e picos.
                                </div>
                            </div>
                            <div class="tip-card tip-info">
                                <div class="tip-title"><span class="tip-icon">&#128200;</span> Variacao em relacao ao P50</div>
                                <div class="tip-text">
                                    Na tabela de percentis, cada valor mostra a <b>variacao percentual</b> em relacao ao P50.
                                    Isso ajuda a entender o quanto os extremos se desviam do comportamento tipico.
                                </div>
                                <div class="tip-example">
                                    P50 = 2.5% &nbsp;|&nbsp; P99 = 8.1% (+224%) &#8594; O P99 e 224% maior que o tipico (pico de stress)<br>
                                    P50 = 2.5% &nbsp;|&nbsp; P25 = 1.2% (-52%) &nbsp;&#8594; O P25 e 52% menor que o tipico (periodos mais calmos)
                                </div>
                            </div>
                            <div class="tip-card tip-warn">
                                <div class="tip-title"><span class="tip-icon">&#128161;</span> Quando se preocupar?</div>
                                <div class="tip-text">
                                    Se o <b>P99 e muito maior que o P50</b> (ex: +500%), existem picos extremos.
                                    Isso pode indicar GC pausando, contencao de lock, ou warm-up.
                                    Correlacione o pico de CPU com GC Pause na <b>Matriz de Correlacao</b>.
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="tip-group collapsed">
                        <div class="tip-group-header" onclick="this.closest('.tip-group').classList.toggle('collapsed')">
                            <span class="tip-group-toggle">&#9660;</span>
                            <span>&#128279;</span>
                            <span>Matriz de Correlacao — Como descobrir relacoes entre metricas</span>
                        </div>
                        <div class="tip-group-body">
                            <div class="tip-card tip-info">
                                <div class="tip-title"><span class="tip-icon">&#128203;</span> O que e a Correlacao de Pearson?</div>
                                <div class="tip-text">
                                    Mede se duas metricas se movem juntas ao longo do tempo. Vai de <b>-1</b> a <b>+1</b>:<br>
                                    <b>+1</b> = quando uma sobe, a outra sempre sobe junto (correlacao positiva perfeita)<br>
                                    <b>0</b> = nenhuma relacao<br>
                                    <b>-1</b> = quando uma sobe, a outra sempre desce (correlacao negativa perfeita)
                                </div>
                                <div class="tip-scale">
                                    <span class="tip-scale-item"><span class="tip-scale-dot" style="background:rgba(239,68,68,0.85)"></span> Forte negativa</span>
                                    <span class="tip-scale-item"><span class="tip-scale-dot" style="background:rgba(239,68,68,0.45)"></span> Moderada neg.</span>
                                    <span class="tip-scale-item"><span class="tip-scale-dot" style="background:var(--border)"></span> Fraca</span>
                                    <span class="tip-scale-item"><span class="tip-scale-dot" style="background:rgba(16,185,129,0.45)"></span> Moderada pos.</span>
                                    <span class="tip-scale-item"><span class="tip-scale-dot" style="background:rgba(16,185,129,0.85)"></span> Forte positiva</span>
                                </div>
                            </div>
                            <div class="tip-card tip-good">
                                <div class="tip-title"><span class="tip-icon">&#127919;</span> Sugestoes de correlacoes uteis</div>
                                <div class="tip-text">
                                    Arraste estas combinacoes para a matriz e veja o que acontece:
                                </div>
                                <div class="tip-example">
                                    &#8226; <b>CPU</b> vs <b>GC Pause (%)</b> &#8594; O GC esta roubando CPU?<br>
                                    &#8226; <b>Heap GC</b> vs <b>GC Pause (ms/s)</b> &#8594; Heap crescendo causa mais pausas?<br>
                                    &#8226; <b>CPU</b> vs <b>Rede Recebido</b> &#8594; Processar dados da rede esta pesando?<br>
                                    &#8226; <b>Working Set</b> vs <b>Heap GC</b> &#8594; Memoria nativa vs gerenciada crescem juntas?<br>
                                    &#8226; <b>GC Pause (%)</b> vs <b>GC Pause (ms/s)</b> &#8594; Devem ser altamente correlacionados
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="tip-group collapsed">
                        <div class="tip-group-header" onclick="this.closest('.tip-group').classList.toggle('collapsed')">
                            <span class="tip-group-toggle">&#9660;</span>
                            <span>&#128218;</span>
                            <span>Linhas de Tendencia — O que a inclinacao revela</span>
                        </div>
                        <div class="tip-group-body">
                            <div class="tip-card tip-info">
                                <div class="tip-title"><span class="tip-icon">&#128203;</span> Como funciona</div>
                                <div class="tip-text">
                                    Cada grafico tem <b>linhas tracejadas</b> que representam a <b>regressao linear</b> da serie.
                                    E uma reta que melhor aproxima a tendencia geral dos dados, ignorando oscilacoes pontuais.
                                </div>
                            </div>
                            <div class="tip-card tip-good">
                                <div class="tip-title"><span class="tip-icon">&#8594;</span> Linha plana = Estavel</div>
                                <div class="tip-text">
                                    Se a linha de tendencia e praticamente <b>horizontal</b>, a metrica esta estavel.
                                    Isso e o ideal para CPU, GC Pause e Heap apos o warm-up.
                                </div>
                            </div>
                            <div class="tip-card tip-warn">
                                <div class="tip-title"><span class="tip-icon">&#8599;&#65039;</span> Linha subindo = Crescimento</div>
                                <div class="tip-text">
                                    Uma tendencia subindo indica crescimento ao longo do tempo.
                                    Para <b>Rede</b> (cumulativo) e esperado. Para <b>Heap</b> ou <b>CPU</b>, pode indicar problema.
                                    Compare a inclinacao do Heap com a do Working Set para distinguir memoria gerenciada de nativa.
                                </div>
                            </div>
                            <div class="tip-card tip-danger">
                                <div class="tip-title"><span class="tip-icon">&#128680;</span> Heap subindo + Gen2 subindo = Alerta</div>
                                <div class="tip-text">
                                    Se as linhas de tendencia de <b>Heap GC</b> e <b>&#916; Gen2</b> estao ambas subindo,
                                    o GC nao esta conseguindo limpar objetos de longa duracao. Isso e o padrao classico de
                                    <b>memory leak</b> em .NET. Investigue objetos retidos por event handlers, caches ou listas estaticas.
                                </div>
                            </div>
                        </div>
                    </div>

                </div>
            </div>
        """);

    // --- Detailed Results (collapsible per benchmark) ---
    sb.Append("""
            <section>
                <h2 class="section-header">Detalhes por Benchmark</h2>
        """);

    var benchIdx = 0;
    foreach (var r in results)
    {
        var statusBadge = r.Status == "WARN"
            ? """<span class="badge badge-warn">WARN</span>"""
            : """<span class="badge badge-pass">PASS</span>""";
        var memBadge = r.MemoryGrowth?.ToUpperInvariant() switch
        {
            "STABLE" => """<span class="badge badge-stable">STABLE</span>""",
            "GROWING" => """<span class="badge badge-growing">GROWING</span>""",
            _ => ""
        };

        var canvasId = $"timeline_{benchIdx}";
        var hasSamples = !string.IsNullOrEmpty(r.SamplesJson);

        sb.Append($"""

                <article class="detail collapsed">
                    <div class="detail-header">
                        <span class="detail-toggle">&#9660;</span>
                        <span class="detail-name">{WebUtility.HtmlEncode(r.Name)}</span>
                        <div class="detail-badges">
                            {statusBadge}
                            {memBadge}
                        </div>
                    </div>
                    <div class="detail-content">
                        <div class="metrics-grid">
            """);

        if (hasMeanTimeData)
        {
            sb.Append($"""
                            <div class="metric"><div class="metric-label">Tempo Medio</div><div class="metric-value">{WebUtility.HtmlEncode(r.MeanTime ?? "-")}</div></div>
                            <div class="metric"><div class="metric-label">Tempo Mediano</div><div class="metric-value">{WebUtility.HtmlEncode(r.MedianTime ?? r.StdDevFormatted ?? "-")}</div></div>
                            <div class="metric"><div class="metric-label">Alocado</div><div class="metric-value">{WebUtility.HtmlEncode(r.Allocated ?? "-")}</div></div>
            """);
        }

        sb.Append($"""
                            <div class="metric"><div class="metric-label">Crescimento Memoria</div><div class="metric-value">{(r.MemoryGrowth?.ToUpperInvariant() == "GROWING" ? $"""<span class="badge badge-growing">{WebUtility.HtmlEncode(r.MemoryGrowth)}</span>""" : $"""<span class="badge badge-stable">{WebUtility.HtmlEncode(r.MemoryGrowth ?? "-")}</span>""")}</div></div>
                            <div class="metric"><div class="metric-label">Amostras</div><div class="metric-value">{r.MemorySamples}</div></div>
                            <div class="metric"><div class="metric-label">CPU Medio</div><div class="metric-value">{r.AvgCpuPercent:F1}%</div></div>
                            <div class="metric"><div class="metric-label">CPU Pico</div><div class="metric-value">{r.PeakCpuPercent:F1}%</div></div>
                            <div class="metric"><div class="metric-label">Heap Inicial</div><div class="metric-value">{r.InitialHeapMb:F2} MB</div></div>
                            <div class="metric"><div class="metric-label">Heap Final</div><div class="metric-value">{r.FinalHeapMb:F2} MB</div></div>
                            <div class="metric"><div class="metric-label">Crescimento Heap</div><div class="metric-value">{r.HeapGrowthPercent:F2}%</div></div>
                            <div class="metric"><div class="metric-label">GC Gen0</div><div class="metric-value">{r.GcGen0}</div></div>
                            <div class="metric"><div class="metric-label">GC Gen1</div><div class="metric-value">{r.GcGen1}</div></div>
                            <div class="metric"><div class="metric-label">GC Gen2</div><div class="metric-value">{r.GcGen2}</div></div>
                            <div class="metric"><div class="metric-label">GC Pause Medio</div><div class="metric-value">{r.AvgGcPausePercent:F2}%</div></div>
                            <div class="metric"><div class="metric-label">GC Pause Pico</div><div class="metric-value">{r.PeakGcPausePercent:F2}%</div></div>
                            <div class="metric"><div class="metric-label">GC Pause Total</div><div class="metric-value">{FormatPauseMs(r.TotalGcPauseMs)}</div></div>
                            <div class="metric"><div class="metric-label">Rede Enviado</div><div class="metric-value">{FormatBytes(r.NetworkBytesSent)}</div></div>
                            <div class="metric"><div class="metric-label">Rede Recebido</div><div class="metric-value">{FormatBytes(r.NetworkBytesReceived)}</div></div>
                        </div>
            """);

        // --- Diagnostic Panel ---
        sb.Append("""
                        <div class="diag-panel">
                            <div class="diag-header" onclick="this.closest('.diag-panel').classList.toggle('collapsed')">
                                <span class="diag-toggle">&#9660;</span>
                                <span>&#129658;</span>
                                <span>Diagnostico Automatico</span>
                            </div>
                            <div class="diag-body">
                                <div class="diag-items">
            """);

        // 1. Memory Growth
        {
            var isGrowing = r.MemoryGrowth?.ToUpperInvariant() == "GROWING";
            var heapGrowth = r.HeapGrowthPercent;
            string memIcon, memMsg;
            if (!isGrowing && heapGrowth <= 5)
            {
                memIcon = "&#9989;"; // green check
                memMsg = $"Memoria estavel. Heap cresceu apenas {heapGrowth:F1}% — GC esta dando conta.";
            }
            else if (!isGrowing && heapGrowth <= 20)
            {
                memIcon = "&#9989;";
                memMsg = $"Memoria estavel. Heap cresceu {heapGrowth:F1}% (abaixo do limiar de 20%).";
            }
            else if (isGrowing && heapGrowth <= 50)
            {
                memIcon = "&#9888;&#65039;"; // yellow warning
                memMsg = $"Heap cresceu {heapGrowth:F1}%. Pode ser warm-up ou vazamento leve. Verifique a linha de tendencia.";
            }
            else if (isGrowing)
            {
                memIcon = "&#128680;"; // red alert
                memMsg = $"Heap cresceu {heapGrowth:F1}%! Crescimento significativo. Investigue possiveis memory leaks.";
            }
            else
            {
                memIcon = "&#9989;";
                memMsg = "Memoria dentro do esperado.";
            }
            sb.Append($"""
                                    <div class="diag-item">
                                        <span class="diag-icon">{memIcon}</span>
                                        <div><div class="diag-label">Memoria</div><div class="diag-msg">{memMsg}</div></div>
                                    </div>
                """);
        }

        // 2. CPU
        {
            var avgCpu = r.AvgCpuPercent;
            var peakCpu = r.PeakCpuPercent;
            var spikeRatio = avgCpu > 0 ? peakCpu / avgCpu : 0;
            string cpuIcon, cpuMsg;
            if (avgCpu < 5)
            {
                cpuIcon = "&#9989;";
                cpuMsg = $"CPU baixo ({avgCpu:F1}% medio). Benchmark provavelmente e I/O-bound.";
            }
            else if (avgCpu <= 50 && spikeRatio < 3)
            {
                cpuIcon = "&#9989;";
                cpuMsg = $"CPU moderado ({avgCpu:F1}% medio, pico {peakCpu:F1}%). Uso consistente.";
            }
            else if (avgCpu <= 50 && spikeRatio >= 3)
            {
                cpuIcon = "&#9888;&#65039;";
                cpuMsg = $"CPU medio de {avgCpu:F1}% mas pico de {peakCpu:F1}% (x{spikeRatio:F1}). Existem picos esporadicos — possivelmente GC ou contencao.";
            }
            else if (avgCpu <= 80)
            {
                cpuIcon = "&#9888;&#65039;";
                cpuMsg = $"CPU elevado ({avgCpu:F1}% medio, pico {peakCpu:F1}%). Considere se e esperado para este benchmark.";
            }
            else
            {
                cpuIcon = "&#128680;";
                cpuMsg = $"CPU muito alto ({avgCpu:F1}% medio, pico {peakCpu:F1}%). Pode haver saturacao de processamento.";
            }
            sb.Append($"""
                                    <div class="diag-item">
                                        <span class="diag-icon">{cpuIcon}</span>
                                        <div><div class="diag-label">CPU</div><div class="diag-msg">{cpuMsg}</div></div>
                                    </div>
                """);
        }

        // 3. GC Pause
        {
            var avgPause = r.AvgGcPausePercent;
            var peakPause = r.PeakGcPausePercent;
            var totalMs = r.TotalGcPauseMs;
            string gcIcon, gcMsg;
            if (avgPause < 1 && peakPause < 2)
            {
                gcIcon = "&#9989;";
                gcMsg = $"GC Pause excelente ({avgPause:F2}% medio). Pausas totais de {FormatPauseMs(totalMs)}.";
            }
            else if (avgPause < 5 && peakPause < 10)
            {
                gcIcon = "&#9888;&#65039;";
                gcMsg = $"GC Pause aceitavel ({avgPause:F2}% medio, pico {peakPause:F2}%). Total: {FormatPauseMs(totalMs)}.";
            }
            else
            {
                gcIcon = "&#128680;";
                gcMsg = $"GC Pause alto ({avgPause:F2}% medio, pico {peakPause:F2}%). Total: {FormatPauseMs(totalMs)}. Reduza alocacoes ou investigue objetos Gen2.";
            }
            sb.Append($"""
                                    <div class="diag-item">
                                        <span class="diag-icon">{gcIcon}</span>
                                        <div><div class="diag-label">GC Pause</div><div class="diag-msg">{gcMsg}</div></div>
                                    </div>
                """);
        }

        // 4. GC Generations
        {
            var gen2 = r.GcGen2;
            var gen1 = r.GcGen1;
            var gen0 = r.GcGen0;
            string genIcon, genMsg;
            if (gen2 == 0 && gen1 <= 5)
            {
                genIcon = "&#9989;";
                genMsg = $"GC saudavel: Gen0={gen0}, Gen1={gen1}, Gen2={gen2}. Sem coletas de geracao 2.";
            }
            else if (gen2 <= 3)
            {
                genIcon = "&#9989;";
                genMsg = $"GC normal: Gen0={gen0}, Gen1={gen1}, Gen2={gen2}. Poucas coletas Gen2.";
            }
            else if (gen2 <= 10)
            {
                genIcon = "&#9888;&#65039;";
                genMsg = $"GC com atencao: Gen0={gen0}, Gen1={gen1}, Gen2={gen2}. Coletas Gen2 frequentes podem impactar latencia.";
            }
            else
            {
                genIcon = "&#128680;";
                genMsg = $"GC sob pressao: Gen0={gen0}, Gen1={gen1}, Gen2={gen2}. Muitas coletas Gen2 indicam objetos de longa duracao excessivos.";
            }
            sb.Append($"""
                                    <div class="diag-item">
                                        <span class="diag-icon">{genIcon}</span>
                                        <div><div class="diag-label">GC Geracoes</div><div class="diag-msg">{genMsg}</div></div>
                                    </div>
                """);
        }

        // 5. Network
        {
            var sent = r.NetworkBytesSent;
            var recv = r.NetworkBytesReceived;
            string netIcon, netMsg;
            if (sent == 0 && recv == 0)
            {
                netIcon = "&#9989;";
                netMsg = "Nenhum trafego de rede detectado. Esperado para benchmarks de computacao pura.";
            }
            else
            {
                netIcon = "&#9989;";
                netMsg = $"Trafego de rede: {FormatBytes(sent)} enviado, {FormatBytes(recv)} recebido. Verifique se e esperado para este benchmark.";
            }
            sb.Append($"""
                                    <div class="diag-item">
                                        <span class="diag-icon">{netIcon}</span>
                                        <div><div class="diag-label">Rede</div><div class="diag-msg">{netMsg}</div></div>
                                    </div>
                """);
        }

        // 6. Heap vs Working Set relationship
        {
            var heapFinal = r.FinalHeapMb;
            var hasMd = medianData.TryGetValue(r.Name, out var mdd);
            var wsMedian = hasMd ? mdd.WsMedian : 0;
            if (heapFinal > 0 && wsMedian > 0)
            {
                var ratio = wsMedian / heapFinal;
                string relIcon, relMsg;
                if (ratio < 1.5)
                {
                    relIcon = "&#9989;";
                    relMsg = $"Working Set ({wsMedian:F1} MB) proximo do Heap ({heapFinal:F2} MB). Quase toda memoria e gerenciada.";
                }
                else if (ratio < 3)
                {
                    relIcon = "&#9888;&#65039;";
                    relMsg = $"Working Set ({wsMedian:F1} MB) e {ratio:F1}x o Heap ({heapFinal:F2} MB). Parte significativa e memoria nativa (buffers, codigo, etc).";
                }
                else
                {
                    relIcon = "&#128680;";
                    relMsg = $"Working Set ({wsMedian:F1} MB) e {ratio:F1}x o Heap ({heapFinal:F2} MB). Grande volume de memoria nativa — investigue buffers, conexoes ou libs nativas.";
                }
                sb.Append($"""
                                    <div class="diag-item">
                                        <span class="diag-icon">{relIcon}</span>
                                        <div><div class="diag-label">Heap vs Working Set</div><div class="diag-msg">{relMsg}</div></div>
                                    </div>
                    """);
            }
        }

        // 7-13: Sample-based diagnostics (need samples for trends/percentiles)
        if (!string.IsNullOrEmpty(r.SamplesJson))
        {
            var diagSamples = ParseSamplesForPercentiles(r.SamplesJson);
            if (diagSamples.Count >= 10)
            {
                // 7. GC Efficiency: ratio of Gen0 to Gen1+Gen2 (promotion rate)
                {
                    var gen0 = r.GcGen0;
                    var gen1 = r.GcGen1;
                    var gen2 = r.GcGen2;
                    var promoted = gen1 + gen2;
                    if (gen0 > 0)
                    {
                        var promotionRate = (double)promoted / gen0 * 100;
                        string effIcon, effMsg;
                        if (promotionRate < 5)
                        {
                            effIcon = "&#9989;";
                            effMsg = $"Taxa de promocao excelente: apenas {promotionRate:F1}% das coletas Gen0 promoveram para geracoes superiores. Objetos sao efemeros.";
                        }
                        else if (promotionRate < 20)
                        {
                            effIcon = "&#9888;&#65039;";
                            effMsg = $"Taxa de promocao de {promotionRate:F1}%. Alguns objetos sobrevivem Gen0 e sao promovidos. Considere pooling ou Span<T> para reducao.";
                        }
                        else
                        {
                            effIcon = "&#128680;";
                            effMsg = $"Taxa de promocao alta: {promotionRate:F1}%! Muitos objetos sobrevivem Gen0. Isso causa pressao nas geracoes superiores e pausas maiores.";
                        }
                        sb.Append($"""
                                    <div class="diag-item">
                                        <span class="diag-icon">{effIcon}</span>
                                        <div><div class="diag-label">Eficiencia do GC</div><div class="diag-msg">{effMsg}</div></div>
                                    </div>
                        """);
                    }
                }

                // 8. CPU Stability (coefficient of variation)
                {
                    var cpuVals = diagSamples.Select(s => s.Cpu).ToArray();
                    var cpuMean = cpuVals.Average();
                    if (cpuMean > 1)
                    {
                        var cpuStdDev = Math.Sqrt(cpuVals.Sum(v => (v - cpuMean) * (v - cpuMean)) / cpuVals.Length);
                        var cv = cpuStdDev / cpuMean * 100;
                        string cvIcon, cvMsg;
                        if (cv < 30)
                        {
                            cvIcon = "&#9989;";
                            cvMsg = $"CPU muito estavel (CV={cv:F0}%). Pouca variacao entre amostras — comportamento previsivel.";
                        }
                        else if (cv < 80)
                        {
                            cvIcon = "&#9888;&#65039;";
                            cvMsg = $"CPU com variacao moderada (CV={cv:F0}%). Existem oscilacoes — pode haver fases de warm-up ou GC intercaladas.";
                        }
                        else
                        {
                            cvIcon = "&#128680;";
                            cvMsg = $"CPU altamente instavel (CV={cv:F0}%). Comportamento erratico — investigue contencao de threads, GC ou I/O bloqueante.";
                        }
                        sb.Append($"""
                                    <div class="diag-item">
                                        <span class="diag-icon">{cvIcon}</span>
                                        <div><div class="diag-label">Estabilidade CPU</div><div class="diag-msg">{cvMsg}</div></div>
                                    </div>
                        """);
                    }
                }

                // 9. Memory trend (linear regression slope on heap)
                {
                    var heapVals = diagSamples.Select(s => s.Heap).ToArray();
                    var n = heapVals.Length;
                    double sx = 0, sy = 0, sxy = 0, sx2 = 0;
                    for (var i = 0; i < n; i++) { sx += i; sy += heapVals[i]; sxy += i * heapVals[i]; sx2 += i * i; }
                    var slope = (n * sxy - sx * sy) / (n * sx2 - sx * sx);
                    var slopePerMin = slope * 60; // MB per minute (samples are ~1s apart)
                    string trendIcon, trendMsg;
                    if (Math.Abs(slopePerMin) < 0.1)
                    {
                        trendIcon = "&#9989;";
                        trendMsg = $"Tendencia do Heap plana ({slopePerMin:+0.00;-0.00} MB/min). Memoria totalmente estabilizada.";
                    }
                    else if (slopePerMin < 0)
                    {
                        trendIcon = "&#9989;";
                        trendMsg = $"Tendencia do Heap decrescente ({slopePerMin:+0.00;-0.00} MB/min). GC esta reduzindo o Heap ativamente.";
                    }
                    else if (slopePerMin < 1)
                    {
                        trendIcon = "&#9888;&#65039;";
                        trendMsg = $"Heap crescendo lentamente ({slopePerMin:+0.00;-0.00} MB/min). Pode ser warm-up ou crescimento lento de cache.";
                    }
                    else
                    {
                        trendIcon = "&#128680;";
                        trendMsg = $"Heap crescendo a {slopePerMin:+0.00;-0.00} MB/min! Ritmo significativo — em 1h seriam +{slopePerMin * 60:F0} MB adicionais.";
                    }
                    sb.Append($"""
                                    <div class="diag-item">
                                        <span class="diag-icon">{trendIcon}</span>
                                        <div><div class="diag-label">Tendencia do Heap</div><div class="diag-msg">{trendMsg}</div></div>
                                    </div>
                        """);
                }

                // 10. GC Pause spikes (P99 vs P50)
                {
                    var gcPauseVals = diagSamples.Select(s => s.GcPause).OrderBy(v => v).ToArray();
                    var p50 = Percentile(gcPauseVals, 50);
                    var p99 = Percentile(gcPauseVals, 99);
                    if (p50 > 0)
                    {
                        var spikeRatio = p99 / p50;
                        string spkIcon, spkMsg;
                        if (spikeRatio < 2)
                        {
                            spkIcon = "&#9989;";
                            spkMsg = $"GC Pause sem picos: P50={p50:F2}%, P99={p99:F2}% (x{spikeRatio:F1}). Comportamento uniforme.";
                        }
                        else if (spikeRatio < 5)
                        {
                            spkIcon = "&#9888;&#65039;";
                            spkMsg = $"GC Pause com picos moderados: P99={p99:F2}% e {spikeRatio:F1}x o P50={p50:F2}%. Existem momentos de maior pressao.";
                        }
                        else
                        {
                            spkIcon = "&#128680;";
                            spkMsg = $"GC Pause com picos severos: P99={p99:F2}% e {spikeRatio:F1}x o P50={p50:F2}%! Pausas esporadicas muito maiores que o normal.";
                        }
                        sb.Append($"""
                                    <div class="diag-item">
                                        <span class="diag-icon">{spkIcon}</span>
                                        <div><div class="diag-label">Picos de GC Pause</div><div class="diag-msg">{spkMsg}</div></div>
                                    </div>
                        """);
                    }
                }

                // 11. Network symmetry (sent vs received ratio)
                {
                    var sent = r.NetworkBytesSent;
                    var recv = r.NetworkBytesReceived;
                    if (sent > 0 && recv > 0)
                    {
                        var ratio = (double)sent / recv;
                        string symIcon, symMsg;
                        if (ratio > 0.5 && ratio < 2)
                        {
                            symIcon = "&#9989;";
                            symMsg = $"Trafego simetrico: enviou {FormatBytes(sent)}, recebeu {FormatBytes(recv)} (ratio {ratio:F2}). Padrao request/response equilibrado.";
                        }
                        else if (ratio >= 2)
                        {
                            symIcon = "&#9888;&#65039;";
                            symMsg = $"Upload dominante: enviou {FormatBytes(sent)} vs recebeu {FormatBytes(recv)} (ratio {ratio:F1}x). Benchmark faz mais upload (ex: streaming, logging remoto).";
                        }
                        else
                        {
                            symIcon = "&#9888;&#65039;";
                            symMsg = $"Download dominante: recebeu {FormatBytes(recv)} vs enviou {FormatBytes(sent)} (ratio 1:{1.0 / ratio:F1}). Benchmark consome mais dados do que envia.";
                        }
                        sb.Append($"""
                                    <div class="diag-item">
                                        <span class="diag-icon">{symIcon}</span>
                                        <div><div class="diag-label">Simetria de Rede</div><div class="diag-msg">{symMsg}</div></div>
                                    </div>
                        """);
                    }
                }

                // 12. Warm-up detection (first quarter avg vs remaining avg for CPU)
                {
                    var cpuVals = diagSamples.Select(s => s.Cpu).ToArray();
                    var quarterSize = Math.Max(1, cpuVals.Length / 4);
                    var firstQ = cpuVals.Take(quarterSize).Average();
                    var restAvg = cpuVals.Skip(quarterSize).Average();
                    if (restAvg > 1)
                    {
                        var warmupDiff = Math.Abs(firstQ - restAvg) / restAvg * 100;
                        string wuIcon, wuMsg;
                        if (warmupDiff < 20)
                        {
                            wuIcon = "&#9989;";
                            wuMsg = $"Sem warm-up detectado. CPU do 1o quartil ({firstQ:F1}%) similar ao restante ({restAvg:F1}%). Benchmark estavel desde o inicio.";
                        }
                        else if (firstQ < restAvg)
                        {
                            wuIcon = "&#9888;&#65039;";
                            wuMsg = $"Warm-up detectado: CPU do 1o quartil ({firstQ:F1}%) e {warmupDiff:F0}% menor que o restante ({restAvg:F1}%). Inicio mais lento — JIT ou caches frios.";
                        }
                        else
                        {
                            wuIcon = "&#9888;&#65039;";
                            wuMsg = $"Cool-down detectado: CPU do 1o quartil ({firstQ:F1}%) e {warmupDiff:F0}% maior que o restante ({restAvg:F1}%). Inicio mais intenso — possivelmente inicializacao pesada.";
                        }
                        sb.Append($"""
                                    <div class="diag-item">
                                        <span class="diag-icon">{wuIcon}</span>
                                        <div><div class="diag-label">Warm-up</div><div class="diag-msg">{wuMsg}</div></div>
                                    </div>
                        """);
                    }
                }

                // 13. Overall health score
                {
                    var score = 100;
                    var issues = new List<string>();

                    // Memory
                    if (r.MemoryGrowth?.ToUpperInvariant() == "GROWING" && r.HeapGrowthPercent > 50) { score -= 25; issues.Add("heap crescendo >50%"); }
                    else if (r.MemoryGrowth?.ToUpperInvariant() == "GROWING") { score -= 10; issues.Add("heap crescendo"); }

                    // CPU
                    if (r.AvgCpuPercent > 80) { score -= 15; issues.Add("CPU >80%"); }
                    else if (r.AvgCpuPercent > 50) { score -= 5; issues.Add("CPU elevado"); }

                    // GC Pause
                    if (r.AvgGcPausePercent >= 5) { score -= 20; issues.Add("GC Pause alto"); }
                    else if (r.AvgGcPausePercent >= 1) { score -= 5; issues.Add("GC Pause moderado"); }

                    // Gen2
                    if (r.GcGen2 > 10) { score -= 15; issues.Add($"Gen2={r.GcGen2}"); }
                    else if (r.GcGen2 > 3) { score -= 5; issues.Add($"Gen2={r.GcGen2}"); }

                    // Heap trend
                    var heapSlope = ComputeHeapSlopePerMin(diagSamples);
                    if (heapSlope > 1) { score -= 15; issues.Add($"heap +{heapSlope:F1} MB/min"); }
                    else if (heapSlope > 0.1) { score -= 5; issues.Add("heap crescendo lentamente"); }

                    score = Math.Max(0, score);
                    string scoreIcon, scoreColor, scoreMsg;
                    if (score >= 90)
                    {
                        scoreIcon = "&#127942;"; // trophy
                        scoreColor = "var(--passed)";
                        scoreMsg = issues.Count == 0
                            ? $"Saude geral: <b>{score}/100</b> — Excelente! Nenhum problema detectado."
                            : $"Saude geral: <b>{score}/100</b> — Muito bom. Pontos menores: {string.Join(", ", issues)}.";
                    }
                    else if (score >= 70)
                    {
                        scoreIcon = "&#128077;"; // thumbs up
                        scoreColor = "var(--passed)";
                        scoreMsg = $"Saude geral: <b>{score}/100</b> — Bom, com pontos de atencao: {string.Join(", ", issues)}.";
                    }
                    else if (score >= 50)
                    {
                        scoreIcon = "&#9888;&#65039;";
                        scoreColor = "var(--warn)";
                        scoreMsg = $"Saude geral: <b>{score}/100</b> — Atencao necessaria: {string.Join(", ", issues)}.";
                    }
                    else
                    {
                        scoreIcon = "&#128680;";
                        scoreColor = "var(--failed)";
                        scoreMsg = $"Saude geral: <b>{score}/100</b> — Problemas criticos: {string.Join(", ", issues)}.";
                    }
                    sb.Append($"""
                                    <div class="diag-item" style="border:2px solid {scoreColor};grid-column:1/-1">
                                        <span class="diag-icon">{scoreIcon}</span>
                                        <div><div class="diag-label">Score de Saude</div><div class="diag-msg">{scoreMsg}</div></div>
                                    </div>
                        """);
                }
            }
        }

        sb.Append("""
                                </div>
                            </div>
                        </div>
            """);

        if (hasSamples)
        {
            sb.Append($"""
                        <div class="chart-label">Linha do Tempo de Metricas</div>
                        <div class="chart-label" style="font-size:.75rem;color:var(--muted)">CPU</div>
                        <div class="timeline-chart"><canvas id="{canvasId}_cpu"></canvas></div>
                        <div class="chart-label" style="font-size:.75rem;color:var(--muted)">Memoria</div>
                        <div class="timeline-chart"><canvas id="{canvasId}_mem"></canvas></div>
                        <div class="chart-label" style="font-size:.75rem;color:var(--muted)">Garbage Collector</div>
                        <div class="timeline-chart"><canvas id="{canvasId}_gc"></canvas></div>
                        <div class="chart-label" style="font-size:.75rem;color:var(--muted)">Rede</div>
                        <div class="timeline-chart"><canvas id="{canvasId}_net"></canvas></div>
                        <script type="application/json" id="{canvasId}_data">{r.SamplesJson}</script>
                """);

            // Percentile table inside the detail
            var percentiles = new[] { 25, 50, 70, 75, 90, 95, 99 };
            var samples = ParseSamplesForPercentiles(r.SamplesJson);
            if (samples.Count >= 2)
            {
                var cpuValues = samples.Select(s => s.Cpu).OrderBy(v => v).ToArray();
                var heapValues = samples.Select(s => s.Heap).OrderBy(v => v).ToArray();
                var ioSentDeltas = new List<double>();
                var ioRecvDeltas = new List<double>();
                for (var i = 1; i < samples.Count; i++)
                {
                    ioSentDeltas.Add((samples[i].NetS - samples[i - 1].NetS) / 1024.0);
                    ioRecvDeltas.Add((samples[i].NetR - samples[i - 1].NetR) / 1024.0);
                }
                var ioSentArr = ioSentDeltas.OrderBy(v => v).ToArray();
                var ioRecvArr = ioRecvDeltas.OrderBy(v => v).ToArray();

                sb.Append("""
                        <div class="chart-label" style="margin-top:1.5rem">Estatisticas e Percentis</div>
                        <div class="percentile-tables">
                            <table class="bench-table percentile-table">
                                <thead><tr><th>Metrica</th><th class="num">Min</th><th class="num">Media</th><th class="num">Max</th><th class="num">Desvio Padrao</th>
                """);
                foreach (var p in percentiles)
                    sb.Append(CultureInfo.InvariantCulture, $"<th class=\"num\">P{p}</th>");
                sb.Append("</tr></thead><tbody>");

                var gcPauseValues = samples.Select(s => s.GcPause).OrderBy(v => v).ToArray();
                var gcPauseMsDeltas = new List<double>();
                for (var i = 1; i < samples.Count; i++)
                    gcPauseMsDeltas.Add(samples[i].GcPauseMs - samples[i - 1].GcPauseMs);
                var gcPauseMsArr = gcPauseMsDeltas.OrderBy(v => v).ToArray();

                AppendStatsRow(sb, "CPU (%)", cpuValues, percentiles, "F1");
                AppendStatsRow(sb, "Heap GC (MB)", heapValues, percentiles, "F2");
                AppendStatsRow(sb, "GC Pause (%)", gcPauseValues, percentiles, "F2");
                AppendStatsRow(sb, "GC Pause (ms/s)", gcPauseMsArr, percentiles, "F1");
                AppendStatsRow(sb, "Rede Enviado (KB/s)", ioSentArr, percentiles, "F1");
                AppendStatsRow(sb, "Rede Recebido (KB/s)", ioRecvArr, percentiles, "F1");

                sb.Append("""
                                </tbody>
                            </table>
                        </div>
                """);

                // Correlation matrix section
                sb.Append($"""
                        <div class="corr-section" id="{canvasId}_corr">
                            <div class="chart-label" style="margin-top:1.5rem">Matriz de Correlacao</div>
                            <p style="font-size:.75rem;color:var(--muted);margin-bottom:.75rem">Arraste as metricas para Linhas e Colunas para calcular a correlacao de Pearson.</p>
                            <div style="display:flex;gap:1.5rem;flex-wrap:wrap;font-size:.7rem;color:var(--muted);margin-bottom:.75rem;align-items:center">
                                <span><b>Escala:</b></span>
                                <span style="display:inline-flex;align-items:center;gap:.3rem"><span style="width:14px;height:14px;border-radius:3px;background:rgba(239,68,68,0.85);display:inline-block"></span> -1.0 a -0.7 Forte negativa</span>
                                <span style="display:inline-flex;align-items:center;gap:.3rem"><span style="width:14px;height:14px;border-radius:3px;background:rgba(239,68,68,0.45);display:inline-block"></span> -0.7 a -0.3 Moderada negativa</span>
                                <span style="display:inline-flex;align-items:center;gap:.3rem"><span style="width:14px;height:14px;border-radius:3px;background:transparent;border:1px solid var(--border);display:inline-block"></span> -0.3 a +0.3 Fraca/nenhuma</span>
                                <span style="display:inline-flex;align-items:center;gap:.3rem"><span style="width:14px;height:14px;border-radius:3px;background:rgba(16,185,129,0.45);display:inline-block"></span> +0.3 a +0.7 Moderada positiva</span>
                                <span style="display:inline-flex;align-items:center;gap:.3rem"><span style="width:14px;height:14px;border-radius:3px;background:rgba(16,185,129,0.85);display:inline-block"></span> +0.7 a +1.0 Forte positiva</span>
                            </div>
                            <div class="corr-chips" data-corr-chips="{canvasId}">
                                <span class="corr-chip" draggable="true" data-metric="cpu">CPU (%)</span>
                                <span class="corr-chip" draggable="true" data-metric="heap">Heap GC (MB)</span>
                                <span class="corr-chip" draggable="true" data-metric="ws">Working Set (MB)</span>
                                <span class="corr-chip" draggable="true" data-metric="gcPause">GC Pause (%)</span>
                                <span class="corr-chip" draggable="true" data-metric="gcPauseMsDelta">GC Pause (ms/s)</span>
                                <span class="corr-chip" draggable="true" data-metric="netSDelta">Rede Enviado (KB/s)</span>
                                <span class="corr-chip" draggable="true" data-metric="netRDelta">Rede Recebido (KB/s)</span>
                            </div>
                            <div class="corr-dropzones">
                                <div>
                                    <div class="corr-dropzone-label">Linhas</div>
                                    <div class="corr-dropzone" data-corr-zone="rows" data-corr-id="{canvasId}"></div>
                                </div>
                                <div>
                                    <div class="corr-dropzone-label">Colunas</div>
                                    <div class="corr-dropzone" data-corr-zone="cols" data-corr-id="{canvasId}"></div>
                                </div>
                            </div>
                            <div class="corr-matrix" id="{canvasId}_corrMatrix"></div>
                        </div>
                """);
            }
        }

        sb.Append("""
                    </div>
                </article>
            """);

        benchIdx++;
    }

    sb.Append("""
            </section>
        """);

    // --- Footer ---
    sb.Append($"""

            <footer class="footer">
                <p><strong>Bedrock Framework</strong> - Relatorio de Benchmarks</p>
                <p>Total: {totalBenchmarks} benchmarks | OK: {passCount} | Warnings: {warnCount}</p>
            </footer>
        </div>
        <script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.1/dist/chart.umd.min.js"></script>
        <script>
        """);

    // Build chart data
    var chartLabels = new StringBuilder();
    var first = true;
    foreach (var r in results)
    {
        if (!first) chartLabels.Append(',');
        first = false;
        var shortName = r.Name.Contains('.') ? r.Name[(r.Name.LastIndexOf('.') + 1)..] : r.Name;
        chartLabels.Append(CultureInfo.InvariantCulture, $"'{EscapeJs(shortName)}'");
    }

    if (hasMeanTimeData)
    {
        var chartData = new StringBuilder();
        var chartColors = new StringBuilder();
        first = true;
        foreach (var r in results)
        {
            if (!first) { chartData.Append(','); chartColors.Append(','); }
            first = false;
            var meanVal = r.Median > 0 ? r.Median / 1_000_000.0 : ParseTimeToMs(r.MeanTime ?? "0");
            chartData.Append(CultureInfo.InvariantCulture, $"{meanVal:F3}");
            chartColors.Append(r.Status == "WARN" ? "'#f59e0b'" : "'#10b981'");
        }

        sb.Append(CultureInfo.InvariantCulture, $@"
        var meanChart=new Chart(document.getElementById('meanChart'),{{type:'bar',data:{{labels:[{chartLabels}],datasets:[{{label:'Tempo Medio (ms)',data:[{chartData}],backgroundColor:[{chartColors}],borderWidth:0,borderRadius:4}}]}},options:{{responsive:true,maintainAspectRatio:false,plugins:{{legend:{{display:false}},title:{{display:true,text:'Tempo Medio por Benchmark (ms)',color:getComputedStyle(document.body).getPropertyValue('--text').trim()}}}},scales:{{y:{{beginAtZero:true,grid:{{color:'rgba(148,163,184,0.1)'}},ticks:{{color:getComputedStyle(document.body).getPropertyValue('--muted').trim()}}}},x:{{grid:{{display:false}},ticks:{{color:getComputedStyle(document.body).getPropertyValue('--muted').trim(),maxRotation:45}}}}}}}}}});");
    }
    else
    {
        // Sustained benchmarks: show Heap Growth, CPU and Network charts
        var heapData = new StringBuilder();
        var heapColors = new StringBuilder();
        var cpuData = new StringBuilder();
        var cpuColors = new StringBuilder();
        var netSentData = new StringBuilder();
        var netRecvData = new StringBuilder();
        first = true;
        foreach (var r in results)
        {
            if (!first) { heapData.Append(','); heapColors.Append(','); cpuData.Append(','); cpuColors.Append(','); netSentData.Append(','); netRecvData.Append(','); }
            first = false;
            var heapGrowthMb = r.FinalHeapMb - r.InitialHeapMb;
            heapData.Append(CultureInfo.InvariantCulture, $"{heapGrowthMb:F2}");
            heapColors.Append(heapGrowthMb > 5 ? "'#f59e0b'" : heapGrowthMb < 0 ? "'#06b6d4'" : "'#10b981'");
            cpuData.Append(CultureInfo.InvariantCulture, $"{r.AvgCpuPercent:F1}");
            cpuColors.Append(r.AvgCpuPercent > 50 ? "'#f59e0b'" : "'#8b5cf6'");
            var sentMb = r.NetworkBytesSent / (1024.0 * 1024.0);
            var recvMb = r.NetworkBytesReceived / (1024.0 * 1024.0);
            netSentData.Append(CultureInfo.InvariantCulture, $"{sentMb:F2}");
            netRecvData.Append(CultureInfo.InvariantCulture, $"{recvMb:F2}");
        }

        sb.Append(CultureInfo.InvariantCulture, $@"
        var heapChart=new Chart(document.getElementById('heapChart'),{{type:'bar',data:{{labels:[{chartLabels}],datasets:[{{label:'Crescimento Heap (MB)',data:[{heapData}],backgroundColor:[{heapColors}],borderWidth:0,borderRadius:4}}]}},options:{{responsive:true,maintainAspectRatio:false,plugins:{{legend:{{display:false}},title:{{display:true,text:'Heap Growth por Benchmark (MB)',color:getComputedStyle(document.body).getPropertyValue('--text').trim()}}}},scales:{{y:{{grid:{{color:'rgba(148,163,184,0.1)'}},ticks:{{color:getComputedStyle(document.body).getPropertyValue('--muted').trim()}}}},x:{{grid:{{display:false}},ticks:{{color:getComputedStyle(document.body).getPropertyValue('--muted').trim(),maxRotation:45}}}}}}}}}});
        var cpuChart=new Chart(document.getElementById('cpuChart'),{{type:'bar',data:{{labels:[{chartLabels}],datasets:[{{label:'CPU Medio (%)',data:[{cpuData}],backgroundColor:[{cpuColors}],borderWidth:0,borderRadius:4}}]}},options:{{responsive:true,maintainAspectRatio:false,plugins:{{legend:{{display:false}},title:{{display:true,text:'CPU Medio por Benchmark (%)',color:getComputedStyle(document.body).getPropertyValue('--text').trim()}}}},scales:{{y:{{beginAtZero:true,grid:{{color:'rgba(148,163,184,0.1)'}},ticks:{{color:getComputedStyle(document.body).getPropertyValue('--muted').trim()}}}},x:{{grid:{{display:false}},ticks:{{color:getComputedStyle(document.body).getPropertyValue('--muted').trim(),maxRotation:45}}}}}}}}}});
        var networkChart=new Chart(document.getElementById('networkChart'),{{type:'bar',data:{{labels:[{chartLabels}],datasets:[{{label:'Enviado (MB)',data:[{netSentData}],backgroundColor:'#10b981',borderWidth:0,borderRadius:4}},{{label:'Recebido (MB)',data:[{netRecvData}],backgroundColor:'#3b82f6',borderWidth:0,borderRadius:4}}]}},options:{{responsive:true,maintainAspectRatio:false,plugins:{{legend:{{display:true,labels:{{color:getComputedStyle(document.body).getPropertyValue('--text').trim(),usePointStyle:true,boxWidth:8}}}},title:{{display:true,text:'Network I/O por Benchmark (MB)',color:getComputedStyle(document.body).getPropertyValue('--text').trim()}}}},scales:{{y:{{beginAtZero:true,grid:{{color:'rgba(148,163,184,0.1)'}},ticks:{{color:getComputedStyle(document.body).getPropertyValue('--muted').trim()}}}},x:{{grid:{{display:false}},ticks:{{color:getComputedStyle(document.body).getPropertyValue('--muted').trim(),maxRotation:45}}}}}}}}}});");
    }

    sb.Append("""

        document.querySelectorAll('.detail-header').forEach(h=>h.addEventListener('click',()=>h.closest('.detail').classList.toggle('collapsed')));
        var overviewCharts=[]; if(typeof meanChart!=='undefined')overviewCharts.push(meanChart); if(typeof heapChart!=='undefined')overviewCharts.push(heapChart); if(typeof cpuChart!=='undefined')overviewCharts.push(cpuChart); if(typeof networkChart!=='undefined')overviewCharts.push(networkChart);
        function toggleTheme(){document.body.classList.toggle('light-theme');localStorage.setItem('bench-theme',document.body.classList.contains('light-theme')?'light':'dark');updateChart();timelineCharts.forEach(c=>c.update());}
        function updateChart(){var c=getComputedStyle(document.body);var txt=c.getPropertyValue('--text').trim();var mt=c.getPropertyValue('--muted').trim();overviewCharts.forEach(function(ch){ch.options.plugins.title.color=txt;if(ch.options.plugins.legend&&ch.options.plugins.legend.display){ch.options.plugins.legend.labels.color=txt;}ch.options.scales.y.ticks.color=mt;ch.options.scales.x.ticks.color=mt;ch.update();});}

        // Timeline charts for each benchmark with samples (4 charts per benchmark)
        var timelineCharts=[];
        var baseIds=new Set();
        document.querySelectorAll('script[id$="_data"]').forEach(function(el){
            var base=el.id.replace('_data','');
            baseIds.add(base);
        });
        baseIds.forEach(function(base){
            var dataEl=document.getElementById(base+'_data');
            if(!dataEl)return;
            var samples=JSON.parse(dataEl.textContent);
            if(!samples.length)return;
            var t0=samples[0].t;
            var labels=samples.map(function(s){var d=s.t-t0;var m=Math.floor(d/60);var sec=d%60;return m+':'+(sec<10?'0':'')+sec;});
            function delta(arr,key){return arr.map(function(s,i){return i===0?0:s[key]-arr[i-1][key];});}
            function trendline(data){var n=data.length;if(n<2)return data.slice();var sx=0,sy=0,sxy=0,sx2=0;for(var i=0;i<n;i++){sx+=i;sy+=data[i];sxy+=i*data[i];sx2+=i*i;}var slope=(n*sxy-sx*sy)/(n*sx2-sx*sx);var intercept=(sy-slope*sx)/n;return data.map(function(_,i){return Math.round((slope*i+intercept)*1000)/1000;});}
            function makeTrend(label,data,color){return{label:'\u2197 '+label,data:trendline(data),borderColor:color,borderDash:[6,4],borderWidth:1.5,pointRadius:0,pointHitRadius:0,fill:false,tension:0};}
            var mt=getComputedStyle(document.body).getPropertyValue('--muted').trim();
            var lg=getComputedStyle(document.body).getPropertyValue('--chart-legend').trim();
            var gridC='rgba(148,163,184,0.1)';
            var baseOpts={responsive:true,maintainAspectRatio:false,interaction:{mode:'index',intersect:false},plugins:{legend:{position:'top',labels:{usePointStyle:true,boxWidth:8,color:lg}},tooltip:{mode:'index',intersect:false}}};
            var xScale={display:true,title:{display:true,text:'Tempo (mm:ss)',color:mt},ticks:{color:mt,maxTicksLimit:20},grid:{color:gridC}};
            function makeY(label){return{type:'linear',display:true,position:'left',title:{display:true,text:label,color:mt},ticks:{color:mt},grid:{color:gridC}};}

            // Pre-compute data arrays for charts + trendlines
            var cpuData=samples.map(function(s){return s.cpu;});
            var heapData=samples.map(function(s){return s.heap;});
            var wsData=samples.map(function(s){return s.ws;});
            var gcPauseData=samples.map(function(s){return s.gcPause||0;});
            var gcPauseMsDelta=delta(samples,'gcPauseMs');
            var gen0Delta=delta(samples,'g0');
            var gen1Delta=delta(samples,'g1');
            var gen2Delta=delta(samples,'g2');
            var netSData=samples.map(function(s){return s.netS/1024;});
            var netRData=samples.map(function(s){return s.netR/1024;});
            var netSDelta=delta(samples,'netS').map(function(v){return v/1024;});
            var netRDelta=delta(samples,'netR').map(function(v){return v/1024;});

            // 1. CPU chart
            var cpuCanvas=document.getElementById(base+'_cpu');
            if(cpuCanvas){var c=new Chart(cpuCanvas,{type:'line',data:{labels:labels,datasets:[
                {label:'CPU (%)',data:cpuData,borderColor:'#f59e0b',backgroundColor:'rgba(245,158,11,0.1)',fill:true,tension:0.3,pointRadius:0,pointHitRadius:6},
                makeTrend('CPU',cpuData,'#f59e0b')
            ]},options:Object.assign({},baseOpts,{scales:{x:xScale,y:Object.assign(makeY('Percentual (%)'),{min:0,max:100})}})});timelineCharts.push(c);}

            // 2. Memory chart
            var memCanvas=document.getElementById(base+'_mem');
            if(memCanvas){var c=new Chart(memCanvas,{type:'line',data:{labels:labels,datasets:[
                {label:'Heap GC (MB)',data:heapData,borderColor:'#8b5cf6',backgroundColor:'rgba(139,92,246,0.1)',fill:true,tension:0.3,pointRadius:0,pointHitRadius:6},
                {label:'Conjunto de Trabalho (MB)',data:wsData,borderColor:'#06b6d4',backgroundColor:'rgba(6,182,212,0.1)',fill:true,tension:0.3,pointRadius:0,pointHitRadius:6},
                makeTrend('Heap',heapData,'#8b5cf6'),
                makeTrend('WS',wsData,'#06b6d4')
            ]},options:Object.assign({},baseOpts,{scales:{x:xScale,y:makeY('Memoria (MB)')}})});timelineCharts.push(c);}

            // 3. GC chart
            var gcCanvas=document.getElementById(base+'_gc');
            if(gcCanvas){var c=new Chart(gcCanvas,{type:'line',data:{labels:labels,datasets:[
                {label:'GC Pause (%)',data:gcPauseData,borderColor:'#e11d48',backgroundColor:'rgba(225,29,72,0.1)',fill:true,tension:0.3,pointRadius:0,pointHitRadius:6,yAxisID:'y'},
                {label:'\u0394 GC Pause (ms)',data:gcPauseMsDelta,borderColor:'#be123c',backgroundColor:'transparent',fill:false,tension:0.3,pointRadius:0,pointHitRadius:6,yAxisID:'y2'},
                {label:'\u0394 Gen0',data:gen0Delta,borderColor:'#ec4899',backgroundColor:'transparent',fill:false,tension:0.3,pointRadius:0,pointHitRadius:6,yAxisID:'y2'},
                {label:'\u0394 Gen1',data:gen1Delta,borderColor:'#f97316',backgroundColor:'transparent',fill:false,tension:0.3,pointRadius:0,pointHitRadius:6,yAxisID:'y2'},
                {label:'\u0394 Gen2',data:gen2Delta,borderColor:'#ef4444',backgroundColor:'transparent',fill:false,tension:0.3,pointRadius:0,pointHitRadius:6,yAxisID:'y2'},
                Object.assign(makeTrend('GC Pause',gcPauseData,'#e11d48'),{yAxisID:'y'}),
                Object.assign(makeTrend('\u0394 GC Pause (ms)',gcPauseMsDelta,'#be123c'),{yAxisID:'y2'}),
                Object.assign(makeTrend('\u0394 Gen0',gen0Delta,'#ec4899'),{yAxisID:'y2'}),
                Object.assign(makeTrend('\u0394 Gen1',gen1Delta,'#f97316'),{yAxisID:'y2'}),
                Object.assign(makeTrend('\u0394 Gen2',gen2Delta,'#ef4444'),{yAxisID:'y2'})
            ]},options:Object.assign({},baseOpts,{scales:{x:xScale,y:Object.assign(makeY('Percentual (%)'),{min:0}),y2:{type:'linear',display:true,position:'right',title:{display:true,text:'Quantidade',color:mt},ticks:{color:mt},grid:{drawOnChartArea:false}}}})});timelineCharts.push(c);}

            // 4. Network chart
            var netCanvas=document.getElementById(base+'_net');
            if(netCanvas){var c=new Chart(netCanvas,{type:'line',data:{labels:labels,datasets:[
                {label:'Enviado (KB)',data:netSData,borderColor:'#10b981',backgroundColor:'rgba(16,185,129,0.1)',fill:true,tension:0.3,pointRadius:0,pointHitRadius:6},
                {label:'Recebido (KB)',data:netRData,borderColor:'#3b82f6',backgroundColor:'rgba(59,130,246,0.1)',fill:true,tension:0.3,pointRadius:0,pointHitRadius:6},
                {label:'\u0394 Enviado (KB)',data:netSDelta,borderColor:'#34d399',backgroundColor:'transparent',fill:false,tension:0.3,pointRadius:0,pointHitRadius:6,hidden:true},
                {label:'\u0394 Recebido (KB)',data:netRDelta,borderColor:'#60a5fa',backgroundColor:'transparent',fill:false,tension:0.3,pointRadius:0,pointHitRadius:6,hidden:true},
                makeTrend('Enviado',netSData,'#10b981'),
                makeTrend('Recebido',netRData,'#3b82f6')
            ]},options:Object.assign({},baseOpts,{scales:{x:xScale,y:makeY('Kilobytes (KB)')}})});timelineCharts.push(c);}
        });

        // Correlation matrix drag-and-drop
        (function(){
            var metricLabels={cpu:'CPU (%)',heap:'Heap GC (MB)',ws:'Working Set (MB)',gcPause:'GC Pause (%)',gcPauseMsDelta:'GC Pause (ms/s)',netSDelta:'Rede Enviado (KB/s)',netRDelta:'Rede Recebido (KB/s)'};

            function getSeries(samples,metric){
                if(metric==='cpu')return samples.map(function(s){return s.cpu;});
                if(metric==='heap')return samples.map(function(s){return s.heap;});
                if(metric==='ws')return samples.map(function(s){return s.ws;});
                if(metric==='gcPause')return samples.map(function(s){return s.gcPause||0;});
                if(metric==='gcPauseMsDelta'){var r=[0];for(var i=1;i<samples.length;i++)r.push((samples[i].gcPauseMs||0)-(samples[i-1].gcPauseMs||0));return r;}
                if(metric==='netSDelta'){var r=[0];for(var i=1;i<samples.length;i++)r.push((samples[i].netS-samples[i-1].netS)/1024);return r;}
                if(metric==='netRDelta'){var r=[0];for(var i=1;i<samples.length;i++)r.push((samples[i].netR-samples[i-1].netR)/1024);return r;}
                return [];
            }

            function pearson(a,b){
                var n=Math.min(a.length,b.length);if(n<2)return 0;
                var ma=0,mb=0;for(var i=0;i<n;i++){ma+=a[i];mb+=b[i];}ma/=n;mb/=n;
                var num=0,da=0,db=0;
                for(var i=0;i<n;i++){var x=a[i]-ma,y=b[i]-mb;num+=x*y;da+=x*x;db+=y*y;}
                var den=Math.sqrt(da*db);return den===0?0:num/den;
            }

            function corrColor(r){
                var abs=Math.abs(r);
                if(abs<0.3)return 'transparent';
                var alpha=((abs-0.3)/0.7*0.7+0.15).toFixed(2);
                return r>0?'rgba(16,185,129,'+alpha+')':'rgba(239,68,68,'+alpha+')';
            }

            function renderMatrix(baseId){
                var dataEl=document.getElementById(baseId+'_data');
                if(!dataEl)return;
                var samples=JSON.parse(dataEl.textContent);
                if(!samples.length)return;
                var rowZone=document.querySelector('[data-corr-zone="rows"][data-corr-id="'+baseId+'"]');
                var colZone=document.querySelector('[data-corr-zone="cols"][data-corr-id="'+baseId+'"]');
                var matrixDiv=document.getElementById(baseId+'_corrMatrix');
                var rows=Array.from(rowZone.querySelectorAll('.corr-dropped')).map(function(e){return e.dataset.metric;});
                var cols=Array.from(colZone.querySelectorAll('.corr-dropped')).map(function(e){return e.dataset.metric;});
                if(rows.length===0||cols.length===0){matrixDiv.innerHTML='';return;}
                var seriesCache={};
                function get(m){if(!seriesCache[m])seriesCache[m]=getSeries(samples,m);return seriesCache[m];}
                var html='<table><thead><tr><th></th>';
                cols.forEach(function(c){html+='<th>'+metricLabels[c]+'</th>';});
                html+='</tr></thead><tbody>';
                rows.forEach(function(r){
                    html+='<tr><th>'+metricLabels[r]+'</th>';
                    cols.forEach(function(c){
                        var v=r===c?1:pearson(get(r),get(c));
                        var bg=corrColor(v);
                        html+='<td style="background:'+bg+'">'+v.toFixed(2)+'</td>';
                    });
                    html+='</tr>';
                });
                html+='</tbody></table>';
                matrixDiv.innerHTML=html;
            }

            function updateChipStates(baseId){
                var rowZone=document.querySelector('[data-corr-zone="rows"][data-corr-id="'+baseId+'"]');
                var colZone=document.querySelector('[data-corr-zone="cols"][data-corr-id="'+baseId+'"]');
                var used=new Set();
                [rowZone,colZone].forEach(function(z){if(z)z.querySelectorAll('.corr-dropped').forEach(function(e){used.add(e.dataset.metric);});});
                var chips=document.querySelector('[data-corr-chips="'+baseId+'"]');
                if(chips)chips.querySelectorAll('.corr-chip').forEach(function(ch){
                    ch.classList.toggle('used',used.has(ch.dataset.metric));
                });
            }

            // Setup drag events
            document.querySelectorAll('.corr-chip').forEach(function(chip){
                chip.addEventListener('dragstart',function(e){
                    e.dataTransfer.setData('text/plain',chip.dataset.metric);
                    e.dataTransfer.effectAllowed='copy';
                });
            });

            document.querySelectorAll('.corr-dropzone').forEach(function(zone){
                zone.addEventListener('dragover',function(e){e.preventDefault();e.dataTransfer.dropEffect='copy';zone.classList.add('drag-over');});
                zone.addEventListener('dragleave',function(){zone.classList.remove('drag-over');});
                zone.addEventListener('drop',function(e){
                    e.preventDefault();zone.classList.remove('drag-over');
                    var metric=e.dataTransfer.getData('text/plain');
                    if(!metric||!metricLabels[metric])return;
                    // Check not already in this zone
                    if(zone.querySelector('[data-metric="'+metric+'"]'))return;
                    var tag=document.createElement('span');
                    tag.className='corr-dropped';
                    tag.dataset.metric=metric;
                    tag.textContent=metricLabels[metric];
                    var baseId=zone.dataset.corrId;
                    tag.addEventListener('click',function(){tag.remove();updateChipStates(baseId);renderMatrix(baseId);});
                    zone.appendChild(tag);
                    updateChipStates(baseId);
                    renderMatrix(baseId);
                });
            });
        })();

        (function(){if(localStorage.getItem('bench-theme')==='light')document.body.classList.add('light-theme');updateChart();})();
        </script>
        </body>
        </html>
        """);

    return sb.ToString();
}

static string EscapeJs(string value)
{
    return value.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\n", "\\n");
}

static double ParseTimeToMs(string timeStr)
{
    if (string.IsNullOrEmpty(timeStr)) return 0;
    var s = timeStr.Trim().ToLowerInvariant();
    if (s.EndsWith("ns")) return double.TryParse(s[..^2], NumberStyles.Float, CultureInfo.InvariantCulture, out var ns) ? ns / 1_000_000 : 0;
    if (s.EndsWith("us")) return double.TryParse(s[..^2], NumberStyles.Float, CultureInfo.InvariantCulture, out var us) ? us / 1_000 : 0;
    if (s.EndsWith("ms")) return double.TryParse(s[..^2], NumberStyles.Float, CultureInfo.InvariantCulture, out var ms) ? ms : 0;
    if (s.EndsWith("s")) return double.TryParse(s[..^1], NumberStyles.Float, CultureInfo.InvariantCulture, out var sec) ? sec * 1_000 : 0;
    return double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var raw) ? raw : 0;
}

static void AppendStatsRow(StringBuilder sb, string label, double[] sorted, int[] percentiles, string fmt)
{
    if (sorted.Length == 0)
    {
        sb.Append($"<tr><td>{label}</td>");
        // 4 stats + percentiles columns
        for (var i = 0; i < 4 + percentiles.Length; i++)
            sb.Append("<td class=\"num\">-</td>");
        sb.Append("</tr>");
        return;
    }

    var min = sorted[0];
    var max = sorted[^1];
    var mean = sorted.Average();
    var stddev = Math.Sqrt(sorted.Sum(v => (v - mean) * (v - mean)) / sorted.Length);
    var p50 = Percentile(sorted, 50);

    sb.Append(CultureInfo.InvariantCulture, $"<tr><td>{label}</td>");
    sb.Append(CultureInfo.InvariantCulture, $"<td class=\"num\">{min.ToString(fmt, CultureInfo.InvariantCulture)}</td>");
    sb.Append(CultureInfo.InvariantCulture, $"<td class=\"num\">{mean.ToString(fmt, CultureInfo.InvariantCulture)}</td>");
    sb.Append(CultureInfo.InvariantCulture, $"<td class=\"num\">{max.ToString(fmt, CultureInfo.InvariantCulture)}</td>");
    sb.Append(CultureInfo.InvariantCulture, $"<td class=\"num\">{stddev.ToString(fmt, CultureInfo.InvariantCulture)}</td>");

    foreach (var p in percentiles)
    {
        var val = Percentile(sorted, p);
        var valStr = val.ToString(fmt, CultureInfo.InvariantCulture);
        if (p == 50)
        {
            sb.Append(CultureInfo.InvariantCulture, $"<td class=\"num\">{valStr}</td>");
        }
        else if (p50 == 0)
        {
            // P50 is zero — show absolute difference instead of percentage
            var absDiff = val - p50;
            if (absDiff == 0)
            {
                sb.Append(CultureInfo.InvariantCulture, $"<td class=\"num\">{valStr}</td>");
            }
            else
            {
                var sign = absDiff >= 0 ? "+" : "";
                var cssClass = absDiff > 0 ? "pct-up" : "pct-down";
                sb.Append(CultureInfo.InvariantCulture, $"<td class=\"num\">{valStr} <span class=\"{cssClass}\">({sign}{absDiff.ToString(fmt, CultureInfo.InvariantCulture)})</span></td>");
            }
        }
        else
        {
            var pctDiff = ((val - p50) / Math.Abs(p50)) * 100.0;
            var sign = pctDiff >= 0 ? "+" : "";
            var cssClass = Math.Abs(pctDiff) < 5 ? "pct-neutral" : pctDiff > 0 ? "pct-up" : "pct-down";
            sb.Append(CultureInfo.InvariantCulture, $"<td class=\"num\">{valStr} <span class=\"{cssClass}\">({sign}{pctDiff:F0}%)</span></td>");
        }
    }
    sb.Append("</tr>");
}

static double Percentile(double[] sorted, int p)
{
    if (sorted.Length == 0) return 0;
    var rank = (p / 100.0) * (sorted.Length - 1);
    var lower = (int)Math.Floor(rank);
    var upper = (int)Math.Ceiling(rank);
    if (lower == upper) return sorted[lower];
    var weight = rank - lower;
    return sorted[lower] * (1 - weight) + sorted[upper] * weight;
}

static double ComputeHeapSlopePerMin(List<SampleData> samples)
{
    var n = samples.Count;
    if (n < 2) return 0;
    double sx = 0, sy = 0, sxy = 0, sx2 = 0;
    for (var i = 0; i < n; i++) { sx += i; sy += samples[i].Heap; sxy += i * samples[i].Heap; sx2 += i * i; }
    var denom = n * sx2 - sx * sx;
    if (denom == 0) return 0;
    var slope = (n * sxy - sx * sy) / denom;
    return slope * 60; // samples ~1s apart, so slope * 60 = MB/min
}

static List<SampleData> ParseSamplesForPercentiles(string json)
{
    var results = new List<SampleData>();
    try
    {
        var doc = JsonDocument.Parse(json);
        foreach (var el in doc.RootElement.EnumerateArray())
        {
            results.Add(new SampleData(
                Cpu: el.TryGetProperty("cpu", out var cpu) ? cpu.GetDouble() : 0,
                Heap: el.TryGetProperty("heap", out var heap) ? heap.GetDouble() : 0,
                Ws: el.TryGetProperty("ws", out var ws) ? ws.GetDouble() : 0,
                NetS: el.TryGetProperty("netS", out var netS) ? netS.GetInt64() : 0,
                NetR: el.TryGetProperty("netR", out var netR) ? netR.GetInt64() : 0,
                GcPause: el.TryGetProperty("gcPause", out var gcPause) ? gcPause.GetDouble() : 0,
                GcPauseMs: el.TryGetProperty("gcPauseMs", out var gcPauseMs) ? gcPauseMs.GetDouble() : 0));
        }
    }
    catch { /* ignore parse errors */ }
    return results;
}

internal sealed record SampleData(double Cpu, double Heap, double Ws, long NetS, long NetR, double GcPause, double GcPauseMs);

// --- Data classes ---

internal sealed class BenchmarkResult
{
    public string Name { get; set; } = "";
    public string Status { get; set; } = "PASS";
    public string? MeanTime { get; set; }
    public string? MedianTime { get; set; }
    public string? StdDevFormatted { get; set; }
    public string? Allocated { get; set; }
    public string? MemoryGrowth { get; set; }
    public int MemorySamples { get; set; }
    public long GcGen0 { get; set; }
    public long GcGen1 { get; set; }
    public long GcGen2 { get; set; }
    public double InitialHeapMb { get; set; }
    public double FinalHeapMb { get; set; }
    public double HeapGrowthPercent { get; set; }
    public double AvgCpuPercent { get; set; }
    public double PeakCpuPercent { get; set; }
    public long NetworkBytesSent { get; set; }
    public long NetworkBytesReceived { get; set; }
    public double AvgGcPausePercent { get; set; }
    public double PeakGcPausePercent { get; set; }
    public double TotalGcPauseMs { get; set; }
    public double Median { get; set; }
    public double StdDev { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public string Parameters { get; set; } = "";
    public string SamplesJson { get; set; } = "";
}

internal sealed class BdnReport
{
    public string FullName { get; set; } = "";
    public string Type { get; set; } = "";
    public string Method { get; set; } = "";
    public double Mean { get; set; }
    public double Median { get; set; }
    public double StdDev { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public long AllocatedBytes { get; set; }
    public int Gen0Collections { get; set; }
    public int Gen1Collections { get; set; }
    public int Gen2Collections { get; set; }
    public string Parameters { get; set; } = "";
}
