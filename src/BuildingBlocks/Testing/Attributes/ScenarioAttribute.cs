namespace Bedrock.BuildingBlocks.Testing.Attributes;

/// <summary>
/// Marks a test method as a scenario for integration test documentation.
/// This attribute is required for integration tests to be included in the HTML report.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class ScenarioAttribute : Attribute
{
    /// <summary>
    /// Gets the scenario description displayed in the report.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScenarioAttribute"/> class.
    /// </summary>
    /// <param name="description">The scenario description displayed in the report.</param>
    public ScenarioAttribute(string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        Description = description;
    }
}
