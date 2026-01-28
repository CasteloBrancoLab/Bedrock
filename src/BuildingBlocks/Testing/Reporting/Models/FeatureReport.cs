namespace Bedrock.BuildingBlocks.Testing.Reporting.Models;

/// <summary>
/// Represents a feature (test class) in the report.
/// </summary>
public sealed class FeatureReport
{
    /// <summary>
    /// Gets or sets the feature name from the [Feature] attribute.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the feature description from the [Feature] attribute.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the test class name.
    /// </summary>
    public string ClassName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the scenarios (test methods) in this feature.
    /// </summary>
    public List<ScenarioReport> Scenarios { get; set; } = [];

    /// <summary>
    /// Gets the number of passed scenarios.
    /// </summary>
    public int PassedCount => Scenarios.Count(s => s.Status == TestStatus.Passed);

    /// <summary>
    /// Gets the number of failed scenarios.
    /// </summary>
    public int FailedCount => Scenarios.Count(s => s.Status == TestStatus.Failed);

    /// <summary>
    /// Gets the number of skipped scenarios.
    /// </summary>
    public int SkippedCount => Scenarios.Count(s => s.Status == TestStatus.Skipped);
}
