using System.Net;
using System.Text;
using System.Text.Json;

// Gerador de relatório HTML para testes de arquitetura
// Uso: ArchitectureReportGenerator <json-dir-or-file> <output-file> <git-branch> <git-commit>
// Aceita um diretório com subpastas contendo architecture-report.json ou um arquivo JSON único.

var jsonInput = args.Length > 0 ? args[0] : "artifacts/architecture";
var outputFile = args.Length > 1 ? args[1] : "artifacts/architecture-report/index.html";
var gitBranch = args.Length > 2 ? args[2] : "unknown";
var gitCommit = args.Length > 3 ? args[3] : "unknown";

// Descobrir JSONs: diretório com subpastas ou arquivo único
var jsonFiles = new List<string>();
if (Directory.Exists(jsonInput))
{
    jsonFiles.AddRange(Directory.GetFiles(jsonInput, "architecture-report.json", SearchOption.AllDirectories)
        .Where(f => !f.Replace('\\', '/').EndsWith("architecture/architecture-report.json"))
        .Order(StringComparer.OrdinalIgnoreCase));
}
else if (File.Exists(jsonInput))
{
    jsonFiles.Add(jsonInput);
}

if (jsonFiles.Count == 0)
{
    Console.WriteLine($"Nenhum relatório JSON encontrado em: {jsonInput}");
    Console.WriteLine("Nenhuma violação de arquitetura para reportar.");
    return 0;
}

Console.WriteLine($"Consolidando {jsonFiles.Count} relatórios JSON...");

// Consolidar todos os JSONs
var totalTypesAnalyzed = 0;
var totalPassed = 0;
var totalViolations = 0;
var errors = 0;
var warnings = 0;
var infos = 0;
var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
var ruleResults = new List<JsonElement>();

foreach (var jsonFile in jsonFiles)
{
    var jsonContent = File.ReadAllText(jsonFile);
    var report = JsonSerializer.Deserialize<JsonElement>(jsonContent);

    totalTypesAnalyzed += report.GetProperty("totalTypesAnalyzed").GetInt32();
    totalPassed += report.GetProperty("totalPassed").GetInt32();
    totalViolations += report.GetProperty("totalViolations").GetInt32();
    errors += report.GetProperty("errors").GetInt32();
    warnings += report.GetProperty("warnings").GetInt32();
    infos += report.GetProperty("infos").GetInt32();

    var ts = report.GetProperty("timestamp").GetString();
    if (!string.IsNullOrEmpty(ts))
        timestamp = ts;

    ruleResults.AddRange(report.GetProperty("ruleResults").EnumerateArray());
    Console.WriteLine($"  + {Path.GetDirectoryName(jsonFile)?.Split(Path.DirectorySeparatorChar).LastOrDefault() ?? jsonFile}");
}

// Agrupar por projeto (ordenado por nome)
var byProject = ruleResults
    .GroupBy(r => r.GetProperty("projectName").GetString() ?? "Unknown")
    .OrderBy(g => g.Key, StringComparer.Ordinal)
    .ToList();

var passRate = totalTypesAnalyzed > 0
    ? (totalPassed * 100.0 / totalTypesAnalyzed).ToString("F1")
    : "100.0";

var html = GenerateHtml(
    totalTypesAnalyzed, totalPassed, totalViolations,
    errors, warnings, infos,
    ruleResults, byProject,
    gitBranch, gitCommit.Length >= 7 ? gitCommit[..7] : gitCommit,
    timestamp, passRate);

var dir = Path.GetDirectoryName(outputFile);
if (!string.IsNullOrEmpty(dir))
    Directory.CreateDirectory(dir);

File.WriteAllText(outputFile, html);
Console.WriteLine($"Relatório gerado: {outputFile}");
Console.WriteLine($"  Tipos: {totalTypesAnalyzed} | Passou: {totalPassed} | Falhou: {totalViolations}");
return 0;

static string GenerateHtml(
    int totalTypes, int totalPassed, int totalViolations,
    int errors, int warnings, int infos,
    List<JsonElement> ruleResults,
    List<IGrouping<string, JsonElement>> byProject,
    string branch, string commit, string timestamp, string passRate)
{
    var sb = new StringBuilder();

    sb.Append($$"""
        <!DOCTYPE html>
        <html lang="pt-BR">
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <title>Bedrock - Relatório de Testes de Arquitetura</title>
            <style>
                :root{--error:#ef4444;--warning:#f59e0b;--info:#3b82f6;--passed:#10b981;--bg:#0f172a;--card:#1e293b;--text:#f1f5f9;--muted:#94a3b8;--border:#334155;--header-bg:linear-gradient(to right,#1e1b4b,#312e81);--feature-bg:linear-gradient(to right,#1e293b,#334155);--table-header-bg:#1e293b;--badge-error-bg:#7f1d1d;--badge-error-text:#fca5a5;--badge-warning-bg:#78350f;--badge-warning-text:#fcd34d;--badge-info-bg:#1e3a5f;--badge-info-text:#93c5fd;--badge-passed-bg:#064e3b;--badge-passed-text:#6ee7b7;--chart-legend:#e5e7eb;--progress-bg:#374151}
                .light-theme{--bg:#f9fafb;--card:#fff;--text:#1f2937;--muted:#6b7280;--border:#e5e7eb;--header-bg:linear-gradient(to right,#eef2ff,#e0e7ff);--feature-bg:linear-gradient(to right,#f8fafc,#f1f5f9);--table-header-bg:#f8fafc;--badge-error-bg:#fee2e2;--badge-error-text:#991b1b;--badge-warning-bg:#fef3c7;--badge-warning-text:#92400e;--badge-info-bg:#dbeafe;--badge-info-text:#1e3a8a;--badge-passed-bg:#d1fae5;--badge-passed-text:#065f46;--chart-legend:#374151;--progress-bg:#e5e7eb}
                *{box-sizing:border-box;margin:0;padding:0}
                body{font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,sans-serif;background:var(--bg);color:var(--text);line-height:1.6;transition:background .3s,color .3s}
                .container{max-width:1200px;margin:0 auto;padding:2rem}
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
                .cards{display:grid;grid-template-columns:repeat(auto-fit,minmax(140px,1fr));gap:1rem;margin-bottom:2rem}
                .card{background:var(--card);padding:1.25rem;border-radius:.75rem;box-shadow:0 1px 3px rgba(0,0,0,.1);text-align:center}
                .card-value{font-size:2.5rem;font-weight:700}
                .card-label{color:var(--muted);font-size:.75rem;text-transform:uppercase}
                .card.error{border-left:4px solid var(--error)}.card.error .card-value{color:var(--error)}
                .card.warning{border-left:4px solid var(--warning)}.card.warning .card-value{color:var(--warning)}
                .card.passed{border-left:4px solid var(--passed)}.card.passed .card-value{color:var(--passed)}
                .card.total{border-left:4px solid var(--info)}.card.total .card-value{color:var(--info)}
                .chart-section{display:grid;grid-template-columns:1fr 1fr;gap:2rem;margin-bottom:2rem;background:var(--card);padding:1.5rem;border-radius:.75rem;box-shadow:0 1px 3px rgba(0,0,0,.1)}
                @media(max-width:768px){.chart-section{grid-template-columns:1fr} }
                .chart-container{max-width:280px;margin:0 auto}
                .env-info{font-size:.875rem}.env-info h3{font-size:1rem;margin-bottom:1rem;border-bottom:1px solid var(--border);padding-bottom:.5rem}
                .env-info dl{display:grid;grid-template-columns:auto 1fr;gap:.5rem 1rem}
                .env-info dt{color:var(--muted);font-size:.75rem;text-transform:uppercase}.env-info dd{font-weight:500}
                .section-header{font-size:1.25rem;font-weight:600;margin-bottom:1rem;border-bottom:2px solid var(--border);padding-bottom:.5rem}
                .project-group{margin-bottom:1.5rem}
                .project-header{padding:1rem 1.5rem;background:var(--header-bg);border-radius:.75rem .75rem 0 0;font-weight:700;display:flex;align-items:center;gap:.75rem;cursor:pointer;transition:background .2s}
                .project-header:hover{filter:brightness(1.1)}
                .project-toggle{font-size:.75rem;color:var(--muted);transition:transform .2s}
                .project-group.collapsed .project-toggle{transform:rotate(-90deg)}
                .project-name{flex:1}
                .project-stats{display:flex;gap:.5rem;align-items:center;font-size:.75rem}
                .project-content{max-height:50000px;overflow:hidden;transition:max-height .3s}
                .project-group.collapsed .project-content{max-height:0}
                .progress-bar{width:120px;height:8px;background:var(--progress-bg);border-radius:4px;overflow:hidden}
                .progress-fill{height:100%;border-radius:4px;transition:width .3s}
                .progress-fill.all-passed{background:var(--passed)}
                .progress-fill.has-failures{background:var(--error)}
                .rule-section{padding:1rem 1.5rem;border-top:1px solid var(--border);background:var(--card)}
                .rule-section:last-child{border-radius:0 0 .75rem .75rem}
                .rule-header-inner{display:flex;align-items:center;gap:.75rem;cursor:pointer;padding:.5rem 0}
                .rule-header-inner:hover{opacity:.8}
                .rule-toggle{font-size:.65rem;color:var(--muted);transition:transform .2s}
                .rule-section.collapsed .rule-toggle{transform:rotate(-90deg)}
                .rule-meta{flex:1}
                .rule-name{font-weight:600;font-size:.95rem}
                .rule-desc{font-size:.8rem;color:var(--muted);margin-top:.125rem}
                .rule-adr{font-size:.75rem;margin-top:.25rem}
                .rule-adr a{color:var(--info);text-decoration:none}
                .rule-adr a:hover{text-decoration:underline}
                .rule-stats{display:flex;gap:.5rem;align-items:center;font-size:.75rem}
                .badge{padding:.25rem .75rem;border-radius:9999px;font-weight:600;font-size:.7rem;text-transform:uppercase}
                .badge-error{background:var(--badge-error-bg);color:var(--badge-error-text)}
                .badge-warning{background:var(--badge-warning-bg);color:var(--badge-warning-text)}
                .badge-info{background:var(--badge-info-bg);color:var(--badge-info-text)}
                .badge-passed{background:var(--badge-passed-bg);color:var(--badge-passed-text)}
                .types-table{width:100%;border-collapse:collapse;margin-top:.5rem}
                .types-table-content{max-height:50000px;overflow:hidden;transition:max-height .3s}
                .rule-section.collapsed .types-table-content{max-height:0}
                .types-table th{padding:.5rem .75rem;text-align:left;font-size:.7rem;text-transform:uppercase;color:var(--muted);border-bottom:1px solid var(--border);background:var(--table-header-bg)}
                .types-table td{padding:.5rem .75rem;border-bottom:1px solid var(--border);font-size:.85rem}
                .types-table tr:last-child td{border-bottom:none}
                .type-file{font-family:monospace;font-size:.8rem;color:var(--muted)}
                .status-icon{font-size:1rem}
                .violation-detail{padding:.5rem .75rem .5rem 2.5rem;font-size:.8rem;color:var(--muted);font-style:italic}
                .violation-hint{padding:.25rem .75rem .5rem 2.5rem;font-size:.75rem;color:var(--info)}
                .success-banner{background:#065f46;border:2px solid #10b981;border-radius:.75rem;padding:2rem;text-align:center;margin-bottom:2rem}
                .success-banner h2{color:#6ee7b7;font-size:1.5rem;margin-bottom:.5rem}
                .success-banner p{color:#a7f3d0}
                .footer{text-align:center;padding:2rem;color:var(--muted);font-size:.75rem;border-top:1px solid var(--border);margin-top:2rem}
                @media print{body{background:#fff;font-size:12px}.container{max-width:none;padding:1rem}.card,.project-group,.chart-section{box-shadow:none;border:1px solid var(--border)}.project-group{break-inside:avoid}.project-group.collapsed .project-content{max-height:none}.rule-section.collapsed .types-table-content{max-height:none}.project-toggle,.rule-toggle{display:none} }
            </style>
        </head>
        <body>
        <div class="container">
            <header class="header">
                <button class="theme-toggle" onclick="toggleTheme()" title="Alternar tema claro/escuro">
                    <svg class="icon-sun" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor"><circle cx="12" cy="12" r="4"/><path stroke-linecap="round" d="M12 2v2m0 16v2M4 12H2m20 0h-2m-2.05-6.95 1.41-1.41M4.64 19.36l1.41-1.41m0-11.9L4.64 4.64m14.72 14.72-1.41-1.41"/></svg>
                    <svg class="icon-moon" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z"/></svg>
                </button>
                <h1>Bedrock - Relatório de Testes de Arquitetura</h1>
                <p class="subtitle">Gerado em: {{timestamp}} UTC</p>
            </header>
            <section class="cards">
                <div class="card total"><div class="card-value">{{totalTypes}}</div><div class="card-label">Tipos Analisados</div></div>
                <div class="card passed"><div class="card-value">{{totalPassed}}</div><div class="card-label">Passaram</div></div>
                <div class="card error"><div class="card-value">{{totalViolations}}</div><div class="card-label">Falharam</div></div>
                <div class="card passed"><div class="card-value">{{passRate}}%</div><div class="card-label">Taxa de Aprovação</div></div>
            </section>
            <section class="chart-section">
                <div class="chart-container"><canvas id="chart"></canvas></div>
                <div class="env-info">
                    <h3>Ambiente</h3>
                    <dl>
                        <dt>Máquina</dt><dd>{{WebUtility.HtmlEncode(Environment.MachineName)}}</dd>
                        <dt>SO</dt><dd>{{WebUtility.HtmlEncode(Environment.OSVersion.ToString())}}</dd>
                        <dt>.NET</dt><dd>{{WebUtility.HtmlEncode(Environment.Version.ToString())}}</dd>
                        <dt>Branch</dt><dd>{{WebUtility.HtmlEncode(branch)}}</dd>
                        <dt>Commit</dt><dd>{{WebUtility.HtmlEncode(commit)}}</dd>
                        <dt>Projetos</dt><dd>{{byProject.Count}}</dd>
                        <dt>Regras</dt><dd>{{ruleResults.Select(r => r.GetProperty("ruleName").GetString()).Distinct().Count()}}</dd>
                    </dl>
                </div>
            </section>
        """);

    if (totalViolations == 0)
    {
        sb.Append($"""
            <div class="success-banner">
                <h2>Todas as regras de arquitetura estão sendo seguidas</h2>
                <p>{totalTypes} tipos analisados — nenhuma violação encontrada.</p>
            </div>
        """);
    }

    // Resultados detalhados por projeto
    sb.Append("""
        <section>
            <h2 class="section-header">Resultados por Projeto</h2>
    """);

    foreach (var projectGroup in byProject)
    {
        var projectName = projectGroup.Key;
        var projectRules = projectGroup
            .OrderBy(r => r.GetProperty("ruleName").GetString() ?? "", StringComparer.Ordinal)
            .ToList();
        var projectTotal = projectRules.Sum(r => r.GetProperty("types").GetArrayLength());
        var projectPassed = projectRules.Sum(r =>
            r.GetProperty("types").EnumerateArray().Count(t => t.GetProperty("status").GetString() == "Passed"));
        var projectFailed = projectTotal - projectPassed;
        var projectPassRate = projectTotal > 0 ? projectPassed * 100.0 / projectTotal : 100.0;
        var progressClass = projectFailed > 0 ? "has-failures" : "all-passed";

        sb.Append($"""
            <article class="project-group collapsed">
                <div class="project-header" onclick="this.closest('.project-group').classList.toggle('collapsed')">
                    <span class="project-toggle">▼</span>
                    <span class="project-name">{WebUtility.HtmlEncode(projectName)}</span>
                    <div class="project-stats">
                        <span class="badge badge-passed">{projectPassed} passed</span>
        """);

        if (projectFailed > 0)
            sb.Append($"""<span class="badge badge-error">{projectFailed} failed</span>""");

        sb.Append($"""
                        <div class="progress-bar"><div class="progress-fill {progressClass}" style="width:{projectPassRate:F0}%"></div></div>
                    </div>
                </div>
                <div class="project-content">
        """);

        foreach (var ruleResult in projectRules)
        {
            var ruleName = ruleResult.GetProperty("ruleName").GetString() ?? "";
            var ruleDesc = ruleResult.GetProperty("ruleDescription").GetString() ?? "";
            var defaultSev = ruleResult.GetProperty("defaultSeverity").GetString() ?? "Error";
            var adrPath = ruleResult.GetProperty("adrPath").GetString() ?? "";
            var types = ruleResult.GetProperty("types").EnumerateArray().ToList();
            var ruleTotal = types.Count;
            var rulePassed = types.Count(t => t.GetProperty("status").GetString() == "Passed");
            var ruleFailed = ruleTotal - rulePassed;
            sb.Append($"""
                    <div class="rule-section collapsed">
                        <div class="rule-header-inner" onclick="this.closest('.rule-section').classList.toggle('collapsed')">
                            <span class="rule-toggle">▼</span>
                            <div class="rule-meta">
                                <div class="rule-name">{WebUtility.HtmlEncode(ruleName)}</div>
                                <div class="rule-desc">{WebUtility.HtmlEncode(ruleDesc)}</div>
                                <div class="rule-adr">ADR: <a href="{WebUtility.HtmlEncode(adrPath)}">{WebUtility.HtmlEncode(adrPath)}</a></div>
                            </div>
                            <div class="rule-stats">
                                <span class="badge badge-passed">{rulePassed}/{ruleTotal}</span>
            """);

            if (ruleFailed > 0)
                sb.Append($"""<span class="badge badge-error">{ruleFailed} failed</span>""");

            sb.Append("""
                            </div>
                        </div>
                        <div class="types-table-content">
                            <table class="types-table">
                                <thead><tr>
                                    <th style="width:2rem">Status</th>
                                    <th>Tipo</th>
                                    <th>Arquivo</th>
                                    <th style="width:4rem">Linha</th>
                                </tr></thead>
                                <tbody>
            """);

            foreach (var typeResult in types.OrderBy(t => t.GetProperty("status").GetString() == "Passed" ? 1 : 0)
                         .ThenBy(t => t.GetProperty("typeName").GetString()))
            {
                var typeName = typeResult.GetProperty("typeName").GetString() ?? "";
                var file = typeResult.GetProperty("file").GetString() ?? "";
                var line = typeResult.GetProperty("line").GetInt32();
                var status = typeResult.GetProperty("status").GetString() ?? "Passed";
                var isPassed = status == "Passed";
                var icon = isPassed ? "\u2705" : "\u274C";

                sb.Append($"""
                                    <tr>
                                        <td class="status-icon">{icon}</td>
                                        <td><strong>{WebUtility.HtmlEncode(typeName)}</strong></td>
                                        <td class="type-file">{WebUtility.HtmlEncode(file)}</td>
                                        <td>{line}</td>
                                    </tr>
                """);

                if (!isPassed && typeResult.TryGetProperty("violation", out var violation) &&
                    violation.ValueKind != JsonValueKind.Null)
                {
                    var message = violation.GetProperty("message").GetString() ?? "";
                    var hint = violation.GetProperty("llmHint").GetString() ?? "";

                    sb.Append($"""
                                    <tr>
                                        <td></td>
                                        <td colspan="3">
                                            <div class="violation-detail">{WebUtility.HtmlEncode(message)}</div>
                                            <div class="violation-hint">{WebUtility.HtmlEncode(hint)}</div>
                                        </td>
                                    </tr>
                    """);
                }
            }

            sb.Append("</tbody></table></div></div>");
        }

        sb.Append("</div></article>");
    }

    sb.Append("</section>");

    // Sumário por Regra agrupado por Categoria
    var byCategory = ruleResults
        .GroupBy(r => r.TryGetProperty("ruleCategory", out var cat) ? cat.GetString() ?? "Other" : "Other")
        .OrderBy(g => g.Key, StringComparer.Ordinal)
        .ToList();

    sb.Append("""
        <section>
            <h2 class="section-header">Sumário por Regra</h2>
    """);

    foreach (var categoryGroup in byCategory)
    {
        var categoryName = categoryGroup.Key;
        var categoryRules = categoryGroup
            .GroupBy(r => r.GetProperty("ruleName").GetString() ?? "")
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .ToList();

        var catTotal = categoryRules.Sum(g => g.SelectMany(r => r.GetProperty("types").EnumerateArray()).Count());
        var catPassed = categoryRules.Sum(g => g.SelectMany(r => r.GetProperty("types").EnumerateArray()).Count(t => t.GetProperty("status").GetString() == "Passed"));
        var catFailed = catTotal - catPassed;

        sb.Append($"""
            <article class="project-group collapsed">
                <div class="project-header" onclick="this.closest('.project-group').classList.toggle('collapsed')">
                    <span class="project-toggle">▼</span>
                    <span class="project-name">{WebUtility.HtmlEncode(categoryName)}</span>
                    <div class="project-stats">
                        <span class="badge badge-passed">{catPassed} passed</span>
        """);

        if (catFailed > 0)
            sb.Append($"""<span class="badge badge-error">{catFailed} failed</span>""");

        sb.Append($"""
                        <span style="font-size:.75rem;color:var(--muted)">{categoryRules.Count} regras</span>
                    </div>
                </div>
                <div class="project-content">
                    <table style="width:100%;border-collapse:collapse;background:var(--card)">
                        <thead><tr>
                            <th style="padding:.75rem 1rem;text-align:left;background:var(--table-header-bg);font-size:.75rem;text-transform:uppercase;color:var(--muted);border-bottom:1px solid var(--border)">Regra</th>
                            <th style="padding:.75rem 1rem;text-align:center;background:var(--table-header-bg);font-size:.75rem;text-transform:uppercase;color:var(--muted);border-bottom:1px solid var(--border)">Severidade</th>
                            <th style="padding:.75rem 1rem;text-align:center;background:var(--table-header-bg);font-size:.75rem;text-transform:uppercase;color:var(--muted);border-bottom:1px solid var(--border)">Tipos</th>
                            <th style="padding:.75rem 1rem;text-align:center;background:var(--table-header-bg);font-size:.75rem;text-transform:uppercase;color:var(--muted);border-bottom:1px solid var(--border)">Passou</th>
                            <th style="padding:.75rem 1rem;text-align:center;background:var(--table-header-bg);font-size:.75rem;text-transform:uppercase;color:var(--muted);border-bottom:1px solid var(--border)">Falhou</th>
                            <th style="padding:.75rem 1rem;text-align:center;background:var(--table-header-bg);font-size:.75rem;text-transform:uppercase;color:var(--muted);border-bottom:1px solid var(--border)">Taxa</th>
                        </tr></thead>
                        <tbody>
        """);

        foreach (var ruleGroup in categoryRules)
        {
            var name = ruleGroup.Key;
            var allTypes = ruleGroup.SelectMany(r => r.GetProperty("types").EnumerateArray()).ToList();
            var rTotal = allTypes.Count;
            var rPassed = allTypes.Count(t => t.GetProperty("status").GetString() == "Passed");
            var rFailed = rTotal - rPassed;
            var rRate = rTotal > 0 ? (rPassed * 100.0 / rTotal).ToString("F1") : "100.0";
            var sev = ruleGroup.First().GetProperty("defaultSeverity").GetString() ?? "Error";
            var sevColor = sev switch { "Error" => "var(--muted)", "Warning" => "var(--warning)", _ => "var(--info)" };

            sb.Append($"""
                            <tr>
                                <td style="padding:.75rem 1rem;border-bottom:1px solid var(--border);font-weight:500">{WebUtility.HtmlEncode(name)}</td>
                                <td style="padding:.75rem 1rem;text-align:center;border-bottom:1px solid var(--border);color:{sevColor}">{WebUtility.HtmlEncode(sev)}</td>
                                <td style="padding:.75rem 1rem;text-align:center;border-bottom:1px solid var(--border)">{rTotal}</td>
                                <td style="padding:.75rem 1rem;text-align:center;border-bottom:1px solid var(--border);color:var(--passed);font-weight:600">{rPassed}</td>
                                <td style="padding:.75rem 1rem;text-align:center;border-bottom:1px solid var(--border);color:{(rFailed > 0 ? "var(--error)" : "var(--muted)")};font-weight:600">{rFailed}</td>
                                <td style="padding:.75rem 1rem;text-align:center;border-bottom:1px solid var(--border);font-weight:600">{rRate}%</td>
                            </tr>
            """);
        }

        sb.Append("</tbody></table></div></article>");
    }

    sb.Append("</section>");

    sb.Append($"""
            <footer class="footer">
                <p><strong>Bedrock Framework</strong> - Relatório de Testes de Arquitetura</p>
                <p>Projetos: {byProject.Count} | Regras: {ruleResults.Select(r => r.GetProperty("ruleName").GetString()).Distinct().Count()} | Tipos: {totalTypes} | Violações: {totalViolations}</p>
            </footer>
        </div>
        <script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.1/dist/chart.umd.min.js"></script>
        <script>
    """);

    sb.Append($"var chartInstance=new Chart(document.getElementById('chart'),{{type:'doughnut',data:{{labels:['Passou','Falhou'],datasets:[{{data:[{totalPassed},{totalViolations}],backgroundColor:['#10b981','#ef4444'],borderWidth:0}}]}},options:{{responsive:true,plugins:{{legend:{{position:'bottom',labels:{{color:getComputedStyle(document.body).getPropertyValue('--chart-legend').trim()}}}}}},cutout:'60%'}}}});");

    sb.Append("""

        document.querySelectorAll('.project-header').forEach(h=>h.addEventListener('click',()=>{}));
        function toggleTheme(){document.body.classList.toggle('light-theme');localStorage.setItem('theme',document.body.classList.contains('light-theme')?'light':'dark');updateChartLegend();}
        function updateChartLegend(){var c=getComputedStyle(document.body).getPropertyValue('--chart-legend').trim();chartInstance.options.plugins.legend.labels.color=c;chartInstance.update();}
        (function(){if(localStorage.getItem('theme')==='light')document.body.classList.add('light-theme');updateChartLegend();})();
        </script>
        </body>
        </html>
    """);

    return sb.ToString();
}
