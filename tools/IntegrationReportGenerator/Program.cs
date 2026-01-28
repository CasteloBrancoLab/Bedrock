using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

// Gerador de relatório HTML para testes de integração
// Uso: IntegrationReportGenerator <trx-files-separados-por-;> <output-file> <git-branch> <git-commit> <root-dir>

var trxFiles = args.Length > 0 ? args[0].Split(';', StringSplitOptions.RemoveEmptyEntries) : [];
var outputFile = args.Length > 1 ? args[1] : "report.html";
var gitBranch = args.Length > 2 ? args[2] : "unknown";
var gitCommit = args.Length > 3 ? args[3] : "unknown";
var rootDir = args.Length > 4 ? args[4] : Directory.GetCurrentDirectory();

if (trxFiles.Length == 0)
{
    Console.WriteLine("Uso: IntegrationReportGenerator <trx-files> <output> <branch> <commit> [root-dir]");
    Console.WriteLine("Nenhum arquivo TRX fornecido.");
    return 1;
}

var ns = XNamespace.Get("http://microsoft.com/schemas/VisualStudio/TeamTest/2010");
var projects = new Dictionary<string, ProjectData>();
var totalDuration = TimeSpan.Zero;
var executionTime = DateTime.UtcNow;

foreach (var trxFile in trxFiles)
{
    if (!File.Exists(trxFile))
    {
        Console.WriteLine($"Arquivo não encontrado: {trxFile}");
        continue;
    }

    Console.WriteLine($"Processando: {trxFile}");

    // Extrair nome do projeto do arquivo TRX (formato: integration-<ProjectName>.trx)
    var trxFileName = Path.GetFileNameWithoutExtension(trxFile);
    var projectName = trxFileName.StartsWith("integration-")
        ? trxFileName["integration-".Length..]
        : trxFileName;

    // Tentar encontrar o .csproj correspondente para obter o DisplayName
    var projectDisplayName = FindProjectDisplayName(rootDir, projectName) ?? projectName;

    if (!projects.TryGetValue(projectName, out var project))
    {
        project = new ProjectData
        {
            Name = projectName,
            DisplayName = projectDisplayName
        };
        projects[projectName] = project;
    }

    var doc = XDocument.Load(trxFile);
    var root = doc.Root!;

    // Parse test definitions
    var definitions = root.Descendants(ns + "UnitTest")
        .ToDictionary(
            ut => ut.Attribute("id")?.Value ?? "",
            ut => new
            {
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

        if (!project.Features.TryGetValue(className, out var feature))
        {
            feature = new FeatureData { ClassName = className, Name = ExtractSimpleName(className) };
            project.Features[className] = feature;
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
                var json = line[(line.IndexOf("##STEP##") + 8)..].Trim();
                try
                {
                    var stepObj = JsonSerializer.Deserialize<JsonElement>(json);
                    steps.Add(new StepData
                    {
                        Type = stepObj.GetProperty("type").GetString() ?? "Given",
                        Description = stepObj.GetProperty("description").GetString() ?? ""
                    });
                }
                catch
                {
                    // Ignora linhas mal formatadas
                }
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

if (projects.Count == 0 || projects.Values.All(p => p.Features.Count == 0))
{
    Console.WriteLine("Nenhum resultado de teste encontrado nos arquivos TRX.");
    return 1;
}

// Enriquecer features com informações dos atributos [Feature] dos arquivos .cs
Console.WriteLine("Buscando atributos [Feature] nos arquivos de código...");
EnrichFeaturesWithAttributes(projects, rootDir);

// Calcular totais
var passed = projects.Values.Sum(p => p.Features.Values.Sum(f => f.Scenarios.Count(s => s.Status == "passed")));
var failed = projects.Values.Sum(p => p.Features.Values.Sum(f => f.Scenarios.Count(s => s.Status == "failed")));
var skipped = projects.Values.Sum(p => p.Features.Values.Sum(f => f.Scenarios.Count(s => s.Status == "skipped")));
var total = passed + failed + skipped;
var passRate = total > 0 ? (passed * 100.0 / total).ToString("F1", CultureInfo.InvariantCulture) : "0.0";

var env = new EnvData
{
    MachineName = Environment.MachineName,
    OsVersion = Environment.OSVersion.ToString(),
    DotNetVersion = Environment.Version.ToString(),
    UserName = Environment.UserName,
    ExecutionTime = executionTime,
    GitBranch = gitBranch,
    GitCommit = gitCommit.Length >= 7 ? gitCommit[..7] : gitCommit
};

var html = GenerateHtml(projects.Values.OrderBy(p => p.DisplayName).ToList(), env, totalDuration, passed, failed, skipped, passRate);

// Garantir que o diretório existe
var dir = Path.GetDirectoryName(outputFile);
if (!string.IsNullOrEmpty(dir))
    Directory.CreateDirectory(dir);

File.WriteAllText(outputFile, html);
Console.WriteLine($"Relatório gerado: {outputFile}");
Console.WriteLine($"  Projetos: {projects.Count} | Total: {total} | Passou: {passed} | Falhou: {failed} | Ignorado: {skipped}");
return 0;

static string? FindProjectDisplayName(string rootDir, string projectName)
{
    // Procurar recursivamente por arquivos .csproj que contenham o nome do projeto
    // Prioriza projetos de integração (em pastas IntegrationTests)
    try
    {
        var csprojFiles = Directory.GetFiles(rootDir, "*.csproj", SearchOption.AllDirectories)
            .OrderByDescending(p => p.Contains("IntegrationTests", StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(p => p.Contains("tests", StringComparison.OrdinalIgnoreCase));

        foreach (var csprojPath in csprojFiles)
        {
            var csprojName = Path.GetFileNameWithoutExtension(csprojPath);

            // Verificar se o nome do projeto corresponde (parcial ou completo)
            // O projectName vem do TRX (ex: "Persistence.PostgreSql")
            // O csprojName é o nome do arquivo (ex: "Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql")
            var matches = csprojName.EndsWith(projectName, StringComparison.OrdinalIgnoreCase) ||
                csprojName.Equals(projectName, StringComparison.OrdinalIgnoreCase);

            // Também verificar se o projeto de integração corresponde
            if (!matches && csprojPath.Contains("IntegrationTests", StringComparison.OrdinalIgnoreCase))
            {
                matches = csprojName.Contains(projectName, StringComparison.OrdinalIgnoreCase);
            }

            if (matches)
            {
                var csprojContent = File.ReadAllText(csprojPath);
                var doc = XDocument.Parse(csprojContent);

                // Procurar por <IntegrationTestDisplayName> no .csproj
                var displayName = doc.Descendants("IntegrationTestDisplayName").FirstOrDefault()?.Value;
                if (!string.IsNullOrWhiteSpace(displayName))
                {
                    return displayName;
                }

                // Fallback: usar o nome do arquivo .csproj formatado
                return FormatProjectName(csprojName);
            }
        }
    }
    catch
    {
        // Ignorar erros de acesso a diretórios
    }

    return FormatProjectName(projectName);
}

static string FormatProjectName(string name)
{
    // Remove prefixos comuns como "Bedrock.IntegrationTests.BuildingBlocks."
    var cleanName = name;
    var prefixes = new[] { "Bedrock.IntegrationTests.BuildingBlocks.", "Bedrock.IntegrationTests.", "IntegrationTests." };
    foreach (var prefix in prefixes)
    {
        if (cleanName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            cleanName = cleanName[prefix.Length..];
            break;
        }
    }

    // Substituir pontos por espaços e adicionar espaços antes de maiúsculas
    var sb = new StringBuilder();
    foreach (var c in cleanName)
    {
        if (c == '.')
        {
            sb.Append(" - ");
        }
        else if (char.IsUpper(c) && sb.Length > 0 && sb[^1] != ' ' && sb[^1] != '-')
        {
            sb.Append(' ');
            sb.Append(c);
        }
        else
        {
            sb.Append(c);
        }
    }

    return sb.ToString();
}

static string ExtractSimpleName(string fullName)
{
    var lastDot = fullName.LastIndexOf('.');
    return lastDot >= 0 ? fullName[(lastDot + 1)..] : fullName;
}

static string ConvertMethodName(string name)
{
    var sb = new StringBuilder();
    foreach (var c in name)
    {
        if (c == '_') sb.Append(' ');
        else if (char.IsUpper(c) && sb.Length > 0 && sb[^1] != ' ')
        {
            sb.Append(' ');
            sb.Append(char.ToLower(c));
        }
        else sb.Append(sb.Length == 0 ? c : char.ToLower(c));
    }
    return sb.ToString();
}

static void EnrichFeaturesWithAttributes(Dictionary<string, ProjectData> projects, string rootDir)
{
    // Regex para extrair o atributo [Feature("name", "description")] ou [Feature("name")]
    var featureRegex = new Regex(
        @"\[Feature\s*\(\s*""([^""]+)""\s*(?:,\s*""([^""]*)"")?\s*\)\]",
        RegexOptions.Compiled);

    // Regex para extrair o nome da classe
    var classRegex = new Regex(
        @"(?:public\s+)?(?:sealed\s+)?(?:partial\s+)?class\s+(\w+)",
        RegexOptions.Compiled);

    try
    {
        // Buscar arquivos .cs em pastas de IntegrationTests
        var csFiles = Directory.GetFiles(rootDir, "*.cs", SearchOption.AllDirectories)
            .Where(f => f.Contains("IntegrationTests", StringComparison.OrdinalIgnoreCase));

        foreach (var csFile in csFiles)
        {
            var content = File.ReadAllText(csFile);

            // Procurar atributo [Feature]
            var featureMatch = featureRegex.Match(content);
            if (!featureMatch.Success) continue;

            var featureName = featureMatch.Groups[1].Value;
            var featureDescription = featureMatch.Groups[2].Success ? featureMatch.Groups[2].Value : null;

            // Procurar nome da classe após o atributo
            var classMatch = classRegex.Match(content, featureMatch.Index);
            if (!classMatch.Success) continue;

            var className = classMatch.Groups[1].Value;

            // Encontrar a feature correspondente em algum projeto
            foreach (var project in projects.Values)
            {
                foreach (var feature in project.Features.Values)
                {
                    // Comparar o nome simples da classe
                    if (feature.Name.Equals(className, StringComparison.OrdinalIgnoreCase) ||
                        feature.ClassName.EndsWith(className, StringComparison.OrdinalIgnoreCase))
                    {
                        feature.DisplayName = featureName;
                        feature.Description = string.IsNullOrWhiteSpace(featureDescription) ? null : featureDescription;
                    }
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  Aviso: Não foi possível buscar atributos [Feature]: {ex.Message}");
    }
}

static string FormatDuration(TimeSpan duration)
{
    return duration.TotalMinutes >= 1
        ? $"{(int)duration.TotalMinutes}m {duration.Seconds}s"
        : duration.TotalSeconds >= 1
            ? $"{duration.TotalSeconds:F2}s"
            : $"{duration.TotalMilliseconds:F0}ms";
}

static string GenerateHtml(List<ProjectData> projects, EnvData env, TimeSpan duration, int passed, int failed, int skipped, string passRate)
{
    var total = passed + failed + skipped;
    var sb = new StringBuilder();

    sb.Append("""
        <!DOCTYPE html>
        <html lang="pt-BR">
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <title>Bedrock - Relatório de Testes de Integração</title>
            <style>
                :root{--passed:#10b981;--failed:#ef4444;--skipped:#f59e0b;--bg:#f9fafb;--card:#fff;--text:#1f2937;--muted:#6b7280;--border:#e5e7eb;--project-bg:#e0e7ff;--project-border:#6366f1}
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

                /* Sumário por projeto */
                .summary-section{margin-bottom:2rem}
                .summary-header{font-size:1.25rem;font-weight:600;margin-bottom:1rem;border-bottom:2px solid var(--border);padding-bottom:.5rem}
                .summary-table{width:100%;border-collapse:collapse;background:var(--card);border-radius:.75rem;overflow:hidden;box-shadow:0 1px 3px rgba(0,0,0,.1)}
                .summary-table th,.summary-table td{padding:.75rem 1rem;text-align:left;border-bottom:1px solid var(--border)}
                .summary-table th{background:#f8fafc;font-weight:600;font-size:.75rem;text-transform:uppercase;color:var(--muted)}
                .summary-table tr:last-child td{border-bottom:none}
                .summary-table .num{text-align:center;font-weight:600}
                .summary-table .passed{color:var(--passed)}.summary-table .failed{color:var(--failed)}.summary-table .skipped{color:var(--skipped)}
                .summary-table .project-row{background:#f8fafc;font-weight:600}
                .summary-table .feature-row td:first-child{padding-left:2rem}
                .progress-bar{height:6px;background:#e5e7eb;border-radius:3px;overflow:hidden;min-width:100px}
                .progress-bar-fill{height:100%;background:var(--passed);transition:width .3s}

                /* Projetos e Features */
                .results-header{font-size:1.25rem;font-weight:600;margin-bottom:1rem;border-bottom:2px solid var(--border);padding-bottom:.5rem}
                .project{margin-bottom:1.5rem}
                .project-header{padding:1rem 1.5rem;background:linear-gradient(to right,#eef2ff,#e0e7ff);border-left:4px solid var(--project-border);border-radius:.75rem .75rem 0 0;font-weight:700;font-size:1.1rem;display:flex;align-items:center;gap:.75rem;cursor:pointer}
                .project-header:hover{background:linear-gradient(to right,#e0e7ff,#c7d2fe)}
                .project-toggle{font-size:.75rem;color:var(--muted);transition:transform .2s}
                .project.collapsed .project-toggle{transform:rotate(-90deg)}
                .project-name{flex:1}
                .project-stats{display:flex;gap:.5rem;font-size:.75rem}
                .project-stat{padding:.25rem .75rem;border-radius:9999px;font-weight:600}
                .project-stat.passed{background:#d1fae5;color:var(--passed)}
                .project-stat.failed{background:#fee2e2;color:var(--failed)}
                .project-stat.skipped{background:#fef3c7;color:var(--skipped)}
                .project-stat.total{background:#e5e7eb;color:var(--text)}
                .project-content{max-height:10000px;overflow:hidden;transition:max-height .3s}
                .project.collapsed .project-content{max-height:0}

                .feature{background:var(--card);margin:0;border-radius:0;box-shadow:none;border-bottom:1px solid var(--border)}
                .feature:last-child{border-bottom:none;border-radius:0 0 .75rem .75rem}
                .feature-header{padding:.875rem 1.5rem;background:linear-gradient(to right,#f8fafc,#f1f5f9);font-weight:600;display:flex;align-items:center;gap:.75rem;cursor:pointer;font-size:.95rem}
                .feature-header:hover{background:linear-gradient(to right,#f1f5f9,#e2e8f0)}
                .feature-toggle{font-size:.75rem;color:var(--muted);transition:transform .2s}
                .feature.collapsed .feature-toggle{transform:rotate(-90deg)}
                .feature-name{flex:1}
                .feature-stats{display:flex;gap:.5rem;font-size:.75rem}
                .feature-stat{padding:.25rem .5rem;border-radius:9999px;font-weight:500}
                .feature-stat.passed{background:#d1fae5;color:var(--passed)}
                .feature-stat.failed{background:#fee2e2;color:var(--failed)}
                .feature-stat.skipped{background:#fef3c7;color:var(--skipped)}
                .feature-description{padding:.5rem 1.5rem;color:var(--muted);font-size:.85rem;font-style:italic;background:#f8fafc;border-top:1px solid var(--border)}
                .feature-content{max-height:5000px;overflow:hidden;transition:max-height .3s}
                .feature.collapsed .feature-content{max-height:0}

                .scenario{padding:.875rem 1.5rem;border-top:1px solid var(--border)}
                .scenario:first-child{border-top:none}
                .scenario-header{display:flex;align-items:center;gap:.75rem}
                .scenario-status{font-size:1.1rem}
                .scenario-name{font-weight:500;flex:1;font-size:.9rem}
                .scenario-duration{color:var(--muted);font-size:.75rem;font-family:monospace}
                .steps{margin-top:.5rem;padding-left:2rem}
                .step{display:flex;gap:.5rem;font-size:.8rem;padding:.25rem 0;color:var(--muted)}
                .step-type{font-weight:600;min-width:45px}
                .step-type.given{color:#3b82f6}.step-type.when{color:#8b5cf6}.step-type.then{color:#10b981}
                .error-box{background:#fee2e2;border:1px solid #fecaca;border-radius:.5rem;padding:.75rem 1rem;margin-top:.5rem;margin-left:2rem;font-size:.8rem;color:#991b1b}
                .error-box strong{display:block;margin-bottom:.25rem}
                .error-msg{font-family:monospace;white-space:pre-wrap;word-break:break-all;font-size:.75rem}

                .footer{text-align:center;padding:2rem;color:var(--muted);font-size:.75rem;border-top:1px solid var(--border);margin-top:2rem}
                @media print{body{background:#fff;font-size:12px}.container{max-width:none;padding:1rem}.card,.project,.feature,.chart-section,.summary-table{box-shadow:none;border:1px solid var(--border)}.project,.feature{break-inside:avoid}.project.collapsed .project-content,.feature.collapsed .feature-content{max-height:none}.project-toggle,.feature-toggle{display:none}}
            </style>
        </head>
        <body>
        <div class="container">
            <header class="header">
                <h1>Bedrock - Relatório de Testes de Integração</h1>
                <p class="subtitle">Gerado em:
        """);
    sb.Append(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
    sb.Append("""
         UTC</p>
            </header>
            <section class="cards">
                <div class="card"><div class="card-value">
        """);
    sb.Append(total);
    sb.Append("""
        </div><div class="card-label">Total</div></div>
                <div class="card passed"><div class="card-value">
        """);
    sb.Append(passed);
    sb.Append("""
        </div><div class="card-label">Passou</div><div class="card-pct">
        """);
    sb.Append(passRate);
    sb.Append("""
        %</div></div>
                <div class="card failed"><div class="card-value">
        """);
    sb.Append(failed);
    sb.Append("""
        </div><div class="card-label">Falhou</div></div>
                <div class="card skipped"><div class="card-value">
        """);
    sb.Append(skipped);
    sb.Append("""
        </div><div class="card-label">Ignorado</div></div>
            </section>
            <section class="chart-section">
                <div class="chart-container"><canvas id="chart"></canvas></div>
                <div class="env-info">
                    <h3>Ambiente</h3>
                    <dl>
                        <dt>Máquina</dt><dd>
        """);
    sb.Append(WebUtility.HtmlEncode(env.MachineName));
    sb.Append("""
        </dd>
                        <dt>SO</dt><dd>
        """);
    sb.Append(WebUtility.HtmlEncode(env.OsVersion));
    sb.Append("""
        </dd>
                        <dt>.NET</dt><dd>
        """);
    sb.Append(WebUtility.HtmlEncode(env.DotNetVersion));
    sb.Append("""
        </dd>
                        <dt>Usuário</dt><dd>
        """);
    sb.Append(WebUtility.HtmlEncode(env.UserName));
    sb.Append("""
        </dd>
                        <dt>Executado</dt><dd>
        """);
    sb.Append(env.ExecutionTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
    sb.Append("""
         UTC</dd>
                        <dt>Branch</dt><dd>
        """);
    sb.Append(WebUtility.HtmlEncode(env.GitBranch));
    sb.Append("""
        </dd>
                        <dt>Commit</dt><dd>
        """);
    sb.Append(WebUtility.HtmlEncode(env.GitCommit));
    sb.Append("""
        </dd>
                    </dl>
                </div>
            </section>
        """);

    // Sumário por Projeto e Classe
    sb.Append("""
            <section class="summary-section">
                <h2 class="summary-header">Sumário por Projeto e Classe</h2>
                <table class="summary-table">
                    <thead>
                        <tr>
                            <th>Projeto / Classe</th>
                            <th class="num">Total</th>
                            <th class="num">Passou</th>
                            <th class="num">Falhou</th>
                            <th class="num">Ignorado</th>
                            <th>Progresso</th>
                        </tr>
                    </thead>
                    <tbody>
        """);

    foreach (var project in projects)
    {
        var pp = project.Features.Values.Sum(f => f.Scenarios.Count(s => s.Status == "passed"));
        var pf = project.Features.Values.Sum(f => f.Scenarios.Count(s => s.Status == "failed"));
        var ps = project.Features.Values.Sum(f => f.Scenarios.Count(s => s.Status == "skipped"));
        var pt = pp + pf + ps;
        var pPct = pt > 0 ? (pp * 100.0 / pt) : 0;

        sb.Append($"""
                        <tr class="project-row">
                            <td><strong>{WebUtility.HtmlEncode(project.DisplayName)}</strong></td>
                            <td class="num">{pt}</td>
                            <td class="num passed">{pp}</td>
                            <td class="num failed">{pf}</td>
                            <td class="num skipped">{ps}</td>
                            <td><div class="progress-bar"><div class="progress-bar-fill" style="width:{pPct:F0}%"></div></div></td>
                        </tr>
        """);

        foreach (var feature in project.Features.Values.OrderBy(f => f.Name))
        {
            var fp = feature.Scenarios.Count(s => s.Status == "passed");
            var ff = feature.Scenarios.Count(s => s.Status == "failed");
            var fs = feature.Scenarios.Count(s => s.Status == "skipped");
            var ft = fp + ff + fs;
            var fPct = ft > 0 ? (fp * 100.0 / ft) : 0;

            sb.Append($"""
                        <tr class="feature-row">
                            <td>{WebUtility.HtmlEncode(feature.EffectiveName)}</td>
                            <td class="num">{ft}</td>
                            <td class="num passed">{fp}</td>
                            <td class="num failed">{ff}</td>
                            <td class="num skipped">{fs}</td>
                            <td><div class="progress-bar"><div class="progress-bar-fill" style="width:{fPct:F0}%"></div></div></td>
                        </tr>
        """);
        }
    }

    sb.Append("""
                    </tbody>
                </table>
            </section>
        """);

    // Resultados detalhados por Projeto
    sb.Append("""
            <section>
                <h2 class="results-header">Resultados Detalhados</h2>
        """);

    foreach (var project in projects)
    {
        var pp = project.Features.Values.Sum(f => f.Scenarios.Count(s => s.Status == "passed"));
        var pf = project.Features.Values.Sum(f => f.Scenarios.Count(s => s.Status == "failed"));
        var ps = project.Features.Values.Sum(f => f.Scenarios.Count(s => s.Status == "skipped"));
        var pt = pp + pf + ps;

        sb.Append($"""

                <article class="project">
                    <div class="project-header">
                        <span class="project-toggle">▼</span>
                        <span class="project-name">{WebUtility.HtmlEncode(project.DisplayName)}</span>
                        <div class="project-stats">
                            <span class="project-stat total">{pt} testes</span>
        """);
        if (pp > 0) sb.Append($"""<span class="project-stat passed">{pp} passou</span>""");
        if (pf > 0) sb.Append($"""<span class="project-stat failed">{pf} falhou</span>""");
        if (ps > 0) sb.Append($"""<span class="project-stat skipped">{ps} ignorado</span>""");
        sb.Append("""
                        </div>
                    </div>
                    <div class="project-content">
        """);

        foreach (var feature in project.Features.Values.OrderBy(f => f.Name))
        {
            var fp = feature.Scenarios.Count(s => s.Status == "passed");
            var ff = feature.Scenarios.Count(s => s.Status == "failed");
            var fs = feature.Scenarios.Count(s => s.Status == "skipped");

            // Usar EffectiveName (DisplayName do atributo [Feature] ou fallback para Name)
            var featureTitle = feature.EffectiveName;
            var hasDescription = !string.IsNullOrWhiteSpace(feature.Description);

            sb.Append($"""

                        <article class="feature">
                            <div class="feature-header">
                                <span class="feature-toggle">▼</span>
                                <span class="feature-name">{WebUtility.HtmlEncode(featureTitle)}</span>
                                <div class="feature-stats">
        """);
            if (fp > 0) sb.Append($"""<span class="feature-stat passed">{fp}</span>""");
            if (ff > 0) sb.Append($"""<span class="feature-stat failed">{ff}</span>""");
            if (fs > 0) sb.Append($"""<span class="feature-stat skipped">{fs}</span>""");
            sb.Append("""
                                </div>
                            </div>
        """);

            // Adicionar descrição se existir
            if (hasDescription)
            {
                sb.Append($"""<div class="feature-description">{WebUtility.HtmlEncode(feature.Description!)}</div>""");
            }

            sb.Append("""
                            <div class="feature-content">
        """);

            foreach (var scenario in feature.Scenarios)
            {
                var icon = scenario.Status switch { "passed" => "\u2705", "failed" => "\u274C", _ => "\u23ED\uFE0F" };
                var dur = FormatDuration(scenario.Duration);

                sb.Append($"""

                                <div class="scenario">
                                    <div class="scenario-header">
                                        <span class="scenario-status">{icon}</span>
                                        <span class="scenario-name">{WebUtility.HtmlEncode(scenario.Name)}</span>
                                        <span class="scenario-duration">{dur}</span>
                                    </div>
        """);

                if (scenario.Steps.Count > 0)
                {
                    sb.Append("""<div class="steps">""");
                    foreach (var step in scenario.Steps)
                    {
                        var cls = step.Type.ToLower();
                        sb.Append($"""<div class="step"><span class="step-type {cls}">{step.Type}:</span><span>{WebUtility.HtmlEncode(step.Description)}</span></div>""");
                    }
                    sb.Append("</div>");
                }

                if (scenario.Status == "failed" && !string.IsNullOrEmpty(scenario.ErrorMessage))
                {
                    sb.Append($"""<div class="error-box"><strong>Erro</strong><div class="error-msg">{WebUtility.HtmlEncode(scenario.ErrorMessage)}</div></div>""");
                }

                sb.Append("</div>");
            }

            sb.Append("</div></article>");
        }

        sb.Append("</div></article>");
    }

    var durationStr = FormatDuration(duration);

    sb.Append($"""

            </section>
            <footer class="footer">
                <p><strong>Bedrock Framework</strong> - Relatório de Testes de Integração</p>
                <p>Projetos: {projects.Count} | Duração Total: {durationStr}</p>
            </footer>
        </div>
        <script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.1/dist/chart.umd.min.js"></script>
        <script>
        """);

    // Chart.js config separada para evitar problemas com chaves em raw string literals
    sb.Append($"new Chart(document.getElementById('chart'),{{type:'doughnut',data:{{labels:['Passou','Falhou','Ignorado'],datasets:[{{data:[{passed},{failed},{skipped}],backgroundColor:['#10b981','#ef4444','#f59e0b'],borderWidth:0}}]}},options:{{responsive:true,plugins:{{legend:{{position:'bottom'}}}},cutout:'60%'}}}});");

    sb.Append("""

        document.querySelectorAll('.project-header').forEach(h=>h.addEventListener('click',()=>h.closest('.project').classList.toggle('collapsed')));
        document.querySelectorAll('.feature-header').forEach(h=>h.addEventListener('click',e=>{e.stopPropagation();h.closest('.feature').classList.toggle('collapsed');}));
        </script>
        </body>
        </html>
        """);

    return sb.ToString();
}

internal sealed class ProjectData
{
    public string Name { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public Dictionary<string, FeatureData> Features { get; } = [];
}

internal sealed class FeatureData
{
    public string ClassName { get; set; } = "";
    public string Name { get; set; } = "";
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public List<ScenarioData> Scenarios { get; } = [];

    /// <summary>
    /// Retorna o nome de exibição (DisplayName do atributo [Feature]) ou o Name como fallback.
    /// </summary>
    public string EffectiveName => DisplayName ?? Name;
}

internal sealed class ScenarioData
{
    public string Name { get; set; } = "";
    public string MethodName { get; set; } = "";
    public string Status { get; set; } = "skipped";
    public TimeSpan Duration { get; set; }
    public List<StepData> Steps { get; set; } = [];
    public string? ErrorMessage { get; set; }
}

internal sealed class StepData
{
    public string Type { get; set; } = "";
    public string Description { get; set; } = "";
}

internal sealed class EnvData
{
    public string MachineName { get; set; } = "";
    public string OsVersion { get; set; } = "";
    public string DotNetVersion { get; set; } = "";
    public string UserName { get; set; } = "";
    public DateTime ExecutionTime { get; set; }
    public string GitBranch { get; set; } = "";
    public string GitCommit { get; set; } = "";
}
