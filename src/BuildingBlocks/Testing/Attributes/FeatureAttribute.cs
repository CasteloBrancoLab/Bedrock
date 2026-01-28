namespace Bedrock.BuildingBlocks.Testing.Attributes;

/// <summary>
/// Marks a test class as a feature for integration test documentation.
/// This attribute is required for integration tests to be included in the HTML report.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class FeatureAttribute : Attribute
{
    /// <summary>
    /// Gets the feature name displayed in the report.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the optional feature description displayed in the report.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureAttribute"/> class.
    /// </summary>
    /// <param name="name">The feature name displayed in the report.</param>
    /// <param name="description">Optional description of the feature.</param>
    public FeatureAttribute(string name, string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        Description = description;
    }
}
