using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules;

public class DE051_IsValidHierarchyInAbstractClassesRuleTests : TestBase
{
    public DE051_IsValidHierarchyInAbstractClassesRuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void AbstractClassWithProtectedAbstractIsValidConcreteInternal_ShouldPass()
    {
        // Arrange
        LogArrange("Creating abstract class with protected abstract IsValidConcreteInternal");
        var rule = new DE051_IsValidHierarchyInAbstractClassesRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractOrder : EntityBase<AbstractOrder>
            {
                protected abstract bool IsValidConcreteInternal();
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing abstract class with protected abstract IsValidConcreteInternal");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying abstract class with protected abstract IsValidConcreteInternal passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "AbstractOrder");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void AbstractClassWithoutIsValidConcreteInternal_ShouldFail()
    {
        // Arrange
        LogArrange("Creating abstract class without IsValidConcreteInternal");
        var rule = new DE051_IsValidHierarchyInAbstractClassesRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractProduct : EntityBase<AbstractProduct>
            {
                protected void SomeOtherMethod() { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing abstract class without IsValidConcreteInternal");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying abstract class without IsValidConcreteInternal fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "AbstractProduct");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("DE051_IsValidHierarchyInAbstractClasses");
        typeResult.Violation.Message.ShouldContain("IsValidConcreteInternal");
    }

    [Fact]
    public void AbstractClassWithNonAbstractIsValidConcreteInternal_ShouldFail()
    {
        // Arrange
        LogArrange("Creating abstract class with non-abstract IsValidConcreteInternal");
        var rule = new DE051_IsValidHierarchyInAbstractClassesRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractCustomer : EntityBase<AbstractCustomer>
            {
                protected bool IsValidConcreteInternal() => true;
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing abstract class with non-abstract IsValidConcreteInternal");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying abstract class with non-abstract IsValidConcreteInternal fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "AbstractCustomer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation!.Message.ShouldContain("abstract");
    }

    [Fact]
    public void AbstractClassWithPrivateIsValidConcreteInternal_ShouldFail()
    {
        // Arrange
        LogArrange("Creating abstract class with private IsValidConcreteInternal");
        var rule = new DE051_IsValidHierarchyInAbstractClassesRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractInvoice : EntityBase<AbstractInvoice>
            {
                private bool IsValidConcreteInternal() => true;
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing abstract class with private IsValidConcreteInternal");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying abstract class with private IsValidConcreteInternal fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "AbstractInvoice");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation!.Message.ShouldContain("protected");
    }

    [Fact]
    public void ConcreteClass_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating concrete class without IsValidConcreteInternal");
        var rule = new DE051_IsValidHierarchyInAbstractClassesRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Order : EntityBase<Order>
            {
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing concrete class without IsValidConcreteInternal");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying concrete class is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void AbstractClassNotInheritingEntityBase_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating abstract class not inheriting EntityBase");
        var rule = new DE051_IsValidHierarchyInAbstractClassesRule();
        var source = """
            #nullable enable
            public abstract class AbstractClass
            {
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing abstract class not inheriting EntityBase");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying abstract class not inheriting EntityBase is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "AbstractClass");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ViolationMetadata_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating abstract class to verify violation metadata");
        var rule = new DE051_IsValidHierarchyInAbstractClassesRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractEntity : EntityBase<AbstractEntity>
            {
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing violation metadata");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation metadata fields");
        var typeResult = results[0].TypeResults.First(t => t.TypeName == "AbstractEntity");
        var violation = typeResult.Violation!;

        violation.Rule.ShouldBe("DE051_IsValidHierarchyInAbstractClasses");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/domain-entities/DE-051-hierarquia-isvalid-em-classes-abstratas.md");
        violation.LlmHint.ShouldContain("IsValidConcreteInternal");
    }

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new DE051_IsValidHierarchyInAbstractClassesRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("DE051_IsValidHierarchyInAbstractClasses");
        rule.Description.ShouldContain("IsValidConcreteInternal");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/domain-entities/DE-051-hierarquia-isvalid-em-classes-abstratas.md");
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
