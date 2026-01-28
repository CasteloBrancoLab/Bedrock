namespace Bedrock.BuildingBlocks.Testing.Reporting.Models;

/// <summary>
/// Represents the execution status of a test.
/// </summary>
public enum TestStatus
{
    /// <summary>
    /// The test passed successfully.
    /// </summary>
    Passed,

    /// <summary>
    /// The test failed.
    /// </summary>
    Failed,

    /// <summary>
    /// The test was skipped.
    /// </summary>
    Skipped
}
