using Humanizer;
using Xunit.Abstractions;

namespace Bedrock.BuildingBlocks.Testing;

/// <summary>
/// Base class for all unit tests.
/// Provides standardized logging methods using ITestOutputHelper with humanized formatting.
/// </summary>
public abstract class TestBase
{
    protected ITestOutputHelper OutputHelper { get; }

    protected TestBase(ITestOutputHelper outputHelper)
    {
        OutputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
    }

    /// <summary>
    /// Logs an informational message.
    /// </summary>
    protected void LogInfo(string message)
    {
        WriteLog("INFO", message);
    }

    /// <summary>
    /// Logs an informational message with formatted arguments.
    /// </summary>
    protected void LogInfo(string message, params object[] args)
    {
        WriteLog("INFO", string.Format(message, args));
    }

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    protected void LogWarning(string message)
    {
        WriteLog("WARN", message);
    }

    /// <summary>
    /// Logs a warning message with formatted arguments.
    /// </summary>
    protected void LogWarning(string message, params object[] args)
    {
        WriteLog("WARN", string.Format(message, args));
    }

    /// <summary>
    /// Logs an error message.
    /// </summary>
    protected void LogError(string message)
    {
        WriteLog("ERROR", message);
    }

    /// <summary>
    /// Logs an error message with formatted arguments.
    /// </summary>
    protected void LogError(string message, params object[] args)
    {
        WriteLog("ERROR", string.Format(message, args));
    }

    /// <summary>
    /// Logs an error message with exception details.
    /// </summary>
    protected void LogError(Exception ex, string message)
    {
        WriteLog("ERROR", $"{message}: {ex.Message}");
        OutputHelper.WriteLine($"         Stack: {ex.StackTrace}");
    }

    /// <summary>
    /// Logs a debug message.
    /// </summary>
    protected void LogDebug(string message)
    {
        WriteLog("DEBUG", message);
    }

    /// <summary>
    /// Logs a debug message with formatted arguments.
    /// </summary>
    protected void LogDebug(string message, params object[] args)
    {
        WriteLog("DEBUG", string.Format(message, args));
    }

    /// <summary>
    /// Logs the elapsed time of an operation in humanized format.
    /// </summary>
    protected void LogElapsed(string operation, TimeSpan elapsed)
    {
        WriteLog("PERF", $"{operation} completed in {elapsed.Humanize()}");
    }

    /// <summary>
    /// Logs the start of a test section.
    /// </summary>
    protected void LogSection(string sectionName)
    {
        OutputHelper.WriteLine("");
        OutputHelper.WriteLine($"=== {sectionName.ToUpper()} ===");
    }

    /// <summary>
    /// Logs the Arrange phase of AAA pattern.
    /// </summary>
    protected void LogArrange(string description = "Setting up test data")
    {
        WriteLog("ARRANGE", description);
    }

    /// <summary>
    /// Logs the Act phase of AAA pattern.
    /// </summary>
    protected void LogAct(string description = "Executing action")
    {
        WriteLog("ACT", description);
    }

    /// <summary>
    /// Logs the Assert phase of AAA pattern.
    /// </summary>
    protected void LogAssert(string description = "Verifying results")
    {
        WriteLog("ASSERT", description);
    }

    private void WriteLog(string level, string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        OutputHelper.WriteLine($"[{timestamp}] [{level,-6}] {message}");
    }
}
