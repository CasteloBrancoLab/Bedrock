using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules;

public class DE002_PrivateConstructorRuleTests : TestBase
{
    public DE002_PrivateConstructorRuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void ClassWithPrivateConstructor_ShouldPass()
    {
        // Arrange
        LogArrange("Creating class with private constructor");
        var rule = new DE002_PrivateConstructorRule();
        var source = """
            public sealed class Order
            {
                private Order() { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing class with private constructor");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying class with private constructor passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
        typeResult.Violation.ShouldBeNull();
    }

    [Fact]
    public void ClassWithMultiplePrivateConstructors_ShouldPass()
    {
        // Arrange
        LogArrange("Creating class with multiple private constructors");
        var rule = new DE002_PrivateConstructorRule();
        var source = """
            public sealed class Product
            {
                private Product() { }
                private Product(string name) { }
                private Product(string name, decimal price) { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing class with multiple private constructors");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying class with multiple private constructors passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Product");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
        typeResult.Violation.ShouldBeNull();
    }

    [Fact]
    public void ClassWithImplicitConstructor_ShouldPass()
    {
        // Arrange
        LogArrange("Creating class with implicit constructor (no declared constructor)");
        var rule = new DE002_PrivateConstructorRule();
        var source = """
            public sealed class Customer { }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing class with implicit constructor");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying class with implicit constructor passes (implicit constructors are ignored)");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Customer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
        typeResult.Violation.ShouldBeNull();
    }

    [Fact]
    public void ClassWithPublicConstructor_ShouldFail()
    {
        // Arrange
        LogArrange("Creating class with public constructor");
        var rule = new DE002_PrivateConstructorRule();
        var source = """
            public sealed class Invoice
            {
                public Invoice() { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing class with public constructor");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying class with public constructor fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Invoice");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("DE002_PrivateConstructor");
        typeResult.Violation.Message.ShouldContain("Invoice");
        typeResult.Violation.Message.ShouldContain("public");
    }

    [Fact]
    public void ClassWithProtectedConstructor_ShouldFail()
    {
        // Arrange
        LogArrange("Creating class with protected constructor");
        var rule = new DE002_PrivateConstructorRule();
        var source = """
            public class Payment
            {
                protected Payment() { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing class with protected constructor");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying class with protected constructor fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Payment");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Message.ShouldContain("protected");
    }

    [Fact]
    public void ClassWithInternalConstructor_ShouldFail()
    {
        // Arrange
        LogArrange("Creating class with internal constructor");
        var rule = new DE002_PrivateConstructorRule();
        var source = """
            public sealed class Shipment
            {
                internal Shipment() { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing class with internal constructor");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying class with internal constructor fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Shipment");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Message.ShouldContain("internal");
    }

    [Fact]
    public void ClassWithMixedConstructors_PrivateAndPublic_ShouldFail()
    {
        // Arrange
        LogArrange("Creating class with mixed private and public constructors");
        var rule = new DE002_PrivateConstructorRule();
        var source = """
            public sealed class Warehouse
            {
                private Warehouse() { }
                public Warehouse(string name) { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing class with mixed constructors");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying class with mixed constructors fails due to public constructor");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Warehouse");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
    }

    [Fact]
    public void ViolationMetadata_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating class to verify violation metadata");
        var rule = new DE002_PrivateConstructorRule();
        var source = """
            public sealed class Account
            {
                public Account() { }
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

        violation.Rule.ShouldBe("DE002_PrivateConstructor");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/domain-entities/DE-002-construtores-privados-com-factory-methods.md");
        violation.Project.ShouldBe("TestProject");
        violation.LlmHint.ShouldContain("private");
        violation.LlmHint.ShouldContain("Account");
    }

    [Fact]
    public void AbstractClass_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating abstract class");
        var rule = new DE002_PrivateConstructorRule();
        var source = """
            public abstract class AbstractEntity
            {
                public AbstractEntity() { }
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
    public void StaticClass_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating static class");
        var rule = new DE002_PrivateConstructorRule();
        var source = """
            public static class Helper { }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing static class");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying static class is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Helper");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void Record_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating record type");
        var rule = new DE002_PrivateConstructorRule();
        var source = """
            public record OrderRecord(string Id, decimal Amount);
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
    public void Interface_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating interface");
        var rule = new DE002_PrivateConstructorRule();
        var source = """
            public interface IRepository { }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing interface");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying interface is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "IRepository");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
        typeResult.Violation.ShouldBeNull();
    }

    [Fact]
    public void Struct_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating struct");
        var rule = new DE002_PrivateConstructorRule();
        var source = """
            public struct Money
            {
                public Money(decimal amount) { Amount = amount; }
                public decimal Amount { get; }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing struct");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying struct is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Money");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
        typeResult.Violation.ShouldBeNull();
    }

    [Fact]
    public void Enum_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating enum");
        var rule = new DE002_PrivateConstructorRule();
        var source = """
            public enum Status { Active, Inactive }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing enum");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying enum is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Status");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
        typeResult.Violation.ShouldBeNull();
    }

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new DE002_PrivateConstructorRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("DE002_PrivateConstructor");
        rule.Description.ShouldContain("privados");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/domain-entities/DE-002-construtores-privados-com-factory-methods.md");
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
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    #endregion
}
