using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

public class DE024_PublicMethodNeverCallsPublicRuleTests : TestBase
{
    public DE024_PublicMethodNeverCallsPublicRuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void PublicMethodNotCallingPublic_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with public method not calling other public methods");
        var rule = new DE024_PublicMethodNeverCallsPublicRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Order : EntityBase<Order>
            {
                public void ChangeName(string name)
                {
                    ChangeNameInternal(name);
                }

                private void ChangeNameInternal(string name) { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing public method not calling public");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying public method not calling public passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void PublicMethodCallingOtherPublicMethod_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with public method calling another public method");
        var rule = new DE024_PublicMethodNeverCallsPublicRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Product : EntityBase<Product>
            {
                public void ChangeName(string name)
                {
                    ChangeStatus("active");
                }

                public void ChangeStatus(string status) { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing public method calling other public method");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying public method calling other public method fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Product");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("DE024_PublicMethodNeverCallsPublic");
        typeResult.Violation.Message.ShouldContain("ChangeStatus");
    }

    [Fact]
    public void PublicMethodCallingValidateMethod_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with public method calling Validate method");
        var rule = new DE024_PublicMethodNeverCallsPublicRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Customer : EntityBase<Customer>
            {
                public void ChangeName(string name)
                {
                    ValidateName(name);
                }

                public static bool ValidateName(string? name) => true;
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing public method calling Validate method");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying public method calling Validate method passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Customer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void PublicMethodCallingIsValid_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with public method calling IsValid");
        var rule = new DE024_PublicMethodNeverCallsPublicRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Invoice : EntityBase<Invoice>
            {
                public void ChangeName(string name)
                {
                    IsValid(name);
                }

                public static bool IsValid(string? name) => true;
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing public method calling IsValid");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying public method calling IsValid passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Invoice");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void PrivateMethod_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating entity with private method calling public method");
        var rule = new DE024_PublicMethodNeverCallsPublicRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Payment : EntityBase<Payment>
            {
                private void PrivateMethod()
                {
                    PublicMethod();
                }

                public void PublicMethod() { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing private method");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying private method is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Payment");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ClassNotInheritingEntityBase_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating class not inheriting EntityBase");
        var rule = new DE024_PublicMethodNeverCallsPublicRule();
        var source = """
            #nullable enable
            public sealed class RegularClass
            {
                public void Method1()
                {
                    Method2();
                }

                public void Method2() { }
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
        var rule = new DE024_PublicMethodNeverCallsPublicRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractEntity : EntityBase<AbstractEntity>
            {
                public void Method1()
                {
                    Method2();
                }

                public void Method2() { }
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
        var rule = new DE024_PublicMethodNeverCallsPublicRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Entity : EntityBase<Entity>
            {
                public void Method1()
                {
                    Method2();
                }

                public void Method2() { }
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

        violation.Rule.ShouldBe("DE024_PublicMethodNeverCallsPublic");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/domain-entities/DE-024-metodo-publico-nunca-chama-outro-publico.md");
        violation.LlmHint.ShouldContain("Internal");
    }

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new DE024_PublicMethodNeverCallsPublicRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("DE024_PublicMethodNeverCallsPublic");
        rule.Description.ShouldContain("NUNCA");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/domain-entities/DE-024-metodo-publico-nunca-chama-outro-publico.md");
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
