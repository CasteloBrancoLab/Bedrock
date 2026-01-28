namespace Bedrock.BuildingBlocks.Testing.Reporting.Models;

/// <summary>
/// Represents the type of a BDD step.
/// </summary>
public enum StepType
{
    /// <summary>
    /// Given step - represents preconditions (Arrange phase).
    /// </summary>
    Given,

    /// <summary>
    /// When step - represents the action being tested (Act phase).
    /// </summary>
    When,

    /// <summary>
    /// Then step - represents the expected outcome (Assert phase).
    /// </summary>
    Then
}
