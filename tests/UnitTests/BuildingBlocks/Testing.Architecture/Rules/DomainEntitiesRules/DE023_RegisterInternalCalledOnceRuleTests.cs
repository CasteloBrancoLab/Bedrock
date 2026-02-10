using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

public class DE023_RegisterInternalCalledOnceRuleTests : TestBase
{
    public DE023_RegisterInternalCalledOnceRuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void MethodWithOneRegisterInternalCall_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with method calling RegisterChangeInternal once");
        var rule = new DE023_RegisterInternalCalledOnceRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Order : EntityBase<Order>
            {
                public Order? ChangeName(string name)
                {
                    return RegisterChangeInternal(() => true);
                }

                private Order? RegisterChangeInternal(System.Func<bool> handler) => null;
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing method with one RegisterChangeInternal call");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying method with one RegisterChangeInternal call passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void MethodWithTwoRegisterInternalCalls_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with method calling RegisterChangeInternal twice");
        var rule = new DE023_RegisterInternalCalledOnceRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Product : EntityBase<Product>
            {
                public Product? ChangeName(string name)
                {
                    RegisterChangeInternal(() => true);
                    return RegisterChangeInternal(() => true);
                }

                private Product? RegisterChangeInternal(System.Func<bool> handler) => null;
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing method with two RegisterChangeInternal calls");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying method with two RegisterChangeInternal calls fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Product");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("DE023_RegisterInternalCalledOnce");
        typeResult.Violation.Message.ShouldContain("2 vezes");
    }

    [Fact]
    public void MethodWithNoRegisterInternalCall_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with method not calling RegisterInternal");
        var rule = new DE023_RegisterInternalCalledOnceRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Customer : EntityBase<Customer>
            {
                public void DoSomething()
                {
                    // No RegisterInternal call
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing method without RegisterInternal call");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying method without RegisterInternal call passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Customer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ValidateMethod_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating entity with Validate method");
        var rule = new DE023_RegisterInternalCalledOnceRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Invoice : EntityBase<Invoice>
            {
                public static bool ValidateName(string? name) => true;
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing Validate method");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying Validate method is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Invoice");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ClassNotInheritingEntityBase_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating class not inheriting EntityBase");
        var rule = new DE023_RegisterInternalCalledOnceRule();
        var source = """
            #nullable enable
            public sealed class RegularClass
            {
                public void ChangeMultiple()
                {
                    RegisterChangeInternal(() => true);
                    RegisterChangeInternal(() => true);
                }

                private void RegisterChangeInternal(System.Func<bool> handler) { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing class not inheriting EntityBase");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying class not inheriting EntityBase is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "RegularClass");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void AbstractClass_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating abstract class");
        var rule = new DE023_RegisterInternalCalledOnceRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractEntity : EntityBase<AbstractEntity>
            {
                public void ChangeMultiple()
                {
                    RegisterChangeInternal(() => true);
                    RegisterChangeInternal(() => true);
                }

                private void RegisterChangeInternal(System.Func<bool> handler) { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing abstract class");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying abstract class is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "AbstractEntity");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ViolationMetadata_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating entity to verify violation metadata");
        var rule = new DE023_RegisterInternalCalledOnceRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Entity : EntityBase<Entity>
            {
                public void ChangeMultiple()
                {
                    RegisterChangeInternal(() => true);
                    RegisterChangeInternal(() => true);
                }

                private void RegisterChangeInternal(System.Func<bool> handler) { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing violation metadata");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation metadata fields");
        var typeResult = results[0].TypeResults.First(t => t.TypeName == "Entity");
        var violation = typeResult.Violation!;

        violation.Rule.ShouldBe("DE023_RegisterInternalCalledOnce");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/domain-entities/DE-023-register-internal-chamado-uma-unica-vez.md");
        violation.LlmHint.ShouldContain("UMA ÚNICA");
    }

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new DE023_RegisterInternalCalledOnceRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("DE023_RegisterInternalCalledOnce");
        rule.Description.ShouldContain("uma vez");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/domain-entities/DE-023-register-internal-chamado-uma-unica-vez.md");
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
