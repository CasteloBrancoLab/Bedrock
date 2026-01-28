namespace Bedrock.BuildingBlocks.Testing.Reporting.Models;

/// <summary>
/// Represents environment information for the test report.
/// </summary>
public sealed class EnvironmentInfo
{
    /// <summary>
    /// Gets or sets the machine name.
    /// </summary>
    public string MachineName { get; set; } = Environment.MachineName;

    /// <summary>
    /// Gets or sets the operating system version.
    /// </summary>
    public string OsVersion { get; set; } = Environment.OSVersion.ToString();

    /// <summary>
    /// Gets or sets the .NET runtime version.
    /// </summary>
    public string DotNetVersion { get; set; } = Environment.Version.ToString();

    /// <summary>
    /// Gets or sets the current Git branch.
    /// </summary>
    public string? GitBranch { get; set; }

    /// <summary>
    /// Gets or sets the current Git commit hash.
    /// </summary>
    public string? GitCommit { get; set; }

    /// <summary>
    /// Gets the short Git commit hash (first 7 characters).
    /// </summary>
    public string? GitCommitShort => GitCommit?.Length >= 7 ? GitCommit[..7] : GitCommit;

    /// <summary>
    /// Gets or sets the test execution timestamp.
    /// </summary>
    public DateTime ExecutionTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the username running the tests.
    /// </summary>
    public string UserName { get; set; } = Environment.UserName;
}
