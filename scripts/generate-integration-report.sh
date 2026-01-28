#!/bin/bash
# Generates HTML report from integration test TRX files
# Uses the IntegrationTestHtmlGenerator from Bedrock.BuildingBlocks.Testing

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

cd "$ROOT_DIR"

REPORT_DIR="artifacts/integration-report"
TRX_DIR="artifacts/test-results"
OUTPUT_FILE="$REPORT_DIR/index.html"

echo ">>> Generating Integration Test Report..."

# Create report directory
mkdir -p "$REPORT_DIR"

# Find TRX files from integration tests
TRX_FILES=$(find "$TRX_DIR" -name "integration-*.trx" 2>/dev/null | tr '\n' ';')

if [ -z "$TRX_FILES" ]; then
    echo "No integration test TRX files found in $TRX_DIR"
    echo "Skipping report generation."
    exit 0
fi

echo "Found TRX files: $TRX_FILES"

# Get git info for environment
GIT_BRANCH=$(git rev-parse --abbrev-ref HEAD 2>/dev/null || echo "unknown")
GIT_COMMIT=$(git rev-parse HEAD 2>/dev/null || echo "unknown")

# Run the report generator
# The generator is implemented as a simple console app embedded in the Testing project
# For now, we use a standalone C# script approach using dotnet-script or inline execution

# Create a temporary C# script to generate the report
TEMP_SCRIPT=$(mktemp --suffix=.csx)

cat > "$TEMP_SCRIPT" << 'CSHARP_SCRIPT'
#r "nuget: Humanizer.Core, 2.14.1"
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

// Simple report generation without needing the full assembly
var args = Environment.GetCommandLineArgs();
var trxFiles = args.Length > 1 ? args[1].Split(';', StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>();
var outputFile = args.Length > 2 ? args[2] : "report.html";
var gitBranch = args.Length > 3 ? args[3] : "unknown";
var gitCommit = args.Length > 4 ? args[4] : "unknown";

if (trxFiles.Length == 0)
{
    Console.WriteLine("No TRX files provided");
    return 1;
}

var ns = XNamespace.Get("http://microsoft.com/schemas/VisualStudio/TeamTest/2010");
var features = new Dictionary<string, FeatureData>();
var totalDuration = TimeSpan.Zero;
var executionTime = DateTime.UtcNow;

foreach (var trxFile in trxFiles)
{
    if (!File.Exists(trxFile)) continue;

    var doc = XDocument.Load(trxFile);
    var root = doc.Root!;

    // Parse test definitions
    var definitions = root.Descendants(ns + "UnitTest")
        .ToDictionary(
            ut => ut.Attribute("id")?.Value ?? "",
            ut => new {
                ClassName = ut.Element(ns + "TestMethod")?.Attribute("className")?.Value ?? "",
                MethodName = ut.Element(ns + "TestMethod")?.Attribute("name")?.Value ?? ""
            });

    // Parse results
    foreach (var result in root.Descendants(ns + "UnitTestResult"))
    {
        var testId = result.Attribute("testId")?.Value ?? "";
        if (!definitions.TryGetValue(testId, out var def)) continue;

        var className = def.ClassName;
        var methodName = def.MethodName;

        if (!features.TryGetValue(className, out var feature))
        {
            feature = new FeatureData { ClassName = className, Name = ExtractSimpleName(className) };
            features[className] = feature;
        }

        var outcome = result.Attribute("outcome")?.Value ?? "Unknown";
        var duration = TimeSpan.TryParse(result.Attribute("duration")?.Value, out var d) ? d : TimeSpan.Zero;
        totalDuration += duration;

        var steps = new List<StepData>();
        var output = result.Descendants(ns + "StdOut").FirstOrDefault()?.Value ?? "";
        foreach (var line in output.Split('\n'))
        {
            if (line.Contains("##STEP##"))
            {
                var json = line.Substring(line.IndexOf("##STEP##") + 8).Trim();
                try
                {
                    var stepObj = JsonSerializer.Deserialize<JsonElement>(json);
                    steps.Add(new StepData
                    {
                        Type = stepObj.GetProperty("type").GetString() ?? "Given",
                        Description = stepObj.GetProperty("description").GetString() ?? ""
                    });
                }
                catch { }
            }
        }

        var scenario = new ScenarioData
        {
            Name = ConvertMethodName(methodName),
            MethodName = methodName,
            Status = outcome.ToLower() switch { "passed" => "passed", "failed" => "failed", _ => "skipped" },
            Duration = duration,
            Steps = steps,
            ErrorMessage = result.Descendants(ns + "ErrorInfo").FirstOrDefault()?.Element(ns + "Message")?.Value
        };

        feature.Scenarios.Add(scenario);
    }

    var times = root.Descendants(ns + "Times").FirstOrDefault();
    if (times != null && DateTime.TryParse(times.Attribute("start")?.Value, out var st))
        executionTime = st.ToUniversalTime();
}

// Generate HTML
var passed = features.Values.Sum(f => f.Scenarios.Count(s => s.Status == "passed"));
var failed = features.Values.Sum(f => f.Scenarios.Count(s => s.Status == "failed"));
var skipped = features.Values.Sum(f => f.Scenarios.Count(s => s.Status == "skipped"));
var total = passed + failed + skipped;
var passRate = total > 0 ? (passed * 100.0 / total).ToString("F1", CultureInfo.InvariantCulture) : "0.0";

var html = GenerateHtml(features.Values.OrderBy(f => f.Name).ToList(), new EnvData
{
    MachineName = Environment.MachineName,
    OsVersion = Environment.OSVersion.ToString(),
    DotNetVersion = Environment.Version.ToString(),
    UserName = Environment.UserName,
    ExecutionTime = executionTime,
    GitBranch = gitBranch,
    GitCommit = gitCommit.Length >= 7 ? gitCommit.Substring(0, 7) : gitCommit
}, totalDuration, passed, failed, skipped, passRate);

File.WriteAllText(outputFile, html);
Console.WriteLine($"Report generated: {outputFile}");
return 0;

static string ExtractSimpleName(string fullName)
{
    var lastDot = fullName.LastIndexOf('.');
    return lastDot >= 0 ? fullName.Substring(lastDot + 1) : fullName;
}

static string ConvertMethodName(string name)
{
    var sb = new StringBuilder();
    foreach (var c in name)
    {
        if (c == '_') sb.Append(' ');
        else if (char.IsUpper(c) && sb.Length > 0 && sb[sb.Length - 1] != ' ')
        {
            sb.Append(' ');
            sb.Append(char.ToLower(c));
        }
        else sb.Append(sb.Length == 0 ? c : char.ToLower(c));
    }
    return sb.ToString();
}

static string GenerateHtml(List<FeatureData> features, EnvData env, TimeSpan duration, int passed, int failed, int skipped, string passRate)
{
    var total = passed + failed + skipped;
    var sb = new StringBuilder();

    sb.Append(@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Bedrock Integration Test Report</title>
    <style>
        :root{--passed:#10b981;--failed:#ef4444;--skipped:#f59e0b;--bg:#f9fafb;--card:#fff;--text:#1f2937;--muted:#6b7280;--border:#e5e7eb}
        *{box-sizing:border-box;margin:0;padding:0}
        body{font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,sans-serif;background:var(--bg);color:var(--text);line-height:1.6}
        .container{max-width:1200px;margin:0 auto;padding:2rem}
        .header{text-align:center;margin-bottom:2rem;border-bottom:2px solid var(--border);padding-bottom:1.5rem}
        .header h1{font-size:2rem;font-weight:700}
        .subtitle{color:var(--muted);font-size:.875rem}
        .cards{display:grid;grid-template-columns:repeat(auto-fit,minmax(140px,1fr));gap:1rem;margin-bottom:2rem}
        .card{background:var(--card);padding:1.25rem;border-radius:.75rem;box-shadow:0 1px 3px rgba(0,0,0,.1);text-align:center}
        .card-value{font-size:2.5rem;font-weight:700}
        .card-label{color:var(--muted);font-size:.75rem;text-transform:uppercase}
        .card-pct{font-size:.875rem;color:var(--muted)}
        .card.passed{border-left:4px solid var(--passed)}.card.passed .card-value{color:var(--passed)}
        .card.failed{border-left:4px solid var(--failed)}.card.failed .card-value{color:var(--failed)}
        .card.skipped{border-left:4px solid var(--skipped)}.card.skipped .card-value{color:var(--skipped)}
        .chart-section{display:grid;grid-template-columns:1fr 1fr;gap:2rem;margin-bottom:2rem;background:var(--card);padding:1.5rem;border-radius:.75rem;box-shadow:0 1px 3px rgba(0,0,0,.1)}
        @media(max-width:768px){.chart-section{grid-template-columns:1fr}}
        .chart-container{max-width:280px;margin:0 auto}
        .env-info{font-size:.875rem}.env-info h3{font-size:1rem;margin-bottom:1rem;border-bottom:1px solid var(--border);padding-bottom:.5rem}
        .env-info dl{display:grid;grid-template-columns:auto 1fr;gap:.5rem 1rem}
        .env-info dt{color:var(--muted);font-size:.75rem;text-transform:uppercase}.env-info dd{font-weight:500}
        .features-header{font-size:1.25rem;font-weight:600;margin-bottom:1rem;border-bottom:2px solid var(--border);padding-bottom:.5rem}
        .feature{background:var(--card);margin-bottom:1rem;border-radius:.75rem;box-shadow:0 1px 3px rgba(0,0,0,.1);overflow:hidden}
        .feature-header{padding:1rem 1.5rem;background:linear-gradient(to right,#f8fafc,#f1f5f9);font-weight:600;display:flex;align-items:center;gap:.75rem;cursor:pointer}
        .feature-header:hover{background:linear-gradient(to right,#f1f5f9,#e2e8f0)}
        .feature-toggle{font-size:.75rem;color:var(--muted);transition:transform .2s}
        .feature.collapsed .feature-toggle{transform:rotate(-90deg)}
        .feature-name{flex:1}
        .feature-stats{display:flex;gap:.5rem;font-size:.75rem}
        .feature-stat{padding:.25rem .5rem;border-radius:9999px;font-weight:500}
        .feature-stat.passed{background:#d1fae5;color:var(--passed)}
        .feature-stat.failed{background:#fee2e2;color:var(--failed)}
        .feature-stat.skipped{background:#fef3c7;color:var(--skipped)}
        .feature-content{max-height:5000px;overflow:hidden;transition:max-height .3s}
        .feature.collapsed .feature-content{max-height:0}
        .scenario{padding:1rem 1.5rem;border-top:1px solid var(--border)}
        .scenario:first-child{border-top:none}
        .scenario-header{display:flex;align-items:center;gap:.75rem}
        .scenario-status{font-size:1.25rem}
        .scenario-name{font-weight:500;flex:1}
        .scenario-duration{color:var(--muted);font-size:.75rem;font-family:monospace}
        .steps{margin-top:.75rem;padding-left:2.25rem}
        .step{display:flex;gap:.5rem;font-size:.875rem;padding:.375rem 0;color:var(--muted)}
        .step-type{font-weight:600;min-width:50px}
        .step-type.given{color:#3b82f6}.step-type.when{color:#8b5cf6}.step-type.then{color:#10b981}
        .error-box{background:#fee2e2;border:1px solid #fecaca;border-radius:.5rem;padding:.75rem 1rem;margin-top:.75rem;margin-left:2.25rem;font-size:.875rem;color:#991b1b}
        .error-box strong{display:block;margin-bottom:.25rem}
        .error-msg{font-family:monospace;white-space:pre-wrap;word-break:break-all}
        .footer{text-align:center;padding:2rem;color:var(--muted);font-size:.75rem;border-top:1px solid var(--border);margin-top:2rem}
        @media print{body{background:#fff;font-size:12px}.container{max-width:none;padding:1rem}.card,.feature,.chart-section{box-shadow:none;border:1px solid var(--border)}.feature{break-inside:avoid}.feature.collapsed .feature-content{max-height:none}.feature-toggle{display:none}}
    </style>
</head>
<body>
<div class=""container"">
    <header class=""header"">
        <h1>Bedrock Integration Test Report</h1>
        <p class=""subtitle"">Generated: ");
    sb.Append(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
    sb.Append(@" UTC</p>
    </header>
    <section class=""cards"">
        <div class=""card""><div class=""card-value"">");
    sb.Append(total);
    sb.Append(@"</div><div class=""card-label"">Total Tests</div></div>
        <div class=""card passed""><div class=""card-value"">");
    sb.Append(passed);
    sb.Append(@"</div><div class=""card-label"">Passed</div><div class=""card-pct"">");
    sb.Append(passRate);
    sb.Append(@"%</div></div>
        <div class=""card failed""><div class=""card-value"">");
    sb.Append(failed);
    sb.Append(@"</div><div class=""card-label"">Failed</div></div>
        <div class=""card skipped""><div class=""card-value"">");
    sb.Append(skipped);
    sb.Append(@"</div><div class=""card-label"">Skipped</div></div>
    </section>
    <section class=""chart-section"">
        <div class=""chart-container""><canvas id=""chart""></canvas></div>
        <div class=""env-info"">
            <h3>Environment</h3>
            <dl>
                <dt>Machine</dt><dd>");
    sb.Append(WebUtility.HtmlEncode(env.MachineName));
    sb.Append(@"</dd>
                <dt>OS</dt><dd>");
    sb.Append(WebUtility.HtmlEncode(env.OsVersion));
    sb.Append(@"</dd>
                <dt>.NET</dt><dd>");
    sb.Append(WebUtility.HtmlEncode(env.DotNetVersion));
    sb.Append(@"</dd>
                <dt>User</dt><dd>");
    sb.Append(WebUtility.HtmlEncode(env.UserName));
    sb.Append(@"</dd>
                <dt>Executed</dt><dd>");
    sb.Append(env.ExecutionTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
    sb.Append(@" UTC</dd>
                <dt>Branch</dt><dd>");
    sb.Append(WebUtility.HtmlEncode(env.GitBranch));
    sb.Append(@"</dd>
                <dt>Commit</dt><dd>");
    sb.Append(WebUtility.HtmlEncode(env.GitCommit));
    sb.Append(@"</dd>
            </dl>
        </div>
    </section>
    <section>
        <h2 class=""features-header"">Test Results</h2>");

    foreach (var feature in features)
    {
        var fp = feature.Scenarios.Count(s => s.Status == "passed");
        var ff = feature.Scenarios.Count(s => s.Status == "failed");
        var fs = feature.Scenarios.Count(s => s.Status == "skipped");

        sb.Append(@"
        <article class=""feature"">
            <div class=""feature-header"">
                <span class=""feature-toggle"">▼</span>
                <span class=""feature-name"">");
        sb.Append(WebUtility.HtmlEncode(feature.Name));
        sb.Append(@"</span>
                <div class=""feature-stats"">");
        if (fp > 0) sb.Append($@"<span class=""feature-stat passed"">{fp} passed</span>");
        if (ff > 0) sb.Append($@"<span class=""feature-stat failed"">{ff} failed</span>");
        if (fs > 0) sb.Append($@"<span class=""feature-stat skipped"">{fs} skipped</span>");
        sb.Append(@"</div>
            </div>
            <div class=""feature-content"">");

        foreach (var scenario in feature.Scenarios)
        {
            var icon = scenario.Status switch { "passed" => "✅", "failed" => "❌", _ => "⏭️" };
            var dur = scenario.Duration.TotalMilliseconds < 1000
                ? $"{scenario.Duration.TotalMilliseconds:F0}ms"
                : $"{scenario.Duration.TotalSeconds:F2}s";

            sb.Append($@"
                <div class=""scenario"">
                    <div class=""scenario-header"">
                        <span class=""scenario-status"">{icon}</span>
                        <span class=""scenario-name"">{WebUtility.HtmlEncode(scenario.Name)}</span>
                        <span class=""scenario-duration"">{dur}</span>
                    </div>");

            if (scenario.Steps.Count > 0)
            {
                sb.Append(@"<div class=""steps"">");
                foreach (var step in scenario.Steps)
                {
                    var cls = step.Type.ToLower();
                    sb.Append($@"<div class=""step""><span class=""step-type {cls}"">{step.Type}:</span><span>{WebUtility.HtmlEncode(step.Description)}</span></div>");
                }
                sb.Append("</div>");
            }

            if (scenario.Status == "failed" && !string.IsNullOrEmpty(scenario.ErrorMessage))
            {
                sb.Append($@"<div class=""error-box""><strong>Error</strong><div class=""error-msg"">{WebUtility.HtmlEncode(scenario.ErrorMessage)}</div></div>");
            }

            sb.Append("</div>");
        }

        sb.Append("</div></article>");
    }

    var durationStr = duration.TotalMinutes >= 1
        ? $"{(int)duration.TotalMinutes}m {duration.Seconds}s"
        : duration.TotalSeconds >= 1
            ? $"{duration.TotalSeconds:F2}s"
            : $"{duration.TotalMilliseconds:F0}ms";

    sb.Append($@"
    </section>
    <footer class=""footer"">
        <p><strong>Bedrock Framework</strong> - Integration Test Report</p>
        <p>Total Duration: {durationStr}</p>
    </footer>
</div>
<script src=""https://cdn.jsdelivr.net/npm/chart.js@4.4.1/dist/chart.umd.min.js""></script>
<script>
new Chart(document.getElementById('chart'),{{type:'doughnut',data:{{labels:['Passed','Failed','Skipped'],datasets:[{{data:[{passed},{failed},{skipped}],backgroundColor:['#10b981','#ef4444','#f59e0b'],borderWidth:0}}]}},options:{{responsive:true,plugins:{{legend:{{position:'bottom'}}}},cutout:'60%'}}}});
document.querySelectorAll('.feature-header').forEach(h=>h.addEventListener('click',()=>h.closest('.feature').classList.toggle('collapsed')));
</script>
</body>
</html>");

    return sb.ToString();
}

class FeatureData
{
    public string ClassName { get; set; } = "";
    public string Name { get; set; } = "";
    public List<ScenarioData> Scenarios { get; set; } = new();
}

class ScenarioData
{
    public string Name { get; set; } = "";
    public string MethodName { get; set; } = "";
    public string Status { get; set; } = "skipped";
    public TimeSpan Duration { get; set; }
    public List<StepData> Steps { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

class StepData
{
    public string Type { get; set; } = "";
    public string Description { get; set; } = "";
}

class EnvData
{
    public string MachineName { get; set; } = "";
    public string OsVersion { get; set; } = "";
    public string DotNetVersion { get; set; } = "";
    public string UserName { get; set; } = "";
    public DateTime ExecutionTime { get; set; }
    public string GitBranch { get; set; } = "";
    public string GitCommit { get; set; } = "";
}
CSHARP_SCRIPT

# Check if dotnet-script is available, otherwise use a simpler approach
if command -v dotnet-script &> /dev/null; then
    dotnet-script "$TEMP_SCRIPT" -- "$TRX_FILES" "$OUTPUT_FILE" "$GIT_BRANCH" "$GIT_COMMIT"
else
    # Fallback: Use dotnet run with inline compilation
    # For simplicity, generate a basic report using bash/grep
    echo "dotnet-script not available, generating basic report..."

    # Create a simple HTML report using bash
    PASSED=$(grep -l 'outcome="Passed"' "$TRX_DIR"/integration-*.trx 2>/dev/null | wc -l || echo 0)
    FAILED=$(grep -l 'outcome="Failed"' "$TRX_DIR"/integration-*.trx 2>/dev/null | wc -l || echo 0)
    TOTAL=$((PASSED + FAILED))

    cat > "$OUTPUT_FILE" << HTML_EOF
<!DOCTYPE html>
<html>
<head>
    <title>Bedrock Integration Test Report</title>
    <style>
        body { font-family: sans-serif; max-width: 800px; margin: 2rem auto; padding: 1rem; }
        .card { display: inline-block; padding: 1rem 2rem; margin: 0.5rem; border-radius: 8px; text-align: center; }
        .passed { background: #d1fae5; color: #10b981; }
        .failed { background: #fee2e2; color: #ef4444; }
        h1 { text-align: center; }
        .info { color: #666; font-size: 0.875rem; text-align: center; }
    </style>
</head>
<body>
    <h1>Bedrock Integration Test Report</h1>
    <p class="info">Generated: $(date -u +"%Y-%m-%d %H:%M:%S UTC")</p>
    <div style="text-align: center; margin: 2rem 0;">
        <div class="card"><strong>$TOTAL</strong><br>Total</div>
        <div class="card passed"><strong>$PASSED</strong><br>Passed</div>
        <div class="card failed"><strong>$FAILED</strong><br>Failed</div>
    </div>
    <p class="info">Branch: $GIT_BRANCH | Commit: ${GIT_COMMIT:0:7}</p>
    <p class="info">For detailed BDD steps, install dotnet-script: dotnet tool install -g dotnet-script</p>
</body>
</html>
HTML_EOF
fi

rm -f "$TEMP_SCRIPT"

echo "Report generated: $OUTPUT_FILE"
