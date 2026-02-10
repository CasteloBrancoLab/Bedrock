using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

public class DE013_MetadataNamingConventionRuleTests : TestBase
{
    public DE013_MetadataNamingConventionRuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void MetadataWithValidNamingConvention_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with valid metadata naming convention");
        var rule = new DE013_MetadataNamingConventionRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Order : EntityBase<Order>
            {
                public string? Name { get; private set; }

                public static class OrderMetadata
                {
                    public static int NameMaxLength => 100;
                    public static bool NameIsRequired => true;
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity with valid metadata naming convention");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity with valid metadata naming convention passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void MetadataWithInvalidPropertyName_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with metadata member referencing non-existent property");
        var rule = new DE013_MetadataNamingConventionRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Product : EntityBase<Product>
            {
                public string? Title { get; private set; }

                public static class ProductMetadata
                {
                    public static int NameMaxLength => 100;
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity with invalid property name in metadata");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity with invalid property name in metadata fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Product");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("DE013_MetadataNamingConvention");
        typeResult.Violation.Message.ShouldContain("Name");
    }

    [Fact]
    public void MetadataWithInvalidSuffix_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with metadata member having invalid suffix");
        var rule = new DE013_MetadataNamingConventionRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Customer : EntityBase<Customer>
            {
                public string? Name { get; private set; }

                public static class CustomerMetadata
                {
                    public static int NameSize => 100;
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity with invalid suffix in metadata");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity with invalid suffix in metadata fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Customer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation!.Message.ShouldContain("{PropertyName}{ConstraintType}");
    }

    [Fact]
    public void MetadataWithAllSupportedConstraintTypes_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with all supported constraint types");
        var rule = new DE013_MetadataNamingConventionRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Invoice : EntityBase<Invoice>
            {
                public string? Code { get; private set; }
                public int Amount { get; private set; }
                public DateTime BirthDate { get; private set; }

                public static class InvoiceMetadata
                {
                    public static string CodePropertyName => "Code";
                    public static bool CodeIsRequired => true;
                    public static bool CodeIsUnique => true;
                    public static bool CodeIsReadOnly => false;
                    public static int CodeMinLength => 1;
                    public static int CodeMaxLength => 50;
                    public static string CodePattern => "^[A-Z]+$";
                    public static string CodeFormat => "UPPER";
                    public static int AmountMinValue => 0;
                    public static int AmountMaxValue => 10000;
                    public static int BirthDateMinAgeInYears => 18;
                    public static int BirthDateMaxAgeInYears => 120;
                    public static int BirthDateMinAgeInDays => 1;
                    public static int BirthDateMaxAgeInDays => 43800;
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity with all supported constraint types");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity with all supported constraint types passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Invoice");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void MetadataWithChangeMethod_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating entity with Change*Metadata method");
        var rule = new DE013_MetadataNamingConventionRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Payment : EntityBase<Payment>
            {
                public string? Name { get; private set; }

                public static class PaymentMetadata
                {
                    public static void ChangeNameMetadata(int maxLength) { }
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity with Change*Metadata method");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying Change*Metadata method is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Payment");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void MetadataWithPrivateMember_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating entity with private metadata member");
        var rule = new DE013_MetadataNamingConventionRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Shipment : EntityBase<Shipment>
            {
                public string? Name { get; private set; }

                public static class ShipmentMetadata
                {
                    private static readonly object _lockObject = new();
                    public static int NameMaxLength => 100;
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity with private metadata member");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying private metadata member is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Shipment");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void EntityWithoutMetadataClass_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity without metadata class");
        var rule = new DE013_MetadataNamingConventionRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Account : EntityBase<Account>
            {
                public string? Name { get; private set; }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity without metadata class");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity without metadata class passes");
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
        var rule = new DE013_MetadataNamingConventionRule();
        var source = """
            #nullable enable
            public sealed class RegularClass
            {
                public string? Name { get; set; }

                public static class RegularClassMetadata
                {
                    public static int InvalidSuffix => 100;
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
        var rule = new DE013_MetadataNamingConventionRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractEntity : EntityBase<AbstractEntity>
            {
                public string? Name { get; protected set; }

                public static class AbstractEntityMetadata
                {
                    public static int InvalidSuffix => 100;
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
        var rule = new DE013_MetadataNamingConventionRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Entity : EntityBase<Entity>
            {
                public string? Name { get; private set; }

                public static class EntityMetadata
                {
                    public static int InvalidMember => 100;
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

        violation.Rule.ShouldBe("DE013_MetadataNamingConvention");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/domain-entities/DE-013-nomenclatura-de-metadados.md");
        violation.LlmHint.ShouldContain("{PropertyName}{ConstraintType}");
    }

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new DE013_MetadataNamingConventionRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("DE013_MetadataNamingConvention");
        rule.Description.ShouldContain("{PropertyName}{ConstraintType}");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/domain-entities/DE-013-nomenclatura-de-metadados.md");
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
