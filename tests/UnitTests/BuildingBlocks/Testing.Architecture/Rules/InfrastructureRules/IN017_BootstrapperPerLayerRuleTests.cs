using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;

public class IN017_BootstrapperPerLayerRuleTests : TestBase
{
    public IN017_BootstrapperPerLayerRuleTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    #region Rule Properties

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new IN017_BootstrapperPerLayerRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("IN017_BootstrapperPerLayer");
        rule.Description.ShouldContain("Bootstrapper");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/infrastructure/IN-017-bootstrapper-por-camada-para-ioc.md");
        rule.Category.ShouldBe("Infrastructure");
    }

    #endregion

    #region Non-Eligible Projects Should Be Ignored

    [Theory]
    [InlineData("ShopDemo.Auth.Domain")]
    [InlineData("ShopDemo.Auth.Domain.Entities")]
    [InlineData("ShopDemo.Auth.Application")]
    [InlineData("ShopDemo.Auth.Infra.Data")]
    [InlineData("Bedrock.BuildingBlocks.Core")]
    public void NonEligibleProject_ShouldBeIgnored(string projectName)
    {
        // Arrange
        LogArrange($"Testing non-eligible project '{projectName}'");
        var rule = new IN017_BootstrapperPerLayerRule();
        var source = """
            namespace Some.Namespace
            {
                public class SomeClass { }
            }
            """;
        var compilations = CreateCompilationWithSource(projectName, source);

        // Act
        LogAct("Analyzing project");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying project is ignored");
        results.ShouldBeEmpty();
    }

    #endregion

    #region Missing Bootstrapper

    [Theory]
    [InlineData("ShopDemo.Auth.Infra.Data.PostgreSql")]
    [InlineData("ShopDemo.Auth.Infra.CrossCutting.Configuration")]
    [InlineData("ShopDemo.Auth.Infra.Data.PostgreSql.Migrations")]
    public void EligibleProject_WithoutBootstrapper_ShouldGenerateViolation(string projectName)
    {
        // Arrange
        LogArrange($"Creating eligible project '{projectName}' without Bootstrapper");
        var rule = new IN017_BootstrapperPerLayerRule();
        var source = """
            namespace Some.Namespace
            {
                public class SomeClass { }
            }
            """;
        var compilations = CreateCompilationWithSource(projectName, source);

        // Act
        LogAct("Analyzing project without Bootstrapper");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation for missing Bootstrapper");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("nao possui classe 'Bootstrapper'");
        violations[0].Rule.ShouldBe("IN017_BootstrapperPerLayer");
        violations[0].Severity.ShouldBe(Severity.Error);
    }

    #endregion

    #region Valid Bootstrapper

    [Fact]
    public void ValidBootstrapper_InInfraDataTech_ShouldPass()
    {
        // Arrange
        LogArrange("Creating valid Bootstrapper in Infra.Data.PostgreSql");
        var rule = new IN017_BootstrapperPerLayerRule();
        var source = CreateSourceWithBootstrapper(
            "ShopDemo.Auth.Infra.Data.PostgreSql",
            isStatic: true,
            isPublic: true,
            hasConfigureServices: true);
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing valid Bootstrapper");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying no violations");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .ToList();
        violations.ShouldBeEmpty();

        var passed = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Status == TypeAnalysisStatus.Passed)
            .ToList();
        passed.Count.ShouldBe(1);
        passed[0].TypeName.ShouldBe("Bootstrapper");
    }

    [Fact]
    public void ValidBootstrapper_InCrossCuttingConfiguration_ShouldPass()
    {
        // Arrange
        LogArrange("Creating valid Bootstrapper in Infra.CrossCutting.Configuration");
        var rule = new IN017_BootstrapperPerLayerRule();
        var source = CreateSourceWithBootstrapper(
            "ShopDemo.Auth.Infra.CrossCutting.Configuration",
            isStatic: true,
            isPublic: true,
            hasConfigureServices: true);
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.CrossCutting.Configuration", source);

        // Act
        LogAct("Analyzing valid Bootstrapper");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying no violations");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .ToList();
        violations.ShouldBeEmpty();
    }

    #endregion

    #region Invalid Bootstrapper

    [Fact]
    public void NonStaticBootstrapper_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating non-static Bootstrapper");
        var rule = new IN017_BootstrapperPerLayerRule();
        var source = CreateSourceWithBootstrapper(
            "ShopDemo.Auth.Infra.Data.PostgreSql",
            isStatic: false,
            isPublic: true,
            hasConfigureServices: true);
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing non-static Bootstrapper");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation for non-static");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("nao e static");
    }

    [Fact]
    public void NonPublicBootstrapper_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating non-public Bootstrapper");
        var rule = new IN017_BootstrapperPerLayerRule();
        var source = """
            namespace Microsoft.Extensions.DependencyInjection
            {
                public interface IServiceCollection { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql
            {
                internal static class Bootstrapper
                {
                    public static Microsoft.Extensions.DependencyInjection.IServiceCollection ConfigureServices(
                        Microsoft.Extensions.DependencyInjection.IServiceCollection services)
                    {
                        return services;
                    }
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing non-public Bootstrapper");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation for non-public");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("nao e public");
    }

    [Fact]
    public void BootstrapperWithoutConfigureServices_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating Bootstrapper without ConfigureServices method");
        var rule = new IN017_BootstrapperPerLayerRule();
        var source = CreateSourceWithBootstrapper(
            "ShopDemo.Auth.Infra.Data.PostgreSql",
            isStatic: true,
            isPublic: true,
            hasConfigureServices: false);
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing Bootstrapper without ConfigureServices");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation for missing ConfigureServices");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("ConfigureServices");
    }

    [Fact]
    public void BootstrapperInWrongNamespace_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating Bootstrapper in wrong namespace");
        var rule = new IN017_BootstrapperPerLayerRule();
        var source = """
            namespace Microsoft.Extensions.DependencyInjection
            {
                public interface IServiceCollection { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Registration
            {
                public static class Bootstrapper
                {
                    public static Microsoft.Extensions.DependencyInjection.IServiceCollection ConfigureServices(
                        Microsoft.Extensions.DependencyInjection.IServiceCollection services)
                    {
                        return services;
                    }
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing Bootstrapper in wrong namespace");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation for wrong namespace");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("namespace");
    }

    #endregion

    #region Non-Bootstrapper With IServiceCollection (New Validation)

    [Fact]
    public void NonBootstrapper_WithIServiceCollectionParam_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating non-Bootstrapper class with IServiceCollection parameter");
        var rule = new IN017_BootstrapperPerLayerRule();
        var source = """
            namespace Microsoft.Extensions.DependencyInjection
            {
                public interface IServiceCollection { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql
            {
                public static class Bootstrapper
                {
                    public static Microsoft.Extensions.DependencyInjection.IServiceCollection ConfigureServices(
                        Microsoft.Extensions.DependencyInjection.IServiceCollection services)
                    {
                        return services;
                    }
                }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Registration
            {
                public static class ServiceCollectionExtensions
                {
                    public static Microsoft.Extensions.DependencyInjection.IServiceCollection AddAuth(
                        Microsoft.Extensions.DependencyInjection.IServiceCollection services)
                    {
                        return services;
                    }
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing non-Bootstrapper with IServiceCollection");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation for non-Bootstrapper with IServiceCollection");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("ServiceCollectionExtensions");
        violations[0].Message.ShouldContain("IServiceCollection");
        violations[0].Message.ShouldContain("Apenas a classe Bootstrapper");
    }

    [Fact]
    public void NonBootstrapper_WithExtensionMethod_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating non-Bootstrapper with extension method on IServiceCollection");
        var rule = new IN017_BootstrapperPerLayerRule();
        var source = """
            namespace Microsoft.Extensions.DependencyInjection
            {
                public interface IServiceCollection { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql
            {
                public static class Bootstrapper
                {
                    public static Microsoft.Extensions.DependencyInjection.IServiceCollection ConfigureServices(
                        Microsoft.Extensions.DependencyInjection.IServiceCollection services)
                    {
                        return services;
                    }
                }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Registration
            {
                public static class AuthExtensions
                {
                    public static Microsoft.Extensions.DependencyInjection.IServiceCollection AddAuthServices(
                        this Microsoft.Extensions.DependencyInjection.IServiceCollection services)
                    {
                        return services;
                    }
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing extension method on IServiceCollection");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation for extension method");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("AuthExtensions");
    }

    [Fact]
    public void NonBootstrapper_WithIServiceCollectionParam_InCrossCuttingConfig_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating non-Bootstrapper with IServiceCollection in CrossCutting.Configuration");
        var rule = new IN017_BootstrapperPerLayerRule();
        var source = """
            namespace Microsoft.Extensions.DependencyInjection
            {
                public interface IServiceCollection { }
            }
            namespace ShopDemo.Auth.Infra.CrossCutting.Configuration
            {
                public static class Bootstrapper
                {
                    public static Microsoft.Extensions.DependencyInjection.IServiceCollection ConfigureServices(
                        Microsoft.Extensions.DependencyInjection.IServiceCollection services)
                    {
                        return services;
                    }
                }
                public static class SomeHelper
                {
                    public static void RegisterStuff(
                        Microsoft.Extensions.DependencyInjection.IServiceCollection services) { }
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.CrossCutting.Configuration", source);

        // Act
        LogAct("Analyzing non-Bootstrapper in CrossCutting.Configuration");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation for non-Bootstrapper helper");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("SomeHelper");
    }

    [Fact]
    public void MultipleNonBootstrappers_WithIServiceCollection_ShouldGenerateMultipleViolations()
    {
        // Arrange
        LogArrange("Creating multiple non-Bootstrapper classes with IServiceCollection");
        var rule = new IN017_BootstrapperPerLayerRule();
        var source = """
            namespace Microsoft.Extensions.DependencyInjection
            {
                public interface IServiceCollection { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql
            {
                public static class Bootstrapper
                {
                    public static Microsoft.Extensions.DependencyInjection.IServiceCollection ConfigureServices(
                        Microsoft.Extensions.DependencyInjection.IServiceCollection services)
                    {
                        return services;
                    }
                }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Registration
            {
                public static class ServiceCollectionExtensions
                {
                    public static Microsoft.Extensions.DependencyInjection.IServiceCollection AddAuth(
                        Microsoft.Extensions.DependencyInjection.IServiceCollection services)
                    {
                        return services;
                    }
                }
                public static class AnotherHelper
                {
                    public static void Register(
                        Microsoft.Extensions.DependencyInjection.IServiceCollection services) { }
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing multiple non-Bootstrappers");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying multiple violations");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(2);
        violations.ShouldContain(v => v.Message.Contains("ServiceCollectionExtensions"));
        violations.ShouldContain(v => v.Message.Contains("AnotherHelper"));
    }

    #endregion

    #region Non-Bootstrapper Without IServiceCollection Should Pass

    [Fact]
    public void NonBootstrapperClass_WithoutIServiceCollection_ShouldNotGenerateViolation()
    {
        // Arrange
        LogArrange("Creating non-Bootstrapper class without IServiceCollection");
        var rule = new IN017_BootstrapperPerLayerRule();
        var source = """
            namespace Microsoft.Extensions.DependencyInjection
            {
                public interface IServiceCollection { }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql
            {
                public static class Bootstrapper
                {
                    public static Microsoft.Extensions.DependencyInjection.IServiceCollection ConfigureServices(
                        Microsoft.Extensions.DependencyInjection.IServiceCollection services)
                    {
                        return services;
                    }
                }
            }
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Connections
            {
                public sealed class AuthPostgreSqlConnection
                {
                    public void Open() { }
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing non-Bootstrapper without IServiceCollection");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying no violations for non-Bootstrapper without IServiceCollection");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .ToList();
        violations.ShouldBeEmpty();
    }

    #endregion

    #region Helpers

    private static string CreateSourceWithBootstrapper(
        string rootNamespace,
        bool isStatic,
        bool isPublic,
        bool hasConfigureServices)
    {
        var accessModifier = isPublic ? "public" : "internal";
        var staticModifier = isStatic ? " static" : "";
        var method = hasConfigureServices
            ? """
                    public static Microsoft.Extensions.DependencyInjection.IServiceCollection ConfigureServices(
                        Microsoft.Extensions.DependencyInjection.IServiceCollection services)
                    {
                        return services;
                    }
              """
            : """
                    public static void DoSomething() { }
              """;

        return
            "namespace Microsoft.Extensions.DependencyInjection { public interface IServiceCollection { } }\n" +
            $"namespace {rootNamespace} {{ {accessModifier}{staticModifier} class Bootstrapper {{ {method} }} }}";
    }

    private static Dictionary<string, Compilation> CreateCompilationWithSource(string projectName, string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, path: "TestFile.cs");
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
        };

        return new Dictionary<string, Compilation>
        {
            [projectName] = CSharpCompilation.Create(
                projectName,
                [syntaxTree],
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
        };
    }

    #endregion
}
