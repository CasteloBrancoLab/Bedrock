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

                .chart-section{display:grid;grid-template-columns:2fr 1fr;gap:2rem;margin-bottom:2rem;background:var(--card);padding:1.5rem;border-radius:.75rem;box-shadow:0 1px 3px rgba(0,0,0,.1)}
                @media(max-width:768px){.chart-section{grid-template-columns:1fr}}
                .chart-container{position:relative;height:300px}
                .env-info{font-size:.875rem}.env-info h3{font-size:1rem;margin-bottom:1rem;border-bottom:1px solid var(--border);padding-bottom:.5rem}
                .env-info dl{display:grid;grid-template-columns:auto 1fr;gap:.5rem 1rem}
                .env-info dt{color:var(--muted);font-size:.75rem;text-transform:uppercase}.env-info dd{font-weight:500}

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

    // --- Chart + Environment ---
    sb.Append("""
            <section class="chart-section">
                <div class="chart-container"><canvas id="meanChart"></canvas></div>
                <div class="env-info">
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
                </div>
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
                            <th class="num">Mean</th>
                            <th class="num">Allocated</th>
                            <th>Memory</th>
                            <th class="num">Avg CPU</th>
                            <th class="num">Network I/O</th>
                            <th class="num">GC (0/1/2)</th>
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
            ? $"S:{FormatBytes(r.NetworkBytesSent)} R:{FormatBytes(r.NetworkBytesReceived)}"
            : "-";

        sb.Append($"""
                        <tr>
                            <td>{statusBadge}</td>
                            <td>{WebUtility.HtmlEncode(r.Name)}{(string.IsNullOrEmpty(r.Parameters) ? "" : $" <small>({WebUtility.HtmlEncode(r.Parameters)})</small>")}</td>
                            <td class="num">{WebUtility.HtmlEncode(r.MeanTime ?? "-")}</td>
                            <td class="num">{WebUtility.HtmlEncode(r.Allocated ?? "-")}</td>
                            <td>{memBadge}</td>
                            <td class="num">{(r.AvgCpuPercent > 0 ? $"{r.AvgCpuPercent:F1}%" : "-")}</td>
                            <td class="num" style="font-size:.75rem">{networkIO}</td>
                            <td class="num">{r.GcGen0}/{r.GcGen1}/{r.GcGen2}</td>
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
                            <div class="metric"><div class="metric-label">Mean Time</div><div class="metric-value">{WebUtility.HtmlEncode(r.MeanTime ?? "-")}</div></div>
                            <div class="metric"><div class="metric-label">Median Time</div><div class="metric-value">{WebUtility.HtmlEncode(r.MedianTime ?? r.StdDevFormatted ?? "-")}</div></div>
                            <div class="metric"><div class="metric-label">Allocated</div><div class="metric-value">{WebUtility.HtmlEncode(r.Allocated ?? "-")}</div></div>
                            <div class="metric"><div class="metric-label">Memory Growth</div><div class="metric-value">{WebUtility.HtmlEncode(r.MemoryGrowth ?? "-")}</div></div>
                            <div class="metric"><div class="metric-label">Initial Heap</div><div class="metric-value">{r.InitialHeapMb:F2} MB</div></div>
                            <div class="metric"><div class="metric-label">Final Heap</div><div class="metric-value">{r.FinalHeapMb:F2} MB</div></div>
                            <div class="metric"><div class="metric-label">Heap Growth</div><div class="metric-value">{r.HeapGrowthPercent:F2}%</div></div>
                            <div class="metric"><div class="metric-label">Samples</div><div class="metric-value">{r.MemorySamples}</div></div>
                            <div class="metric"><div class="metric-label">Avg CPU</div><div class="metric-value">{r.AvgCpuPercent:F1}%</div></div>
                            <div class="metric"><div class="metric-label">Peak CPU</div><div class="metric-value">{r.PeakCpuPercent:F1}%</div></div>
                            <div class="metric"><div class="metric-label">GC Gen0</div><div class="metric-value">{r.GcGen0}</div></div>
                            <div class="metric"><div class="metric-label">GC Gen1</div><div class="metric-value">{r.GcGen1}</div></div>
                            <div class="metric"><div class="metric-label">GC Gen2</div><div class="metric-value">{r.GcGen2}</div></div>
                            <div class="metric"><div class="metric-label">Network Sent</div><div class="metric-value">{FormatBytes(r.NetworkBytesSent)}</div></div>
                            <div class="metric"><div class="metric-label">Network Received</div><div class="metric-value">{FormatBytes(r.NetworkBytesReceived)}</div></div>
                        </div>
            """);

        if (hasSamples)
        {
            sb.Append($"""
                        <div class="chart-label">Timeline de Metricas</div>
                        <div class="timeline-chart"><canvas id="{canvasId}"></canvas></div>
                        <script type="application/json" id="{canvasId}_data">{r.SamplesJson}</script>
                """);
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

    // Build chart data - bar chart of mean times
    var chartLabels = new StringBuilder();
    var chartData = new StringBuilder();
    var chartColors = new StringBuilder();
    var first = true;
    foreach (var r in results)
    {
        if (!first) { chartLabels.Append(','); chartData.Append(','); chartColors.Append(','); }
        first = false;

        var shortName = r.Name.Contains('.') ? r.Name[(r.Name.LastIndexOf('.') + 1)..] : r.Name;
        chartLabels.Append(CultureInfo.InvariantCulture, $"'{EscapeJs(shortName)}'");

        // Parse mean time back to number for chart (use raw nanoseconds if available)
        var meanVal = r.Median > 0 ? r.Median / 1_000_000.0 : ParseTimeToMs(r.MeanTime ?? "0");
        chartData.Append(CultureInfo.InvariantCulture, $"{meanVal:F3}");
        chartColors.Append(r.Status == "WARN" ? "'#f59e0b'" : "'#10b981'");
    }

    sb.Append(CultureInfo.InvariantCulture, $@"
        var meanChart=new Chart(document.getElementById('meanChart'),{{type:'bar',data:{{labels:[{chartLabels}],datasets:[{{label:'Mean Time (ms)',data:[{chartData}],backgroundColor:[{chartColors}],borderWidth:0,borderRadius:4}}]}},options:{{responsive:true,maintainAspectRatio:false,plugins:{{legend:{{display:false}},title:{{display:true,text:'Tempo Medio por Benchmark (ms)',color:getComputedStyle(document.body).getPropertyValue('--text').trim()}}}},scales:{{y:{{beginAtZero:true,grid:{{color:'rgba(148,163,184,0.1)'}},ticks:{{color:getComputedStyle(document.body).getPropertyValue('--muted').trim()}}}},x:{{grid:{{display:false}},ticks:{{color:getComputedStyle(document.body).getPropertyValue('--muted').trim(),maxRotation:45}}}}}}}}}});");

    sb.Append("""

        document.querySelectorAll('.detail-header').forEach(h=>h.addEventListener('click',()=>h.closest('.detail').classList.toggle('collapsed')));
        function toggleTheme(){document.body.classList.toggle('light-theme');localStorage.setItem('bench-theme',document.body.classList.contains('light-theme')?'light':'dark');updateChart();timelineCharts.forEach(c=>c.update());}
        function updateChart(){var c=getComputedStyle(document.body);meanChart.options.plugins.title.color=c.getPropertyValue('--text').trim();meanChart.options.scales.y.ticks.color=c.getPropertyValue('--muted').trim();meanChart.options.scales.x.ticks.color=c.getPropertyValue('--muted').trim();meanChart.update();}

        // Timeline charts for each benchmark with samples
        var timelineCharts=[];
        document.querySelectorAll('.timeline-chart canvas').forEach(function(canvas){
            var dataEl=document.getElementById(canvas.id+'_data');
            if(!dataEl)return;
            var samples=JSON.parse(dataEl.textContent);
            if(!samples.length)return;
            var t0=samples[0].t;
            var labels=samples.map(function(s){var d=s.t-t0;var m=Math.floor(d/60);var sec=d%60;return m+':'+(sec<10?'0':'')+sec;});
            var chart=new Chart(canvas,{type:'line',data:{labels:labels,datasets:[
                {label:'GC Heap (MB)',data:samples.map(function(s){return s.heap;}),borderColor:'#8b5cf6',backgroundColor:'rgba(139,92,246,0.1)',fill:true,tension:0.3,pointRadius:0,pointHitRadius:6,yAxisID:'y'},
                {label:'Working Set (MB)',data:samples.map(function(s){return s.ws;}),borderColor:'#06b6d4',backgroundColor:'rgba(6,182,212,0.1)',fill:true,tension:0.3,pointRadius:0,pointHitRadius:6,yAxisID:'y'},
                {label:'CPU (%)',data:samples.map(function(s){return s.cpu;}),borderColor:'#f59e0b',backgroundColor:'rgba(245,158,11,0.1)',fill:false,tension:0.3,pointRadius:0,pointHitRadius:6,yAxisID:'y1'},
                {label:'Net Sent (KB)',data:samples.map(function(s){return s.netS/1024;}),borderColor:'#10b981',backgroundColor:'transparent',fill:false,tension:0.3,pointRadius:0,pointHitRadius:6,yAxisID:'y2',hidden:true},
                {label:'Net Recv (KB)',data:samples.map(function(s){return s.netR/1024;}),borderColor:'#3b82f6',backgroundColor:'transparent',fill:false,tension:0.3,pointRadius:0,pointHitRadius:6,yAxisID:'y2',hidden:true},
                {label:'GC Gen0',data:samples.map(function(s){return s.g0;}),borderColor:'#ec4899',backgroundColor:'transparent',fill:false,tension:0.3,pointRadius:0,pointHitRadius:6,yAxisID:'y2',hidden:true},
                {label:'GC Gen1',data:samples.map(function(s){return s.g1;}),borderColor:'#f97316',backgroundColor:'transparent',fill:false,tension:0.3,pointRadius:0,pointHitRadius:6,yAxisID:'y2',hidden:true},
                {label:'GC Gen2',data:samples.map(function(s){return s.g2;}),borderColor:'#ef4444',backgroundColor:'transparent',fill:false,tension:0.3,pointRadius:0,pointHitRadius:6,yAxisID:'y2',hidden:true}
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
