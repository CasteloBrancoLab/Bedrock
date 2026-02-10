using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

public class DE005_AggregateRootInterfaceRuleTests : TestBase
{
    public DE005_AggregateRootInterfaceRuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void AggregateRootImplementingInterface_ShouldPass()
    {
        // Arrange
        LogArrange("Creating aggregate root implementing IAggregateRoot");
        var rule = new DE005_AggregateRootInterfaceRule();
        var source = """
            #nullable enable
            public interface IAggregateRoot { }
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class OrderAggregateRoot : EntityBase<OrderAggregateRoot>, IAggregateRoot
            {
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing aggregate root implementing interface");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying aggregate root implementing interface passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "OrderAggregateRoot");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void AggregateRootNotImplementingInterface_ShouldFail()
    {
        // Arrange
        LogArrange("Creating aggregate root not implementing IAggregateRoot");
        var rule = new DE005_AggregateRootInterfaceRule();
        var source = """
            #nullable enable
            public interface IAggregateRoot { }
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class CustomerAggregateRoot : EntityBase<CustomerAggregateRoot>
            {
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing aggregate root not implementing interface");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying aggregate root not implementing interface fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "CustomerAggregateRoot");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("DE005_AggregateRootInterface");
        typeResult.Violation.Message.ShouldContain("IAggregateRoot");
    }

    [Fact]
    public void EntityWithoutAggregateRootInName_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating entity without AggregateRoot in name");
        var rule = new DE005_AggregateRootInterfaceRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Product : EntityBase<Product>
            {
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity without AggregateRoot in name");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity without AggregateRoot in name passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Product");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ClassNotInheritingEntityBase_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating class with AggregateRoot name but not inheriting EntityBase");
        var rule = new DE005_AggregateRootInterfaceRule();
        var source = """
            #nullable enable
            public sealed class SomeAggregateRoot
            {
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing class not inheriting EntityBase");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying class not inheriting EntityBase is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "SomeAggregateRoot");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void AbstractClass_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating abstract aggregate root");
        var rule = new DE005_AggregateRootInterfaceRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractAggregateRoot : EntityBase<AbstractAggregateRoot>
            {
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing abstract class");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying abstract class is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "AbstractAggregateRoot");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ViolationMetadata_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating aggregate root to verify violation metadata");
        var rule = new DE005_AggregateRootInterfaceRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class InvoiceAggregateRoot : EntityBase<InvoiceAggregateRoot>
            {
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing violation metadata");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation metadata fields");
        var typeResult = results[0].TypeResults.First(t => t.TypeName == "InvoiceAggregateRoot");
        var violation = typeResult.Violation!;

        violation.Rule.ShouldBe("DE005_AggregateRootInterface");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/domain-entities/DE-005-aggregateroot-deve-implementar-iaggregateroot.md");
        violation.LlmHint.ShouldContain("IAggregateRoot");
    }

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new DE005_AggregateRootInterfaceRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("DE005_AggregateRootInterface");
        rule.Description.ShouldContain("IAggregateRoot");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/domain-entities/DE-005-aggregateroot-deve-implementar-iaggregateroot.md");
    }

    #region Helpers

    private static Dictionary<string, Compilation> CreateCompilations(string source)
    {
        return new Dictionary<string, Compilation>
        {
            ["TestProject"] = CreateSingleCompilation(source, "TestProject")
        };
    }

    private static Compilation CreateSingleCompilation(string source, string assemblyName)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, path: "TestFile.cs");
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
        };

        return CSharpCompilation.Create(
            assemblyName,
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithNullableContextOptions(NullableContextOptions.Enable));
    }

    #endregion
}
