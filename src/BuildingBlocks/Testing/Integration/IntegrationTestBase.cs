using Xunit.Abstractions;

namespace Bedrock.BuildingBlocks.Testing.Integration;

/// <summary>
/// Base class for integration tests.
/// Provides logging and access to the fixture's environments.
/// </summary>
public abstract class IntegrationTestBase : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IntegrationTestBase"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit test output helper.</param>
    protected IntegrationTestBase(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    /// <summary>
    /// Logs the start of environment setup.
    /// </summary>
    /// <param name="environmentKey">The environment key being set up.</param>
    protected void LogEnvironmentSetup(string environmentKey)
    {
        WriteLog("ENV", $"Setting up environment: {environmentKey}");
    }

    /// <summary>
    /// Logs a database connection.
    /// </summary>
    /// <param name="database">The database name.</param>
    /// <param name="user">The username.</param>
    protected void LogDatabaseConnection(string database, string user)
    {
        WriteLog("DB", $"Connecting to {database} as {user}");
    }

    /// <summary>
    /// Logs a seed operation.
    /// </summary>
    /// <param name="database">The database name.</param>
    /// <param name="description">The seed description.</param>
    protected void LogSeed(string database, string description)
    {
        WriteLog("SEED", $"[{database}] {description}");
    }

    /// <summary>
    /// Logs a SQL operation.
    /// </summary>
    /// <param name="operation">The operation description.</param>
    protected void LogSql(string operation)
    {
        WriteLog("SQL", operation);
    }

    private void WriteLog(string level, string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        OutputHelper.WriteLine($"[{timestamp}] [{level,-6}] {message}");
    }
}
