using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules;

public class DE010_ValidationUtilsForStandardValidationsRuleTests : TestBase
{
    public DE010_ValidationUtilsForStandardValidationsRuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void ValidateMethodUsingValidationUtils_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with Validate method using ValidationUtils");
        var rule = new DE010_ValidationUtilsForStandardValidationsRule();
        var source = """
            #nullable enable
            public static class ValidationUtils
            {
                public static bool ValidateIsRequired(object? ctx, string? value, string fieldName) => true;
            }
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Order : EntityBase<Order>
            {
                public static bool ValidateName(object ctx, string? name)
                {
                    return ValidationUtils.ValidateIsRequired(ctx, name, "Name");
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing Validate method using ValidationUtils");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying Validate method using ValidationUtils passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ValidateMethodNotUsingValidationUtils_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with Validate method not using ValidationUtils");
        var rule = new DE010_ValidationUtilsForStandardValidationsRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Product : EntityBase<Product>
            {
                public static bool ValidateName(string? name)
                {
                    return !string.IsNullOrEmpty(name);
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing Validate method not using ValidationUtils");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying Validate method not using ValidationUtils fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Product");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("DE010_ValidationUtilsForStandardValidations");
        typeResult.Violation.Message.ShouldContain("ValidationUtils");
    }

    [Fact]
    public void IsValidMethod_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating entity with IsValid method (orchestrator, not validator)");
        var rule = new DE010_ValidationUtilsForStandardValidationsRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Customer : EntityBase<Customer>
            {
                public static bool IsValid(string? name)
                {
                    return !string.IsNullOrEmpty(name);
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing IsValid method");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying IsValid method is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Customer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void IsValidInternalMethod_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating entity with IsValidInternal method");
        var rule = new DE010_ValidationUtilsForStandardValidationsRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Invoice : EntityBase<Invoice>
            {
                protected bool IsValidInternal()
                {
                    return true;
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing IsValidInternal method");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying IsValidInternal method is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Invoice");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ValidateInternalMethod_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating entity with Validate*Internal method");
        var rule = new DE010_ValidationUtilsForStandardValidationsRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Payment : EntityBase<Payment>
            {
                private bool ValidateNameInternal(string? name)
                {
                    return true;
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing Validate*Internal method");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying Validate*Internal method is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Payment");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void NonPublicValidateMethod_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating entity with private Validate method (DE009 handles this)");
        var rule = new DE010_ValidationUtilsForStandardValidationsRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Shipment : EntityBase<Shipment>
            {
                private static bool ValidateName(string? name)
                {
                    return true;
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing private Validate method");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying private Validate method is ignored by this rule");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Shipment");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ClassNotInheritingEntityBase_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating class not inheriting EntityBase");
        var rule = new DE010_ValidationUtilsForStandardValidationsRule();
        var source = """
            #nullable enable
            public sealed class RegularClass
            {
                public static bool ValidateName(string? name)
                {
                    return true;
                }
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
        var rule = new DE010_ValidationUtilsForStandardValidationsRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractEntity : EntityBase<AbstractEntity>
            {
                public static bool ValidateName(string? name)
                {
                    return true;
                }
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
        var rule = new DE010_ValidationUtilsForStandardValidationsRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Account : EntityBase<Account>
            {
                public static bool ValidateEmail(string? email)
                {
                    return email != null;
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing violation metadata");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation metadata fields");
        var typeResult = results[0].TypeResults.First(t => t.TypeName == "Account");
        var violation = typeResult.Violation!;

        violation.Rule.ShouldBe("DE010_ValidationUtilsForStandardValidations");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/domain-entities/DE-010-validationutils-para-validacoes-padrao.md");
        violation.LlmHint.ShouldContain("ValidationUtils");
    }

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new DE010_ValidationUtilsForStandardValidationsRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("DE010_ValidationUtilsForStandardValidations");
        rule.Description.ShouldContain("ValidationUtils");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/domain-entities/DE-010-validationutils-para-validacoes-padrao.md");
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
