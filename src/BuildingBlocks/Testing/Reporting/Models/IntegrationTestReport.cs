namespace Bedrock.BuildingBlocks.Testing.Reporting.Models;

/// <summary>
/// Represents the complete integration test report.
/// </summary>
public sealed class IntegrationTestReport
{
    /// <summary>
    /// Gets or sets the environment information.
    /// </summary>
    public EnvironmentInfo Environment { get; set; } = new();

    /// <summary>
    /// Gets or sets the report generation timestamp.
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the total test execution duration.
    /// </summary>
    public TimeSpan TotalDuration { get; set; }

    /// <summary>
    /// Gets or sets the features (test classes) in the report.
    /// </summary>
    public List<FeatureReport> Features { get; set; } = [];

    /// <summary>
    /// Gets the total number of tests.
    /// </summary>
    public int TotalTests => Features.Sum(f => f.Scenarios.Count);

    /// <summary>
    /// Gets the number of passed tests.
    /// </summary>
    public int PassedTests => Features.Sum(f => f.PassedCount);

    /// <summary>
    /// Gets the number of failed tests.
    /// </summary>
    public int FailedTests => Features.Sum(f => f.FailedCount);

    /// <summary>
    /// Gets the number of skipped tests.
    /// </summary>
    public int SkippedTests => Features.Sum(f => f.SkippedCount);

    /// <summary>
    /// Gets the pass rate as a percentage (0-100).
    /// </summary>
    public double PassRate => TotalTests > 0 ? (double)PassedTests / TotalTests * 100 : 0;
}
