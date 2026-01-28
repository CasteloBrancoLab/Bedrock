namespace Bedrock.BuildingBlocks.Testing.Reporting.Models;

/// <summary>
/// Represents a BDD step (Given/When/Then) in a test scenario.
/// </summary>
public sealed class StepReport
{
    /// <summary>
    /// Gets or sets the type of step (Given, When, Then).
    /// </summary>
    public StepType Type { get; set; }

    /// <summary>
    /// Gets or sets the step description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the step was executed.
    /// </summary>
    public DateTime Timestamp { get; set; }
}
