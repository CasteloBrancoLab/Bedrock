using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

public class DE012_StaticMetadataOverDataAnnotationsRuleTests : TestBase
{
    public DE012_StaticMetadataOverDataAnnotationsRuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void EntityWithoutDataAnnotations_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity without Data Annotations");
        var rule = new DE012_StaticMetadataOverDataAnnotationsRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Order : EntityBase<Order>
            {
                public string? Name { get; private set; }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity without Data Annotations");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity without Data Annotations passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void EntityWithRequiredAttribute_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with [Required] attribute");
        var rule = new DE012_StaticMetadataOverDataAnnotationsRule();
        var source = """
            #nullable enable
            using System.ComponentModel.DataAnnotations;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Product : EntityBase<Product>
            {
                [Required]
                public string? Name { get; private set; }
            }
            """;
        var compilations = CreateCompilationsWithDataAnnotations(source);

        // Act
        LogAct("Analyzing entity with [Required] attribute");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity with [Required] attribute fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Product");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("DE012_StaticMetadataOverDataAnnotations");
        typeResult.Violation.Message.ShouldContain("Data Annotations");
    }

    [Fact]
    public void EntityWithMaxLengthAttribute_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with [MaxLength] attribute");
        var rule = new DE012_StaticMetadataOverDataAnnotationsRule();
        var source = """
            #nullable enable
            using System.ComponentModel.DataAnnotations;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Customer : EntityBase<Customer>
            {
                [MaxLength(100)]
                public string? Name { get; private set; }
            }
            """;
        var compilations = CreateCompilationsWithDataAnnotations(source);

        // Act
        LogAct("Analyzing entity with [MaxLength] attribute");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity with [MaxLength] attribute fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Customer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
    }

    [Fact]
    public void EntityWithKeyAttribute_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with [Key] attribute from Schema namespace");
        var rule = new DE012_StaticMetadataOverDataAnnotationsRule();
        var source = """
            #nullable enable
            using System.ComponentModel.DataAnnotations;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Invoice : EntityBase<Invoice>
            {
                [Key]
                public string? Id { get; private set; }
            }
            """;
        var compilations = CreateCompilationsWithDataAnnotations(source);

        // Act
        LogAct("Analyzing entity with [Key] attribute");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity with [Key] attribute fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Invoice");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
    }

    [Fact]
    public void EntityWithDataAnnotationOnField_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with Data Annotation on field");
        var rule = new DE012_StaticMetadataOverDataAnnotationsRule();
        var source = """
            #nullable enable
            using System.ComponentModel.DataAnnotations;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Payment : EntityBase<Payment>
            {
                [Required]
                private string? _name;
            }
            """;
        var compilations = CreateCompilationsWithDataAnnotations(source);

        // Act
        LogAct("Analyzing entity with Data Annotation on field");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity with Data Annotation on field fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Payment");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
    }

    [Fact]
    public void EntityWithNonDataAnnotationAttribute_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with non-Data Annotation attribute");
        var rule = new DE012_StaticMetadataOverDataAnnotationsRule();
        var source = """
            #nullable enable
            using System;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Shipment : EntityBase<Shipment>
            {
                [Obsolete]
                public string? Name { get; private set; }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity with non-Data Annotation attribute");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity with non-Data Annotation attribute passes");
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
        var rule = new DE012_StaticMetadataOverDataAnnotationsRule();
        var source = """
            #nullable enable
            using System.ComponentModel.DataAnnotations;
            public sealed class RegularClass
            {
                [Required]
                public string? Name { get; set; }
            }
            """;
        var compilations = CreateCompilationsWithDataAnnotations(source);

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
        var rule = new DE012_StaticMetadataOverDataAnnotationsRule();
        var source = """
            #nullable enable
            using System.ComponentModel.DataAnnotations;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractEntity : EntityBase<AbstractEntity>
            {
                [Required]
                public string? Name { get; protected set; }
            }
            """;
        var compilations = CreateCompilationsWithDataAnnotations(source);

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
        var rule = new DE012_StaticMetadataOverDataAnnotationsRule();
        var source = """
            #nullable enable
            using System.ComponentModel.DataAnnotations;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Account : EntityBase<Account>
            {
                [Required]
                public string? Email { get; private set; }
            }
            """;
        var compilations = CreateCompilationsWithDataAnnotations(source);

        // Act
        LogAct("Analyzing violation metadata");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation metadata fields");
        var typeResult = results[0].TypeResults.First(t => t.TypeName == "Account");
        var violation = typeResult.Violation!;

        violation.Rule.ShouldBe("DE012_StaticMetadataOverDataAnnotations");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/domain-entities/DE-012-metadados-estaticos-vs-data-annotations.md");
        violation.LlmHint.ShouldContain("Metadata");
    }

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new DE012_StaticMetadataOverDataAnnotationsRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("DE012_StaticMetadataOverDataAnnotations");
        rule.Description.ShouldContain("Data Annotations");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/domain-entities/DE-012-metadados-estaticos-vs-data-annotations.md");
    }

    #region Helpers

    private static Dictionary<string, Compilation> CreateCompilations(string source)
    {
        return new Dictionary<string, Compilation>
        {
            ["TestProject"] = CreateSingleCompilation(source, "TestProject")
        };
    }

    private static Dictionary<string, Compilation> CreateCompilationsWithDataAnnotations(string source)
    {
        return new Dictionary<string, Compilation>
        {
            ["TestProject"] = CreateSingleCompilationWithDataAnnotations(source, "TestProject")
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

    private static Compilation CreateSingleCompilationWithDataAnnotations(string source, string assemblyName)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, path: "TestFile.cs");
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.ComponentModel.DataAnnotations.RequiredAttribute).Assembly.Location)
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
