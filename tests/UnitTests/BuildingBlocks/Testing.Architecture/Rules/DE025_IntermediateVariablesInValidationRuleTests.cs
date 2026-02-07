using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules;

public class DE025_IntermediateVariablesInValidationRuleTests : TestBase
{
    public DE025_IntermediateVariablesInValidationRuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void InternalMethodWithIsSuccessVariable_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with Internal method having isSuccess variable");
        var rule = new DE025_IntermediateVariablesInValidationRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Order : EntityBase<Order>
            {
                private bool ChangeNameInternal(string name, string status)
                {
                    var isSuccess = SetName(name) & SetStatus(status);
                    return isSuccess;
                }

                private bool SetName(string name) => true;
                private bool SetStatus(string status) => true;
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing Internal method with isSuccess variable");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying Internal method with isSuccess variable passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void InternalMethodWithMultipleSetCallsWithoutIsSuccess_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with Internal method having multiple Set calls without isSuccess");
        var rule = new DE025_IntermediateVariablesInValidationRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Product : EntityBase<Product>
            {
                private bool ChangeNameInternal(string name, string status)
                {
                    return SetName(name) & SetStatus(status);
                }

                private bool SetName(string name) => true;
                private bool SetStatus(string status) => true;
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing Internal method without isSuccess variable");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying Internal method without isSuccess variable fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Product");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("DE025_IntermediateVariablesInValidation");
        typeResult.Violation.Message.ShouldContain("isSuccess");
    }

    [Fact]
    public void InternalMethodWithSingleSetCall_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with Internal method having single Set call");
        var rule = new DE025_IntermediateVariablesInValidationRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Customer : EntityBase<Customer>
            {
                private bool ChangeNameInternal(string name)
                {
                    return SetName(name);
                }

                private bool SetName(string name) => true;
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing Internal method with single Set call");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying Internal method with single Set call passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Customer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void NonInternalMethod_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating entity with non-Internal method");
        var rule = new DE025_IntermediateVariablesInValidationRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Invoice : EntityBase<Invoice>
            {
                private bool ChangeName(string name, string status)
                {
                    return SetName(name) & SetStatus(status);
                }

                private bool SetName(string name) => true;
                private bool SetStatus(string status) => true;
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing non-Internal method");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying non-Internal method is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Invoice");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void InternalMethodNotReturningBool_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating entity with Internal method not returning bool");
        var rule = new DE025_IntermediateVariablesInValidationRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Payment : EntityBase<Payment>
            {
                private void ChangeNameInternal(string name, string status)
                {
                    SetName(name);
                    SetStatus(status);
                }

                private void SetName(string name) { }
                private void SetStatus(string status) { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing Internal method not returning bool");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying Internal method not returning bool is ignored");
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
        var rule = new DE025_IntermediateVariablesInValidationRule();
        var source = """
            #nullable enable
            public sealed class RegularClass
            {
                private bool ChangeInternal(string name, string status)
                {
                    return SetName(name) & SetStatus(status);
                }

                private bool SetName(string name) => true;
                private bool SetStatus(string status) => true;
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
        var rule = new DE025_IntermediateVariablesInValidationRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractEntity : EntityBase<AbstractEntity>
            {
                private bool ChangeInternal(string name, string status)
                {
                    return SetName(name) & SetStatus(status);
                }

                private bool SetName(string name) => true;
                private bool SetStatus(string status) => true;
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
        var rule = new DE025_IntermediateVariablesInValidationRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Entity : EntityBase<Entity>
            {
                private bool ChangeInternal(string name, string status)
                {
                    return SetName(name) & SetStatus(status);
                }

                private bool SetName(string name) => true;
                private bool SetStatus(string status) => true;
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

        violation.Rule.ShouldBe("DE025_IntermediateVariablesInValidation");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/domain-entities/DE-025-variaveis-intermediarias-para-legibilidade-e-debug.md");
        violation.LlmHint.ShouldContain("isSuccess");
    }

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new DE025_IntermediateVariablesInValidationRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("DE025_IntermediateVariablesInValidation");
        rule.Description.ShouldContain("isSuccess");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/domain-entities/DE-025-variaveis-intermediarias-para-legibilidade-e-debug.md");
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
