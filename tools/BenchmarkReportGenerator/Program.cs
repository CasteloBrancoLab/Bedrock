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
var benchmarkResults = new List<BenchmarkResult>();
if (Directory.Exists(pendingDir))
{
    foreach (var file in Directory.GetFiles(pendingDir, "benchmark_*.txt").OrderBy(f => f))
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

                .section-header{font-size:1.25rem;font-weight:600;margin-bottom:1rem;border-bottom:2px solid var(--border);padding-bottom:.5rem}

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
                            <th class="num">CPU Medio</th>
                            <th class="num">Rede I/O</th>
                            <th class="num">GC (0/1/2)</th>
                            <th class="num">GC Pause</th>
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

        sb.Append($"""
                            <td>{memBadge}</td>
                            <td class="num">{(r.AvgCpuPercent > 0 ? $"{r.AvgCpuPercent:F1}%" : "-")}</td>
                            <td class="num" style="font-size:.75rem">{networkIO}</td>
                            <td class="num">{r.GcGen0}/{r.GcGen1}/{r.GcGen2}</td>
                            <td class="num">{(r.AvgGcPausePercent > 0 || r.TotalGcPauseMs > 0 ? $"{r.AvgGcPausePercent:F2}% avg ({FormatPauseMs(r.TotalGcPauseMs)})" : "-")}</td>
                        </tr>
            """);
    }

    sb.Append("""
                    </tbody>
                </table>
            </section>
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
                            <div class="metric"><div class="metric-label">Crescimento Memoria</div><div class="metric-value">{WebUtility.HtmlEncode(r.MemoryGrowth ?? "-")}</div></div>
                            <div class="metric"><div class="metric-label">Heap Inicial</div><div class="metric-value">{r.InitialHeapMb:F2} MB</div></div>
                            <div class="metric"><div class="metric-label">Heap Final</div><div class="metric-value">{r.FinalHeapMb:F2} MB</div></div>
                            <div class="metric"><div class="metric-label">Crescimento Heap</div><div class="metric-value">{r.HeapGrowthPercent:F2}%</div></div>
                            <div class="metric"><div class="metric-label">Amostras</div><div class="metric-value">{r.MemorySamples}</div></div>
                            <div class="metric"><div class="metric-label">CPU Medio</div><div class="metric-value">{r.AvgCpuPercent:F1}%</div></div>
                            <div class="metric"><div class="metric-label">CPU Pico</div><div class="metric-value">{r.PeakCpuPercent:F1}%</div></div>
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

        if (hasSamples)
        {
            sb.Append($"""
                        <div class="chart-label">Linha do Tempo de Metricas</div>
                        <div class="timeline-chart"><canvas id="{canvasId}"></canvas></div>
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

        // Timeline charts for each benchmark with samples
        var timelineCharts=[];
        document.querySelectorAll('.timeline-chart canvas').forEach(function(canvas){
            var dataEl=document.getElementById(canvas.id+'_data');
            if(!dataEl)return;
            var samples=JSON.parse(dataEl.textContent);
            if(!samples.length)return;
            var t0=samples[0].t;
            var labels=samples.map(function(s){var d=s.t-t0;var m=Math.floor(d/60);var sec=d%60;return m+':'+(sec<10?'0':'')+sec;});
            function delta(arr,key){return arr.map(function(s,i){return i===0?0:s[key]-arr[i-1][key];});}
            var chart=new Chart(canvas,{type:'line',data:{labels:labels,datasets:[
                {label:'Heap GC (MB)',data:samples.map(function(s){return s.heap;}),borderColor:'#8b5cf6',backgroundColor:'rgba(139,92,246,0.1)',fill:true,tension:0.3,pointRadius:0,pointHitRadius:6,yAxisID:'y'},
                {label:'Conjunto de Trabalho (MB)',data:samples.map(function(s){return s.ws;}),borderColor:'#06b6d4',backgroundColor:'rgba(6,182,212,0.1)',fill:true,tension:0.3,pointRadius:0,pointHitRadius:6,yAxisID:'y'},
                {label:'CPU (%)',data:samples.map(function(s){return s.cpu;}),borderColor:'#f59e0b',backgroundColor:'rgba(245,158,11,0.1)',fill:false,tension:0.3,pointRadius:0,pointHitRadius:6,yAxisID:'y1'},
                {label:'Rede Enviado (KB)',data:samples.map(function(s){return s.netS/1024;}),borderColor:'#10b981',backgroundColor:'transparent',fill:false,tension:0.3,pointRadius:0,pointHitRadius:6,yAxisID:'y2',hidden:true},
                {label:'Rede Recebido (KB)',data:samples.map(function(s){return s.netR/1024;}),borderColor:'#3b82f6',backgroundColor:'transparent',fill:false,tension:0.3,pointRadius:0,pointHitRadius:6,yAxisID:'y2',hidden:true},
                {label:'\u0394 Rede Enviado (KB)',data:delta(samples,'netS').map(function(v){return v/1024;}),borderColor:'#34d399',backgroundColor:'transparent',fill:false,tension:0.3,pointRadius:0,pointHitRadius:6,yAxisID:'y2',hidden:true},
                {label:'\u0394 Rede Recebido (KB)',data:delta(samples,'netR').map(function(v){return v/1024;}),borderColor:'#60a5fa',backgroundColor:'transparent',fill:false,tension:0.3,pointRadius:0,pointHitRadius:6,yAxisID:'y2',hidden:true},
                {label:'GC Gen0',data:delta(samples,'g0'),borderColor:'#ec4899',backgroundColor:'transparent',fill:false,tension:0.3,pointRadius:0,pointHitRadius:6,yAxisID:'y2',hidden:true},
                {label:'GC Gen1',data:delta(samples,'g1'),borderColor:'#f97316',backgroundColor:'transparent',fill:false,tension:0.3,pointRadius:0,pointHitRadius:6,yAxisID:'y2',hidden:true},
                {label:'GC Gen2',data:delta(samples,'g2'),borderColor:'#ef4444',backgroundColor:'transparent',fill:false,tension:0.3,pointRadius:0,pointHitRadius:6,yAxisID:'y2',hidden:true},
                {label:'GC Pause (%)',data:samples.map(function(s){return s.gcPause||0;}),borderColor:'#e11d48',backgroundColor:'rgba(225,29,72,0.1)',fill:false,tension:0.3,pointRadius:0,pointHitRadius:6,yAxisID:'y1',hidden:false},
                {label:'\u0394 GC Pause (ms)',data:delta(samples,'gcPauseMs'),borderColor:'#be123c',backgroundColor:'transparent',fill:false,tension:0.3,pointRadius:0,pointHitRadius:6,yAxisID:'y2',hidden:true}
            ]},options:{responsive:true,maintainAspectRatio:false,interaction:{mode:'index',intersect:false},plugins:{legend:{position:'top',labels:{usePointStyle:true,boxWidth:8,color:getComputedStyle(document.body).getPropertyValue('--chart-legend').trim()}},tooltip:{mode:'index',intersect:false}},scales:{x:{display:true,title:{display:true,text:'Tempo (mm:ss)',color:getComputedStyle(document.body).getPropertyValue('--muted').trim()},ticks:{color:getComputedStyle(document.body).getPropertyValue('--muted').trim(),maxTicksLimit:20},grid:{color:'rgba(148,163,184,0.1)'}},y:{type:'linear',display:true,position:'left',title:{display:true,text:'Memoria (MB)',color:getComputedStyle(document.body).getPropertyValue('--muted').trim()},ticks:{color:getComputedStyle(document.body).getPropertyValue('--muted').trim()},grid:{color:'rgba(148,163,184,0.1)'}},y1:{type:'linear',display:true,position:'right',title:{display:true,text:'CPU (%)',color:getComputedStyle(document.body).getPropertyValue('--muted').trim()},ticks:{color:getComputedStyle(document.body).getPropertyValue('--muted').trim()},min:0,max:100,grid:{drawOnChartArea:false}},y2:{type:'linear',display:false,ticks:{color:getComputedStyle(document.body).getPropertyValue('--muted').trim()}}}}});
            timelineCharts.push(chart);
        });

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
        if (p == 50 || p50 == 0)
        {
            sb.Append(CultureInfo.InvariantCulture, $"<td class=\"num\">{valStr}</td>");
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
