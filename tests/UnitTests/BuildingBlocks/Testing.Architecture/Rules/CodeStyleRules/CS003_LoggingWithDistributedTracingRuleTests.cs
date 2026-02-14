using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.CodeStyleRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.CodeStyleRules;

public class CS003_LoggingWithDistributedTracingRuleTests : TestBase
{
    public CS003_LoggingWithDistributedTracingRuleTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    #region Pass Cases

    [Fact]
    public void MethodWithoutExecutionContext_UsingLogError_ShouldPass()
    {
        // Arrange
        LogArrange("Creating method without ExecutionContext that uses LogError");
        var rule = new CS003_LoggingWithDistributedTracingRule();
        var source = """
            namespace MyProject.Repositories;
            public sealed class Logger
            {
                public void LogError(string message) { }
            }
            public sealed class MyService
            {
                private readonly Logger _logger = new();
                public void DoWork()
                {
                    _logger.LogError("Something failed");
                }
            }
            """;
        var compilations = CreateCompilations(source, "src/Repositories/MyService.cs");

        // Act
        LogAct("Analyzing method without ExecutionContext");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying method without ExecutionContext passes");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "MyService");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
        typeResult.Violation.ShouldBeNull();
    }

    [Fact]
    public void MethodWithExecutionContext_UsingDistributedTracing_ShouldPass()
    {
        // Arrange
        LogArrange("Creating method with ExecutionContext using ForDistributedTracing");
        var rule = new CS003_LoggingWithDistributedTracingRule();
        var source = """
            namespace MyProject.Repositories;
            public sealed class ExecutionContext { }
            public sealed class Logger
            {
                public void LogExceptionForDistributedTracing(ExecutionContext ctx, object ex, string msg) { }
            }
            public sealed class MyRepository
            {
                private readonly Logger _logger = new();
                public void GetById(ExecutionContext executionContext)
                {
                    _logger.LogExceptionForDistributedTracing(executionContext, new object(), "error");
                }
            }
            """;
        var compilations = CreateCompilations(source, "src/Repositories/MyRepository.cs");

        // Act
        LogAct("Analyzing method using ForDistributedTracing");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying ForDistributedTracing method passes");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "MyRepository");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
        typeResult.Violation.ShouldBeNull();
    }

    [Fact]
    public void MethodWithExecutionContext_NoLogging_ShouldPass()
    {
        // Arrange
        LogArrange("Creating method with ExecutionContext but no logging");
        var rule = new CS003_LoggingWithDistributedTracingRule();
        var source = """
            namespace MyProject.Repositories;
            public sealed class ExecutionContext { }
            public sealed class MyRepository
            {
                public int GetValue(ExecutionContext executionContext)
                {
                    return 42;
                }
            }
            """;
        var compilations = CreateCompilations(source, "src/Repositories/MyRepository.cs");

        // Act
        LogAct("Analyzing method with no logging");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying method without logging passes");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "MyRepository");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
        typeResult.Violation.ShouldBeNull();
    }

    [Fact]
    public void SystemThreadingExecutionContext_UsingLogError_ShouldPass()
    {
        // Arrange
        LogArrange("Creating method with System.Threading.ExecutionContext (should be ignored)");
        var rule = new CS003_LoggingWithDistributedTracingRule();
        var source = """
            namespace System.Threading;
            public sealed class ExecutionContext { }
            namespace MyProject.Services;
            public sealed class Logger
            {
                public void LogError(string message) { }
            }
            public sealed class MyService
            {
                private readonly Logger _logger = new();
                public void DoWork(System.Threading.ExecutionContext executionContext)
                {
                    _logger.LogError("Something failed");
                }
            }
            """;
        var compilations = CreateCompilations(source, "src/Services/MyService.cs");

        // Act
        LogAct("Analyzing method with System.Threading.ExecutionContext");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying System.Threading.ExecutionContext is ignored");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "MyService");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
        typeResult.Violation.ShouldBeNull();
    }

    [Fact]
    public void SuppressedWithDisableOnce_ShouldPass()
    {
        // Arrange
        LogArrange("Creating LogError suppressed with CS003 disable once");
        var rule = new CS003_LoggingWithDistributedTracingRule();
        var source = """
            namespace MyProject.Repositories;
            public sealed class ExecutionContext { }
            public sealed class Logger
            {
                public void LogError(object ex, string message) { }
            }
            public sealed class MyRepository
            {
                private readonly Logger _logger = new();
                public void GetById(ExecutionContext executionContext)
                {
                    // CS003 disable once : bootstrap sem distributed tracing
                    _logger.LogError(new object(), "error");
                }
            }
            """;
        var compilations = CreateCompilations(source, "src/Repositories/MyRepository.cs");

        // Act
        LogAct("Analyzing suppressed LogError with disable once");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying suppressed LogError passes");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "MyRepository");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
        typeResult.Violation.ShouldBeNull();
    }

    [Fact]
    public void SuppressedWithDisableRestoreBlock_ShouldPass()
    {
        // Arrange
        LogArrange("Creating LogError in CS003 disable/restore block");
        var rule = new CS003_LoggingWithDistributedTracingRule();
        var source = """
            namespace MyProject.Repositories;
            public sealed class ExecutionContext { }
            public sealed class Logger
            {
                public void LogError(object ex, string message) { }
                public void LogWarning(string message) { }
            }
            public sealed class MyRepository
            {
                private readonly Logger _logger = new();
                public void GetById(ExecutionContext executionContext)
                {
                    // CS003 disable : bloco de compatibilidade legada
                    _logger.LogError(new object(), "error");
                    _logger.LogWarning("warning");
                    // CS003 restore
                }
            }
            """;
        var compilations = CreateCompilations(source, "src/Repositories/MyRepository.cs");

        // Act
        LogAct("Analyzing suppressed LogError in disable/restore block");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying suppressed LogError in block passes");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "MyRepository");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
        typeResult.Violation.ShouldBeNull();
    }

    [Fact]
    public void MethodWithExecutionContext_UsingLogForDistributedTracing_ShouldPass()
    {
        // Arrange
        LogArrange("Creating method with ExecutionContext using LogForDistributedTracing (generic)");
        var rule = new CS003_LoggingWithDistributedTracingRule();
        var source = """
            namespace MyProject.Repositories;
            public sealed class ExecutionContext { }
            public sealed class Logger
            {
                public void LogForDistributedTracing(ExecutionContext ctx, int logLevel, object ex, string msg) { }
            }
            public sealed class MyRepository
            {
                private readonly Logger _logger = new();
                public void GetById(ExecutionContext executionContext)
                {
                    _logger.LogForDistributedTracing(executionContext, 4, null!, "error");
                }
            }
            """;
        var compilations = CreateCompilations(source, "src/Repositories/MyRepository.cs");

        // Act
        LogAct("Analyzing method using LogForDistributedTracing");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying LogForDistributedTracing method passes");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "MyRepository");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
        typeResult.Violation.ShouldBeNull();
    }

    [Fact]
    public void TypeWithoutMethods_ShouldPass()
    {
        // Arrange
        LogArrange("Creating type without methods");
        var rule = new CS003_LoggingWithDistributedTracingRule();
        var source = """
            namespace MyProject.Models;
            public sealed class DataModel
            {
                public int Id { get; set; }
                public string Name { get; set; } = string.Empty;
            }
            """;
        var compilations = CreateCompilations(source, "src/Models/DataModel.cs");

        // Act
        LogAct("Analyzing type without methods");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying type without methods passes");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "DataModel");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
        typeResult.Violation.ShouldBeNull();
    }

    #endregion

    #region Fail Cases

    [Fact]
    public void MethodWithExecutionContext_UsingLogError_ShouldFail()
    {
        // Arrange
        LogArrange("Creating method with ExecutionContext using LogError");
        var rule = new CS003_LoggingWithDistributedTracingRule();
        var source = """
            namespace MyProject.Repositories;
            public sealed class ExecutionContext { }
            public sealed class Logger
            {
                public void LogError(object ex, string message) { }
            }
            public sealed class MyRepository
            {
                private readonly Logger _logger = new();
                public void GetByEmail(ExecutionContext executionContext)
                {
                    _logger.LogError(new object(), "An error occurred.");
                }
            }
            """;
        var compilations = CreateCompilations(source, "src/Repositories/MyRepository.cs");

        // Act
        LogAct("Analyzing LogError in method with ExecutionContext");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying LogError with ExecutionContext fails");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "MyRepository");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("CS003_LoggingWithDistributedTracing");
        typeResult.Violation.Message.ShouldContain("LogError");
    }

    [Fact]
    public void MethodWithExecutionContext_UsingLogWarning_ShouldFail()
    {
        // Arrange
        LogArrange("Creating method with ExecutionContext using LogWarning");
        var rule = new CS003_LoggingWithDistributedTracingRule();
        var source = """
            namespace MyProject.Repositories;
            public sealed class ExecutionContext { }
            public sealed class Logger
            {
                public void LogWarning(string message) { }
            }
            public sealed class MyRepository
            {
                private readonly Logger _logger = new();
                public void GetByEmail(ExecutionContext executionContext)
                {
                    _logger.LogWarning("Something suspicious.");
                }
            }
            """;
        var compilations = CreateCompilations(source, "src/Repositories/MyRepository.cs");

        // Act
        LogAct("Analyzing LogWarning in method with ExecutionContext");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying LogWarning with ExecutionContext fails");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "MyRepository");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Message.ShouldContain("LogWarning");
    }

    [Fact]
    public void MethodWithExecutionContext_UsingLogInformation_ShouldFail()
    {
        // Arrange
        LogArrange("Creating method with ExecutionContext using LogInformation");
        var rule = new CS003_LoggingWithDistributedTracingRule();
        var source = """
            namespace MyProject.Repositories;
            public sealed class ExecutionContext { }
            public sealed class Logger
            {
                public void LogInformation(string message) { }
            }
            public sealed class MyRepository
            {
                private readonly Logger _logger = new();
                public void GetByEmail(ExecutionContext executionContext)
                {
                    _logger.LogInformation("Getting user by email.");
                }
            }
            """;
        var compilations = CreateCompilations(source, "src/Repositories/MyRepository.cs");

        // Act
        LogAct("Analyzing LogInformation in method with ExecutionContext");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying LogInformation with ExecutionContext fails");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "MyRepository");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Message.ShouldContain("LogInformation");
    }

    [Fact]
    public void MethodWithExecutionContext_UsingLogDebug_ShouldFail()
    {
        // Arrange
        LogArrange("Creating method with ExecutionContext using LogDebug");
        var rule = new CS003_LoggingWithDistributedTracingRule();
        var source = """
            namespace MyProject.Repositories;
            public sealed class ExecutionContext { }
            public sealed class Logger
            {
                public void LogDebug(string message) { }
            }
            public sealed class MyRepository
            {
                private readonly Logger _logger = new();
                public void GetByEmail(ExecutionContext executionContext)
                {
                    _logger.LogDebug("Debug info.");
                }
            }
            """;
        var compilations = CreateCompilations(source, "src/Repositories/MyRepository.cs");

        // Act
        LogAct("Analyzing LogDebug in method with ExecutionContext");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying LogDebug with ExecutionContext fails");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "MyRepository");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Message.ShouldContain("LogDebug");
    }

    [Fact]
    public void MethodWithExecutionContext_UsingLogTrace_ShouldFail()
    {
        // Arrange
        LogArrange("Creating method with ExecutionContext using LogTrace");
        var rule = new CS003_LoggingWithDistributedTracingRule();
        var source = """
            namespace MyProject.Repositories;
            public sealed class ExecutionContext { }
            public sealed class Logger
            {
                public void LogTrace(string message) { }
            }
            public sealed class MyRepository
            {
                private readonly Logger _logger = new();
                public void GetByEmail(ExecutionContext executionContext)
                {
                    _logger.LogTrace("Trace info.");
                }
            }
            """;
        var compilations = CreateCompilations(source, "src/Repositories/MyRepository.cs");

        // Act
        LogAct("Analyzing LogTrace in method with ExecutionContext");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying LogTrace with ExecutionContext fails");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "MyRepository");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Message.ShouldContain("LogTrace");
    }

    [Fact]
    public void MethodWithExecutionContext_UsingLogCritical_ShouldFail()
    {
        // Arrange
        LogArrange("Creating method with ExecutionContext using LogCritical");
        var rule = new CS003_LoggingWithDistributedTracingRule();
        var source = """
            namespace MyProject.Repositories;
            public sealed class ExecutionContext { }
            public sealed class Logger
            {
                public void LogCritical(string message) { }
            }
            public sealed class MyRepository
            {
                private readonly Logger _logger = new();
                public void GetByEmail(ExecutionContext executionContext)
                {
                    _logger.LogCritical("Critical failure.");
                }
            }
            """;
        var compilations = CreateCompilations(source, "src/Repositories/MyRepository.cs");

        // Act
        LogAct("Analyzing LogCritical in method with ExecutionContext");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying LogCritical with ExecutionContext fails");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "MyRepository");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Message.ShouldContain("LogCritical");
    }

    [Fact]
    public void MethodWithExecutionContext_UsingLog_ShouldFail()
    {
        // Arrange
        LogArrange("Creating method with ExecutionContext using Log (generic ILogger.Log)");
        var rule = new CS003_LoggingWithDistributedTracingRule();
        var source = """
            namespace MyProject.Repositories;
            public sealed class ExecutionContext { }
            public sealed class Logger
            {
                public void Log(int logLevel, string message) { }
            }
            public sealed class MyRepository
            {
                private readonly Logger _logger = new();
                public void GetByEmail(ExecutionContext executionContext)
                {
                    _logger.Log(4, "An error occurred.");
                }
            }
            """;
        var compilations = CreateCompilations(source, "src/Repositories/MyRepository.cs");

        // Act
        LogAct("Analyzing Log in method with ExecutionContext");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying Log with ExecutionContext fails");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "MyRepository");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Message.ShouldContain("'Log'");
        typeResult.Violation.LlmHint.ShouldContain("LogForDistributedTracing");
    }

    [Fact]
    public void LogErrorAfterRestoreComment_ShouldFail()
    {
        // Arrange
        LogArrange("Creating LogError after CS003 restore (outside block)");
        var rule = new CS003_LoggingWithDistributedTracingRule();
        var source = """
            namespace MyProject.Repositories;
            public sealed class ExecutionContext { }
            public sealed class Logger
            {
                public void LogError(object ex, string message) { }
            }
            public sealed class MyRepository
            {
                private readonly Logger _logger = new();
                public void GetByEmail(ExecutionContext executionContext)
                {
                    // CS003 disable : bloco legado
                    _logger.LogError(new object(), "suppressed");
                    // CS003 restore
                    _logger.LogError(new object(), "not suppressed");
                }
            }
            """;
        var compilations = CreateCompilations(source, "src/Repositories/MyRepository.cs");

        // Act
        LogAct("Analyzing LogError after restore comment");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying LogError after restore fails");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "MyRepository");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
    }

    #endregion

    #region Violation Metadata

    [Fact]
    public void Violation_ShouldHaveCorrectMetadata()
    {
        // Arrange
        LogArrange("Creating LogError to verify violation metadata");
        var rule = new CS003_LoggingWithDistributedTracingRule();
        var source = """
            namespace MyProject.Repositories;
            public sealed class ExecutionContext { }
            public sealed class Logger
            {
                public void LogError(object ex, string message) { }
            }
            public sealed class UserRepository
            {
                private readonly Logger _logger = new();
                public void GetByEmail(ExecutionContext executionContext)
                {
                    _logger.LogError(new object(), "An error occurred.");
                }
            }
            """;
        var compilations = CreateCompilations(source, "src/Repositories/UserRepository.cs");

        // Act
        LogAct("Analyzing violation metadata");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying violation metadata fields");
        var typeResult = results[0].TypeResults.First(t => t.TypeName == "UserRepository");
        var violation = typeResult.Violation!;

        violation.Rule.ShouldBe("CS003_LoggingWithDistributedTracing");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/code-style/CS-003-logging-sempre-com-distributed-tracing.md");
        violation.Project.ShouldBe("MyProject.Repositories");
        violation.Message.ShouldContain("LogError");
        violation.Message.ShouldContain("GetByEmail");
        violation.Message.ShouldContain("UserRepository");
        violation.Message.ShouldContain("ExecutionContext");
        violation.LlmHint.ShouldContain("LogError");
        violation.LlmHint.ShouldContain("ForDistributedTracing");
    }

    #endregion

    #region Rule Properties

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new CS003_LoggingWithDistributedTracingRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("CS003_LoggingWithDistributedTracing");
        rule.Description.ShouldContain("ForDistributedTracing");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/code-style/CS-003-logging-sempre-com-distributed-tracing.md");
    }

    #endregion

    #region Helpers

    private static Dictionary<string, Compilation> CreateCompilations(string source, string filePath)
    {
        return new Dictionary<string, Compilation>
        {
            ["MyProject.Repositories"] = CreateSingleCompilation(source, "MyProject.Repositories", filePath)
        };
    }

    private static Compilation CreateSingleCompilation(string source, string assemblyName, string filePath)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, path: filePath);

        var coreAssemblyDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        var references = new MetadataReference[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(Path.Combine(coreAssemblyDir, "System.Runtime.dll")),
        };

        return CSharpCompilation.Create(
            assemblyName,
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    #endregion
}
