namespace Bedrock.BuildingBlocks.Testing.Reporting.Models;

/// <summary>
/// Represents a test scenario (test method) in the report.
/// </summary>
public sealed class ScenarioReport
{
    /// <summary>
    /// Gets or sets the scenario name from the [Scenario] attribute.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the test method name.
    /// </summary>
    public string MethodName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the test execution status.
    /// </summary>
    public TestStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the test execution duration.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets the error message if the test failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the stack trace if the test failed.
    /// </summary>
    public string? StackTrace { get; set; }

    /// <summary>
    /// Gets or sets the BDD steps (Given/When/Then) for this scenario.
    /// </summary>
    public List<StepReport> Steps { get; set; } = [];
}
