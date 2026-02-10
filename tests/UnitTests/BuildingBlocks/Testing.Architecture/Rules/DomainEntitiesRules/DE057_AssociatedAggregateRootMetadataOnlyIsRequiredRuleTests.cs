using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

public class DE057_AssociatedAggregateRootMetadataOnlyIsRequiredRuleTests : TestBase
{
    public DE057_AssociatedAggregateRootMetadataOnlyIsRequiredRuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void EntityWithAssociatedARAndOnlyIsRequiredMetadata_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with associated AR and only IsRequired metadata");
        var rule = new DE057_AssociatedAggregateRootMetadataOnlyIsRequiredRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Customer : EntityBase<Customer> { }
            public sealed class Order : EntityBase<Order>
            {
                public Customer? AssignedCustomer { get; }
                public static class OrderMetadata
                {
                    public static bool AssignedCustomerIsRequired => true;
                    public static string AssignedCustomerPropertyName => "AssignedCustomer";
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity with associated AR and only IsRequired metadata");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity with correct metadata passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void EntityWithAssociatedARAndMinLengthMetadata_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with associated AR and MinLength metadata");
        var rule = new DE057_AssociatedAggregateRootMetadataOnlyIsRequiredRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Customer : EntityBase<Customer> { }
            public sealed class Order : EntityBase<Order>
            {
                public Customer? AssignedCustomer { get; }
                public static class OrderMetadata
                {
                    public static bool AssignedCustomerIsRequired => true;
                    public static int AssignedCustomerMinLength => 1;
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity with associated AR and MinLength metadata");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity with MinLength metadata fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("DE057_AssociatedAggregateRootMetadataOnlyIsRequired");
        typeResult.Violation.Message.ShouldContain("MinLength");
    }

    [Fact]
    public void EntityWithAssociatedARAndMaxLengthMetadata_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with associated AR and MaxLength metadata");
        var rule = new DE057_AssociatedAggregateRootMetadataOnlyIsRequiredRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Product : EntityBase<Product> { }
            public sealed class Invoice : EntityBase<Invoice>
            {
                public Product? MainProduct { get; }
                public static class InvoiceMetadata
                {
                    public static int MainProductMaxLength => 100;
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity with associated AR and MaxLength metadata");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity with MaxLength metadata fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Invoice");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation!.Message.ShouldContain("MaxLength");
    }

    [Fact]
    public void EntityWithAssociatedARAndMinValueMetadata_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with associated AR and MinValue metadata");
        var rule = new DE057_AssociatedAggregateRootMetadataOnlyIsRequiredRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Supplier : EntityBase<Supplier> { }
            public sealed class Purchase : EntityBase<Purchase>
            {
                public Supplier? PrimarySupplier { get; }
                public static class PurchaseMetadata
                {
                    public static int PrimarySupplierMinValue => 0;
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity with associated AR and MinValue metadata");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity with MinValue metadata fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Purchase");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation!.Message.ShouldContain("MinValue");
    }

    [Fact]
    public void EntityWithAssociatedARAndMaxValueMetadata_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with associated AR and MaxValue metadata");
        var rule = new DE057_AssociatedAggregateRootMetadataOnlyIsRequiredRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Warehouse : EntityBase<Warehouse> { }
            public sealed class Shipment : EntityBase<Shipment>
            {
                public Warehouse? OriginWarehouse { get; }
                public static class ShipmentMetadata
                {
                    public static int OriginWarehouseMaxValue => 999;
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity with associated AR and MaxValue metadata");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity with MaxValue metadata fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Shipment");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation!.Message.ShouldContain("MaxValue");
    }

    [Fact]
    public void EntityWithPrimitivePropertyAndMinMaxMetadata_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with primitive property and MinMax metadata");
        var rule = new DE057_AssociatedAggregateRootMetadataOnlyIsRequiredRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Order : EntityBase<Order>
            {
                public string Name { get; }
                public static class OrderMetadata
                {
                    public static int NameMinLength => 1;
                    public static int NameMaxLength => 100;
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity with primitive property and MinMax metadata");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity with primitive property passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void EntityWithNoMetadataClass_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity without metadata class");
        var rule = new DE057_AssociatedAggregateRootMetadataOnlyIsRequiredRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Customer : EntityBase<Customer> { }
            public sealed class Order : EntityBase<Order>
            {
                public Customer? AssignedCustomer { get; }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity without metadata class");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity without metadata class passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void AbstractClass_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating abstract class");
        var rule = new DE057_AssociatedAggregateRootMetadataOnlyIsRequiredRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Customer : EntityBase<Customer> { }
            public abstract class AbstractOrder : EntityBase<AbstractOrder>
            {
                public Customer? AssignedCustomer { get; }
                public static class AbstractOrderMetadata
                {
                    public static int AssignedCustomerMinLength => 1;
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
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "AbstractOrder");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ViolationMetadata_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating entity to verify violation metadata");
        var rule = new DE057_AssociatedAggregateRootMetadataOnlyIsRequiredRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Customer : EntityBase<Customer> { }
            public sealed class Order : EntityBase<Order>
            {
                public Customer? AssignedCustomer { get; }
                public static class OrderMetadata
                {
                    public static int AssignedCustomerMinLength => 1;
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing violation metadata");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation metadata fields");
        var typeResult = results[0].TypeResults.First(t => t.TypeName == "Order");
        var violation = typeResult.Violation!;

        violation.Rule.ShouldBe("DE057_AssociatedAggregateRootMetadataOnlyIsRequired");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/domain-entities/DE-057-metadata-aggregate-roots-associadas-apenas-isrequired.md");
        violation.LlmHint.ShouldContain("Remover");
    }

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new DE057_AssociatedAggregateRootMetadataOnlyIsRequiredRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("DE057_AssociatedAggregateRootMetadataOnlyIsRequired");
        rule.Description.ShouldContain("IsRequired");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/domain-entities/DE-057-metadata-aggregate-roots-associadas-apenas-isrequired.md");
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
