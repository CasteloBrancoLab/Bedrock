using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules;

public class DE004_InvalidStateNeverExistsRuleTests : TestBase
{
    public DE004_InvalidStateNeverExistsRuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void EntityWithRegisterNewReturningNullable_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with RegisterNew returning nullable");
        var rule = new DE004_InvalidStateNeverExistsRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Order : EntityBase<Order>
            {
                public static Order? RegisterNew(string id) => null;
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity with RegisterNew returning nullable");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity with RegisterNew returning nullable passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
        typeResult.Violation.ShouldBeNull();
    }

    [Fact]
    public void EntityWithoutRegisterNew_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity without RegisterNew");
        var rule = new DE004_InvalidStateNeverExistsRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Product : EntityBase<Product>
            {
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity without RegisterNew");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity without RegisterNew fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Product");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("DE004_InvalidStateNeverExists");
        typeResult.Violation.Message.ShouldContain("RegisterNew");
        typeResult.Violation.Message.ShouldContain("Product");
    }

    [Fact]
    public void EntityWithRegisterNewReturningNonNullable_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with RegisterNew returning non-nullable");
        var rule = new DE004_InvalidStateNeverExistsRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Customer : EntityBase<Customer>
            {
                public static Customer RegisterNew(string id) => new Customer();
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity with RegisterNew returning non-nullable");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity with RegisterNew returning non-nullable fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Customer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Message.ShouldContain("Customer?");
        typeResult.Violation.Message.ShouldContain("nullable");
    }

    [Fact]
    public void EntityWithPrivateRegisterNew_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with private RegisterNew");
        var rule = new DE004_InvalidStateNeverExistsRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Invoice : EntityBase<Invoice>
            {
                private static Invoice? RegisterNew(string id) => null;
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity with private RegisterNew");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity with private RegisterNew fails (must be public)");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Invoice");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
    }

    [Fact]
    public void EntityWithInstanceRegisterNew_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with instance (non-static) RegisterNew");
        var rule = new DE004_InvalidStateNeverExistsRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Payment : EntityBase<Payment>
            {
                public Payment? RegisterNew(string id) => null;
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity with instance RegisterNew");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity with instance RegisterNew fails (must be static)");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Payment");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
    }

    [Fact]
    public void EntityWithRegisterNewReturningVoid_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with RegisterNew returning void");
        var rule = new DE004_InvalidStateNeverExistsRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Shipment : EntityBase<Shipment>
            {
                public static void RegisterNew(string id) { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity with RegisterNew returning void");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity with RegisterNew returning void fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Shipment");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
    }

    [Fact]
    public void ClassNotInheritingEntityBase_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating class not inheriting EntityBase");
        var rule = new DE004_InvalidStateNeverExistsRule();
        var source = """
            #nullable enable
            public sealed class RegularClass
            {
                // No RegisterNew needed
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
        LogArrange("Creating abstract entity class");
        var rule = new DE004_InvalidStateNeverExistsRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractEntity : EntityBase<AbstractEntity>
            {
                // No RegisterNew needed for abstract
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
    public void Record_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating record type");
        var rule = new DE004_InvalidStateNeverExistsRule();
        var source = """
            #nullable enable
            public record OrderRecord(string Id);
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing record");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying record is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "OrderRecord");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ViolationMetadata_MissingMethod_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating entity to verify violation metadata for missing method");
        var rule = new DE004_InvalidStateNeverExistsRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Account : EntityBase<Account>
            {
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

        violation.Rule.ShouldBe("DE004_InvalidStateNeverExists");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/domain-entities/DE-004-estado-invalido-nunca-existe-na-memoria.md");
        violation.Project.ShouldBe("TestProject");
        violation.LlmHint.ShouldContain("RegisterNew");
        violation.LlmHint.ShouldContain("Account?");
    }

    [Fact]
    public void ViolationMetadata_WrongReturnType_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating entity to verify violation metadata for wrong return type");
        var rule = new DE004_InvalidStateNeverExistsRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Warehouse : EntityBase<Warehouse>
            {
                public static Warehouse RegisterNew(string id) => new Warehouse();
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing violation metadata");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation metadata for wrong return type");
        var typeResult = results[0].TypeResults.First(t => t.TypeName == "Warehouse");
        var violation = typeResult.Violation!;

        violation.Message.ShouldContain("Warehouse?");
        violation.Message.ShouldContain("nullable");
        violation.LlmHint.ShouldContain("null");
    }

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new DE004_InvalidStateNeverExistsRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("DE004_InvalidStateNeverExists");
        rule.Description.ShouldContain("RegisterNew");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/domain-entities/DE-004-estado-invalido-nunca-existe-na-memoria.md");
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
