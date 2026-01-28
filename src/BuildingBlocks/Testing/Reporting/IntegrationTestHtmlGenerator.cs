using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text;
using Bedrock.BuildingBlocks.Testing.Reporting.Models;
using Humanizer;

namespace Bedrock.BuildingBlocks.Testing.Reporting;

/// <summary>
/// Generates HTML reports from integration test results using a template-based approach.
/// </summary>
public sealed class IntegrationTestHtmlGenerator
{
    private readonly string _templateContent;

    /// <summary>
    /// Initializes a new instance using the embedded default template.
    /// </summary>
    public IntegrationTestHtmlGenerator()
    {
        _templateContent = LoadEmbeddedTemplate();
    }

    /// <summary>
    /// Initializes a new instance using a custom template file.
    /// </summary>
    /// <param name="templatePath">Path to the HTML template file.</param>
    public IntegrationTestHtmlGenerator(string templatePath)
    {
        _templateContent = File.ReadAllText(templatePath);
    }

    /// <summary>
    /// Generates the HTML report content.
    /// </summary>
    /// <param name="report">The integration test report data.</param>
    /// <returns>The complete HTML content.</returns>
    public string Generate(IntegrationTestReport report)
    {
        return _templateContent
            .Replace("{{GENERATED_AT}}", report.GeneratedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + " UTC")
            .Replace("{{SUMMARY_CARDS}}", GenerateSummaryCards(report))
            .Replace("{{ENVIRONMENT_INFO}}", GenerateEnvironmentInfo(report.Environment))
            .Replace("{{FEATURES}}", GenerateFeatures(report.Features))
            .Replace("{{TOTAL_DURATION}}", FormatDuration(report.TotalDuration))
            .Replace("{{CHARTJS_SCRIPT}}", GetChartJsScript())
            .Replace("{{CHART_DATA}}", GenerateChartData(report));
    }

    private static string GenerateSummaryCards(IntegrationTestReport report)
    {
        var passRate = report.TotalTests > 0
            ? (report.PassedTests * 100.0 / report.TotalTests).ToString("F1", CultureInfo.InvariantCulture)
            : "0.0";

        return $"""
            <div class="card total">
                <div class="card-value">{report.TotalTests}</div>
                <div class="card-label">Total Tests</div>
            </div>
            <div class="card passed">
                <div class="card-value">{report.PassedTests}</div>
                <div class="card-label">Passed</div>
                <div class="card-percent">{passRate}%</div>
            </div>
            <div class="card failed">
                <div class="card-value">{report.FailedTests}</div>
                <div class="card-label">Failed</div>
            </div>
            <div class="card skipped">
                <div class="card-value">{report.SkippedTests}</div>
                <div class="card-label">Skipped</div>
            </div>
            """;
    }

    private static string GenerateEnvironmentInfo(EnvironmentInfo env)
    {
        var sb = new StringBuilder();

        AddEnvItem(sb, "Machine", env.MachineName);
        AddEnvItem(sb, "OS", env.OsVersion);
        AddEnvItem(sb, ".NET Version", env.DotNetVersion);
        AddEnvItem(sb, "User", env.UserName);
        AddEnvItem(sb, "Executed At", env.ExecutionTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + " UTC");

        if (!string.IsNullOrEmpty(env.GitBranch))
            AddEnvItem(sb, "Git Branch", env.GitBranch);

        if (!string.IsNullOrEmpty(env.GitCommitShort))
            AddEnvItem(sb, "Git Commit", env.GitCommitShort);

        return sb.ToString();
    }

    private static void AddEnvItem(StringBuilder sb, string label, string value)
    {
        sb.Append($"""
            <dt>{Encode(label)}</dt>
            <dd>{Encode(value)}</dd>
            """);
    }

    private static string GenerateFeatures(List<FeatureReport> features)
    {
        if (features.Count == 0)
        {
            return """
                <div class="no-results">
                    <div class="no-results-icon">📭</div>
                    <p>No test results found.</p>
                    <p>Make sure your integration tests have [Feature] and [Scenario] attributes.</p>
                </div>
                """;
        }

        var sb = new StringBuilder();

        foreach (var feature in features)
        {
            sb.Append($"""
                <article class="feature">
                    <div class="feature-header">
                        <span class="feature-toggle">▼</span>
                        <span class="feature-name">{Encode(feature.Name)}</span>
                        <div class="feature-stats">
                """);

            if (feature.PassedCount > 0)
                sb.Append($"<span class=\"feature-stat passed\">{feature.PassedCount} passed</span>");
            if (feature.FailedCount > 0)
                sb.Append($"<span class=\"feature-stat failed\">{feature.FailedCount} failed</span>");
            if (feature.SkippedCount > 0)
                sb.Append($"<span class=\"feature-stat skipped\">{feature.SkippedCount} skipped</span>");

            sb.Append("</div></div>");

            if (!string.IsNullOrEmpty(feature.Description))
            {
                sb.Append($"<div class=\"feature-description\">{Encode(feature.Description)}</div>");
            }

            sb.Append("<div class=\"feature-content\">");

            foreach (var scenario in feature.Scenarios)
            {
                GenerateScenario(sb, scenario);
            }

            sb.Append("</div></article>");
        }

        return sb.ToString();
    }

    private static void GenerateScenario(StringBuilder sb, ScenarioReport scenario)
    {
        var statusIcon = scenario.Status switch
        {
            TestStatus.Passed => "✅",
            TestStatus.Failed => "❌",
            TestStatus.Skipped => "⏭️",
            _ => "❓"
        };

        var durationMs = scenario.Duration.TotalMilliseconds;
        var durationStr = durationMs < 1000
            ? $"{durationMs:F0}ms"
            : $"{scenario.Duration.TotalSeconds:F2}s";

        sb.Append($"""
            <div class="scenario">
                <div class="scenario-header">
                    <span class="scenario-status">{statusIcon}</span>
                    <span class="scenario-name">{Encode(scenario.Name)}</span>
                    <span class="scenario-duration">{durationStr}</span>
                </div>
            """);

        // Steps
        if (scenario.Steps.Count > 0)
        {
            sb.Append("<div class=\"steps\">");
            foreach (var step in scenario.Steps)
            {
                var stepClass = step.Type.ToString().ToLowerInvariant();
                sb.Append($"""
                    <div class="step">
                        <span class="step-type {stepClass}">{step.Type}:</span>
                        <span class="step-description">{Encode(step.Description)}</span>
                    </div>
                    """);
            }
            sb.Append("</div>");
        }

        // Error message for failed tests
        if (scenario.Status == TestStatus.Failed && !string.IsNullOrEmpty(scenario.ErrorMessage))
        {
            sb.Append($"""
                <div class="error-box">
                    <strong>Error</strong>
                    <div class="error-message">{Encode(scenario.ErrorMessage)}</div>
                </div>
                """);
        }

        sb.Append("</div>");
    }

    private static string GenerateChartData(IntegrationTestReport report)
    {
        return $"{{\"passed\":{report.PassedTests},\"failed\":{report.FailedTests},\"skipped\":{report.SkippedTests}}}";
    }

    private static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalSeconds < 1)
            return $"{duration.TotalMilliseconds:F0}ms";
        if (duration.TotalMinutes < 1)
            return $"{duration.TotalSeconds:F2}s";
        return duration.Humanize(precision: 2);
    }

    private static string Encode(string value) => WebUtility.HtmlEncode(value);

    private static string GetChartJsScript()
    {
        // Chart.js v4 minified (loaded from embedded resource or CDN fallback)
        // For production, embed the minified script. For now, use CDN with fallback.
        return """
            // Chart.js v4.4.1 - Using CDN with inline fallback
            if (typeof Chart === 'undefined') {
                document.write('<script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.1/dist/chart.umd.min.js"><\/script>');
            }
            """;
    }

    private static string LoadEmbeddedTemplate()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "Bedrock.BuildingBlocks.Testing.Reporting.Templates.integration-report.html";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new InvalidOperationException($"Embedded template resource '{resourceName}' not found.");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
