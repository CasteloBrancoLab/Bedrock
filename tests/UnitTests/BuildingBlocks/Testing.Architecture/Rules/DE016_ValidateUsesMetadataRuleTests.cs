using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules;

public class DE016_ValidateUsesMetadataRuleTests : TestBase
{
    public DE016_ValidateUsesMetadataRuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void ValidateMethodReferencingMetadata_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with Validate method referencing metadata");
        var rule = new DE016_ValidateUsesMetadataRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Order : EntityBase<Order>
            {
                public string? Name { get; private set; }

                public static bool ValidateName(string? name)
                {
                    return name != null && name.Length <= OrderMetadata.NameMaxLength;
                }

                public static class OrderMetadata
                {
                    public static int NameMaxLength { get; private set; } = 100;
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing Validate method referencing metadata");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying Validate method referencing metadata passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ValidateMethodWithHardcodedValue_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with Validate method using hardcoded value");
        var rule = new DE016_ValidateUsesMetadataRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Product : EntityBase<Product>
            {
                public string? Name { get; private set; }

                public static bool ValidateName(string? name)
                {
                    return name != null && name.Length <= 100; // Hardcoded!
                }

                public static class ProductMetadata
                {
                    public static int NameMaxLength { get; private set; } = 100;
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing Validate method with hardcoded value");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying Validate method with hardcoded value fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Product");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("DE016_ValidateUsesMetadata");
        typeResult.Violation.Message.ShouldContain("ProductMetadata");
    }

    [Fact]
    public void EntityWithoutMetadataClass_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity without metadata class");
        var rule = new DE016_ValidateUsesMetadataRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Customer : EntityBase<Customer>
            {
                public string? Name { get; private set; }

                public static bool ValidateName(string? name)
                {
                    return name != null && name.Length <= 100; // No metadata class exists
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity without metadata class");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity without metadata class passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Customer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void IsValidMethod_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating entity with IsValid method (orchestrator)");
        var rule = new DE016_ValidateUsesMetadataRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Invoice : EntityBase<Invoice>
            {
                public string? Name { get; private set; }

                public static bool IsValid(string? name)
                {
                    return name != null && name.Length <= 100; // No metadata reference needed
                }

                public static class InvoiceMetadata
                {
                    public static int NameMaxLength { get; private set; } = 100;
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
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Invoice");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void IsValidInternalMethod_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating entity with IsValidInternal method");
        var rule = new DE016_ValidateUsesMetadataRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Payment : EntityBase<Payment>
            {
                public string? Name { get; private set; }

                protected bool IsValidInternal()
                {
                    return Name != null && Name.Length <= 100; // No metadata reference needed
                }

                public static class PaymentMetadata
                {
                    public static int NameMaxLength { get; private set; } = 100;
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
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Payment");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ValidateInternalMethod_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating entity with Validate*Internal method");
        var rule = new DE016_ValidateUsesMetadataRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Shipment : EntityBase<Shipment>
            {
                public string? Name { get; private set; }

                private bool ValidateNameInternal(string? name)
                {
                    return name != null && name.Length <= 100; // No metadata reference needed
                }

                public static class ShipmentMetadata
                {
                    public static int NameMaxLength { get; private set; } = 100;
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
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Shipment");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void NonPublicValidateMethod_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating entity with private Validate method");
        var rule = new DE016_ValidateUsesMetadataRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Account : EntityBase<Account>
            {
                public string? Name { get; private set; }

                private static bool ValidateName(string? name)
                {
                    return name != null && name.Length <= 100; // No metadata reference needed
                }

                public static class AccountMetadata
                {
                    public static int NameMaxLength { get; private set; } = 100;
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing private Validate method");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying private Validate method is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Account");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ClassNotInheritingEntityBase_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating class not inheriting EntityBase");
        var rule = new DE016_ValidateUsesMetadataRule();
        var source = """
            #nullable enable
            public sealed class RegularClass
            {
                public string? Name { get; set; }

                public static bool ValidateName(string? name)
                {
                    return name != null && name.Length <= 100; // No metadata reference needed
                }

                public static class RegularClassMetadata
                {
                    public static int NameMaxLength { get; private set; } = 100;
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
        var rule = new DE016_ValidateUsesMetadataRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractEntity : EntityBase<AbstractEntity>
            {
                public string? Name { get; protected set; }

                public static bool ValidateName(string? name)
                {
                    return name != null && name.Length <= 100; // No metadata reference needed
                }

                public static class AbstractEntityMetadata
                {
                    public static int NameMaxLength { get; private set; } = 100;
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
        var rule = new DE016_ValidateUsesMetadataRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Entity : EntityBase<Entity>
            {
                public string? Name { get; private set; }

                public static bool ValidateName(string? name)
                {
                    return name != null && name.Length <= 100; // Hardcoded!
                }

                public static class EntityMetadata
                {
                    public static int NameMaxLength { get; private set; } = 100;
                }
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

        violation.Rule.ShouldBe("DE016_ValidateUsesMetadata");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/domain-entities/DE-016-single-source-of-truth-para-regras-de-validacao.md");
        violation.LlmHint.ShouldContain("EntityMetadata");
    }

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new DE016_ValidateUsesMetadataRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("DE016_ValidateUsesMetadata");
        rule.Description.ShouldContain("Single Source of Truth");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/domain-entities/DE-016-single-source-of-truth-para-regras-de-validacao.md");
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
