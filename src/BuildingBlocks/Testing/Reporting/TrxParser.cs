using System.Reflection;
using System.Text.Json;
using System.Xml.Linq;
using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Reporting.Models;

namespace Bedrock.BuildingBlocks.Testing.Reporting;

/// <summary>
/// Parses TRX (Visual Studio Test Results) files and extracts test data.
/// </summary>
public sealed class TrxParser
{
    private static readonly XNamespace TrxNamespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";

    /// <summary>
    /// Parses multiple TRX files and aggregates results into a single report.
    /// </summary>
    /// <param name="trxFilePaths">Paths to TRX files.</param>
    /// <param name="testAssemblies">Assemblies containing test classes (for attribute extraction).</param>
    /// <returns>The aggregated integration test report.</returns>
    public IntegrationTestReport Parse(IEnumerable<string> trxFilePaths, IEnumerable<Assembly> testAssemblies)
    {
        var report = new IntegrationTestReport();
        var featuresByClass = new Dictionary<string, FeatureReport>();

        // Build a lookup for [Feature] and [Scenario] attributes
        var attributeLookup = BuildAttributeLookup(testAssemblies);

        foreach (var trxPath in trxFilePaths)
        {
            ParseTrxFile(trxPath, featuresByClass, attributeLookup, report);
        }

        report.Features = featuresByClass.Values
            .OrderBy(f => f.Name)
            .ToList();

        return report;
    }

    private void ParseTrxFile(
        string trxPath,
        Dictionary<string, FeatureReport> featuresByClass,
        AttributeLookup attributeLookup,
        IntegrationTestReport report)
    {
        var doc = XDocument.Load(trxPath);
        var root = doc.Root!;

        // Parse test definitions
        var testDefinitions = root
            .Descendants(TrxNamespace + "UnitTest")
            .ToDictionary(
                ut => ut.Attribute("id")?.Value ?? string.Empty,
                ut => new TestDefinition
                {
                    ClassName = ut.Element(TrxNamespace + "TestMethod")?.Attribute("className")?.Value ?? string.Empty,
                    MethodName = ut.Element(TrxNamespace + "TestMethod")?.Attribute("name")?.Value ?? string.Empty
                });

        // Parse test results
        var results = root.Descendants(TrxNamespace + "UnitTestResult");

        foreach (var result in results)
        {
            var testId = result.Attribute("testId")?.Value ?? string.Empty;
            if (!testDefinitions.TryGetValue(testId, out var definition))
                continue;

            var className = definition.ClassName;
            var methodName = definition.MethodName;

            // Get or create feature report
            if (!featuresByClass.TryGetValue(className, out var feature))
            {
                var featureAttr = attributeLookup.GetFeatureAttribute(className);
                feature = new FeatureReport
                {
                    ClassName = className,
                    Name = featureAttr?.Name ?? ExtractSimpleClassName(className),
                    Description = featureAttr?.Description
                };
                featuresByClass[className] = feature;
            }

            // Parse scenario
            var scenario = ParseScenario(result, methodName, attributeLookup, className);
            feature.Scenarios.Add(scenario);

            // Update total duration
            if (TimeSpan.TryParse(result.Attribute("duration")?.Value, out var duration))
            {
                report.TotalDuration += duration;
            }
        }

        // Parse execution times
        var times = root.Descendants(TrxNamespace + "Times").FirstOrDefault();
        if (times != null)
        {
            if (DateTime.TryParse(times.Attribute("start")?.Value, out var startTime))
            {
                report.Environment.ExecutionTime = startTime.ToUniversalTime();
            }
        }
    }

    private ScenarioReport ParseScenario(
        XElement result,
        string methodName,
        AttributeLookup attributeLookup,
        string className)
    {
        var scenarioAttr = attributeLookup.GetScenarioAttribute(className, methodName);
        var outcome = result.Attribute("outcome")?.Value ?? "Unknown";

        var scenario = new ScenarioReport
        {
            MethodName = methodName,
            Name = scenarioAttr?.Description ?? ConvertMethodNameToDescription(methodName),
            Status = ParseOutcome(outcome),
            Duration = TimeSpan.TryParse(result.Attribute("duration")?.Value, out var d) ? d : TimeSpan.Zero
        };

        // Parse output for steps
        var output = result.Descendants(TrxNamespace + "StdOut").FirstOrDefault()?.Value;
        if (!string.IsNullOrEmpty(output))
        {
            scenario.Steps = ParseSteps(output);
        }

        // Parse error info
        if (scenario.Status == TestStatus.Failed)
        {
            var errorInfo = result.Descendants(TrxNamespace + "ErrorInfo").FirstOrDefault();
            if (errorInfo != null)
            {
                scenario.ErrorMessage = errorInfo.Element(TrxNamespace + "Message")?.Value;
                scenario.StackTrace = errorInfo.Element(TrxNamespace + "StackTrace")?.Value;
            }
        }

        return scenario;
    }

    private List<StepReport> ParseSteps(string output)
    {
        var steps = new List<StepReport>();
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (!trimmedLine.StartsWith(TestBase.StepMarker))
                continue;

            var json = trimmedLine[TestBase.StepMarker.Length..];
            try
            {
                var stepData = JsonSerializer.Deserialize<StepJsonData>(json);
                if (stepData != null)
                {
                    steps.Add(new StepReport
                    {
                        Type = ParseStepType(stepData.Type),
                        Description = stepData.Description,
                        Timestamp = DateTime.TryParse(stepData.Timestamp, out var ts) ? ts : DateTime.UtcNow
                    });
                }
            }
            catch (JsonException)
            {
                // Ignore malformed step data
            }
        }

        return steps;
    }

    private static TestStatus ParseOutcome(string outcome) => outcome.ToLowerInvariant() switch
    {
        "passed" => TestStatus.Passed,
        "failed" => TestStatus.Failed,
        "notexecuted" or "skipped" => TestStatus.Skipped,
        _ => TestStatus.Skipped
    };

    private static StepType ParseStepType(string type) => type.ToLowerInvariant() switch
    {
        "given" => StepType.Given,
        "when" => StepType.When,
        "then" => StepType.Then,
        _ => StepType.Given
    };

    private static string ExtractSimpleClassName(string fullClassName)
    {
        var lastDot = fullClassName.LastIndexOf('.');
        return lastDot >= 0 ? fullClassName[(lastDot + 1)..] : fullClassName;
    }

    private static string ConvertMethodNameToDescription(string methodName)
    {
        // Convert "GetByIdAsync_Should_ReturnEntity_WhenExists" to "Get by id async should return entity when exists"
        var result = new System.Text.StringBuilder();
        var prevWasUpper = false;

        foreach (var c in methodName)
        {
            if (c == '_')
            {
                result.Append(' ');
                prevWasUpper = false;
            }
            else if (char.IsUpper(c) && result.Length > 0 && !prevWasUpper)
            {
                result.Append(' ');
                result.Append(char.ToLower(c));
                prevWasUpper = true;
            }
            else
            {
                result.Append(result.Length == 0 ? c : char.ToLower(c));
                prevWasUpper = char.IsUpper(c);
            }
        }

        return result.ToString();
    }

    private static AttributeLookup BuildAttributeLookup(IEnumerable<Assembly> assemblies)
    {
        var lookup = new AttributeLookup();

        foreach (var assembly in assemblies)
        {
            try
            {
                foreach (var type in assembly.GetTypes())
                {
                    var featureAttr = type.GetCustomAttribute<FeatureAttribute>();
                    if (featureAttr != null)
                    {
                        lookup.Features[type.FullName ?? type.Name] = featureAttr;
                    }

                    foreach (var method in type.GetMethods())
                    {
                        var scenarioAttr = method.GetCustomAttribute<ScenarioAttribute>();
                        if (scenarioAttr != null)
                        {
                            var key = $"{type.FullName}.{method.Name}";
                            lookup.Scenarios[key] = scenarioAttr;
                        }
                    }
                }
            }
            catch
            {
                // Ignore assemblies that can't be scanned
            }
        }

        return lookup;
    }

    private sealed class TestDefinition
    {
        public string ClassName { get; set; } = string.Empty;
        public string MethodName { get; set; } = string.Empty;
    }

    private sealed class StepJsonData
    {
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
    }

    private sealed class AttributeLookup
    {
        public Dictionary<string, FeatureAttribute> Features { get; } = new();
        public Dictionary<string, ScenarioAttribute> Scenarios { get; } = new();

        public FeatureAttribute? GetFeatureAttribute(string className) =>
            Features.TryGetValue(className, out var attr) ? attr : null;

        public ScenarioAttribute? GetScenarioAttribute(string className, string methodName)
        {
            var key = $"{className}.{methodName}";
            return Scenarios.TryGetValue(key, out var attr) ? attr : null;
        }
    }
}
