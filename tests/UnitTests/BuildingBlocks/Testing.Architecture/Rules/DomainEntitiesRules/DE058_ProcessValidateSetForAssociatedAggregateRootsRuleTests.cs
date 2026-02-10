using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

public class DE058_ProcessValidateSetForAssociatedAggregateRootsRuleTests : TestBase
{
    public DE058_ProcessValidateSetForAssociatedAggregateRootsRuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void EntityWithAssociatedARAndProcessMethod_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with associated AR and Process method");
        var rule = new DE058_ProcessValidateSetForAssociatedAggregateRootsRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Customer : EntityBase<Customer> { }
            public sealed class Order : EntityBase<Order>
            {
                public Customer? AssignedCustomer { get; private set; }
                protected void ProcessAssignedCustomerForRegisterNewInternal() { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity with associated AR and Process method");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity with Process method passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void EntityWithAssociatedARWithoutProcessMethod_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with associated AR without Process method");
        var rule = new DE058_ProcessValidateSetForAssociatedAggregateRootsRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Customer : EntityBase<Customer> { }
            public sealed class Order : EntityBase<Order>
            {
                public Customer? AssignedCustomer { get; private set; }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity with associated AR without Process method");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity without Process method fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("DE058_ProcessValidateSetForAssociatedAggregateRoots");
        typeResult.Violation.Message.ShouldContain("Process");
    }

    [Fact]
    public void EntityWithAssociatedARAndProcessChangeMethod_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with associated AR and ProcessChange method");
        var rule = new DE058_ProcessValidateSetForAssociatedAggregateRootsRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Product : EntityBase<Product> { }
            public sealed class Invoice : EntityBase<Invoice>
            {
                public Product? MainProduct { get; private set; }
                protected void ProcessMainProductForChangeInternal() { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity with associated AR and ProcessChange method");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity with ProcessChange method passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Invoice");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void EntityWithPrimitiveProperty_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with primitive property");
        var rule = new DE058_ProcessValidateSetForAssociatedAggregateRootsRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Order : EntityBase<Order>
            {
                public string Name { get; private set; }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity with primitive property");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity with primitive property passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void EntityWithCollectionProperty_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with collection property (handled by DE-040)");
        var rule = new DE058_ProcessValidateSetForAssociatedAggregateRootsRule();
        var source = """
            #nullable enable
            using System.Collections.Generic;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class OrderItem : EntityBase<OrderItem> { }
            public sealed class Order : EntityBase<Order>
            {
                public List<OrderItem>? Items { get; private set; }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity with collection property");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity with collection property passes (DE-040 handles this)");
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
        var rule = new DE058_ProcessValidateSetForAssociatedAggregateRootsRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Customer : EntityBase<Customer> { }
            public abstract class AbstractOrder : EntityBase<AbstractOrder>
            {
                public Customer? AssignedCustomer { get; }
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
    public void EntityWithMultipleAssociatedARs_AllWithProcessMethods_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with multiple associated ARs all with Process methods");
        var rule = new DE058_ProcessValidateSetForAssociatedAggregateRootsRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Customer : EntityBase<Customer> { }
            public sealed class Supplier : EntityBase<Supplier> { }
            public sealed class Order : EntityBase<Order>
            {
                public Customer? AssignedCustomer { get; private set; }
                public Supplier? PrimarySupplier { get; private set; }
                protected void ProcessAssignedCustomerForRegisterNewInternal() { }
                protected void ProcessPrimarySupplierForRegisterNewInternal() { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity with multiple associated ARs");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity with multiple ARs and Process methods passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ViolationMetadata_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating entity to verify violation metadata");
        var rule = new DE058_ProcessValidateSetForAssociatedAggregateRootsRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Customer : EntityBase<Customer> { }
            public sealed class Order : EntityBase<Order>
            {
                public Customer? AssignedCustomer { get; private set; }
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

        violation.Rule.ShouldBe("DE058_ProcessValidateSetForAssociatedAggregateRoots");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/domain-entities/DE-058-padroes-process-validate-set-para-aggregate-roots-associadas.md");
        violation.LlmHint.ShouldContain("Process");
    }

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new DE058_ProcessValidateSetForAssociatedAggregateRootsRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("DE058_ProcessValidateSetForAssociatedAggregateRoots");
        rule.Description.ShouldContain("Process");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/domain-entities/DE-058-padroes-process-validate-set-para-aggregate-roots-associadas.md");
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
