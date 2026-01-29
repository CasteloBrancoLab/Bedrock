using Humanizer;
using Xunit.Abstractions;

namespace Bedrock.BuildingBlocks.Testing;

/// <summary>
/// Base class for all unit tests.
/// Provides standardized logging methods using ITestOutputHelper with humanized formatting.
/// </summary>
public abstract class TestBase
{
    /// <summary>
    /// Marker used to identify structured step output for report parsing.
    /// </summary>
    internal const string StepMarker = "##STEP##";

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
    /// Logs the Arrange phase of AAA pattern (Dado in BDD).
    /// Emits structured output for report generation.
    /// </summary>
    protected void LogArrange(string description = "Preparando dados de teste")
    {
        WriteLog("ARRANGE", description);
        WriteStep("Dado", description);
    }

    /// <summary>
    /// Logs the Act phase of AAA pattern (Quando in BDD).
    /// Emits structured output for report generation.
    /// </summary>
    protected void LogAct(string description = "Executando ação")
    {
        WriteLog("ACT", description);
        WriteStep("Quando", description);
    }

    /// <summary>
    /// Logs the Assert phase of AAA pattern (Então in BDD).
    /// Emits structured output for report generation.
    /// </summary>
    protected void LogAssert(string description = "Verificando resultados")
    {
        WriteLog("ASSERT", description);
        WriteStep("Então", description);
    }

    private void WriteLog(string level, string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        OutputHelper.WriteLine($"[{timestamp}] [{level,-6}] {message}");
    }

    private void WriteStep(string type, string description)
    {
        var escapedDescription = EscapeJson(description);
        var timestamp = DateTime.UtcNow.ToString("O");
        OutputHelper.WriteLine($"{StepMarker}{{\"type\":\"{type}\",\"description\":\"{escapedDescription}\",\"timestamp\":\"{timestamp}\"}}");
    }

    private static string EscapeJson(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }
}
