using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

// Gerador de relatório HTML para testes de unidade com cobertura
// Uso: UnitTestReportGenerator <coverage-dir> <output-file> <git-branch> <git-commit> [ci-workflow] [mutation-dir]

var coverageDir = args.Length > 0 ? args[0] : "artifacts/coverage/raw";
var outputFile = args.Length > 1 ? args[1] : "artifacts/unittest-report/index.html";
var gitBranch = args.Length > 2 ? args[2] : "unknown";
var gitCommit = args.Length > 3 ? args[3] : "unknown";
var ciWorkflow = args.Length > 4 ? args[4] : "";
var mutationDir = args.Length > 5 ? args[5] : "";

if (!Directory.Exists(coverageDir))
{
    Console.WriteLine($"Diretório de cobertura não encontrado: {coverageDir}");
    return 1;
}

Console.WriteLine($"Processando: {coverageDir}");

// Parse SonarCloud exclusions from CI workflow
var sonarExclusions = new SonarExclusions();
if (!string.IsNullOrEmpty(ciWorkflow) && File.Exists(ciWorkflow))
{
    Console.WriteLine($"  CI workflow: {ciWorkflow}");
    sonarExclusions = ParseSonarExclusions(ciWorkflow);
    Console.WriteLine($"  Sonar exclusions: {sonarExclusions.Exclusions.Count} patterns");
    Console.WriteLine($"  Sonar coverage exclusions: {sonarExclusions.CoverageExclusions.Count} patterns");
    Console.WriteLine($"  Sonar CPD exclusions: {sonarExclusions.CpdExclusions.Count} patterns");
}

var trxNs = XNamespace.Get("http://microsoft.com/schemas/VisualStudio/TeamTest/2010");
var projects = new List<ProjectData>();

// Discover projects by finding <name>.cobertura.xml files
foreach (var coberturaFile in Directory.GetFiles(coverageDir, "*.cobertura.xml").OrderBy(f => f, StringComparer.Ordinal))
{
    var projectName = Path.GetFileNameWithoutExtension(coberturaFile).Replace(".cobertura", "");
    Console.WriteLine($"  Projeto: {projectName}");

    var project = new ProjectData { Name = projectName };

    // Parse Cobertura coverage
    ParseCoverage(coberturaFile, project, sonarExclusions);

    // Parse TRX test results
    var trxFile = Path.Combine(coverageDir, projectName, "results.trx");
    if (File.Exists(trxFile))
        ParseTrx(trxFile, trxNs, project);

    projects.Add(project);
}

if (projects.Count == 0)
{
    Console.WriteLine("Nenhum projeto encontrado.");
    return 1;
}

// Parse mutation data and enrich test entries
if (!string.IsNullOrEmpty(mutationDir) && Directory.Exists(mutationDir))
{
    Console.WriteLine($"  Mutation dir: {mutationDir}");
    ParseMutationData(mutationDir, projects);
}

var html = GenerateHtml(projects, gitBranch, gitCommit.Length >= 7 ? gitCommit[..7] : gitCommit, sonarExclusions);

var dir = Path.GetDirectoryName(outputFile);
if (!string.IsNullOrEmpty(dir))
    Directory.CreateDirectory(dir);

File.WriteAllText(outputFile, html);
Console.WriteLine($"Relatório gerado: {outputFile}");

var totalTests = projects.Sum(p => p.TotalTests);
var totalPassed = projects.Sum(p => p.Passed);
var totalFailed = projects.Sum(p => p.Failed);
var totalSkipped = projects.Sum(p => p.Skipped);
Console.WriteLine($"  Projetos: {projects.Count} | Testes: {totalTests} | Passou: {totalPassed} | Falhou: {totalFailed} | Ignorados: {totalSkipped}");
return 0;

// ─── Parsing ───────────────────────────────────────────────────────────────

static void ParseCoverage(string coberturaFile, ProjectData project, SonarExclusions sonarExclusions)
{
    var doc = XDocument.Load(coberturaFile);
    var root = doc.Root!;

    // Default rates from root (will be overridden by matching package)
    project.LineRate = ParseRate(root.Attribute("line-rate")?.Value);
    project.BranchRate = ParseRate(root.Attribute("branch-rate")?.Value);

    // Get source base path to reconstruct relative paths
    var sourceBase = root.Descendants("source").FirstOrDefault()?.Value ?? "";

    // Determine the expected package name for this project
    // Project name "Core" -> package "Bedrock.BuildingBlocks.Core"
    // Project name "Serialization.Json" -> package "Bedrock.BuildingBlocks.Serialization.Json"
    var expectedPackageSuffix = "." + project.Name;

    foreach (var pkg in root.Descendants("package"))
    {
        var packageName = pkg.Attribute("name")?.Value ?? "";

        // Only include classes from the package that matches this project
        // This filters out transitive dependencies (e.g., Core classes appearing in Serialization.Json coverage)
        if (!packageName.EndsWith(expectedPackageSuffix, StringComparison.Ordinal) &&
            !packageName.Equals("Bedrock.BuildingBlocks." + project.Name, StringComparison.Ordinal))
            continue;

        // Use this package's line/branch rates as the project rates (more accurate than root)
        project.BranchRate = ParseRate(pkg.Attribute("branch-rate")?.Value);
        project.BranchesCovered = 0;
        project.BranchesValid = 0;

        // Count branches from condition-coverage attributes on lines
        foreach (var line in pkg.Descendants("line").Where(l => l.Attribute("branch")?.Value == "True"))
        {
            var condCoverage = line.Attribute("condition-coverage")?.Value ?? "";
            // Format: "100% (2/2)" or "50% (1/2)"
            var match = Regex.Match(condCoverage, @"\((\d+)/(\d+)\)");
            if (match.Success)
            {
                project.BranchesCovered += int.Parse(match.Groups[1].Value);
                project.BranchesValid += int.Parse(match.Groups[2].Value);
            }
        }

        foreach (var cls in pkg.Descendants("class"))
        {
            var className = cls.Attribute("name")?.Value ?? "";
            var fileName = cls.Attribute("filename")?.Value ?? "";
            var clsLineRate = ParseRate(cls.Attribute("line-rate")?.Value);
            var clsBranchRate = ParseRate(cls.Attribute("branch-rate")?.Value);
            var complexity = int.TryParse(cls.Attribute("complexity")?.Value, out var cx) ? cx : 0;

            var lines = cls.Descendants("line").ToList();
            var coveredLines = lines.Count(l => int.TryParse(l.Attribute("hits")?.Value, out var h) && h > 0);
            var totalLines = lines.Count;

            var uncoveredLineNumbers = lines
                .Where(l => int.TryParse(l.Attribute("hits")?.Value, out var h) && h == 0)
                .Select(l => int.TryParse(l.Attribute("number")?.Value, out var n) ? n : 0)
                .Where(n => n > 0)
                .ToList();

            var methods = cls.Descendants("method").Select(m => new MethodData
            {
                Name = m.Attribute("name")?.Value ?? "",
                LineRate = ParseRate(m.Attribute("line-rate")?.Value),
                BranchRate = ParseRate(m.Attribute("branch-rate")?.Value),
                Lines = m.Descendants("line").Count(),
                CoveredLines = m.Descendants("line").Count(l => int.TryParse(l.Attribute("hits")?.Value, out var h) && h > 0)
            }).ToList();

            // Parse uncovered branches
            var uncoveredBranches = new List<BranchInfo>();
            foreach (var branchLine in cls.Descendants("line").Where(l => l.Attribute("branch")?.Value == "True"))
            {
                var condCov = branchLine.Attribute("condition-coverage")?.Value ?? "";
                if (condCov.StartsWith("100%")) continue;

                var lineNum = int.TryParse(branchLine.Attribute("number")?.Value, out var ln) ? ln : 0;
                var covMatch = Regex.Match(condCov, @"\((\d+)/(\d+)\)");
                var covered = covMatch.Success ? int.Parse(covMatch.Groups[1].Value) : 0;
                var total = covMatch.Success ? int.Parse(covMatch.Groups[2].Value) : 0;

                var uncovConds = branchLine.Elements("conditions").Elements("condition")
                    .Where(c => c.Attribute("coverage")?.Value != "100%")
                    .Select(c => (
                        Number: int.TryParse(c.Attribute("number")?.Value, out var cn) ? cn : 0,
                        Coverage: c.Attribute("coverage")?.Value ?? "0%"
                    ))
                    .ToList();

                uncoveredBranches.Add(new BranchInfo
                {
                    Line = lineNum,
                    ConditionCoverage = condCov,
                    Covered = covered,
                    Total = total,
                    UncoveredConditions = uncovConds
                });
            }

            // Deduplicate branches (Coverlet can duplicate across packages)
            uncoveredBranches = uncoveredBranches
                .GroupBy(b => b.Line)
                .Select(g => g.First())
                .OrderBy(b => b.Line)
                .ToList();

            // Build relative path for sonar matching (e.g., "src/BuildingBlocks/Core/Validations/ValidationUtils.cs")
            var relativePath = BuildRelativePath(sourceBase, fileName);
            var sonarStatus = sonarExclusions.Classify(relativePath);

            project.Classes.Add(new ClassData
            {
                FullName = className,
                FileName = fileName,
                RelativePath = relativePath,
                LineRate = clsLineRate,
                BranchRate = clsBranchRate,
                Complexity = complexity,
                TotalLines = totalLines,
                CoveredLines = coveredLines,
                UncoveredLines = uncoveredLineNumbers,
                UncoveredBranches = uncoveredBranches,
                Methods = methods,
                SonarStatus = sonarStatus
            });
        }
    }

    // Recalculate project-level line stats from filtered classes only
    project.LinesCovered = project.Classes.Sum(c => c.CoveredLines);
    project.LinesValid = project.Classes.Sum(c => c.TotalLines);
    if (project.LinesValid > 0)
        project.LineRate = (double)project.LinesCovered / project.LinesValid;
    // BranchRate is already set from the matching package element
}

static string BuildRelativePath(string sourceBase, string fileName)
{
    // sourceBase: "C:\dev\repos\Bedrock\src\BuildingBlocks\Core\"
    // fileName: "Validations\ValidationUtils.cs"
    // We want: "src/BuildingBlocks/Core/Validations/ValidationUtils.cs"
    var fullPath = Path.Combine(sourceBase, fileName).Replace('\\', '/');

    // Find the "src/" or root marker
    var srcIdx = fullPath.IndexOf("/src/", StringComparison.OrdinalIgnoreCase);
    if (srcIdx >= 0)
        return fullPath[(srcIdx + 1)..]; // "src/BuildingBlocks/Core/..."

    // Fallback: try to find common markers
    var repoMarkers = new[] { "/tests/", "/tools/", "/samples/", "/templates/" };
    foreach (var marker in repoMarkers)
    {
        var idx = fullPath.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (idx >= 0)
            return fullPath[(idx + 1)..];
    }

    return fileName.Replace('\\', '/');
}

static void ParseTrx(string trxFile, XNamespace ns, ProjectData project)
{
    var doc = XDocument.Load(trxFile);
    var root = doc.Root!;

    // Parse times
    var times = root.Element(ns + "Times");
    if (times != null)
    {
        if (DateTime.TryParse(times.Attribute("start")?.Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var start) &&
            DateTime.TryParse(times.Attribute("finish")?.Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var finish))
        {
            project.Duration = finish - start;
        }
    }

    // Parse test definitions for class mapping
    var definitions = root.Descendants(ns + "UnitTest")
        .ToDictionary(
            ut => ut.Attribute("id")?.Value ?? "",
            ut => ut.Element(ns + "TestMethod")?.Attribute("className")?.Value ?? "");

    // Parse results
    foreach (var result in root.Descendants(ns + "UnitTestResult"))
    {
        var testId = result.Attribute("testId")?.Value ?? "";
        var testName = result.Attribute("testName")?.Value ?? "";
        var outcome = result.Attribute("outcome")?.Value ?? "Unknown";
        var duration = TimeSpan.TryParse(result.Attribute("duration")?.Value, out var d) ? d : TimeSpan.Zero;
        var className = definitions.GetValueOrDefault(testId, "");

        project.TotalTests++;
        switch (outcome)
        {
            case "Passed": project.Passed++; break;
            case "Failed": project.Failed++; break;
            default: project.Skipped++; break;
        }

        project.Tests.Add(new TestData
        {
            Name = testName,
            ClassName = className,
            Outcome = outcome,
            Duration = duration,
            ErrorMessage = outcome == "Failed"
                ? result.Descendants(ns + "Message").FirstOrDefault()?.Value
                : null
        });
    }
}

static double ParseRate(string? value) =>
    double.TryParse(value, CultureInfo.InvariantCulture, out var v) ? v : 0;

static SonarExclusions ParseSonarExclusions(string ciWorkflowPath)
{
    var result = new SonarExclusions();
    var content = File.ReadAllText(ciWorkflowPath);

    // Extract sonar.exclusions
    var exclusionsMatch = Regex.Match(content, @"sonar\.exclusions=""([^""]+)""");
    if (exclusionsMatch.Success)
        result.Exclusions = exclusionsMatch.Groups[1].Value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

    // Extract sonar.coverage.exclusions
    var coverageMatch = Regex.Match(content, @"sonar\.coverage\.exclusions=""([^""]+)""");
    if (coverageMatch.Success)
        result.CoverageExclusions = coverageMatch.Groups[1].Value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

    // Extract sonar.cpd.exclusions
    var cpdMatch = Regex.Match(content, @"sonar\.cpd\.exclusions=""([^""]+)""");
    if (cpdMatch.Success)
        result.CpdExclusions = cpdMatch.Groups[1].Value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

    return result;
}

static void ParseMutationData(string mutationDir, List<ProjectData> projects)
{
    // Each project has artifacts/mutation/<ProjectName>/reports/mutation-report.json
    foreach (var project in projects)
    {
        var reportPath = Path.Combine(mutationDir, project.Name, "reports", "mutation-report.json");
        if (!File.Exists(reportPath))
            continue;

        Console.WriteLine($"  Mutation report: {project.Name}");

        using var doc = JsonDocument.Parse(File.ReadAllText(reportPath));
        var root = doc.RootElement;

        // Count mutant statuses at project level
        // Score = Killed / (Killed + Survived + NoCoverage + Timeout)
        // Ignored mutants are excluded from the calculation (Stryker convention)
        int killed = 0, survived = 0, noCoverage = 0, timeout = 0, ignored = 0;

        if (root.TryGetProperty("files", out var files))
        {
            foreach (var f in files.EnumerateObject())
            {
                foreach (var mutant in f.Value.GetProperty("mutants").EnumerateArray())
                {
                    var status = mutant.GetProperty("status").GetString() ?? "";
                    switch (status)
                    {
                        case "Killed": killed++; break;
                        case "Survived": survived++; break;
                        case "NoCoverage": noCoverage++; break;
                        case "Timeout": timeout++; break;
                        case "Ignored": ignored++; break;
                    }
                }
            }
        }

        var totalDetectable = killed + survived + noCoverage + timeout;
        project.MutationKilled = killed;
        project.MutationTotal = totalDetectable;
        project.MutationIgnored = ignored;
        project.HasMutationData = totalDetectable > 0;

        var score = totalDetectable > 0 ? killed * 100.0 / totalDetectable : 0;
        Console.WriteLine($"    Killed: {killed}, Survived: {survived}, NoCoverage: {noCoverage}, Timeout: {timeout}, Ignored: {ignored} -> Score: {score:F1}%");
    }
}

// ─── HTML Generation ───────────────────────────────────────────────────────

static string GenerateHtml(List<ProjectData> projects, string branch, string commit, SonarExclusions sonarExclusions)
{
    var totalTests = projects.Sum(p => p.TotalTests);
    var totalPassed = projects.Sum(p => p.Passed);
    var totalFailed = projects.Sum(p => p.Failed);
    var totalSkipped = projects.Sum(p => p.Skipped);
    var totalDuration = TimeSpan.FromTicks(projects.Sum(p => p.Duration.Ticks));
    var totalLinesCovered = projects.Sum(p => p.LinesCovered);
    var totalLinesValid = projects.Sum(p => p.LinesValid);
    var totalBranchesCovered = projects.Sum(p => p.BranchesCovered);
    var totalBranchesValid = projects.Sum(p => p.BranchesValid);
    var overallLineRate = totalLinesValid > 0 ? totalLinesCovered * 100.0 / totalLinesValid : 100.0;
    var overallBranchRate = totalBranchesValid > 0 ? totalBranchesCovered * 100.0 / totalBranchesValid : 100.0;
    var passRate = totalTests > 0 ? totalPassed * 100.0 / totalTests : 100.0;
    var hasMutationData = projects.Any(p => p.HasMutationData);
    var totalMutationKilled = projects.Sum(p => p.MutationKilled);
    var totalMutationTotal = projects.Sum(p => p.MutationTotal);
    var overallMutationScore = totalMutationTotal > 0 ? totalMutationKilled * 100.0 / totalMutationTotal : 0;

    var sb = new StringBuilder();

    sb.Append($$"""
        <!DOCTYPE html>
        <html lang="pt-BR">
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <title>Bedrock - Relatório de Testes de Unidade</title>
            <style>
                :root{--passed:#10b981;--failed:#ef4444;--skipped:#f59e0b;--info:#3b82f6;--bg:#0f172a;--card:#1e293b;--text:#f1f5f9;--muted:#94a3b8;--border:#334155;--header-bg:linear-gradient(to right,#1e1b4b,#312e81);--table-header-bg:#1e293b;--badge-passed-bg:#064e3b;--badge-passed-text:#6ee7b7;--badge-failed-bg:#7f1d1d;--badge-failed-text:#fca5a5;--badge-skipped-bg:#78350f;--badge-skipped-text:#fcd34d;--badge-info-bg:#1e3a5f;--badge-info-text:#93c5fd;--chart-legend:#e5e7eb;--progress-bg:#374151;--coverage-high:#10b981;--coverage-med:#f59e0b;--coverage-low:#ef4444}
                .light-theme{--bg:#f9fafb;--card:#fff;--text:#1f2937;--muted:#6b7280;--border:#e5e7eb;--header-bg:linear-gradient(to right,#eef2ff,#e0e7ff);--table-header-bg:#f8fafc;--badge-passed-bg:#d1fae5;--badge-passed-text:#065f46;--badge-failed-bg:#fee2e2;--badge-failed-text:#991b1b;--badge-skipped-bg:#fef3c7;--badge-skipped-text:#92400e;--badge-info-bg:#dbeafe;--badge-info-text:#1e3a8a;--chart-legend:#374151;--progress-bg:#e5e7eb}
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
                .cards{display:grid;grid-template-columns:repeat(auto-fit,minmax(140px,1fr));gap:1rem;margin-bottom:2rem}
                .card{background:var(--card);padding:1.25rem;border-radius:.75rem;box-shadow:0 1px 3px rgba(0,0,0,.1);text-align:center}
                .card-value{font-size:2.5rem;font-weight:700}
                .card-label{color:var(--muted);font-size:.75rem;text-transform:uppercase}
                .card.passed{border-left:4px solid var(--passed)}.card.passed .card-value{color:var(--passed)}
                .card.failed{border-left:4px solid var(--failed)}.card.failed .card-value{color:var(--failed)}
                .card.skipped{border-left:4px solid var(--skipped)}.card.skipped .card-value{color:var(--skipped)}
                .card.total{border-left:4px solid var(--info)}.card.total .card-value{color:var(--info)}
                .card.coverage{border-left:4px solid var(--passed)}.card.coverage .card-value{color:var(--passed)}
                .section-label{font-size:.75rem;text-transform:uppercase;letter-spacing:.08em;color:var(--muted);font-weight:600;margin:0 0 .5rem;padding-bottom:.25rem;border-bottom:1px solid var(--border)}
                .chart-section{display:grid;grid-template-columns:1fr 1fr;gap:2rem;margin-bottom:2rem;background:var(--card);padding:1.5rem;border-radius:.75rem;box-shadow:0 1px 3px rgba(0,0,0,.1)}
                @media(max-width:768px){.chart-section{grid-template-columns:1fr} }
                .coverage-chart-section{margin-bottom:2rem;background:var(--card);padding:1.5rem;border-radius:.75rem;box-shadow:0 1px 3px rgba(0,0,0,.1)}
                .cov-accordion-header{display:flex;align-items:center;gap:.75rem;cursor:pointer;padding:.5rem 0;font-weight:600;font-size:.95rem;user-select:none}
                .cov-accordion-header .toggle-icon{transition:transform .2s;font-size:.75rem;color:var(--muted)}
                .cov-accordion-header.collapsed .toggle-icon{transform:rotate(-90deg)}
                .cov-accordion-body{overflow:hidden;transition:max-height .3s ease}
                .cov-accordion-body.collapsed{max-height:0!important}
                .cov-project-row{display:grid;grid-template-columns:220px 1fr 1fr;gap:1rem;align-items:center;padding:.5rem .75rem;border-bottom:1px solid var(--border);font-size:.85rem}
                .cov-project-row-4col{grid-template-columns:180px 1fr 1fr 1fr}
                .cov-project-row:last-child{border-bottom:none}
                .cov-project-row:hover{background:rgba(148,163,184,.06)}
                .cov-project-name{font-weight:500;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}
                .cov-bar-wrapper{display:flex;align-items:center;gap:.5rem}
                .cov-bar-label{font-size:.7rem;color:var(--muted);min-width:52px;text-transform:uppercase}
                .cov-bar-track{flex:1;height:10px;background:var(--progress-bg);border-radius:5px;overflow:hidden}
                .cov-bar-fill{height:100%;border-radius:5px;transition:width .3s}
                .cov-bar-pct{font-weight:600;font-size:.8rem;min-width:48px;text-align:right}
                .cov-filter-bar{display:flex;gap:.75rem;align-items:center;margin-bottom:.75rem}
                .cov-filter-bar input{padding:.4rem .75rem;border:1px solid var(--border);border-radius:.5rem;background:var(--bg);color:var(--text);font-size:.85rem;flex:1;max-width:300px}
                .cov-filter-bar .cov-count{font-size:.75rem;color:var(--muted)}
                @media(max-width:768px){.cov-project-row{grid-template-columns:1fr;gap:.25rem} }
                .chart-container{max-width:280px;margin:0 auto}
                .chart-title{text-align:center;font-weight:600;margin-bottom:.5rem;font-size:.9rem}
                .env-info{font-size:.875rem}.env-info h3{font-size:1rem;margin-bottom:1rem;border-bottom:1px solid var(--border);padding-bottom:.5rem}
                .env-info dl{display:grid;grid-template-columns:auto 1fr;gap:.5rem 1rem}
                .env-info dt{color:var(--muted);font-size:.75rem;text-transform:uppercase}.env-info dd{font-weight:500}
                .section-header{font-size:1.25rem;font-weight:600;margin:1.5rem 0 1rem;border-bottom:2px solid var(--border);padding-bottom:.5rem}
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
                .progress-fill.high{background:var(--coverage-high)}
                .progress-fill.med{background:var(--coverage-med)}
                .progress-fill.low{background:var(--coverage-low)}
                .badge{padding:.25rem .75rem;border-radius:9999px;font-weight:600;font-size:.7rem;text-transform:uppercase}
                .badge-passed{background:var(--badge-passed-bg);color:var(--badge-passed-text)}
                .badge-failed{background:var(--badge-failed-bg);color:var(--badge-failed-text)}
                .badge-skipped{background:var(--badge-skipped-bg);color:var(--badge-skipped-text)}
                .badge-info{background:var(--badge-info-bg);color:var(--badge-info-text)}
                .data-table{width:100%;border-collapse:collapse;background:var(--card);border-radius:.75rem;overflow:hidden;box-shadow:0 1px 3px rgba(0,0,0,.1)}
                .data-table th{padding:.5rem .75rem;text-align:left;font-size:.7rem;text-transform:uppercase;color:var(--muted);border-bottom:1px solid var(--border);background:var(--table-header-bg)}
                .data-table td{padding:.5rem .75rem;border-bottom:1px solid var(--border);font-size:.85rem}
                .data-table tr:last-child td{border-bottom:none}
                .data-table th.right,.data-table td.right{text-align:right}
                .data-table th.center,.data-table td.center{text-align:center}
                .mono{font-family:'Cascadia Code','Fira Code',monospace;font-size:.8rem}
                .coverage-bar{display:inline-flex;align-items:center;gap:.5rem}
                .coverage-bar-track{width:80px;height:6px;background:var(--progress-bg);border-radius:3px;overflow:hidden;display:inline-block}
                .coverage-bar-fill{height:100%;border-radius:3px}
                .coverage-pct{font-weight:600;font-size:.8rem;min-width:3.5rem;text-align:right;display:inline-block}
                .success-banner{background:#065f46;border:2px solid #10b981;border-radius:.75rem;padding:2rem;text-align:center;margin-bottom:2rem}
                .success-banner h2{color:#6ee7b7;font-size:1.5rem;margin-bottom:.5rem}
                .success-banner p{color:#a7f3d0}
                .tab-container{margin-bottom:1rem}
                .tab-buttons{display:flex;gap:.25rem;border-bottom:2px solid var(--border);margin-bottom:1rem}
                .tab-btn{padding:.5rem 1rem;border:none;background:none;color:var(--muted);cursor:pointer;font-size:.85rem;font-weight:500;border-bottom:2px solid transparent;margin-bottom:-2px;transition:all .2s}
                .tab-btn:hover{color:var(--text)}
                .tab-btn.active{color:var(--info);border-bottom-color:var(--info)}
                .tab-panel{display:none}
                .tab-panel.active{display:block}
                .uncovered-lines{font-family:monospace;font-size:.75rem;color:var(--failed);word-break:break-all}
                .filter-bar{display:flex;gap:1rem;margin-bottom:1rem;align-items:center;flex-wrap:wrap}
                .filter-bar input{padding:.4rem .75rem;border:1px solid var(--border);border-radius:.5rem;background:var(--card);color:var(--text);font-size:.85rem;flex:1;min-width:200px}
                .filter-bar select{padding:.4rem .75rem;border:1px solid var(--border);border-radius:.5rem;background:var(--card);color:var(--text);font-size:.85rem}
                .footer{text-align:center;padding:2rem;color:var(--muted);font-size:.75rem;border-top:1px solid var(--border);margin-top:2rem}
                .slowest-table .duration-bar{display:inline-block;height:6px;background:var(--info);border-radius:3px;min-width:2px}
                .sonar-badge{display:inline-flex;align-items:center;gap:.25rem;padding:.15rem .5rem;border-radius:9999px;font-size:.65rem;font-weight:600;text-transform:uppercase;white-space:nowrap}
                .sonar-excluded{background:#7c3aed20;color:#a78bfa;border:1px solid #7c3aed40}
                .sonar-coverage-excluded{background:#f59e0b20;color:#fbbf24;border:1px solid #f59e0b40}
                .sonar-cpd-excluded{background:#6366f120;color:#818cf8;border:1px solid #6366f140}
                .light-theme .sonar-excluded{background:#ede9fe;color:#6d28d9;border-color:#c4b5fd}
                .light-theme .sonar-coverage-excluded{background:#fef3c7;color:#92400e;border-color:#fcd34d}
                .light-theme .sonar-cpd-excluded{background:#e0e7ff;color:#4338ca;border-color:#a5b4fc}
                .sonar-legend{display:flex;gap:1rem;flex-wrap:wrap;margin-bottom:1rem;padding:.75rem 1rem;background:var(--card);border-radius:.5rem;border:1px solid var(--border);font-size:.8rem;align-items:center}
                .sonar-legend-title{font-weight:600;color:var(--muted);font-size:.75rem;text-transform:uppercase}
                .sonar-info{margin-bottom:1.5rem;padding:1rem 1.25rem;background:var(--card);border-radius:.75rem;border:1px solid var(--border);font-size:.85rem;color:var(--muted)}
                .sonar-info summary{cursor:pointer;font-weight:600;color:var(--text);font-size:.9rem}
                .sonar-info ul{margin:.75rem 0 0 1.5rem;list-style:disc}
                .sonar-info li{margin-bottom:.25rem}
                .sonar-info code{font-family:monospace;font-size:.8rem;background:var(--border);padding:.1rem .4rem;border-radius:.25rem}
                .row-sonar-excluded{opacity:.7}
                .branch-toggle{cursor:pointer;user-select:none}
                .branch-toggle:hover{filter:brightness(1.2)}
                .branch-detail-row td{padding:.25rem .75rem!important;border-bottom:none!important}
                .branch-detail{font-size:.8rem;color:var(--muted);padding:.25rem .5rem}
                .branch-detail-list{display:flex;flex-wrap:wrap;gap:.5rem;padding:.25rem 0}
                .branch-chip{display:inline-flex;align-items:center;gap:.35rem;padding:.2rem .6rem;border-radius:.375rem;font-size:.75rem;font-family:monospace;background:var(--badge-failed-bg);color:var(--badge-failed-text);border:1px solid var(--failed)}
                .branch-chip.partial{background:var(--badge-skipped-bg);color:var(--badge-skipped-text);border-color:var(--skipped)}
                .branch-detail-row.collapsed{display:none}
                @media print{body{background:#fff;font-size:12px}.container{max-width:none;padding:1rem}.card,.project-group,.chart-section,.coverage-chart-section{box-shadow:none;border:1px solid var(--border)}.project-group.collapsed .project-content{max-height:none}.project-toggle{display:none} }
            </style>
        </head>
        <body>
        <div class="container">
            <header class="header">
                <button class="theme-toggle" onclick="toggleTheme()" title="Alternar tema claro/escuro">
                    <svg class="icon-sun" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor"><circle cx="12" cy="12" r="4"/><path stroke-linecap="round" d="M12 2v2m0 16v2M4 12H2m20 0h-2m-2.05-6.95 1.41-1.41M4.64 19.36l1.41-1.41m0-11.9L4.64 4.64m14.72 14.72-1.41-1.41"/></svg>
                    <svg class="icon-moon" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z"/></svg>
                </button>
                <h1>Bedrock - Relatório de Testes de Unidade</h1>
                <p class="subtitle">Gerado em: {{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}} UTC</p>
            </header>

            <section class="cards">
                <div class="card total"><div class="card-value">{{totalTests}}</div><div class="card-label">Total de Testes</div></div>
                <div class="card passed"><div class="card-value">{{totalPassed}}</div><div class="card-label">Passaram</div></div>
                <div class="card failed"><div class="card-value">{{totalFailed}}</div><div class="card-label">Falharam</div></div>
                <div class="card skipped"><div class="card-value">{{totalSkipped}}</div><div class="card-label">Ignorados</div></div>
                <div class="card coverage"><div class="card-value">{{overallLineRate:F1}}%</div><div class="card-label">Cobertura (Linhas)</div></div>
                <div class="card coverage"><div class="card-value">{{overallBranchRate:F1}}%</div><div class="card-label">Cobertura (Branches)</div></div>
            </section>
        """);

    if (hasMutationData)
    {
        var mutCardColor = overallMutationScore >= 100 ? "var(--passed)" : overallMutationScore >= 80 ? "var(--skipped)" : "var(--failed)";
        sb.Append($"""
            <p class="section-label">Testes de Mutação</p>
            <section class="cards">
                <div class="card" style="border-left:4px solid {mutCardColor}"><div class="card-value" style="color:{mutCardColor}">{overallMutationScore:F1}%</div><div class="card-label">Mutação</div></div>
                <div class="card" style="border-left:4px solid var(--info)"><div class="card-value" style="color:var(--info)">{totalMutationKilled}</div><div class="card-label">Mutantes Mortos</div></div>
                <div class="card" style="border-left:4px solid var(--muted)"><div class="card-value" style="color:var(--muted)">{totalMutationTotal}</div><div class="card-label">Total Detectáveis</div></div>
                <div class="card" style="border-left:4px solid var(--border)"><div class="card-value" style="color:var(--muted)">{projects.Sum(p => p.MutationIgnored)}</div><div class="card-label">Ignorados</div></div>
            </section>
        """);
    }

    if (totalFailed == 0 && totalTests > 0)
    {
        sb.Append($"""
            <div class="success-banner">
                <h2>Todos os testes passaram com sucesso</h2>
                <p>{totalTests} testes executados — {overallLineRate:F1}% de cobertura de linhas — {overallBranchRate:F1}% de cobertura de branches.</p>
            </div>
        """);
    }

    sb.Append($$"""
            <section class="chart-section">
                <div>
                    <div class="chart-title">Resultado dos Testes</div>
                    <div class="chart-container"><canvas id="testChart"></canvas></div>
                </div>
                <div class="env-info">
                    <h3>Ambiente</h3>
                    <dl>
                        <dt>Máquina</dt><dd>{{WebUtility.HtmlEncode(Environment.MachineName)}}</dd>
                        <dt>SO</dt><dd>{{WebUtility.HtmlEncode(Environment.OSVersion.ToString())}}</dd>
                        <dt>.NET</dt><dd>{{WebUtility.HtmlEncode(Environment.Version.ToString())}}</dd>
                        <dt>Branch</dt><dd>{{WebUtility.HtmlEncode(branch)}}</dd>
                        <dt>Commit</dt><dd>{{WebUtility.HtmlEncode(commit)}}</dd>
                        <dt>Projetos</dt><dd>{{projects.Count}}</dd>
                        <dt>Duração</dt><dd>{{FormatDuration(totalDuration)}}</dd>
                    </dl>
                </div>
            </section>

            <section class="coverage-chart-section">
                <div class="cov-accordion-header" onclick="this.classList.toggle('collapsed');this.nextElementSibling.classList.toggle('collapsed')">
                    <span class="toggle-icon">▼</span>
                    <span>Cobertura por Projeto</span>
                    <span style="font-weight:400;font-size:.8rem;color:var(--muted)">{{projects.Count}} projetos</span>
                </div>
                <div class="cov-accordion-body" style="max-height:{{projects.Count * 52 + 60}}px">
                    <div class="cov-filter-bar">
                        <input type="text" placeholder="Filtrar projetos..." oninput="filterCoverageProjects(this)">
                        <span class="cov-count" id="covCount">{{projects.Count}} de {{projects.Count}}</span>
                    </div>
                    <div id="covProjectList">
        """);

    foreach (var p in projects)
    {
        var linePct = p.LineRate * 100;
        var branchPct = p.BranchRate * 100;
        var lineColor = linePct >= 90 ? "var(--coverage-high)" : linePct >= 70 ? "var(--coverage-med)" : "var(--coverage-low)";
        var branchColor = branchPct >= 90 ? "var(--coverage-high)" : branchPct >= 70 ? "var(--coverage-med)" : "var(--coverage-low)";

        sb.Append($"""
                        <div class="cov-project-row{(hasMutationData ? " cov-project-row-4col" : "")}" data-project="{WebUtility.HtmlEncode(p.Name.ToLowerInvariant())}">
                            <div class="cov-project-name" title="{WebUtility.HtmlEncode(p.Name)}">{WebUtility.HtmlEncode(p.Name)}</div>
                            <div class="cov-bar-wrapper">
                                <span class="cov-bar-label">Linhas</span>
                                <div class="cov-bar-track"><div class="cov-bar-fill" style="width:{linePct:F0}%;background:{lineColor}"></div></div>
                                <span class="cov-bar-pct" style="color:{lineColor}">{linePct:F1}%</span>
                            </div>
                            <div class="cov-bar-wrapper">
                                <span class="cov-bar-label">Branches</span>
                                <div class="cov-bar-track"><div class="cov-bar-fill" style="width:{branchPct:F0}%;background:{branchColor}"></div></div>
                                <span class="cov-bar-pct" style="color:{branchColor}">{branchPct:F1}%</span>
                            </div>
        """);

        if (hasMutationData)
        {
            var mutPct = p.MutationScore;
            var mutColor = mutPct >= 100 ? "var(--coverage-high)" : mutPct >= 80 ? "var(--coverage-med)" : "var(--coverage-low)";
            sb.Append($"""
                            <div class="cov-bar-wrapper">
                                <span class="cov-bar-label">Mutação</span>
                                <div class="cov-bar-track"><div class="cov-bar-fill" style="width:{mutPct:F0}%;background:{mutColor}"></div></div>
                                <span class="cov-bar-pct" style="color:{mutColor}">{mutPct:F1}%</span>
                            </div>
            """);
        }

        sb.Append("</div>");
    }

    sb.Append("""
                    </div>
                </div>
            </section>
        """);

    // ─── Sonar exclusions info ────────────────────────────────────────────

    if (sonarExclusions.HasAny)
    {
        var excludedCount = projects.Sum(p => p.Classes.Count(c => c.SonarStatus != SonarClassStatus.Analyzed));
        var totalClasses = projects.Sum(p => p.Classes.Count);

        sb.Append($"""
            <details class="sonar-info">
                <summary>SonarCloud — {excludedCount} de {totalClasses} classes com exclusões configuradas</summary>
                <p style="margin-top:.75rem">As seguintes exclusões estão configuradas no CI (<code>.github/workflows/ci.yml</code>) e afetam a análise do SonarCloud:</p>
        """);

        if (sonarExclusions.Exclusions.Count > 0)
        {
            sb.Append("""<p style="margin-top:.5rem;font-weight:600;color:var(--text)">Exclusões de análise (sonar.exclusions):</p><ul>""");
            foreach (var p in sonarExclusions.Exclusions)
                sb.Append($"<li><code>{WebUtility.HtmlEncode(p)}</code></li>");
            sb.Append("</ul>");
        }

        if (sonarExclusions.CoverageExclusions.Count > 0)
        {
            sb.Append("""<p style="margin-top:.5rem;font-weight:600;color:var(--text)">Exclusões de cobertura (sonar.coverage.exclusions):</p><ul>""");
            foreach (var p in sonarExclusions.CoverageExclusions)
                sb.Append($"<li><code>{WebUtility.HtmlEncode(p)}</code></li>");
            sb.Append("</ul>");
        }

        if (sonarExclusions.CpdExclusions.Count > 0)
        {
            sb.Append("""<p style="margin-top:.5rem;font-weight:600;color:var(--text)">Exclusões de duplicação (sonar.cpd.exclusions):</p><ul>""");
            foreach (var p in sonarExclusions.CpdExclusions)
                sb.Append($"<li><code>{WebUtility.HtmlEncode(p)}</code></li>");
            sb.Append("</ul>");
        }

        sb.Append("""
                <div class="sonar-legend" style="margin-top:1rem">
                    <span class="sonar-legend-title">Legenda:</span>
                    <span class="sonar-badge sonar-excluded">Excluído do Sonar</span> <span>Arquivo não analisado pelo SonarCloud</span>
                    <span class="sonar-badge sonar-coverage-excluded">Cobertura ignorada</span> <span>Cobertura não contabilizada no Sonar</span>
                    <span class="sonar-badge sonar-cpd-excluded">CPD ignorado</span> <span>Duplicação ignorada no Sonar</span>
                </div>
            </details>
        """);
    }

    // ─── Summary table per project ─────────────────────────────────────────

    sb.Append("""
        <section>
            <h2 class="section-header">Sumário por Projeto</h2>
            <table class="data-table">
                <thead><tr>
                    <th>Projeto</th>
                    <th class="center">Testes</th>
                    <th class="center">Passou</th>
                    <th class="center">Falhou</th>
                    <th class="center">Ignorados</th>
                    <th class="center">Duração</th>
                    <th class="center">Linhas</th>
                    <th class="center">Branches</th>
                </tr></thead>
                <tbody>
    """);

    foreach (var p in projects)
    {
        var linePct = p.LineRate * 100;
        var branchPct = p.BranchRate * 100;
        sb.Append($"""
                    <tr>
                        <td><strong>{WebUtility.HtmlEncode(p.Name)}</strong></td>
                        <td class="center">{p.TotalTests}</td>
                        <td class="center" style="color:var(--passed)">{p.Passed}</td>
                        <td class="center" style="color:{(p.Failed > 0 ? "var(--failed)" : "var(--muted)")}">{p.Failed}</td>
                        <td class="center" style="color:{(p.Skipped > 0 ? "var(--skipped)" : "var(--muted)")}">{p.Skipped}</td>
                        <td class="center mono">{FormatDuration(p.Duration)}</td>
                        <td class="center">{CoverageBarHtml(linePct)}</td>
                        <td class="center">{CoverageBarHtml(branchPct)}</td>
                    </tr>
        """);
    }

    sb.Append("</tbody></table></section>");

    // ─── Detailed per-project sections ─────────────────────────────────────

    sb.Append("""<section><h2 class="section-header">Detalhes por Projeto</h2>""");

    foreach (var p in projects)
    {
        var pPassRate = p.TotalTests > 0 ? p.Passed * 100.0 / p.TotalTests : 100.0;
        var progressClass = p.Failed > 0 ? "low" : "high";

        sb.Append($"""
            <article class="project-group">
                <div class="project-header" onclick="this.closest('.project-group').classList.toggle('collapsed')">
                    <span class="project-toggle">▼</span>
                    <span class="project-name">{WebUtility.HtmlEncode(p.Name)}</span>
                    <div class="project-stats">
                        <span class="badge badge-passed">{p.Passed} passed</span>
        """);

        if (p.Failed > 0)
            sb.Append($"""<span class="badge badge-failed">{p.Failed} failed</span>""");
        if (p.Skipped > 0)
            sb.Append($"""<span class="badge badge-skipped">{p.Skipped} skipped</span>""");

        sb.Append($"""
                        <span class="badge badge-info">{p.LineRate * 100:F1}% lines</span>
                        <div class="progress-bar"><div class="progress-fill {progressClass}" style="width:{pPassRate:F0}%"></div></div>
                    </div>
                </div>
                <div class="project-content">
                    <div class="tab-container" data-tabs>
                        <div class="tab-buttons">
                            <button class="tab-btn active" data-tab="coverage">Cobertura</button>
                            <button class="tab-btn" data-tab="tests">Testes</button>
                            <button class="tab-btn" data-tab="slowest">Mais Lentos</button>
                        </div>
        """);

        // ── Coverage tab ───────────────────────────────────────────────────
        var hasSonarColumn = sonarExclusions.HasAny;
        var pLinePct = p.LineRate * 100;
        var pBranchPct = p.BranchRate * 100;
        var pLineColor = pLinePct >= 90 ? "var(--coverage-high)" : pLinePct >= 70 ? "var(--coverage-med)" : "var(--coverage-low)";
        var pBranchColor = pBranchPct >= 90 ? "var(--coverage-high)" : pBranchPct >= 70 ? "var(--coverage-med)" : "var(--coverage-low)";
        var gridCols = p.HasMutationData ? "1fr 1fr 1fr" : "1fr 1fr";

        sb.Append($"""
                        <div class="tab-panel active" data-panel="coverage">
                            <div style="display:grid;grid-template-columns:{gridCols};gap:1rem;margin-bottom:1rem;padding:.75rem 1rem;background:var(--bg);border-radius:.5rem">
                                <div class="cov-bar-wrapper">
                                    <span class="cov-bar-label">Linhas</span>
                                    <div class="cov-bar-track"><div class="cov-bar-fill" style="width:{pLinePct:F0}%;background:{pLineColor}"></div></div>
                                    <span class="cov-bar-pct" style="color:{pLineColor}">{pLinePct:F1}%</span>
                                    <span style="font-size:.7rem;color:var(--muted)">({p.LinesCovered}/{p.LinesValid})</span>
                                </div>
                                <div class="cov-bar-wrapper">
                                    <span class="cov-bar-label">Branches</span>
                                    <div class="cov-bar-track"><div class="cov-bar-fill" style="width:{pBranchPct:F0}%;background:{pBranchColor}"></div></div>
                                    <span class="cov-bar-pct" style="color:{pBranchColor}">{pBranchPct:F1}%</span>
                                    <span style="font-size:.7rem;color:var(--muted)">({p.BranchesCovered}/{p.BranchesValid})</span>
                                </div>
        """);

        if (p.HasMutationData)
        {
            var pMutPct = p.MutationScore;
            var pMutColor = pMutPct >= 100 ? "var(--coverage-high)" : pMutPct >= 80 ? "var(--coverage-med)" : "var(--coverage-low)";
            sb.Append($"""
                                <div class="cov-bar-wrapper">
                                    <span class="cov-bar-label">Mutação</span>
                                    <div class="cov-bar-track"><div class="cov-bar-fill" style="width:{pMutPct:F0}%;background:{pMutColor}"></div></div>
                                    <span class="cov-bar-pct" style="color:{pMutColor}">{pMutPct:F1}%</span>
                                    <span style="font-size:.7rem;color:var(--muted)">({p.MutationKilled}/{p.MutationTotal})</span>
                                </div>
            """);
        }

        sb.Append("</div>");

        sb.Append($"""
                            <table class="data-table">
                                <thead><tr>
                                    <th>Classe</th>
                                    <th>Arquivo</th>
                                    <th class="center">Linhas</th>
                                    <th class="center">Branches</th>
                                    <th class="center">Complexidade</th>
                                    <th>Linhas Não Cobertas</th>
        """);

        if (hasSonarColumn)
            sb.Append("""<th class="center">Sonar</th>""");

        sb.Append("""
                                </tr></thead>
                                <tbody>
        """);

        foreach (var cls in p.Classes.OrderBy(c => c.LineRate).ThenBy(c => c.FullName))
        {
            var uncovered = cls.UncoveredLines.Count > 0
                ? string.Join(", ", cls.UncoveredLines.Take(20)) + (cls.UncoveredLines.Count > 20 ? $" ... (+{cls.UncoveredLines.Count - 20})" : "")
                : "—";

            var rowClass = cls.SonarStatus == SonarClassStatus.Excluded ? " class=\"row-sonar-excluded\"" : "";

            var hasBranches = cls.UncoveredBranches.Count > 0;
            var branchCellHtml = hasBranches
                ? $"""<span class="branch-toggle" onclick="this.closest('tr').nextElementSibling.classList.toggle('collapsed')" title="Clique para ver branches">{CoverageBarHtml(cls.BranchRate * 100)} <span style="font-size:.7rem">▶</span></span>"""
                : CoverageBarHtml(cls.BranchRate * 100);

            sb.Append($"""
                                    <tr{rowClass}>
                                        <td class="mono">{WebUtility.HtmlEncode(ExtractSimpleName(cls.FullName))}</td>
                                        <td class="mono" style="color:var(--muted);font-size:.75rem">{WebUtility.HtmlEncode(cls.FileName)}</td>
                                        <td class="center">{CoverageBarHtml(cls.LineRate * 100)}</td>
                                        <td class="center">{branchCellHtml}</td>
                                        <td class="center">{cls.Complexity}</td>
                                        <td class="uncovered-lines">{WebUtility.HtmlEncode(uncovered)}</td>
            """);

            if (hasSonarColumn)
            {
                var sonarBadge = cls.SonarStatus switch
                {
                    SonarClassStatus.Excluded => """<span class="sonar-badge sonar-excluded">Excluído</span>""",
                    SonarClassStatus.CoverageExcluded => """<span class="sonar-badge sonar-coverage-excluded">Cobertura</span>""",
                    SonarClassStatus.CpdExcluded => """<span class="sonar-badge sonar-cpd-excluded">CPD</span>""",
                    _ => """<span style="color:var(--passed);font-size:.8rem">✓</span>"""
                };
                sb.Append($"""<td class="center">{sonarBadge}</td>""");
            }

            sb.Append("</tr>");

            // Expandable branch detail row
            if (hasBranches)
            {
                var colSpan = hasSonarColumn ? 7 : 6;
                sb.Append($"""<tr class="branch-detail-row collapsed"><td colspan="{colSpan}"><div class="branch-detail"><div class="branch-detail-list">""");
                foreach (var br in cls.UncoveredBranches)
                {
                    var chipClass = br.Covered > 0 ? "branch-chip partial" : "branch-chip";
                    sb.Append($"""<span class="{chipClass}">L{br.Line} {br.ConditionCoverage}</span>""");
                }
                sb.Append("</div></div></td></tr>");
            }
        }

        sb.Append("</tbody></table></div>");

        // ── Tests tab ──────────────────────────────────────────────────────
        sb.Append("""
                        <div class="tab-panel" data-panel="tests">
                            <div class="filter-bar">
                                <input type="text" placeholder="Filtrar testes..." oninput="filterTests(this)">
                                <select onchange="filterOutcome(this)">
                                    <option value="">Todos</option>
                                    <option value="Passed">Passou</option>
                                    <option value="Failed">Falhou</option>
                                </select>
                            </div>
                            <table class="data-table test-list">
                                <thead><tr>
                                    <th style="width:2rem">Status</th>
                                    <th>Teste</th>
                                    <th class="right">Duração</th>
                                </tr></thead>
                                <tbody>
        """);

        var testsByClass = p.Tests.GroupBy(t => t.ClassName).OrderBy(g => g.Key);
        foreach (var classGroup in testsByClass)
        {
            var className = ExtractSimpleName(classGroup.Key);
            sb.Append($"""
                                    <tr class="class-separator"><td colspan="3" style="font-weight:600;font-size:.8rem;color:var(--muted);padding-top:1rem">{WebUtility.HtmlEncode(className)}</td></tr>
            """);

            foreach (var test in classGroup.OrderBy(t => t.Name))
            {
                var icon = test.Outcome == "Passed" ? "\u2705" : test.Outcome == "Failed" ? "\u274C" : "\u23ED";
                var simpleName = ExtractTestName(test.Name);
                sb.Append($"""
                                    <tr data-outcome="{test.Outcome}" data-search="{WebUtility.HtmlEncode(test.Name.ToLowerInvariant())}">
                                        <td class="center">{icon}</td>
                                        <td>{WebUtility.HtmlEncode(simpleName)}</td>
                                        <td class="right mono">{FormatDuration(test.Duration)}</td>
                                    </tr>
                """);

                if (test.ErrorMessage != null)
                {
                    sb.Append($"""
                                    <tr data-outcome="{test.Outcome}" data-search="{WebUtility.HtmlEncode(test.Name.ToLowerInvariant())}">
                                        <td></td>
                                        <td colspan="2" style="color:var(--failed);font-size:.8rem;font-style:italic;padding-left:1rem">{WebUtility.HtmlEncode(Truncate(test.ErrorMessage, 200))}</td>
                                    </tr>
                    """);
                }
            }
        }

        sb.Append("</tbody></table></div>");

        // ── Slowest tab ────────────────────────────────────────────────────
        var slowest = p.Tests.OrderByDescending(t => t.Duration).Take(20).ToList();
        var maxDuration = slowest.Count > 0 ? slowest[0].Duration.TotalMilliseconds : 1;
        if (maxDuration < 0.001) maxDuration = 1;

        sb.Append("""
                        <div class="tab-panel" data-panel="slowest">
                            <table class="data-table slowest-table">
                                <thead><tr>
                                    <th style="width:2rem">#</th>
                                    <th>Teste</th>
                                    <th class="right">Duração</th>
                                    <th style="width:150px"></th>
                                </tr></thead>
                                <tbody>
        """);

        for (var i = 0; i < slowest.Count; i++)
        {
            var t = slowest[i];
            var barWidth = t.Duration.TotalMilliseconds / maxDuration * 100;
            sb.Append($"""
                                    <tr>
                                        <td class="center mono">{i + 1}</td>
                                        <td>{WebUtility.HtmlEncode(ExtractTestName(t.Name))}</td>
                                        <td class="right mono">{FormatDuration(t.Duration)}</td>
                                        <td><span class="duration-bar" style="width:{barWidth:F0}%"></span></td>
                                    </tr>
            """);
        }

        sb.Append("</tbody></table></div>");

        // Close tabs & project
        sb.Append("</div></div></article>");
    }

    sb.Append("</section>");

    // ─── Footer & Scripts ──────────────────────────────────────────────────

    sb.Append($"""
            <footer class="footer">
                <p><strong>Bedrock Framework</strong> - Relatório de Testes de Unidade</p>
                <p>Projetos: {projects.Count} | Testes: {totalTests} | Cobertura: {overallLineRate:F1}% linhas, {overallBranchRate:F1}% branches</p>
            </footer>
        </div>
        <script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.1/dist/chart.umd.min.js"></script>
        <script>
    """);

    // Test result doughnut chart
    sb.Append($"var testChart=new Chart(document.getElementById('testChart'),{{type:'doughnut',data:{{labels:['Passou','Falhou','Ignorados'],datasets:[{{data:[{totalPassed},{totalFailed},{totalSkipped}],backgroundColor:['#10b981','#ef4444','#f59e0b'],borderWidth:0}}]}},options:{{responsive:true,plugins:{{legend:{{position:'bottom',labels:{{color:getComputedStyle(document.body).getPropertyValue('--chart-legend').trim()}}}}}},cutout:'60%'}}}});");

    sb.Append("""

        // Tab functionality
        document.querySelectorAll('[data-tabs]').forEach(container=>{
            container.querySelectorAll('.tab-btn').forEach(btn=>{
                btn.addEventListener('click',()=>{
                    container.querySelectorAll('.tab-btn').forEach(b=>b.classList.remove('active'));
                    container.querySelectorAll('.tab-panel').forEach(p=>p.classList.remove('active'));
                    btn.classList.add('active');
                    container.querySelector('[data-panel="'+btn.dataset.tab+'"]').classList.add('active');
                });
            });
        });

        // Test filtering
        function filterTests(input){
            var table=input.closest('.tab-panel').querySelector('.test-list tbody');
            var term=input.value.toLowerCase();
            table.querySelectorAll('tr').forEach(r=>{
                var s=r.dataset.search||'';
                r.style.display=(!term||s.includes(term))?'':'none';
            });
        }
        function filterOutcome(select){
            var table=select.closest('.tab-panel').querySelector('.test-list tbody');
            var val=select.value;
            table.querySelectorAll('tr').forEach(r=>{
                var o=r.dataset.outcome;
                if(!o){r.style.display='';return;}
                r.style.display=(!val||o===val)?'':'none';
            });
        }

        // Theme
        function toggleTheme(){document.body.classList.toggle('light-theme');localStorage.setItem('theme',document.body.classList.contains('light-theme')?'light':'dark');updateCharts();}
        function updateCharts(){
            var c=getComputedStyle(document.body).getPropertyValue('--chart-legend').trim();
            testChart.options.plugins.legend.labels.color=c;testChart.update();
        }
        function filterCoverageProjects(input){
            var term=input.value.toLowerCase();
            var rows=document.querySelectorAll('#covProjectList .cov-project-row');
            var visible=0;
            rows.forEach(function(r){var show=!term||r.dataset.project.includes(term);r.style.display=show?'':'none';if(show)visible++;});
            document.getElementById('covCount').textContent=visible+' de '+rows.length;
        }
        (function(){if(localStorage.getItem('theme')==='light')document.body.classList.add('light-theme');updateCharts();})();
        </script>
        </body>
        </html>
    """);

    return sb.ToString();
}

static string CoverageBarHtml(double pct)
{
    var color = pct >= 90 ? "var(--coverage-high)" : pct >= 70 ? "var(--coverage-med)" : "var(--coverage-low)";
    return $"""<span class="coverage-bar"><span class="coverage-bar-track"><span class="coverage-bar-fill" style="width:{pct:F0}%;background:{color}"></span></span><span class="coverage-pct" style="color:{color}">{pct:F1}%</span></span>""";
}

static string FormatDuration(TimeSpan d) =>
    d.TotalSeconds >= 1 ? $"{d.TotalSeconds:F2}s" :
    d.TotalMilliseconds >= 1 ? $"{d.TotalMilliseconds:F1}ms" :
    $"{d.TotalMicroseconds:F0}μs";

static string ExtractSimpleName(string fullName)
{
    var lastDot = fullName.LastIndexOf('.');
    return lastDot >= 0 ? fullName[(lastDot + 1)..] : fullName;
}

static string ExtractTestName(string fullTestName)
{
    // Remove namespace prefix: "Bedrock.UnitTests.BuildingBlocks.Core.Foo.Bar" -> "Bar"
    var lastDot = fullTestName.LastIndexOf('.');
    if (lastDot >= 0)
    {
        var simple = fullTestName[(lastDot + 1)..];
        // But keep inline data: "Method(param: value)" -> look for the method part
        var parenIdx = fullTestName.IndexOf('(');
        if (parenIdx >= 0 && parenIdx < lastDot)
        {
            // The paren is before the last dot — find the method name
            var methodStart = fullTestName[..parenIdx].LastIndexOf('.') + 1;
            return fullTestName[methodStart..];
        }
        return simple;
    }
    return fullTestName;
}

static string Truncate(string s, int max) =>
    s.Length <= max ? s : s[..max] + "...";

// ─── Data Models ───────────────────────────────────────────────────────────

internal sealed class ProjectData
{
    public string Name { get; set; } = "";
    public double LineRate { get; set; }
    public double BranchRate { get; set; }
    public int LinesCovered { get; set; }
    public int LinesValid { get; set; }
    public int BranchesCovered { get; set; }
    public int BranchesValid { get; set; }
    public int TotalTests { get; set; }
    public int Passed { get; set; }
    public int Failed { get; set; }
    public int Skipped { get; set; }
    public TimeSpan Duration { get; set; }
    public List<ClassData> Classes { get; } = [];
    public List<TestData> Tests { get; } = [];
    public bool HasMutationData { get; set; }
    public int MutationKilled { get; set; }
    public int MutationTotal { get; set; }
    public int MutationIgnored { get; set; }
    public double MutationScore => MutationTotal > 0 ? MutationKilled * 100.0 / MutationTotal : 0;
}

internal sealed class ClassData
{
    public string FullName { get; set; } = "";
    public string FileName { get; set; } = "";
    public string RelativePath { get; set; } = "";
    public double LineRate { get; set; }
    public double BranchRate { get; set; }
    public int Complexity { get; set; }
    public int TotalLines { get; set; }
    public int CoveredLines { get; set; }
    public List<int> UncoveredLines { get; set; } = [];
    public List<BranchInfo> UncoveredBranches { get; set; } = [];
    public List<MethodData> Methods { get; set; } = [];
    public SonarClassStatus SonarStatus { get; set; } = SonarClassStatus.Analyzed;
}

internal sealed class MethodData
{
    public string Name { get; set; } = "";
    public double LineRate { get; set; }
    public double BranchRate { get; set; }
    public int Lines { get; set; }
    public int CoveredLines { get; set; }
}

internal sealed class BranchInfo
{
    public int Line { get; set; }
    public string ConditionCoverage { get; set; } = "";
    public int Covered { get; set; }
    public int Total { get; set; }
    public List<(int Number, string Coverage)> UncoveredConditions { get; set; } = [];
}

internal sealed class TestData
{
    public string Name { get; set; } = "";
    public string ClassName { get; set; } = "";
    public string Outcome { get; set; } = "";
    public TimeSpan Duration { get; set; }
    public string? ErrorMessage { get; set; }
}

internal enum SonarClassStatus
{
    Analyzed,
    Excluded,
    CoverageExcluded,
    CpdExcluded
}

internal sealed class SonarExclusions
{
    public List<string> Exclusions { get; set; } = [];
    public List<string> CoverageExclusions { get; set; } = [];
    public List<string> CpdExclusions { get; set; } = [];

    public bool HasAny => Exclusions.Count > 0 || CoverageExclusions.Count > 0 || CpdExclusions.Count > 0;

    public SonarClassStatus Classify(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return SonarClassStatus.Analyzed;

        // Normalize path separators
        var path = relativePath.Replace('\\', '/');

        if (MatchesAny(path, Exclusions))
            return SonarClassStatus.Excluded;
        if (MatchesAny(path, CoverageExclusions))
            return SonarClassStatus.CoverageExcluded;
        if (MatchesAny(path, CpdExclusions))
            return SonarClassStatus.CpdExcluded;

        return SonarClassStatus.Analyzed;
    }

    private static bool MatchesAny(string path, List<string> patterns)
    {
        foreach (var pattern in patterns)
        {
            if (MatchGlob(path, pattern))
                return true;
        }
        return false;
    }

    private static bool MatchGlob(string path, string pattern)
    {
        // Convert sonar glob pattern to regex
        // ** matches any path segments, * matches within a segment
        var regexPattern = "^" + Regex.Escape(pattern)
            .Replace(@"\*\*", "§§")  // temp placeholder for **
            .Replace(@"\*", "[^/]*")  // * matches within segment
            .Replace("§§", ".*")     // ** matches across segments
            + "$";

        return Regex.IsMatch(path, regexPattern, RegexOptions.IgnoreCase);
    }
}
